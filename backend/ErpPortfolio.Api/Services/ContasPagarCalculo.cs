// =====================================================================================
// Arquivo....: ContasPagarCalculo.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Parcelas de uma conta avulsa (sem banco): valor dividido igualmente com o
//              resto na última (mesma divisão das outras parcelas) e vencimento da parcela
//              i = 1º vencimento + (i-1) × intervalo (SPEC.md etapa 12, AV2).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Usado por ContasPagarService.CriarAvulsaAsync. Testado em
//              ContasPagarCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class ContasPagarCalculo
{
    public static IReadOnlyList<(decimal Valor, DateOnly Vencimento)> ParcelasAvulsa(
        decimal valorTotal, int numeroParcelas, DateOnly primeiroVencimento, int intervaloDias)
    {
        var valores = ContasReceberCalculo.Dividir(valorTotal, numeroParcelas);
        return valores.Select((valor, i) => (valor, primeiroVencimento.AddDays(i * intervaloDias))).ToList();
    }
}
