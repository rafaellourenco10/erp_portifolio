// =====================================================================================
// Arquivo....: ComissaoCalculoTests.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Testes do cálculo da comissão de uma parcela (SPEC.md, CM1).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/ComissaoCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class ComissaoCalculoTests
{
    [Theory]
    [InlineData("700", "5", "35")]
    [InlineData("333.33", "5.5", "18.33")] // 18,33315
    [InlineData("100", "0.5", "0.5")]
    [InlineData("10.10", "5", "0.51")] // 0,505 -> meio para cima
    [InlineData("100", "0", "0")]
    public void Valor_e_parcela_vezes_percentual_com_2_casas(string parcela, string percentual, string esperado)
    {
        Assert.Equal(D(esperado), ComissaoCalculo.Valor(D(parcela), D(percentual)));
    }

    private static decimal D(string valor) => decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture);
}
