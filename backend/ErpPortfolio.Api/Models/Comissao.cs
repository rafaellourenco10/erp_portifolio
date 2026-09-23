// =====================================================================================
// Arquivo....: Comissao.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Comissão de um vendedor sobre UMA parcela recebida (SPEC.md, CM1/CM2).
//              Guarda a base (valor da parcela), a % (congelada no pedido) e o valor,
//              para a conta ficar auditável; o status controla o repasse ao vendedor.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.comissoes (FK parcela_receber_id único, pedido_id, vendedor_id)
// Fontes.....: Mapeada em ErpPortfolioDbContext.Comissoes. Gerada por
//              ContasReceberService.MarcarRecebidaAsync; paga por ComissaoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

public class Comissao
{
    public int Id { get; set; }

    public int ParcelaReceberId { get; set; }

    public ParcelaReceber? ParcelaReceber { get; set; }

    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    public int VendedorId { get; set; }

    public Vendedor? Vendedor { get; set; }

    /// <summary>Valor da parcela recebida (base do cálculo).</summary>
    public decimal ValorBase { get; set; }

    /// <summary>% de comissão congelada no pedido ao confirmar.</summary>
    public decimal Percentual { get; set; }

    public decimal Valor { get; set; }

    /// <summary>Data/hora (UTC) em que a parcela foi recebida.</summary>
    public DateTime DataGeracao { get; set; }

    public StatusComissao Status { get; set; } = StatusComissao.Pendente;

    /// <summary>Nulo até a comissão ser paga ao vendedor.</summary>
    public DateTime? DataPagamento { get; set; }
}

/// <summary>Situação do repasse da comissão ao vendedor; gravado como texto ("Pendente", "Paga").</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusComissao
{
    /// <summary>Gerada e ainda não paga ao vendedor.</summary>
    Pendente,

    /// <summary>Paga ao vendedor; estado final.</summary>
    Paga
}
