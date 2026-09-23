// =====================================================================================
// Arquivo....: ComissaoCalculo.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Cálculo puro da comissão de uma parcela (sem banco): valor da parcela ×
//              % / 100, arredondado a 2 casas com meio para cima (SPEC.md, CM1). A
//              migration CriacaoTabelaComissoes usa a mesma regra em SQL (ROUND).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Usado por ContasReceberService. Testado em ComissaoCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class ComissaoCalculo
{
    public static decimal Valor(decimal valorParcela, decimal percentual) =>
        Math.Round(valorParcela * percentual / 100m, 2, MidpointRounding.AwayFromZero);
}
