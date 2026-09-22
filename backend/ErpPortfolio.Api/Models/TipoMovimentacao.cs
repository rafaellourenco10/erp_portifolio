// =====================================================================================
// Arquivo....: TipoMovimentacao.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Tipo de uma movimentação de estoque. Gravado como texto no banco e
//              escrito/lido como texto no JSON ("Entrada", "Saida").
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.estoque_movimentacoes (coluna tipo, varchar(20)).
// Fontes.....: Cálculo de saldo em Services/EstoqueCalculo.cs (SPEC.md, E1).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoMovimentacao
{
    /// <summary>Aumenta o saldo: lançamento manual (compra) ou estorno de cancelamento.</summary>
    Entrada,

    /// <summary>Reduz o saldo: baixa automática ao confirmar um pedido.</summary>
    Saida
}
