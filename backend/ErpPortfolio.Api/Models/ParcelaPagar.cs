// =====================================================================================
// Arquivo....: ParcelaPagar.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Parcela a pagar: número (X de Y), valor, vencimento e status. A origem diz
//              de onde veio (SPEC.md etapa 12, CP1): Compra (confirmar pedido de compra),
//              Comissao (fechamento de comissões de um vendedor) ou Avulsa (lançada à mão).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.parcelas_pagar (FKs opcionais pedido_compra_id -> pedidos_compra e
//              vendedor_id -> vendedores, conforme a origem)
// Fontes.....: Mapeada em ErpPortfolioDbContext.ParcelasPagar (EF Core / Npgsql).
//              Gerada e cancelada por Services/ContasPagarService.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Origem (Compra/Comissao/Avulsa), vendedor, descrição, favorecido e total
//                        de parcelas gravado (etapa 12).
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class ParcelaPagar
{
    public int Id { get; set; }

    public OrigemContaPagar Origem { get; set; } = OrigemContaPagar.Compra;

    /// <summary>Preenchido quando a origem é Compra.</summary>
    public int? PedidoCompraId { get; set; }

    public PedidoCompra? PedidoCompra { get; set; }

    /// <summary>Preenchido quando a origem é Comissao (o favorecido é o vendedor).</summary>
    public int? VendedorId { get; set; }

    public Vendedor? Vendedor { get; set; }

    /// <summary>Obrigatória na Avulsa; na Comissao, gerada ("Comissões — Nome (N)"); nula na Compra.</summary>
    public string? Descricao { get; set; }

    /// <summary>Favorecido digitado na Avulsa (opcional); nas outras origens vem do fornecedor/vendedor.</summary>
    public string? Favorecido { get; set; }

    /// <summary>1-based.</summary>
    public int NumeroParcela { get; set; }

    /// <summary>Quantas parcelas o lançamento tem (o "Y" de X/Y), gravado na criação.</summary>
    public int TotalParcelas { get; set; }

    public decimal Valor { get; set; }

    public DateOnly Vencimento { get; set; }

    public StatusParcelaPagar Status { get; set; }

    /// <summary>Nulo até a parcela ser marcada como paga.</summary>
    public DateTime? DataPagamento { get; set; }
}

/// <summary>De onde veio a conta a pagar; gravado como texto ("Compra", "Comissao", "Avulsa").</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum OrigemContaPagar
{
    Compra,
    Comissao,
    Avulsa
}
