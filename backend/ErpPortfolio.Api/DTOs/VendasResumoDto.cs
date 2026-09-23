// =====================================================================================
// Arquivo....: VendasResumoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de GET /api/dashboard/vendas: faturamento e ticket médio do
//              mês atual (só pedidos Confirmados), pedidos por status e faturamento
//              diário para o gráfico (SPEC.md, D1-D4).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção agregada de public.pedidos.
// Fontes.....: Montado por PedidoService.ObterResumoVendasAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record VendasResumoDto(
    decimal Faturamento,
    decimal TicketMedio,
    int QuantidadeConfirmados,
    PedidosPorStatusDto PorStatus,
    IReadOnlyList<FaturamentoDiaDto> FaturamentoPorDia);
