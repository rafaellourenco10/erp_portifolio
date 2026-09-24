// =====================================================================================
// Arquivo....: DevolucaoCalculo.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Contas puras da devolução de venda (SPEC.md etapa 14), sem banco: valor de
//              cada item devolvido com os descontos do pedido (DV3), valor total (com "o que
//              falta" na devolução que zera o pedido), abatimento nas parcelas pendentes da
//              última para a primeira e o reembolso do que sobrar (DV4/DV5) e o estorno de
//              comissão sobre o reembolso (DV6). Arredondamento a 2 casas, meio para cima,
//              igual ao CalculoPedido e ao ComissaoCalculo.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Usado pelo DevolucaoService. Testado em DevolucaoCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class DevolucaoCalculo
{
    /// <summary>Parcela pendente candidata ao abatimento.</summary>
    public record ParcelaPendente(int Id, int NumeroParcela, decimal Valor);

    /// <summary>Novo valor de uma parcela; zero = a parcela deve ser cancelada (mantendo o valor original).</summary>
    public record AjusteParcela(int Id, decimal NovoValor)
    {
        public bool Cancelar => NovoValor == 0;
    }

    public record ResultadoAbatimento(IReadOnlyList<AjusteParcela> Ajustes, decimal Abatido, decimal Reembolso);

    /// <summary>DV3: quantidade × preço × (1 − desc. item) × (1 − desc. pedido), a 2 casas.</summary>
    public static decimal ValorItem(decimal quantidade, decimal precoUnitario, decimal descontoItem, decimal descontoPedido) =>
        Arredondar(quantidade * precoUnitario * (1 - descontoItem / 100) * (1 - descontoPedido / 100));

    /// <summary>
    /// DV3: soma dos itens; na devolução que zera o pedido, é exatamente o que falta devolver (os centavos
    /// fecham com o total do pedido). Nunca passa do que falta, mesmo com arredondamentos acumulados.
    /// </summary>
    public static decimal ValorTotal(IEnumerable<decimal> valoresItens, bool devolveTudo, decimal valorTotalPedido, decimal jaDevolvido)
    {
        var restante = valorTotalPedido - jaDevolvido;
        return devolveTudo ? restante : Math.Min(valoresItens.Sum(), restante);
    }

    /// <summary>DV4/DV5: desconta das pendentes, da de maior número para a menor; o que sobrar é reembolso.</summary>
    public static ResultadoAbatimento Abater(IEnumerable<ParcelaPendente> pendentes, decimal valorDevolucao)
    {
        var falta = valorDevolucao;
        var ajustes = new List<AjusteParcela>();

        foreach (var parcela in pendentes.OrderByDescending(p => p.NumeroParcela))
        {
            if (falta <= 0)
                break;

            var desconto = Math.Min(parcela.Valor, falta);
            ajustes.Add(new AjusteParcela(parcela.Id, parcela.Valor - desconto));
            falta -= desconto;
        }

        return new ResultadoAbatimento(ajustes, valorDevolucao - falta, falta);
    }

    /// <summary>DV6: −reembolso × %, a 2 casas; zero se não houve reembolso ou o pedido não tem comissão.</summary>
    public static decimal Estorno(decimal reembolso, decimal? percentualComissao) =>
        reembolso > 0 && percentualComissao is > 0 ? -ComissaoCalculo.Valor(reembolso, percentualComissao.Value) : 0m;

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
