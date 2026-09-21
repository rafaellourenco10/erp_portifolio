// =====================================================================================
// Arquivo....: StatusPedido.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Situação de um pedido de venda. Gravado como texto no banco e escrito/lido
//              como texto no JSON ("Rascunho", "Confirmado", "Cancelado").
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedidos (coluna status, varchar(20)).
// Fontes.....: Regras de transição em Services/TransicoesPedido.cs (SPEC.md, R1 e R2).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusPedido
{
    /// <summary>Em montagem; é o único estado em que o pedido pode ser editado.</summary>
    Rascunho,

    /// <summary>Venda fechada; não se edita, só se cancela.</summary>
    Confirmado,

    /// <summary>Venda desfeita; estado final (o registro continua no histórico).</summary>
    Cancelado
}
