// =====================================================================================
// Arquivo....: PedidoRespostaDto.cs
// Versão.....: 1.2.0
// Data.......: 23/09/2026
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
//   1.1.0 - 23/09/2026 - Vendedor e % de comissão congelada (etapa 10).
//   1.2.0 - 24/09/2026 - QuantidadeDevolvida por item e ValorDevolvido (etapa 14).
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
    decimal Subtotal,
    decimal QuantidadeDevolvida = 0)
{
    public static PedidoItemRespostaDto DeEntidade(PedidoItem item, decimal quantidadeDevolvida = 0) => new(
        item.Id,
        item.ProdutoId,
        item.Produto!.Nome,
        item.Produto.Sku,
        item.Produto.Unidade,
        item.Quantidade,
        item.PrecoUnitario,
        item.DescontoPercentual,
        CalculoPedido.Subtotal(item.Quantidade, item.PrecoUnitario, item.DescontoPercentual),
        quantidadeDevolvida);
}

public record PedidoRespostaDto(
    int Id,
    int ClienteId,
    string ClienteNome,
    string ClienteDocumento,
    DateTime DataPedido,
    StatusPedido Status,
    FormaPagamento? FormaPagamento,
    int? VendedorId,
    string? VendedorNome,
    decimal? PercentualComissao,
    decimal DescontoPercentual,
    decimal SubtotalItens,
    decimal ValorTotal,
    IReadOnlyList<PedidoItemRespostaDto> Itens,
    decimal ValorDevolvido = 0)
{
    /// <param name="pedido">Com cliente, vendedor e itens (com produto) carregados.</param>
    /// <param name="devolvidoPorItem">Quantidade já devolvida por id do item (etapa 14); ausente = nada devolvido.</param>
    /// <param name="valorDevolvido">Soma das devoluções do pedido.</param>
    public static PedidoRespostaDto DeEntidade(Pedido pedido, IReadOnlyDictionary<int, decimal>? devolvidoPorItem = null, decimal valorDevolvido = 0)
    {
        var itens = pedido.Itens.OrderBy(i => i.Id)
            .Select(i => PedidoItemRespostaDto.DeEntidade(i, devolvidoPorItem?.GetValueOrDefault(i.Id) ?? 0))
            .ToList();

        return new PedidoRespostaDto(
            pedido.Id,
            pedido.ClienteId,
            pedido.Cliente!.Nome,
            pedido.Cliente.Documento,
            pedido.DataPedido,
            pedido.Status,
            pedido.FormaPagamento,
            pedido.VendedorId,
            pedido.Vendedor?.Nome,
            pedido.PercentualComissao,
            pedido.DescontoPercentual,
            itens.Sum(i => i.Subtotal),
            pedido.ValorTotal,
            itens,
            valorDevolvido);
    }
}
