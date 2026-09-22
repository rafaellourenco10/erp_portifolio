// =====================================================================================
// Arquivo....: ContasReceberCalculo.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Divisão do valor de um pedido em N parcelas iguais, em função pura (sem
//              banco): as primeiras N-1 parcelas levam valorTotal/N arredondado para
//              baixo em 2 casas; a última leva o resto, garantindo que a soma bate
//              exatamente com valorTotal.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica (o resultado é gravado em parcelas_receber pelo serviço).
// Fontes.....: SPEC.md (regras C1 e C2). Testes: ContasReceberCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class ContasReceberCalculo
{
    /// <summary>Divide <paramref name="valorTotal"/> em <paramref name="numeroParcelas"/> valores cuja soma bate exatamente com o total.</summary>
    public static IReadOnlyList<decimal> Dividir(decimal valorTotal, int numeroParcelas)
    {
        var valorParcela = Math.Floor(valorTotal / numeroParcelas * 100) / 100;
        var parcelas = new decimal[numeroParcelas];

        for (var i = 0; i < numeroParcelas - 1; i++)
            parcelas[i] = valorParcela;

        parcelas[numeroParcelas - 1] = valorTotal - valorParcela * (numeroParcelas - 1);

        return parcelas;
    }
}
