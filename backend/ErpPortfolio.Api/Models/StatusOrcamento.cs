// =====================================================================================
// Arquivo....: StatusOrcamento.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Situação de um orçamento (SPEC.md etapa 13). Gravado como texto no banco e
//              no JSON. "Vencido" não é status: é calculado (Orcamento.EstaVencido, OR4).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.orcamentos (coluna status, varchar(20), CHECK ck_orcamentos_status).
// Fontes.....: Transições em Services/OrcamentoService.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusOrcamento
{
    /// <summary>Em negociação; único estado editável.</summary>
    Aberto,

    /// <summary>Virou pedido de venda (automático ao gerar o pedido); estado final.</summary>
    Aprovado,

    /// <summary>Cliente não fechou; marcado à mão; estado final.</summary>
    Perdido
}
