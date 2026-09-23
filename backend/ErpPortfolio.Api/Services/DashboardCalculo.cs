// =====================================================================================
// Arquivo....: DashboardCalculo.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Cálculos puros do Dashboard (sem banco): ticket médio (sem dividir por
//              zero) e preenchimento de todos os dias de um período com 0 onde não
//              houve valor (evita buraco no gráfico de faturamento diário).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: SPEC.md (regras D2 e D4). Testes: DashboardResumoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class DashboardCalculo
{
    public static decimal TicketMedio(decimal faturamento, int quantidadeConfirmados) =>
        quantidadeConfirmados == 0 ? 0m : faturamento / quantidadeConfirmados;

    /// <summary>
    /// Devolve um valor para cada dia de [primeiroDia, ultimoDia], 0 onde <paramref name="valoresPorDia"/> não
    /// tiver o dia. <paramref name="valoresPorDia"/> não pode ter o mesmo dia repetido (agrupe antes de chamar).
    /// </summary>
    public static IReadOnlyList<(DateOnly Dia, decimal Valor)> PreencherDias(
        IEnumerable<(DateOnly Dia, decimal Valor)> valoresPorDia, DateOnly primeiroDia, DateOnly ultimoDia)
    {
        var porDia = valoresPorDia.ToDictionary(v => v.Dia, v => v.Valor);
        var dias = new List<(DateOnly, decimal)>();

        for (var dia = primeiroDia; dia <= ultimoDia; dia = dia.AddDays(1))
            dias.Add((dia, porDia.GetValueOrDefault(dia)));

        return dias;
    }
}
