// =====================================================================================
// Arquivo....: DevolucaoItem.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Item de uma devolução: qual item do pedido, quanto voltou, o valor dessa
//              parte (DV3) e se voltou ao estoque (DV7; sem = perda, ex.: defeito).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.devolucao_itens (FKs devolucao_id -> public.devolucoes e
//              pedido_item_id -> public.pedido_itens)
// Fontes.....: Mapeada em ErpPortfolioDbContext.DevolucaoItens.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class DevolucaoItem
{
    public int Id { get; set; }

    public int DevolucaoId { get; set; }

    public Devolucao? Devolucao { get; set; }

    public int PedidoItemId { get; set; }

    public PedidoItem? PedidoItem { get; set; }

    /// <summary>Até 3 casas; inteira para UN e CX.</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Valor desta parte do item, com os descontos do item e do pedido.</summary>
    public decimal Valor { get; set; }

    public bool VoltaEstoque { get; set; }
}
