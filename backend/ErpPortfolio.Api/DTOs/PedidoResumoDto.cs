// =====================================================================================
// Arquivo....: PedidoResumoDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de saída de uma linha da listagem de pedidos (sem os itens): número,
//              cliente, data, status, total e quantidade de itens.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.pedidos (JOIN com public.clientes e contagem de
//              public.pedido_itens).
// Fontes.....: Montado direto na consulta do PedidoService.ListarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record PedidoResumoDto(
    int Id,
    int ClienteId,
    string ClienteNome,
    DateTime DataPedido,
    StatusPedido Status,
    decimal ValorTotal,
    int QuantidadeItens);
