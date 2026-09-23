// =====================================================================================
// Arquivo....: DashboardResumoTests.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Testes dos cálculos puros do Dashboard: ticket médio (sem dividir por
//              zero) e preenchimento de dias sem venda com 0.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/DashboardCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class DashboardResumoTests
{
    [Fact]
    public void TicketMedio_sem_pedidos_confirmados_e_zero_sem_dividir_por_zero()
    {
        Assert.Equal(0m, DashboardCalculo.TicketMedio(0m, 0));
    }

    [Fact]
    public void TicketMedio_divide_o_faturamento_pela_quantidade()
    {
        Assert.Equal(250m, DashboardCalculo.TicketMedio(1000m, 4));
    }

    [Fact]
    public void PreencherDias_preenche_todos_os_dias_do_periodo_mesmo_sem_valor()
    {
        var primeiroDia = new DateOnly(2026, 9, 1);
        var ultimoDia = new DateOnly(2026, 9, 5);
        (DateOnly, decimal)[] valores = [(new DateOnly(2026, 9, 3), 100m)];

        var dias = DashboardCalculo.PreencherDias(valores, primeiroDia, ultimoDia);

        Assert.Equal(5, dias.Count);
        Assert.Equal(0m, dias[0].Valor); // dia 1, sem venda
        Assert.Equal(0m, dias[1].Valor); // dia 2, sem venda
        Assert.Equal(100m, dias[2].Valor); // dia 3, com venda
        Assert.Equal(0m, dias[3].Valor); // dia 4, sem venda
        Assert.Equal(0m, dias[4].Valor); // dia 5, sem venda
    }

    [Fact]
    public void PreencherDias_com_periodo_de_um_dia_so()
    {
        var dia = new DateOnly(2026, 9, 10);
        (DateOnly, decimal)[] valores = [(dia, 50m)];

        var dias = DashboardCalculo.PreencherDias(valores, dia, dia);

        Assert.Equal(50m, dias.Single().Valor);
    }
}
