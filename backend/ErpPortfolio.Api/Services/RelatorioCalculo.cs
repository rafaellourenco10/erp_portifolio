// =====================================================================================
// Arquivo....: RelatorioCalculo.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Regras puras dos relatórios (sem banco): validação do período (SPEC.md, R1)
//              e resumo de uma lista de pedidos (quantidade, soma, ticket médio).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Usado por RelatorioFiltroDtos (validação) e RelatorioService (resumo).
//              Testado em ErpPortfolio.Tests/RelatorioCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class RelatorioCalculo
{
    public const int MaximoDiasPeriodo = 366;

    /// <summary>Mensagem de erro do período, ou null se for válido (fim ≥ início, no máximo 366 dias, datas inclusivas).</summary>
    public static string? ErroPeriodo(DateOnly inicio, DateOnly fim)
    {
        if (fim < inicio)
            return "A data final deve ser igual ou posterior à data inicial.";

        return fim.DayNumber - inicio.DayNumber + 1 > MaximoDiasPeriodo
            ? $"O período deve ter no máximo {MaximoDiasPeriodo} dias."
            : null;
    }

    /// <summary>Quantidade, soma e ticket médio dos valores listados (ticket 0 sem pedidos).</summary>
    public static (int Quantidade, decimal Total, decimal TicketMedio) ResumoPedidos(IEnumerable<decimal> valores)
    {
        var lista = valores as IReadOnlyCollection<decimal> ?? valores.ToList();
        var total = lista.Sum();
        return (lista.Count, total, DashboardCalculo.TicketMedio(total, lista.Count));
    }
}
