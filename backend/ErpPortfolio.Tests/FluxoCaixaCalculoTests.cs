// =====================================================================================
// Arquivo....: FluxoCaixaCalculoTests.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Testes do fluxo de caixa (SPEC.md etapa 15): períodos com vazios, dia e
//              mês, realizado × previsto, atrasadas em hoje, saldo inicial (inclusive
//              período futuro), saldo acumulado, menor saldo e validação do período.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/FluxoCaixaCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class FluxoCaixaCalculoTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 24);

    private static MovimentoCaixa Recebido(int dia, decimal valor) => new(new DateOnly(2026, 9, dia), valor, true, true);
    private static MovimentoCaixa Pago(int dia, decimal valor) => new(new DateOnly(2026, 9, dia), valor, false, true);

    [Fact]
    public void Diario_tem_um_periodo_por_dia_mesmo_sem_movimento()
    {
        var r = FluxoCaixaCalculo.Montar(0m, [Recebido(2, 100m)], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5), AgrupamentoFluxoCaixa.Dia);

        Assert.Equal(5, r.Periodos.Count);
        Assert.Equal(new DateOnly(2026, 9, 1), r.Periodos[0].Inicio);
        Assert.Equal(0m, r.Periodos[0].EntradasRealizadas);
        Assert.Equal(100m, r.Periodos[1].EntradasRealizadas);
        Assert.All(r.Periodos.Skip(1), p => Assert.Equal(100m, p.Saldo));
    }

    [Fact]
    public void Saldo_acumula_a_partir_do_saldo_anterior()
    {
        var r = FluxoCaixaCalculo.Montar(50m, [Recebido(1, 100m), Pago(2, 30m), Pago(3, 200m)],
            new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), AgrupamentoFluxoCaixa.Dia);

        Assert.Equal(50m, r.SaldoInicial);
        Assert.Equal([150m, 120m, -80m], r.Periodos.Select(p => p.Saldo));
        Assert.Equal(100m, r.TotalEntradas);
        Assert.Equal(230m, r.TotalSaidas);
        Assert.Equal(-80m, r.SaldoFinal);
        Assert.Equal(r.SaldoInicial + r.TotalEntradas - r.TotalSaidas, r.SaldoFinal);
    }

    [Fact]
    public void Menor_saldo_e_o_primeiro_dia_do_aperto()
    {
        var r = FluxoCaixaCalculo.Montar(100m, [Pago(2, 150m), Recebido(3, 10m), Pago(4, 10m)],
            new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5), AgrupamentoFluxoCaixa.Dia);

        Assert.Equal(-50m, r.MenorSaldo);
        Assert.Equal(new DateOnly(2026, 9, 2), r.DataMenorSaldo);
    }

    [Fact]
    public void Realizado_e_previsto_ficam_separados_no_mesmo_dia()
    {
        var r = FluxoCaixaCalculo.Montar(0m,
            [Recebido(24, 10m), new(Hoje, 20m, true, false), Pago(24, 3m), new(Hoje, 4m, false, false)],
            Hoje, Hoje, AgrupamentoFluxoCaixa.Dia);

        var dia = Assert.Single(r.Periodos);
        Assert.Equal((10m, 20m, 3m, 4m, 23m), (dia.EntradasRealizadas, dia.EntradasPrevistas, dia.SaidasRealizadas, dia.SaidasPrevistas, dia.Saldo));
    }

    [Theory]
    [InlineData(2026, 9, 10, 2026, 9, 24)] // atrasada: conta hoje
    [InlineData(2026, 9, 24, 2026, 9, 24)] // vence hoje
    [InlineData(2026, 10, 5, 2026, 10, 5)] // futura: no vencimento
    public void Pendente_conta_no_vencimento_ou_hoje_se_atrasada(int ano, int mes, int dia, int anoEsperado, int mesEsperado, int diaEsperado) =>
        Assert.Equal(new DateOnly(anoEsperado, mesEsperado, diaEsperado), FluxoCaixaCalculo.DiaPrevisto(new DateOnly(ano, mes, dia), Hoje));

    [Fact]
    public void Periodo_futuro_leva_o_previsto_anterior_para_o_saldo_inicial()
    {
        // Período de 01 a 05/10; previstos em 30/09 e 03/10; fora do fim é ignorado.
        var movimentos = new MovimentoCaixa[]
        {
            new(new DateOnly(2026, 9, 30), 40m, true, false),
            new(new DateOnly(2026, 9, 30), 15m, false, false),
            new(new DateOnly(2026, 10, 3), 5m, true, false),
            new(new DateOnly(2026, 10, 9), 999m, true, false),
        };

        var r = FluxoCaixaCalculo.Montar(100m, movimentos, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), AgrupamentoFluxoCaixa.Dia);

        Assert.Equal(125m, r.SaldoInicial);
        Assert.Equal(5m, r.TotalEntradas);
        Assert.Equal(130m, r.SaldoFinal);
    }

    [Fact]
    public void Mensal_soma_igual_ao_diario_e_tem_meses_vazios()
    {
        var movimentos = new MovimentoCaixa[]
        {
            new(new DateOnly(2026, 8, 20), 10m, true, true),
            new(new DateOnly(2026, 8, 31), 7m, false, true),
            new(new DateOnly(2026, 10, 1), 30m, true, false),
        };
        var (inicio, fim) = (new DateOnly(2026, 8, 15), new DateOnly(2026, 10, 10));

        var mensal = FluxoCaixaCalculo.Montar(0m, movimentos, inicio, fim, AgrupamentoFluxoCaixa.Mes);
        var diario = FluxoCaixaCalculo.Montar(0m, movimentos, inicio, fim, AgrupamentoFluxoCaixa.Dia);

        Assert.Equal([new DateOnly(2026, 8, 1), new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1)], mensal.Periodos.Select(p => p.Inicio));
        Assert.Equal([3m, 3m, 33m], mensal.Periodos.Select(p => p.Saldo));
        Assert.Equal((diario.TotalEntradas, diario.TotalSaidas, diario.SaldoFinal), (mensal.TotalEntradas, mensal.TotalSaidas, mensal.SaldoFinal));
    }

    [Fact]
    public void Mes_parcial_nas_pontas_so_considera_dias_do_periodo()
    {
        var movimentos = new MovimentoCaixa[] { Recebido(1, 100m), Recebido(20, 5m) };

        var r = FluxoCaixaCalculo.Montar(0m, movimentos, new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 30), AgrupamentoFluxoCaixa.Mes);

        Assert.Equal(100m, r.SaldoInicial); // o dia 1 é antes do início: vai para o saldo inicial
        Assert.Equal(5m, Assert.Single(r.Periodos).EntradasRealizadas);
    }

    [Theory]
    [InlineData(2026, 9, 1, 2026, 12, 2, AgrupamentoFluxoCaixa.Dia, true)]   // 93 dias
    [InlineData(2026, 9, 1, 2026, 12, 3, AgrupamentoFluxoCaixa.Dia, false)]  // 94 dias
    [InlineData(2026, 9, 1, 2026, 12, 3, AgrupamentoFluxoCaixa.Mes, true)]
    [InlineData(2026, 1, 1, 2027, 1, 2, AgrupamentoFluxoCaixa.Mes, false)]   // 367 dias
    [InlineData(2026, 9, 2, 2026, 9, 1, AgrupamentoFluxoCaixa.Mes, false)]   // fim antes do início
    public void Valida_o_periodo(int a1, int m1, int d1, int a2, int m2, int d2, AgrupamentoFluxoCaixa agrupamento, bool valido) =>
        Assert.Equal(valido, FluxoCaixaCalculo.ErroPeriodo(new DateOnly(a1, m1, d1), new DateOnly(a2, m2, d2), agrupamento) is null);
}
