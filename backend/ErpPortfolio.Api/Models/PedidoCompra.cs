// =====================================================================================
// Arquivo....: PedidoCompra.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Entidade de domínio que representa um pedido de compra: cabeçalho com
//              fornecedor, status, desconto do pedido e o total calculado. Sem forma de
//              pagamento (SPEC.md, "Fora do escopo" — não há Contas a Pagar ainda).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedidos_compra (FK fornecedor_id -> public.fornecedores)
// Fontes.....: Mapeada em ErpPortfolioDbContext.PedidosCompra (EF Core / Npgsql).
//              Colunas: id, fornecedor_id, data_pedido, status, desconto_percentual,
//              valor_total (o mapeamento de nomes fica no DbContext).
//              O total é recalculado pelo serviço a cada gravação (Services/CalculoPedido.cs,
//              reaproveitado do Pedido de Venda).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class PedidoCompra
{
    /// <summary>Também é o número do pedido de compra.</summary>
    public int Id { get; set; }

    public int FornecedorId { get; set; }

    public Fornecedor? Fornecedor { get; set; }

    /// <summary>Data/hora de criação em UTC; não é editável.</summary>
    public DateTime DataPedido { get; set; }

    public StatusPedido Status { get; set; } = StatusPedido.Rascunho;

    /// <summary>Desconto em % sobre a soma dos itens (0 a 100).</summary>
    public decimal DescontoPercentual { get; set; }

    /// <summary>Total do pedido em reais, gravado a cada salvamento.</summary>
    public decimal ValorTotal { get; set; }

    public List<PedidoCompraItem> Itens { get; set; } = [];
}
