// =====================================================================================
// Arquivo....: RelatorioRespostaDtos.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Saída dos relatórios (formato JSON): linhas + resumo. Os mesmos objetos
//              alimentam a exportação .xlsx/.pdf (SPEC.md, R3). Vendas e Compras usam o
//              mesmo formato (um pedido por linha); "Nome" é o cliente ou o fornecedor.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeções de public.pedidos / public.pedidos_compra (com o nome do
//              cliente/fornecedor e a contagem de itens) e de public.produtos (com saldo
//              calculado de public.estoque_movimentacoes).
// Fontes.....: Montados pelo RelatorioService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record RelatorioPedidoLinhaDto(
    int Id,
    DateTime DataPedido,
    string Nome,
    int QuantidadeItens,
    decimal ValorTotal,
    StatusPedido Status);

public record RelatorioPedidosDto(
    IReadOnlyList<RelatorioPedidoLinhaDto> Linhas,
    int QuantidadePedidos,
    decimal ValorTotal,
    decimal TicketMedio);

public record RelatorioEstoqueLinhaDto(
    int ProdutoId,
    string Nome,
    string Sku,
    string? CategoriaNome,
    string Unidade,
    decimal Saldo,
    decimal EstoqueMinimo,
    decimal Custo,
    decimal ValorEstoque,
    bool AbaixoMinimo);

public record RelatorioEstoqueDto(
    IReadOnlyList<RelatorioEstoqueLinhaDto> Linhas,
    int QuantidadeProdutos,
    decimal ValorTotalEstoque,
    int QuantidadeAbaixoMinimo);
