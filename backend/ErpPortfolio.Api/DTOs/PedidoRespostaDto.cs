// =====================================================================================
// Arquivo....: PedidoRespostaDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de saída com um pedido completo (cabeçalho, cliente e itens) e o
//              subtotal de cada item. O subtotal do item é derivado (CalculoPedido), não
//              gravado; valorTotal é o total gravado no pedido.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.pedidos, public.pedido_itens, public.clientes e
//              public.produtos.
// Fontes.....: Entidade Models.Pedido com Cliente e Itens (com Produto) carregados,
//              convertida por DeEntidade().
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Api.DTOs;

public record PedidoItemRespostaDto(
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
    public static PedidoItemRespostaDto DeEntidade(PedidoItem item) => new(
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

public record PedidoRespostaDto(
    int Id,
    int ClienteId,
    string ClienteNome,
    string ClienteDocumento,
    DateTime DataPedido,
    StatusPedido Status,
    FormaPagamento? FormaPagamento,
    decimal DescontoPercentual,
    decimal SubtotalItens,
    decimal ValorTotal,
    IReadOnlyList<PedidoItemRespostaDto> Itens)
{
    public static PedidoRespostaDto DeEntidade(Pedido pedido)
    {
        var itens = pedido.Itens.OrderBy(i => i.Id).Select(PedidoItemRespostaDto.DeEntidade).ToList();

        return new PedidoRespostaDto(
            pedido.Id,
            pedido.ClienteId,
            pedido.Cliente!.Nome,
            pedido.Cliente.Documento,
            pedido.DataPedido,
            pedido.Status,
            pedido.FormaPagamento,
            pedido.DescontoPercentual,
            itens.Sum(i => i.Subtotal),
            pedido.ValorTotal,
            itens);
    }
}
