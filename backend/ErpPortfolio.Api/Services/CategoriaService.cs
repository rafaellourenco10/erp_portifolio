// =====================================================================================
// Arquivo....: CategoriaService.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Regras de negócio e persistência de categorias de produtos.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.categorias
//                - SELECT : listagem (ILIKE em nome, ativo = @ativo, ORDER BY nome, id,
//                           LIMIT/OFFSET), consulta por id e checagem de nome duplicado
//                - INSERT : inclusão
//                - UPDATE : edição e inativação (ativo = false)
// Fontes.....: ErpPortfolioDbContext.Categorias (EF Core / Npgsql).
//              Violação do índice único ix_categorias_nome (SQLSTATE 23505) é convertida
//              em ConflitoException.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ErpPortfolio.Api.Services;

public class CategoriaService(ErpPortfolioDbContext contexto) : ICategoriaService
{
    private const string MensagemNomeDuplicado = "Já existe uma categoria com este nome.";
    private const string MensagemNomeDuplicadoInativa =
        "Já existe uma categoria INATIVA com este nome. Reative o cadastro existente em vez de criar outra.";

    public async Task<ResultadoPaginadoDto<CategoriaRespostaDto>> ListarAsync(CategoriaFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Categorias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var padrao = $"%{ClienteService.EscaparCuringasLike(filtro.Busca.Trim())}%";
            consulta = consulta.Where(c => EF.Functions.ILike(c.Nome, padrao));
        }

        if (filtro.Ativo is bool ativo)
        {
            consulta = consulta.Where(c => c.Ativo == ativo);
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(c => c.Nome)
            .ThenBy(c => c.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(c => CategoriaRespostaDto.DeEntidade(c))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<CategoriaRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<CategoriaRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        var categoria = await contexto.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancelamento);
        return categoria is null ? null : CategoriaRespostaDto.DeEntidade(categoria);
    }

    public async Task<CategoriaRespostaDto> CriarAsync(CategoriaCriacaoDto dados, CancellationToken cancelamento)
    {
        var categoria = new Categoria
        {
            Nome = dados.Nome,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        await GarantirNomeUnicoAsync(categoria.Nome, idIgnorado: null, cancelamento);

        contexto.Categorias.Add(categoria);
        await SalvarAsync(cancelamento);

        return CategoriaRespostaDto.DeEntidade(categoria);
    }

    public async Task<CategoriaRespostaDto?> AtualizarAsync(int id, CategoriaAtualizacaoDto dados, CancellationToken cancelamento)
    {
        var categoria = await contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancelamento);
        if (categoria is null)
            return null;

        categoria.Nome = dados.Nome;
        if (dados.Ativo is bool ativo)
            categoria.Ativo = ativo;

        await GarantirNomeUnicoAsync(categoria.Nome, idIgnorado: id, cancelamento);
        await SalvarAsync(cancelamento);

        return CategoriaRespostaDto.DeEntidade(categoria);
    }

    // Inativar não altera os produtos: eles continuam ligados à categoria (só deixa de ser oferecida em novos cadastros).
    public async Task<bool> InativarAsync(int id, CancellationToken cancelamento)
    {
        var categoria = await contexto.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancelamento);
        if (categoria is null)
            return false;

        categoria.Ativo = false;
        await SalvarAsync(cancelamento);

        return true;
    }

    // ILIKE sem curingas = igualdade sem diferenciar maiúsculas/minúsculas ("Cabos" e "cabos" são a mesma categoria).
    private async Task GarantirNomeUnicoAsync(string nome, int? idIgnorado, CancellationToken cancelamento)
    {
        var padrao = ClienteService.EscaparCuringasLike(nome);
        var existente = await contexto.Categorias
            .Where(c => c.Id != idIgnorado && EF.Functions.ILike(c.Nome, padrao))
            .Select(c => new { c.Ativo })
            .FirstOrDefaultAsync(cancelamento);

        if (existente is not null)
            throw new ConflitoException(existente.Ativo ? MensagemNomeDuplicado : MensagemNomeDuplicadoInativa);
    }

    // O índice único (exato) cobre a corrida entre a checagem prévia e o INSERT/UPDATE.
    private async Task SalvarAsync(CancellationToken cancelamento)
    {
        try
        {
            await contexto.SaveChangesAsync(cancelamento);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflitoException(MensagemNomeDuplicado);
        }
    }
}
