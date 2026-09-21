// =====================================================================================
// Arquivo....: CalculoPedidoTests.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Testes do cálculo de subtotal do item e total do pedido. Os casos de
//              referência (1 a 6) são os mesmos da SPEC.md e do front (calculoPedido.ts).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/CalculoPedido.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class CalculoPedidoTests
{
    // quantidade, preço unitário, desconto do item (%), subtotal esperado
    public static TheoryData<decimal, decimal, decimal, decimal> CasosDeSubtotal => new()
    {
        { 2m, 350.00m, 10m, 630.00m },      // caso 1: mouse com 10%
        { 1m, 200.00m, 0m, 200.00m },       // caso 1: teclado sem desconto
        { 3m, 33.33m, 0m, 99.99m },         // caso 3
        { 1.5m, 10.00m, 0m, 15.00m },       // caso 4: produto em KG
        { 1m, 0.05m, 50m, 0.03m },          // caso 5: 0,025 arredonda para cima
        { 1m, 100.00m, 100m, 0.00m },       // caso 6: desconto de 100%
        { 3m, 0.10m, 33.33m, 0.20m },       // 0,30 × 0,6667 = 0,20001
        { 0.001m, 0.01m, 0m, 0.00m },       // menor quantidade possível
        { 999999.999m, 9999999999.99m, 0m, 9999999989990000.00m }, // maiores valores aceitos: o decimal não estoura
    };

    [Theory]
    [MemberData(nameof(CasosDeSubtotal))]
    public void Subtotal_calcula_e_arredonda_para_2_casas(decimal quantidade, decimal preco, decimal desconto, decimal esperado)
    {
        var subtotal = CalculoPedido.Subtotal(quantidade, preco, desconto);

        Assert.Equal(esperado, subtotal);
    }

    [Fact]
    public void Subtotal_arredonda_metade_para_cima_e_nao_para_o_par_mais_proximo()
    {
        // 0,025 → 0,03 (o arredondamento "bancário", para o par mais próximo, daria 0,02).
        Assert.Equal(0.03m, CalculoPedido.Subtotal(1m, 0.05m, 50m));
        // 0,045 → 0,05 (o "bancário" daria 0,04).
        Assert.Equal(0.05m, CalculoPedido.Subtotal(1m, 0.09m, 50m));
    }

    [Fact]
    public void Total_soma_os_subtotais_sem_desconto_no_pedido()
    {
        // caso 1: 630,00 + 200,00
        Assert.Equal(830.00m, CalculoPedido.Total([630.00m, 200.00m], 0m));
    }

    [Fact]
    public void Total_aplica_o_desconto_do_pedido_sobre_a_soma()
    {
        // caso 2: 830,00 com 5% = 788,50
        Assert.Equal(788.50m, CalculoPedido.Total([630.00m, 200.00m], 5m));
    }

    [Fact]
    public void Total_arredonda_depois_de_aplicar_o_desconto_do_pedido()
    {
        // 0,05 com 50% = 0,025 → 0,03
        Assert.Equal(0.03m, CalculoPedido.Total([0.05m], 50m));
    }

    [Fact]
    public void Total_com_desconto_de_100_por_cento_e_zero()
    {
        Assert.Equal(0.00m, CalculoPedido.Total([630.00m, 200.00m], 100m));
    }

    [Fact]
    public void Total_sem_itens_e_zero()
    {
        Assert.Equal(0.00m, CalculoPedido.Total([], 10m));
    }

    [Theory]
    [InlineData("UN", "2", true)]
    [InlineData("UN", "2.5", false)]
    [InlineData("CX", "10", true)]
    [InlineData("CX", "1.001", false)]
    [InlineData("KG", "2.5", true)]
    [InlineData("L", "0.001", true)]
    [InlineData("M", "1.234", true)]
    public void Quantidade_decimal_so_vale_para_unidades_que_nao_sao_UN_nem_CX(string unidade, string quantidade, bool esperado) =>
        Assert.Equal(esperado, CalculoPedido.QuantidadeValidaParaUnidade(unidade, decimal.Parse(quantidade, System.Globalization.CultureInfo.InvariantCulture)));

    [Fact]
    public void Limite_do_total_e_o_maior_valor_de_numeric_12_2() =>
        Assert.Equal(9_999_999_999.99m, CalculoPedido.LimiteTotal);

    [Fact]
    public void Pedido_completo_do_caso_1_e_do_caso_2_ponta_a_ponta()
    {
        var subtotais = new[]
        {
            CalculoPedido.Subtotal(2m, 350.00m, 10m),
            CalculoPedido.Subtotal(1m, 200.00m, 0m),
        };

        Assert.Equal(830.00m, CalculoPedido.Total(subtotais, 0m));
        Assert.Equal(788.50m, CalculoPedido.Total(subtotais, 5m));
    }
}
