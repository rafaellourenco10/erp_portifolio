// =====================================================================================
// Arquivo....: Pedido.cs
// Versão.....: 1.1.0
// Data.......: 21/09/2026
// Descrição..: Entidade de domínio que representa um pedido de venda: cabeçalho com cliente,
//              status, forma de pagamento, desconto do pedido e o total calculado.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedidos (FK cliente_id -> public.clientes)
// Fontes.....: Mapeada em ErpPortfolioDbContext.Pedidos (EF Core / Npgsql).
//              Colunas: id, cliente_id, data_pedido, status, forma_pagamento,
//              desconto_percentual, valor_total (o mapeamento de nomes fica no DbContext).
//              O total é recalculado pelo serviço a cada gravação (Services/CalculoPedido.cs).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Vendedor e % de comissão congelada ao confirmar (etapa 10).
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Pedido
{
    /// <summary>Também é o número do pedido.</summary>
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    /// <summary>Data/hora de criação em UTC; não é editável.</summary>
    public DateTime DataPedido { get; set; }

    public StatusPedido Status { get; set; } = StatusPedido.Rascunho;

    /// <summary>Opcional no rascunho; obrigatória para confirmar.</summary>
    public FormaPagamento? FormaPagamento { get; set; }

    /// <summary>Opcional no rascunho; obrigatório (e ativo) para confirmar. Pedidos antigos ficam sem.</summary>
    public int? VendedorId { get; set; }

    public Vendedor? Vendedor { get; set; }

    /// <summary>% de comissão do vendedor copiada ao confirmar; nula no rascunho e nunca muda depois.</summary>
    public decimal? PercentualComissao { get; set; }

    /// <summary>Desconto em % sobre a soma dos itens (0 a 100).</summary>
    public decimal DescontoPercentual { get; set; }

    /// <summary>Total do pedido em reais, gravado a cada salvamento.</summary>
    public decimal ValorTotal { get; set; }

    public List<PedidoItem> Itens { get; set; } = [];
}
