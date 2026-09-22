// =====================================================================================
// Arquivo....: EstoqueMovimentacao.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Movimentação de estoque de um produto: entrada (manual ou estorno de
//              cancelamento) ou saída (baixa automática ao confirmar um pedido).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.estoque_movimentacoes (FKs produto_id -> public.produtos e
//              pedido_id -> public.pedidos, opcional)
// Fontes.....: Mapeada em ErpPortfolioDbContext.EstoqueMovimentacoes (EF Core / Npgsql).
//              O saldo não é gravado: é derivado por Services/EstoqueCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
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

    /// <summary>Nulo em entrada manual; preenchido na baixa por venda e no estorno de cancelamento.</summary>
    public int? PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    public DateTime DataMovimentacao { get; set; }
}
