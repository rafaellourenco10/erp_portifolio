// =====================================================================================
// Arquivo....: EstoqueCalculo.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Cálculo do saldo de estoque, em função pura (sem banco):
//                saldo = Σ quantidade das Entradas − Σ quantidade das Saídas
//              O saldo nunca é gravado; é sempre recalculado a partir das movimentações.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica (opera sobre movimentações já lidas de estoque_movimentacoes).
// Fontes.....: SPEC.md (seção "Modelo de dados", regra E1). Testes: EstoqueCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public static class EstoqueCalculo
{
    public static decimal Saldo(IEnumerable<(TipoMovimentacao Tipo, decimal Quantidade)> movimentacoes) =>
        movimentacoes.Sum(m => m.Tipo == TipoMovimentacao.Entrada ? m.Quantidade : -m.Quantidade);
}
