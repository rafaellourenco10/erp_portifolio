// =====================================================================================
// Arquivo....: PedidoCompraRespostaDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída com um pedido de compra completo (cabeçalho, fornecedor e
//              itens) e o subtotal de cada item. O subtotal do item é derivado
//              (CalculoPedido, reaproveitado do Pedido de Venda), não gravado; valorTotal
//              é o total gravado no pedido. Espelho de PedidoRespostaDto, sem forma de
//              pagamento.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.pedidos_compra, public.pedido_compra_itens,
//              public.fornecedores e public.produtos.
// Fontes.....: Entidade Models.PedidoCompra com Fornecedor e Itens (com Produto)
//              carregados, convertida por DeEntidade().
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Api.DTOs;

public record PedidoCompraItemRespostaDto(
    int Id,
    int ProdutoId,
    string ProdutoNome,
    string Sku,
    string Unidade,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal DescontoPercentual,
    decimal Subtotal)
{
    public static PedidoCompraItemRespostaDto DeEntidade(PedidoCompraItem item) => new(
        item.Id,
        item.ProdutoId,
        item.Produto!.Nome,
        item.Produto.Sku,
        item.Produto.Unidade,
        item.Quantidade,
        item.PrecoUnitario,
        item.DescontoPercentual,
        CalculoPedido.Subtotal(item.Quantidade, item.PrecoUnitario, item.DescontoPercentual));
}

public record PedidoCompraRespostaDto(
    int Id,
    int FornecedorId,
    string FornecedorNome,
    string FornecedorDocumento,
    DateTime DataPedido,
    StatusPedido Status,
    decimal DescontoPercentual,
    decimal SubtotalItens,
    decimal ValorTotal,
    IReadOnlyList<PedidoCompraItemRespostaDto> Itens)
{
    public static PedidoCompraRespostaDto DeEntidade(PedidoCompra pedido)
    {
        var itens = pedido.Itens.OrderBy(i => i.Id).Select(PedidoCompraItemRespostaDto.DeEntidade).ToList();

        return new PedidoCompraRespostaDto(
            pedido.Id,
            pedido.FornecedorId,
            pedido.Fornecedor!.Nome,
            pedido.Fornecedor.Documento,
            pedido.DataPedido,
            pedido.Status,
            pedido.DescontoPercentual,
            itens.Sum(i => i.Subtotal),
            pedido.ValorTotal,
            itens);
    }
}
