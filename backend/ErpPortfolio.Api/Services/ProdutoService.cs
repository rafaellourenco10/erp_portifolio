// =====================================================================================
// Arquivo....: ProdutoService.cs
// Versão.....: 1.1.0
// Data.......: 21/09/2026
// Descrição..: Regras de negócio e persistência de produtos.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.produtos
//                - SELECT : listagem (ILIKE em nome ou sku, ativo = @ativo, LEFT JOIN em
//                           categorias, ORDER BY nome, id, LIMIT/OFFSET),
//                           consulta por id e checagem de SKU duplicado
//                - INSERT : inclusão
//                - UPDATE : edição e inativação (ativo = false)
//              public.categorias
//                - SELECT : validação da categoria escolhida (existe e está ativa)
// Fontes.....: ErpPortfolioDbContext.Produtos / Categorias (EF Core / Npgsql).
//              Violação do índice único ix_produtos_sku (SQLSTATE 23505) é convertida
//              em ConflitoException.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 21/09/2026 - Categoria como registro: valida o CategoriaId (DadoInvalidoException)
//                        e devolve o nome da categoria nas consultas.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ErpPortfolio.Api.Services;

public class ProdutoService(ErpPortfolioDbContext contexto) : IProdutoService
{
    private const string MensagemSkuDuplicado = "Já existe um produto cadastrado com este SKU.";
    private const string MensagemSkuDuplicadoInativo =
        "Já existe um produto INATIVO com este SKU. Reative o cadastro existente em vez de criar outro.";
    private const string MensagemCategoriaInvalida = "Categoria inexistente ou inativa.";

    public async Task<ResultadoPaginadoDto<ProdutoRespostaDto>> ListarAsync(ProdutoFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Produtos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var padrao = $"%{ClienteService.EscaparCuringasLike(filtro.Busca.Trim())}%";
            consulta = consulta.Where(p => EF.Functions.ILike(p.Nome, padrao) || EF.Functions.ILike(p.Sku, padrao));
        }

        if (filtro.Ativo is bool ativo)
        {
            consulta = consulta.Where(p => p.Ativo == ativo);
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(p => p.Nome)
            .ThenBy(p => p.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(ProdutoRespostaDto.Projecao)
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<ProdutoRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<ProdutoRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        return await contexto.Produtos.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProdutoRespostaDto.Projecao)
            .FirstOrDefaultAsync(cancelamento);
    }

    public async Task<ProdutoRespostaDto> CriarAsync(ProdutoCriacaoDto dados, CancellationToken cancelamento)
    {
        var categoria = await ObterCategoriaValidaAsync(dados.CategoriaId, categoriaAtualId: null, cancelamento);

        var produto = new Produto
        {
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };
        AplicarDados(produto, dados, categoria);

        await GarantirSkuUnicoAsync(produto.Sku, idIgnorado: null, cancelamento);

        contexto.Produtos.Add(produto);
        await SalvarAsync(cancelamento);

        return ProdutoRespostaDto.DeEntidade(produto);
    }

    public async Task<ProdutoRespostaDto?> AtualizarAsync(int id, ProdutoAtualizacaoDto dados, CancellationToken cancelamento)
    {
        var produto = await contexto.Produtos.FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (produto is null)
            return null;

        var categoria = await ObterCategoriaValidaAsync(dados.CategoriaId, categoriaAtualId: produto.CategoriaId, cancelamento);

        AplicarDados(produto, dados, categoria);
        if (dados.Ativo is bool ativo)
            produto.Ativo = ativo;

        await GarantirSkuUnicoAsync(produto.Sku, idIgnorado: id, cancelamento);
        await SalvarAsync(cancelamento);

        return ProdutoRespostaDto.DeEntidade(produto);
    }

    public async Task<bool> InativarAsync(int id, CancellationToken cancelamento)
    {
        var produto = await contexto.Produtos.FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (produto is null)
            return false;

        produto.Ativo = false;
        await SalvarAsync(cancelamento);

        return true;
    }

    // Os campos obrigatórios já foram validados pelo [ApiController]; o "!" só remove o anulável do DTO.
    private static void AplicarDados(Produto produto, ProdutoCriacaoDto dados, Categoria? categoria)
    {
        produto.Nome = dados.Nome;
        produto.Sku = dados.Sku;
        produto.Categoria = categoria;
        produto.CategoriaId = categoria?.Id;
        produto.Unidade = dados.Unidade;
        produto.PrecoVenda = dados.PrecoVenda!.Value;
        produto.Custo = dados.Custo!.Value;
    }

    // Só o banco sabe se a categoria existe e está ativa. Uma categoria inativa continua aceita
    // quando é a que o produto já tem (senão editar qualquer campo de um produto antigo falharia).
    private async Task<Categoria?> ObterCategoriaValidaAsync(int? categoriaId, int? categoriaAtualId, CancellationToken cancelamento)
    {
        if (categoriaId is null)
            return null;

        var categoria = await contexto.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId, cancelamento);
        if (categoria is null || (!categoria.Ativo && categoria.Id != categoriaAtualId))
            throw new DadoInvalidoException(nameof(ProdutoCriacaoDto.CategoriaId), MensagemCategoriaInvalida);

        return categoria;
    }

    private async Task GarantirSkuUnicoAsync(string sku, int? idIgnorado, CancellationToken cancelamento)
    {
        var existente = await contexto.Produtos
            .Where(p => p.Sku == sku && p.Id != idIgnorado)
            .Select(p => new { p.Ativo })
            .FirstOrDefaultAsync(cancelamento);

        if (existente is not null)
            throw new ConflitoException(existente.Ativo ? MensagemSkuDuplicado : MensagemSkuDuplicadoInativo);
    }

    // O índice único cobre a corrida entre a checagem prévia e o INSERT/UPDATE.
    private async Task SalvarAsync(CancellationToken cancelamento)
    {
        try
        {
            await contexto.SaveChangesAsync(cancelamento);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflitoException(MensagemSkuDuplicado);
        }
    }
}
