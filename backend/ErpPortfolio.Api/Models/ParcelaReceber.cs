// =====================================================================================
// Arquivo....: ParcelaReceber.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Parcela a receber de um pedido de venda: número, valor, vencimento e
//              status. Gerada automaticamente ao confirmar o pedido.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.parcelas_receber (FK pedido_id -> public.pedidos)
// Fontes.....: Mapeada em ErpPortfolioDbContext.ParcelasReceber (EF Core / Npgsql).
//              Gerada e cancelada por Services/ContasReceberService.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class ParcelaReceber
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    /// <summary>1-based.</summary>
    public int NumeroParcela { get; set; }

    public decimal Valor { get; set; }

    public DateOnly Vencimento { get; set; }

    public StatusParcela Status { get; set; }

    /// <summary>Nulo até a parcela ser marcada como recebida.</summary>
    public DateTime? DataRecebimento { get; set; }
}
