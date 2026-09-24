// =====================================================================================
// Arquivo....: FluxoCaixaCalculo.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Regra do fluxo de caixa, sem banco (SPEC.md etapa 15, FC2-FC7): recebe os
//              movimentos já no dia de Brasília e monta os períodos (dia ou mês, com os
//              vazios), realizado × previsto, saldo acumulado a partir do saldo anterior e
//              o menor saldo. Atrasadas (pendentes vencidas) contam no dia de hoje.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Chamado pelo FluxoCaixaService; testado em FluxoCaixaCalculoTests.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public enum AgrupamentoFluxoCaixa
{
    Dia,
    Mes
}

/// <summary>Uma parcela no caixa: entrada (a receber) ou saída (a pagar), realizada ou prevista, no dia em que conta.</summary>
public record MovimentoCaixa(DateOnly Dia, decimal Valor, bool Entrada, bool Realizado);

/// <summary>Período (dia ou 1º dia do mês) com as somas e o saldo ao fim dele.</summary>
public record PeriodoFluxoCaixa(
    DateOnly Inicio,
    decimal EntradasRealizadas,
    decimal SaidasRealizadas,
    decimal EntradasPrevistas,
    decimal SaidasPrevistas,
    decimal Saldo);

public record FluxoCaixaResultado(
    decimal SaldoInicial,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoFinal,
    decimal MenorSaldo,
    DateOnly DataMenorSaldo,
    IReadOnlyList<PeriodoFluxoCaixa> Periodos);

public static class FluxoCaixaCalculo
{
    public const int MaximoDiasDiario = 93;

    /// <summary>Erro do período (FC7), ou null: regra dos relatórios e, na visão diária, no máximo 93 dias.</summary>
    public static string? ErroPeriodo(DateOnly inicio, DateOnly fim, AgrupamentoFluxoCaixa agrupamento)
    {
        if (RelatorioCalculo.ErroPeriodo(inicio, fim) is string erro)
            return erro;

        return agrupamento == AgrupamentoFluxoCaixa.Dia && fim.DayNumber - inicio.DayNumber + 1 > MaximoDiasDiario
            ? $"Na visão diária o período deve ter no máximo {MaximoDiasDiario} dias; use a visão mensal."
            : null;
    }

    /// <summary>Dia em que uma parcela pendente conta (FC2): o vencimento, ou hoje se já venceu.</summary>
    public static DateOnly DiaPrevisto(DateOnly vencimento, DateOnly hoje) => vencimento < hoje ? hoje : vencimento;

    /// <summary>
    /// Monta o fluxo (FC3-FC6). <paramref name="saldoAnterior"/> é o realizado antes do início; movimentos antes do
    /// início (previstos, quando o período começa depois de hoje) também entram no saldo inicial; depois do fim são ignorados.
    /// </summary>
    public static FluxoCaixaResultado Montar(
        decimal saldoAnterior, IEnumerable<MovimentoCaixa> movimentos, DateOnly inicio, DateOnly fim, AgrupamentoFluxoCaixa agrupamento)
    {
        DateOnly Chave(DateOnly dia) => agrupamento == AgrupamentoFluxoCaixa.Dia ? dia : new DateOnly(dia.Year, dia.Month, 1);

        var lista = movimentos.ToList();
        var saldoInicial = saldoAnterior + lista.Where(m => m.Dia < inicio).Sum(m => m.Entrada ? m.Valor : -m.Valor);
        var porPeriodo = lista.Where(m => m.Dia >= inicio && m.Dia <= fim).ToLookup(m => Chave(m.Dia));

        var periodos = new List<PeriodoFluxoCaixa>();
        var saldo = saldoInicial;
        for (var chave = Chave(inicio); chave <= fim; chave = agrupamento == AgrupamentoFluxoCaixa.Dia ? chave.AddDays(1) : chave.AddMonths(1))
        {
            var doPeriodo = porPeriodo[chave].ToList();
            decimal Soma(bool entrada, bool realizado) => doPeriodo.Where(m => m.Entrada == entrada && m.Realizado == realizado).Sum(m => m.Valor);

            var (er, sr, ep, sp) = (Soma(true, true), Soma(false, true), Soma(true, false), Soma(false, false));
            saldo += er + ep - sr - sp;
            periodos.Add(new PeriodoFluxoCaixa(chave, er, sr, ep, sp, saldo));
        }

        // Menor saldo ao fim de um período; no empate, o primeiro (o aperto começa ali).
        var menor = periodos.MinBy(p => p.Saldo)!;
        return new FluxoCaixaResultado(
            saldoInicial,
            periodos.Sum(p => p.EntradasRealizadas + p.EntradasPrevistas),
            periodos.Sum(p => p.SaidasRealizadas + p.SaidasPrevistas),
            saldo,
            menor.Saldo,
            menor.Inicio,
            periodos);
    }
}
