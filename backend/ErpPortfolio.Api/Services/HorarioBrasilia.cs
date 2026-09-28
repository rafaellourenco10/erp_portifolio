// =====================================================================================
// Arquivo....: HorarioBrasilia.cs
// Versão.....: 1.1.0
// Data.......: 24/09/2026
// Descrição..: O "hoje" e os limites de dia/mês do ERP em horário de Brasília. As datas
//              continuam gravadas em UTC; só a conta de "que dia é" usa o fuso, para que
//              às 21h não seja amanhã (vencido/atrasado, validade, filtros por período).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: TimeZoneInfo "America/Sao_Paulo" (IANA; funciona no Windows e no Linux).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo (ParaBrasilia veio do FormatoRelatorioTexto).
//   1.1.0 - 28/09/2026 - ComFuso (dhEmi da NF-e, etapa 16).
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class HorarioBrasilia
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime ParaBrasilia(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Fuso);

    /// <summary>Data/hora de Brasília com o deslocamento (ex.: 2026-09-28T10:00:00-03:00), como pede a NF-e.</summary>
    public static DateTimeOffset ComFuso(DateTime utc)
    {
        var local = ParaBrasilia(utc);
        return new DateTimeOffset(local, Fuso.GetUtcOffset(local));
    }

    public static DateOnly Hoje() => DateOnly.FromDateTime(ParaBrasilia(DateTime.UtcNow));

    /// <summary>Meia-noite do dia em Brasília, em UTC: limite para comparar com as datas gravadas.</summary>
    public static DateTime InicioDoDiaUtc(DateOnly dia) =>
        TimeZoneInfo.ConvertTimeToUtc(dia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), Fuso);
}
