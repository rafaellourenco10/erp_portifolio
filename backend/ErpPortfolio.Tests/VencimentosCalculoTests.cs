// =====================================================================================
// Arquivo....: VencimentosCalculoTests.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Testes dos quadros de vencimentos do Dashboard: ordem, dias (negativo =
//              atrasada), limite da lista e totais da janela inteira.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/VencimentosCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class VencimentosCalculoTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 23);

    [Fact]
    public void Janela_vai_ate_7_dias_depois_de_hoje()
    {
        Assert.Equal(new DateOnly(2026, 9, 30), VencimentosCalculo.FimDaJanela(Hoje));
    }

    [Fact]
    public void Mais_urgente_primeiro_com_dias_negativos_para_atrasadas()
    {
        var resumo = VencimentosCalculo.Montar(
        [
            (1, "A", "x", 10m, Hoje.AddDays(3)),
            (2, "B", "x", 20m, Hoje.AddDays(-2)),
            (3, "C", "x", 30m, Hoje),
        ], Hoje);

        Assert.Equal([2, 3, 1], resumo.Itens.Select(i => i.Id));
        Assert.Equal([-2, 0, 3], resumo.Itens.Select(i => i.Dias));
        Assert.Equal(1, resumo.QuantidadeAtrasadas);
    }

    [Fact]
    public void Lista_corta_no_limite_mas_quantidade_e_total_contam_tudo()
    {
        var parcelas = Enumerable.Range(1, VencimentosCalculo.LimiteItens + 5)
            .Select(i => (i, "T", "d", 1m, Hoje.AddDays(i % 7)));

        var resumo = VencimentosCalculo.Montar(parcelas, Hoje);

        Assert.Equal(VencimentosCalculo.LimiteItens, resumo.Itens.Count);
        Assert.Equal(VencimentosCalculo.LimiteItens + 5, resumo.Quantidade);
        Assert.Equal(VencimentosCalculo.LimiteItens + 5, resumo.Total);
    }

    [Fact]
    public void Sem_parcelas_tudo_zerado()
    {
        var resumo = VencimentosCalculo.Montar([], Hoje);
        Assert.Equal((0, 0m, 0), (resumo.Quantidade, resumo.Total, resumo.QuantidadeAtrasadas));
        Assert.Empty(resumo.Itens);
    }
}
