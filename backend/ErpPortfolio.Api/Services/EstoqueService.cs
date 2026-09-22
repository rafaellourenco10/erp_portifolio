// =====================================================================================
// Arquivo....: EstoqueService.cs
// Versão.....: 1.1.0
// Data.......: 22/09/2026
// Descrição..: Consulta de estoque (listagem com saldo, extrato por produto) e entrada
//              manual. O saldo nunca é gravado: é sempre Σ Entrada − Σ Saída, calculado
//              na consulta (SPEC.md, E1).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.produtos
//                - SELECT : listagem (ILIKE em nome ou sku, ORDER BY nome, id, LIMIT/OFFSET)
//                           e validação do produto na entrada manual (existe e está ativo)
//              public.estoque_movimentacoes
//                - SELECT : saldo agregado por produto (subconsulta correlacionada) e
//                           extrato paginado de um produto (ORDER BY data_movimentacao DESC)
//                - INSERT : entrada manual (E4)
// Fontes.....: ErpPortfolioDbContext.Produtos / EstoqueMovimentacoes (EF Core / Npgsql).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e extrato).
//   1.1.0 - 22/09/2026 - Entrada manual (RegistrarEntradaAsync).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class EstoqueService(ErpPortfolioDbContext contexto) : IEstoqueService
{
    public async Task<ResultadoPaginadoDto<EstoqueResumoDto>> ListarAsync(EstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Produtos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var padrao = $"%{ClienteService.EscaparCuringasLike(filtro.Busca.Trim())}%";
            consulta = consulta.Where(p => EF.Functions.ILike(p.Nome, padrao) || EF.Functions.ILike(p.Sku, padrao));
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(p => p.Nome)
            .ThenBy(p => p.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            // Subconsulta correlacionada, traduzida em SQL: COALESCE(SUM(CASE WHEN tipo = 'Entrada' THEN quantidade ELSE -quantidade END), 0).
            // Precisa ficar inline no Select: um método de instância separado não é traduzido pelo EF (InvalidOperationException).
            .Select(p => new EstoqueResumoDto(
                p.Id,
                p.Nome,
                p.Sku,
                p.Unidade,
                contexto.EstoqueMovimentacoes
                    .Where(m => m.ProdutoId == p.Id)
                    .Sum(m => m.Tipo == TipoMovimentacao.Entrada ? m.Quantidade : -m.Quantidade)))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<EstoqueResumoDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<ResultadoPaginadoDto<MovimentacaoRespostaDto>?> ObterExtratoAsync(
        int produtoId, EstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        var produtoExiste = await contexto.Produtos.AsNoTracking().AnyAsync(p => p.Id == produtoId, cancelamento);
        if (!produtoExiste)
            return null;

        var consulta = contexto.EstoqueMovimentacoes.AsNoTracking().Where(m => m.ProdutoId == produtoId);

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(m => new MovimentacaoRespostaDto(m.Tipo, m.Quantidade, m.Motivo, m.PedidoId, m.DataMovimentacao))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<MovimentacaoRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<MovimentacaoRespostaDto?> RegistrarEntradaAsync(EstoqueEntradaDto dados, CancellationToken cancelamento)
    {
        var produto = await contexto.Produtos.FirstOrDefaultAsync(p => p.Id == dados.ProdutoId, cancelamento);
        if (produto is null)
            return null;

        if (!produto.Ativo)
            throw new DadoInvalidoException(nameof(EstoqueEntradaDto.ProdutoId), $"O produto \"{produto.Nome}\" está inativo.");

        var movimentacao = new EstoqueMovimentacao
        {
            ProdutoId = produto.Id,
            Tipo = TipoMovimentacao.Entrada,
            Quantidade = dados.Quantidade!.Value,
            Motivo = dados.Motivo,
            DataMovimentacao = DateTime.UtcNow
        };

        contexto.EstoqueMovimentacoes.Add(movimentacao);
        await contexto.SaveChangesAsync(cancelamento);

        return new MovimentacaoRespostaDto(
            movimentacao.Tipo, movimentacao.Quantidade, movimentacao.Motivo, movimentacao.PedidoId, movimentacao.DataMovimentacao);
    }
}
