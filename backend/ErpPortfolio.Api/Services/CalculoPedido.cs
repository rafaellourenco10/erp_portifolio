// =====================================================================================
// Arquivo....: CalculoPedido.cs
// Versão.....: 1.1.0
// Data.......: 21/09/2026
// Descrição..: Regra de cálculo de dinheiro do pedido, em funções puras (sem banco):
//                subtotal do item = arredonda2(quantidade × preço × (1 − desconto/100))
//                total do pedido  = arredonda2(soma dos subtotais × (1 − desconto/100))
//              Arredondamento em 2 casas, metade para cima (MidpointRounding.AwayFromZero).
//              Os mesmos casos de referência valem no front (utils/calculoPedido.ts).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica (o resultado é gravado em pedidos.valor_total pelo serviço).
// Fontes.....: SPEC.md (seção "Cálculo"). Testes: ErpPortfolio.Tests/CalculoPedidoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 21/09/2026 - Limite do total (R10) e regra de quantidade inteira para UN/CX (R8).
// =====================================================================================

namespace ErpPortfolio.Api.Services;

/// <remarks>As entradas já devem estar validadas (quantidade &gt; 0, descontos de 0 a 100).</remarks>
public static class CalculoPedido
{
    public static decimal Subtotal(decimal quantidade, decimal precoUnitario, decimal descontoPercentual) =>
        Arredondar(quantidade * precoUnitario * (1 - descontoPercentual / 100));

    public static decimal Total(IEnumerable<decimal> subtotais, decimal descontoPedidoPercentual) =>
        Arredondar(subtotais.Sum() * (1 - descontoPedidoPercentual / 100));

    /// <summary>Maior total que cabe em pedidos.valor_total (numeric(12,2)); acima disso a API recusa (R10).</summary>
    public const decimal LimiteTotal = 9_999_999_999.99m;

    /// <summary>Produtos em UN e CX só vendem quantidade inteira; KG, L e M aceitam decimais (R8).</summary>
    public static bool QuantidadeValidaParaUnidade(string unidade, decimal quantidade) =>
        unidade is not ("UN" or "CX") || quantidade == decimal.Truncate(quantidade);

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
