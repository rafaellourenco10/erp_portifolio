// =====================================================================================
// Arquivo....: FluxoCaixaService.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Fluxo de caixa (SPEC.md etapa 15): busca as parcelas recebidas/pagas no
//              período (realizado, no dia de Brasília), as pendentes até o fim (previsto,
//              no vencimento ou hoje se atrasadas) e o realizado antes do início (saldo
//              anterior, somado no banco); a regra fica no FluxoCaixaCalculo.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio"), só leitura.
// Tabelas....: public.parcelas_receber, public.parcelas_pagar
//                - SELECT : SUM do realizado antes do início; recebidas/pagas no período;
//                           pendentes com vencimento até o fim. Canceladas nunca entram.
// Fontes.....: ErpPortfolioDbContext (EF Core / Npgsql), AsNoTracking.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class FluxoCaixaService(ErpPortfolioDbContext contexto) : IFluxoCaixaService
{
    public async Task<FluxoCaixaDto> ObterAsync(FluxoCaixaFiltroDto filtro, CancellationToken cancelamento)
    {
        var (inicio, fim) = (filtro.DataInicio!.Value, filtro.DataFim!.Value);
        var hoje = HorarioBrasilia.Hoje();
        var desde = HorarioBrasilia.InicioDoDiaUtc(inicio);
        var ate = HorarioBrasilia.InicioDoDiaUtc(fim.AddDays(1));

        var receber = contexto.ParcelasReceber.AsNoTracking();
        var pagar = contexto.ParcelasPagar.AsNoTracking();

        // FC3: realizado antes do início, somado no banco (não carrega as linhas).
        var recebidoAntes = await receber.Where(p => p.Status == StatusParcela.Recebido && p.DataRecebimento < desde).SumAsync(p => p.Valor, cancelamento);
        var pagoAntes = await pagar.Where(p => p.Status == StatusParcelaPagar.Pago && p.DataPagamento < desde).SumAsync(p => p.Valor, cancelamento);

        // FC1: realizado no período.
        var recebidos = await receber
            .Where(p => p.Status == StatusParcela.Recebido && p.DataRecebimento >= desde && p.DataRecebimento < ate)
            .Select(p => new { Data = p.DataRecebimento!.Value, p.Valor })
            .ToListAsync(cancelamento);
        var pagos = await pagar
            .Where(p => p.Status == StatusParcelaPagar.Pago && p.DataPagamento >= desde && p.DataPagamento < ate)
            .Select(p => new { Data = p.DataPagamento!.Value, p.Valor })
            .ToListAsync(cancelamento);

        // FC2: pendentes até o fim; as de antes do início (período futuro) vão para o saldo inicial no cálculo.
        var aReceber = await receber
            .Where(p => p.Status == StatusParcela.Pendente && p.Vencimento <= fim)
            .Select(p => new { p.Vencimento, p.Valor })
            .ToListAsync(cancelamento);
        var aPagar = await pagar
            .Where(p => p.Status == StatusParcelaPagar.Pendente && p.Vencimento <= fim)
            .Select(p => new { p.Vencimento, p.Valor })
            .ToListAsync(cancelamento);

        DateOnly DiaBrasilia(DateTime utc) => DateOnly.FromDateTime(HorarioBrasilia.ParaBrasilia(utc));
        var movimentos = recebidos.Select(r => new MovimentoCaixa(DiaBrasilia(r.Data), r.Valor, true, true))
            .Concat(pagos.Select(p => new MovimentoCaixa(DiaBrasilia(p.Data), p.Valor, false, true)))
            .Concat(aReceber.Select(p => new MovimentoCaixa(FluxoCaixaCalculo.DiaPrevisto(p.Vencimento, hoje), p.Valor, true, false)))
            .Concat(aPagar.Select(p => new MovimentoCaixa(FluxoCaixaCalculo.DiaPrevisto(p.Vencimento, hoje), p.Valor, false, false)));

        var fluxo = FluxoCaixaCalculo.Montar(recebidoAntes - pagoAntes, movimentos, inicio, fim, filtro.Agrupamento);

        // FC6: atrasados só fazem sentido quando hoje está no período (é onde eles caem).
        var hojeNoPeriodo = inicio <= hoje && hoje <= fim;
        return new FluxoCaixaDto(
            fluxo.SaldoInicial, fluxo.TotalEntradas, fluxo.TotalSaidas, fluxo.SaldoFinal, fluxo.MenorSaldo, fluxo.DataMenorSaldo,
            hojeNoPeriodo ? aReceber.Where(p => p.Vencimento < hoje).Sum(p => p.Valor) : 0m,
            hojeNoPeriodo ? aPagar.Where(p => p.Vencimento < hoje).Sum(p => p.Valor) : 0m,
            hoje,
            fluxo.Periodos);
    }

    public async Task<RelatorioModelo> ModeloAsync(FluxoCaixaFiltroDto filtro, CancellationToken cancelamento)
    {
        var fluxo = await ObterAsync(filtro, cancelamento);
        var (inicio, fim) = (filtro.DataInicio!.Value, filtro.DataFim!.Value);
        var mensal = filtro.Agrupamento == AgrupamentoFluxoCaixa.Mes;

        var resumo = new List<CampoRelatorio>
        {
            new("Saldo inicial", fluxo.SaldoInicial, TipoValor.Moeda),
            new("Entradas", fluxo.TotalEntradas, TipoValor.Moeda),
            new("Saídas", fluxo.TotalSaidas, TipoValor.Moeda),
            new("Saldo final", fluxo.SaldoFinal, TipoValor.Moeda),
            new($"Menor saldo ({(mensal ? fluxo.DataMenorSaldo.ToString("MM/yyyy") : fluxo.DataMenorSaldo.ToString("dd/MM/yyyy"))})", fluxo.MenorSaldo, TipoValor.Moeda),
        };

        return new RelatorioModelo(
            "Fluxo de Caixa",
            $"fluxo-caixa-{inicio:yyyy-MM-dd}_{fim:yyyy-MM-dd}",
            HorarioBrasilia.ParaBrasilia(DateTime.UtcNow),
            [
                $"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}",
                $"Visão: {(mensal ? "Mensal" : "Diária")}",
                $"Previsto a partir de {fluxo.Hoje:dd/MM/yyyy} (atrasadas contam em hoje)",
            ],
            resumo,
            [
                new ColunaRelatorio(mensal ? "Mês" : "Dia", TipoValor.Texto),
                new ColunaRelatorio("Entradas realizadas", TipoValor.Moeda),
                new ColunaRelatorio("Entradas previstas", TipoValor.Moeda),
                new ColunaRelatorio("Saídas realizadas", TipoValor.Moeda),
                new ColunaRelatorio("Saídas previstas", TipoValor.Moeda),
                new ColunaRelatorio("Resultado", TipoValor.Moeda),
                new ColunaRelatorio("Saldo", TipoValor.Moeda),
            ],
            fluxo.Periodos
                .Select(p => new object?[]
                {
                    mensal ? p.Inicio.ToString("MM/yyyy") : p.Inicio.ToString("dd/MM/yyyy"),
                    p.EntradasRealizadas,
                    p.EntradasPrevistas,
                    p.SaidasRealizadas,
                    p.SaidasPrevistas,
                    p.EntradasRealizadas + p.EntradasPrevistas - p.SaidasRealizadas - p.SaidasPrevistas,
                    p.Saldo,
                })
                .ToList());
    }
}
