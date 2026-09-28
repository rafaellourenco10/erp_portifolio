// =====================================================================================
// Arquivo....: NotaFiscalItem.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Item de uma NF-e, com os dados do produto e os impostos congelados na emissão
//              (NF4, NF8).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.nota_fiscal_itens (FKs nota_fiscal_id -> notas_fiscais,
//              produto_id -> produtos)
// Fontes.....: Mapeada em ErpPortfolioDbContext.NotaFiscalItens. Valores do NfeCalculo.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class NotaFiscalItem
{
    public int Id { get; set; }

    public int NotaFiscalId { get; set; }

    public NotaFiscal? NotaFiscal { get; set; }

    /// <summary>nItem do XML, 1-based.</summary>
    public int NumeroItem { get; set; }

    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    /// <summary>SKU do produto na emissão.</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Ncm { get; set; } = string.Empty;

    public string Cfop { get; set; } = string.Empty;

    public string Unidade { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    /// <summary>vProd = quantidade × valor unitário.</summary>
    public decimal ValorBruto { get; set; }

    /// <summary>vDesc = desconto do item + rateio do desconto do pedido.</summary>
    public decimal ValorDesconto { get; set; }

    public decimal BaseIcms { get; set; }

    public decimal AliquotaIcms { get; set; }

    public decimal ValorIcms { get; set; }

    public decimal ValorPis { get; set; }

    public decimal ValorCofins { get; set; }
}
