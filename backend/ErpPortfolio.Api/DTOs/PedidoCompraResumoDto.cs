// =====================================================================================
// Arquivo....: PedidoCompraResumoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de uma linha da listagem de pedidos de compra (sem os itens):
//              número, fornecedor, data, status, total e quantidade de itens. Espelho de
//              PedidoResumoDto.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.pedidos_compra (JOIN com public.fornecedores e
//              contagem de public.pedido_compra_itens).
// Fontes.....: Montado direto na consulta do PedidoCompraService.ListarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record PedidoCompraResumoDto(
    int Id,
    int FornecedorId,
    string FornecedorNome,
    DateTime DataPedido,
    StatusPedido Status,
    decimal ValorTotal,
    int QuantidadeItens);
