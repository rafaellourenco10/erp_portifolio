// =====================================================================================
// Arquivo....: FaturamentoDiaDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Um ponto do gráfico de faturamento diário do Dashboard: dia do mês e o
//              faturamento confirmado nesse dia (0 se não houve venda, sem buraco no gráfico).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção agregada de public.pedidos, agrupada por dia.
// Fontes.....: Montado por PedidoService.ObterResumoVendasAsync (Services/DashboardCalculo.cs).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record FaturamentoDiaDto(DateOnly Dia, decimal Valor);
