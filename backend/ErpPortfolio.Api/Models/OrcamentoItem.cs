// =====================================================================================
// Arquivo....: OrcamentoItem.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Item de um orçamento: produto, quantidade, preço unitário copiado do produto
//              quando o item é adicionado (congelado, OR2) e desconto do item. É esse preço
//              que vai para o pedido gerado (GP3).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.orcamento_itens (FKs orcamento_id -> orcamentos, produto_id -> produtos)
// Fontes.....: Mapeada em ErpPortfolioDbContext.OrcamentoItens (EF Core / Npgsql).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class OrcamentoItem
{
    public int Id { get; set; }

    public int OrcamentoId { get; set; }

    public Orcamento? Orcamento { get; set; }

    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    /// <summary>Até 3 casas decimais; inteira para produtos em UN e CX.</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Preço de venda do produto no momento em que o item foi adicionado (congelado).</summary>
    public decimal PrecoUnitario { get; set; }

    /// <summary>Desconto em % do item (0 a 100).</summary>
    public decimal DescontoPercentual { get; set; }
}
