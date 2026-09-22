// =====================================================================================
// Arquivo....: StatusParcela.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Situação de uma parcela a receber. Gravado como texto no banco e
//              escrito/lido como texto no JSON ("Pendente", "Recebido", "Cancelado").
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.parcelas_receber (coluna status, varchar(20)).
// Fontes.....: Regras em Services/ContasReceberService.cs (SPEC.md, C4, C6, C7).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusParcela
{
    /// <summary>Aguardando recebimento; "atrasada" é calculada (vencimento no passado), não gravada.</summary>
    Pendente,

    /// <summary>Recebida; estado final.</summary>
    Recebido,

    /// <summary>Cancelada (pedido cancelado antes de receber); estado final.</summary>
    Cancelado
}
