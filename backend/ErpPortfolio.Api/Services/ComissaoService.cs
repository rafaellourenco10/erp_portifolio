// =====================================================================================
// Arquivo....: ComissaoService.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Consulta de comissões (filtro por vendedor, status e período da data do
//              recebimento, com totais do filtro calculados no servidor) e pagamento ao
//              vendedor em lote, idempotente (SPEC.md etapa 11, CM4/CM6).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.comissoes
//                - SELECT : listagem (JOIN vendedores, pedidos/clientes, parcelas_receber;
//                           ORDER BY data_geracao DESC, id DESC, LIMIT/OFFSET), contagem de
//                           parcelas irmãs (X de Y) e somas por status do filtro
//                - UPDATE : status Pendente -> Paga e data_pagamento
// Fontes.....: ErpPortfolioDbContext.Comissoes (EF Core / Npgsql), AsNoTracking na leitura.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class ComissaoService(ErpPortfolioDbContext contexto) : IComissaoService
{
    public async Task<ComissaoListaDto> ListarAsync(ComissaoFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Comissoes.AsNoTracking();

        if (filtro.VendedorId is int vendedorId)
            consulta = consulta.Where(c => c.VendedorId == vendedorId);

        // Datas inclusivas em UTC, mesma convenção dos relatórios e do Dashboard.
        if (filtro.DataInicio is DateOnly inicio)
        {
            var desde = inicio.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            consulta = consulta.Where(c => c.DataGeracao >= desde);
        }
        if (filtro.DataFim is DateOnly fim)
        {
            var ate = fim.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            consulta = consulta.Where(c => c.DataGeracao < ate);
        }

        // Totais antes do filtro de status: os cards mostram o quadro do período/vendedor inteiro.
        var somas = await consulta
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Total = g.Sum(c => c.Valor) })
            .ToListAsync(cancelamento);
        var pendente = somas.Where(s => s.Status == StatusComissao.Pendente).Sum(s => s.Total);
        var pago = somas.Where(s => s.Status == StatusComissao.Paga).Sum(s => s.Total);

        if (filtro.Status is StatusComissao status)
            consulta = consulta.Where(c => c.Status == status);

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderByDescending(c => c.DataGeracao)
            .ThenByDescending(c => c.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(c => new ComissaoRespostaDto(
                c.Id,
                c.VendedorId,
                c.Vendedor!.Nome,
                c.PedidoId,
                c.Pedido!.Cliente!.Nome,
                c.ParcelaReceber!.NumeroParcela,
                contexto.ParcelasReceber.Count(p => p.PedidoId == c.PedidoId),
                c.ValorBase,
                c.Percentual,
                c.Valor,
                c.DataGeracao,
                c.Status,
                c.DataPagamento))
            .ToListAsync(cancelamento);

        return new ComissaoListaDto(
            new ResultadoPaginadoDto<ComissaoRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens),
            new ComissaoTotaisDto(pendente + pago, pendente, pago));
    }

    public async Task PagarAsync(IReadOnlyCollection<int> ids, CancellationToken cancelamento)
    {
        var distintos = ids.Distinct().ToList();
        var comissoes = await contexto.Comissoes.Where(c => distintos.Contains(c.Id)).ToListAsync(cancelamento);

        // Tudo ou nada: se algum id não existe, nada é alterado.
        var inexistentes = distintos.Except(comissoes.Select(c => c.Id)).ToList();
        if (inexistentes.Count > 0)
            throw new DadoInvalidoException(nameof(ComissaoPagarDto.Ids), $"Comissão(ões) inexistente(s): {string.Join(", ", inexistentes.Select(id => $"#{id}"))}.");

        var agora = DateTime.UtcNow;
        // CM4: pagar de novo é sucesso; as já pagas mantêm a data original.
        foreach (var comissao in comissoes.Where(c => c.Status == StatusComissao.Pendente))
        {
            comissao.Status = StatusComissao.Paga;
            comissao.DataPagamento = agora;
        }

        await contexto.SaveChangesAsync(cancelamento);
    }
}
