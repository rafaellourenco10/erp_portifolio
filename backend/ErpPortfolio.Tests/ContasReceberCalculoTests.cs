// =====================================================================================
// Arquivo....: ContasReceberCalculoTests.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Testes da divisão do valor de um pedido em N parcelas (resto na última).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/ContasReceberCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class ContasReceberCalculoTests
{
    [Fact]
    public void Uma_parcela_leva_o_valor_inteiro()
    {
        Assert.Equal([100.00m], ContasReceberCalculo.Dividir(100.00m, 1));
    }

    [Fact]
    public void Tres_parcelas_de_100_deixam_o_resto_na_ultima()
    {
        var parcelas = ContasReceberCalculo.Dividir(100.00m, 3);

        Assert.Equal([33.33m, 33.33m, 33.34m], parcelas);
        Assert.Equal(100.00m, parcelas.Sum());
    }

    [Fact]
    public void Divisao_exata_fica_igual_em_todas_as_parcelas()
    {
        var parcelas = ContasReceberCalculo.Dividir(300.00m, 3);

        Assert.Equal([100.00m, 100.00m, 100.00m], parcelas);
    }

    [Theory]
    [InlineData(788.50, 2)]
    [InlineData(9999999999.99, 12)]
    [InlineData(0.03, 2)]
    [InlineData(1234.56, 7)]
    public void Soma_das_parcelas_sempre_bate_com_o_total(decimal valorTotal, int numeroParcelas)
    {
        var parcelas = ContasReceberCalculo.Dividir(valorTotal, numeroParcelas);

        Assert.Equal(numeroParcelas, parcelas.Count);
        Assert.Equal(valorTotal, parcelas.Sum());
    }

    [Fact]
    public void Doze_parcelas_soma_bate_com_o_total()
    {
        var parcelas = ContasReceberCalculo.Dividir(1000.00m, 12);

        Assert.Equal(12, parcelas.Count);
        Assert.Equal(1000.00m, parcelas.Sum());
        // 1000 / 12 = 83,33...; as 11 primeiras levam 83,33 e a última leva o resto (83,37).
        Assert.Equal(83.33m, parcelas[0]);
        Assert.Equal(83.37m, parcelas[11]);
    }
}
