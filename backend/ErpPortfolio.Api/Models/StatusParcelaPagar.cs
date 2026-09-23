// =====================================================================================
// Arquivo....: StatusParcelaPagar.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Situação de uma parcela a pagar. Gravado como texto no banco e
//              escrito/lido como texto no JSON ("Pendente", "Pago", "Cancelado").
//              Separado de StatusParcela porque "Recebido" não faz sentido numa conta a pagar.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.parcelas_pagar (coluna status, varchar(20)).
// Fontes.....: Regras em Services/ContasPagarService.cs (SPEC.md, P3, P4, P5).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusParcelaPagar
{
    /// <summary>Aguardando pagamento; "atrasada" é calculada (vencimento no passado), não gravada.</summary>
    Pendente,

    /// <summary>Paga; estado final.</summary>
    Pago,

    /// <summary>Cancelada (pedido de compra cancelado antes de pagar); estado final.</summary>
    Cancelado
}
