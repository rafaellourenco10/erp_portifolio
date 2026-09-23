// =====================================================================================
// Arquivo....: ParcelaPagar.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Parcela a pagar de um pedido de compra: número, valor, vencimento e
//              status. Gerada automaticamente ao confirmar o pedido de compra.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.parcelas_pagar (FK pedido_compra_id -> public.pedidos_compra)
// Fontes.....: Mapeada em ErpPortfolioDbContext.ParcelasPagar (EF Core / Npgsql).
//              Gerada e cancelada por Services/ContasPagarService.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class ParcelaPagar
{
    public int Id { get; set; }

    public int PedidoCompraId { get; set; }

    public PedidoCompra? PedidoCompra { get; set; }

    /// <summary>1-based.</summary>
    public int NumeroParcela { get; set; }

    public decimal Valor { get; set; }

    public DateOnly Vencimento { get; set; }

    public StatusParcelaPagar Status { get; set; }

    /// <summary>Nulo até a parcela ser marcada como paga.</summary>
    public DateTime? DataPagamento { get; set; }
}
