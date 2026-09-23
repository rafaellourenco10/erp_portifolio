// =====================================================================================
// Arquivo....: EstoqueMovimentacao.cs
// Versão.....: 1.1.0
// Data.......: 22/09/2026
// Descrição..: Movimentação de estoque de um produto: entrada (manual, compra ou estorno
//              de cancelamento de venda) ou saída (baixa automática ao confirmar um pedido
//              de venda, ou estorno de cancelamento de um pedido de compra).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.estoque_movimentacoes (FKs produto_id -> public.produtos,
//              pedido_id -> public.pedidos e pedido_compra_id -> public.pedidos_compra,
//              ambas opcionais e mutuamente exclusivas)
// Fontes.....: Mapeada em ErpPortfolioDbContext.EstoqueMovimentacoes (EF Core / Npgsql).
//              O saldo não é gravado: é derivado por Services/EstoqueCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
//   1.1.0 - 22/09/2026 - PedidoCompraId/PedidoCompra, para a entrada automática e o
//                        estorno do Pedido de Compra (SPEC.md, PC6/PC7).
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class EstoqueMovimentacao
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    public TipoMovimentacao Tipo { get; set; }

    /// <summary>Sempre positiva; o tipo é que define se soma ou subtrai do saldo.</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Opcional. Preenchido automaticamente nas movimentações geradas por pedido.</summary>
    public string? Motivo { get; set; }

    /// <summary>Nulo em entrada manual ou de compra; preenchido na baixa por venda e no estorno de cancelamento de venda.</summary>
    public int? PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    /// <summary>Nulo fora do fluxo de compra; preenchido na entrada por compra e no estorno de cancelamento de compra.</summary>
    public int? PedidoCompraId { get; set; }

    public PedidoCompra? PedidoCompra { get; set; }

    public DateTime DataMovimentacao { get; set; }
}
