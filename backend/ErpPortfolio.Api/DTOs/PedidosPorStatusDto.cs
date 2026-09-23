// =====================================================================================
// Arquivo....: PedidosPorStatusDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Quantidade de pedidos do mês atual em cada status, para o card
//              "Pedidos por status" do Dashboard.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Contagem agregada de public.pedidos, agrupada por status.
// Fontes.....: Montado por PedidoService.ObterResumoVendasAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record PedidosPorStatusDto(int Rascunho, int Confirmado, int Cancelado);
