// =====================================================================================
// Arquivo....: PedidoCompraItem.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Item de um pedido de compra: produto, quantidade, preço unitário copiado
//              do Custo do produto no momento em que o item é adicionado (fica congelado,
//              SPEC.md PC2) e desconto do item.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedido_compra_itens (FKs pedido_compra_id -> public.pedidos_compra e
//              produto_id -> public.produtos)
// Fontes.....: Mapeada em ErpPortfolioDbContext.PedidoCompraItens (EF Core / Npgsql).
//              Colunas: id, pedido_compra_id, produto_id, quantidade, preco_unitario,
//              desconto_percentual. O subtotal do item não é gravado: é derivado por
//              Services/CalculoPedido.cs (reaproveitado do Pedido de Venda).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class PedidoCompraItem
{
    public int Id { get; set; }

    public int PedidoCompraId { get; set; }

    public PedidoCompra? PedidoCompra { get; set; }

    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    /// <summary>Até 3 casas decimais; inteira para produtos em UN e CX.</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Custo do produto no momento em que o item foi adicionado (congelado).</summary>
    public decimal PrecoUnitario { get; set; }

    /// <summary>Desconto em % do item (0 a 100).</summary>
    public decimal DescontoPercentual { get; set; }
}
