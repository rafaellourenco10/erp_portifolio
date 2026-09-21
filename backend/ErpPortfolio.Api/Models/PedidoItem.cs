// =====================================================================================
// Arquivo....: PedidoItem.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Item de um pedido: produto, quantidade, preço unitário copiado do produto no
//              momento em que o item é adicionado (fica congelado) e desconto do item.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedido_itens (FKs pedido_id -> public.pedidos e produto_id ->
//              public.produtos)
// Fontes.....: Mapeada em ErpPortfolioDbContext.PedidoItens (EF Core / Npgsql).
//              Colunas: id, pedido_id, produto_id, quantidade, preco_unitario,
//              desconto_percentual. O subtotal do item não é gravado: é derivado por
//              Services/CalculoPedido.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class PedidoItem
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    /// <summary>Até 3 casas decimais; inteira para produtos em UN e CX.</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Preço de venda do produto no momento em que o item foi adicionado (congelado).</summary>
    public decimal PrecoUnitario { get; set; }

    /// <summary>Desconto em % do item (0 a 100).</summary>
    public decimal DescontoPercentual { get; set; }
}
