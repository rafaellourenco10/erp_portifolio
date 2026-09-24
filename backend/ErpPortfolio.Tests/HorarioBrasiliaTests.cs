// =====================================================================================
// Arquivo....: HorarioBrasiliaTests.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Testes do horário de Brasília (UTC-3, sem horário de verão desde 2019):
//              à noite ainda é o mesmo dia e o limite do dia cai às 03:00 UTC.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/HorarioBrasilia.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class HorarioBrasiliaTests
{
    [Fact]
    public void Noite_em_Brasilia_ainda_e_o_mesmo_dia()
    {
        // 25/09 01:30 UTC = 24/09 22:30 em Brasília.
        var local = HorarioBrasilia.ParaBrasilia(new DateTime(2026, 9, 25, 1, 30, 0, DateTimeKind.Utc));
        Assert.Equal(new DateTime(2026, 9, 24, 22, 30, 0), local);
    }

    [Fact]
    public void Inicio_do_dia_de_Brasilia_e_3h_UTC()
    {
        var inicio = HorarioBrasilia.InicioDoDiaUtc(new DateOnly(2026, 10, 1));
        Assert.Equal(new DateTime(2026, 10, 1, 3, 0, 0, DateTimeKind.Utc), inicio);
        Assert.Equal(DateTimeKind.Utc, inicio.Kind);
    }
}
