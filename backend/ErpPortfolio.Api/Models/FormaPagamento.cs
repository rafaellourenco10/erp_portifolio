// =====================================================================================
// Arquivo....: FormaPagamento.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Forma de pagamento do pedido (lista fixa, sem parcelas). Gravada como texto
//              no banco e escrita/lida como texto no JSON ("Dinheiro", "Pix", "Boleto",
//              "Cartao").
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedidos (coluna forma_pagamento, varchar(20), nula no rascunho).
// Fontes.....: SPEC.md (decisão "Pagamento" e regra R6).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormaPagamento
{
    Dinheiro,
    Pix,
    Boleto,
    Cartao
}
