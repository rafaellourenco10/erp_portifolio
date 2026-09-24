// =====================================================================================
// Arquivo....: Comissao.cs
// Versão.....: 1.2.0
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
//   1.1.0 - 23/09/2026 - Status EmPagamento e vínculo com a conta a pagar (etapa 12).
//   1.2.0 - 24/09/2026 - Estorno de devolução: valor negativo, DevolucaoId e ParcelaReceberId
//                        nulo (SPEC.md etapa 14, DV6).
// =====================================================================================

using System.Text.Json.Serialization;

namespace ErpPortfolio.Api.Models;

public class Comissao
{
    public int Id { get; set; }

    /// <summary>Parcela recebida que gerou a comissão; nula no estorno de devolução.</summary>
    public int? ParcelaReceberId { get; set; }

    public ParcelaReceber? ParcelaReceber { get; set; }

    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    public int VendedorId { get; set; }

    public Vendedor? Vendedor { get; set; }

    /// <summary>Valor da parcela recebida (base do cálculo).</summary>
    public decimal ValorBase { get; set; }

    /// <summary>% de comissão congelada no pedido ao confirmar.</summary>
    public decimal Percentual { get; set; }

    /// <summary>Positivo na comissão normal; negativo no estorno de devolução (descontado no próximo fechamento).</summary>
    public decimal Valor { get; set; }

    /// <summary>Data/hora (UTC) em que a parcela foi recebida.</summary>
    public DateTime DataGeracao { get; set; }

    public StatusComissao Status { get; set; } = StatusComissao.Pendente;

    /// <summary>Nulo até a comissão ser paga ao vendedor.</summary>
    public DateTime? DataPagamento { get; set; }

    /// <summary>Conta a pagar gerada no fechamento (etapa 12); nula enquanto Pendente e nas pagas antes dela.</summary>
    public int? ParcelaPagarId { get; set; }

    public ParcelaPagar? ParcelaPagar { get; set; }

    /// <summary>Só no estorno: a devolução que o gerou (DV6).</summary>
    public int? DevolucaoId { get; set; }

    public Devolucao? Devolucao { get; set; }
}

/// <summary>Situação do repasse da comissão ao vendedor; gravado como texto ("Pendente", "EmPagamento", "Paga").</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusComissao
{
    /// <summary>Gerada e ainda não paga ao vendedor.</summary>
    Pendente,

    /// <summary>Já virou conta a pagar (etapa 12); fica Paga quando a conta for paga.</summary>
    EmPagamento,

    /// <summary>Paga ao vendedor; estado final.</summary>
    Paga
}
