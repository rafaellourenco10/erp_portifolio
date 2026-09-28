// =====================================================================================
// Arquivo....: NotaFiscal.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: NF-e simulada (SPEC.md etapa 16): de saída (venda de um pedido) ou de entrada
//              (devolução de venda, referenciando a nota original). Nasce autorizada e nunca
//              muda (NF8): destinatário, itens, totais e XML ficam congelados.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.notas_fiscais (FKs cliente_id, pedido_id, devolucao_id e
//              nota_referenciada_id -> notas_fiscais)
// Fontes.....: Mapeada em ErpPortfolioDbContext.NotasFiscais. Criada pelo NotaFiscalService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class NotaFiscal
{
    public int Id { get; set; }

    public TipoNotaFiscal Tipo { get; set; }

    public int Serie { get; set; }

    public int Numero { get; set; }

    /// <summary>Chave de acesso de 44 posições (NF5).</summary>
    public string Chave { get; set; } = string.Empty;

    /// <summary>Data/hora (UTC) da emissão e da autorização simulada.</summary>
    public DateTime DataEmissao { get; set; }

    /// <summary>Protocolo de autorização simulado (15 dígitos).</summary>
    public string Protocolo { get; set; } = string.Empty;

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    /// <summary>Na saída: o pedido faturado. Na entrada: o pedido da devolução.</summary>
    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    /// <summary>Só na entrada.</summary>
    public int? DevolucaoId { get; set; }

    public Devolucao? Devolucao { get; set; }

    /// <summary>Só na entrada: a NF-e de saída que está sendo devolvida (refNFe).</summary>
    public int? NotaReferenciadaId { get; set; }

    public NotaFiscal? NotaReferenciada { get; set; }

    // Destinatário congelado na emissão.
    public string DestinatarioNome { get; set; } = string.Empty;

    public string DestinatarioDocumento { get; set; } = string.Empty;

    public string DestinatarioUf { get; set; } = string.Empty;

    public decimal ValorProdutos { get; set; }

    public decimal ValorDesconto { get; set; }

    public decimal BaseIcms { get; set; }

    public decimal ValorIcms { get; set; }

    public decimal ValorPis { get; set; }

    public decimal ValorCofins { get; set; }

    public decimal ValorTotal { get; set; }

    /// <summary>nfeProc (NFe + protNFe) no layout 4.00, como texto.</summary>
    public string Xml { get; set; } = string.Empty;

    public List<NotaFiscalItem> Itens { get; set; } = [];
}

/// <summary>Gravado como texto ("Saida", "Entrada").</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum TipoNotaFiscal
{
    Saida,
    Entrada
}
