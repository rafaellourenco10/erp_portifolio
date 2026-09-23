// =====================================================================================
// Arquivo....: Orcamento.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Orçamento de venda (SPEC.md etapa 13): proposta ao cliente com itens,
//              descontos e validade. Quando o cliente aprova, gera um pedido de venda em
//              rascunho com os mesmos preços (GP3) e passa a Aprovado.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.orcamentos (FKs cliente_id -> clientes, vendedor_id -> vendedores,
//              pedido_id -> pedidos)
// Fontes.....: Mapeada em ErpPortfolioDbContext.Orcamentos (EF Core / Npgsql).
//              Colunas: id, cliente_id, vendedor_id, data_orcamento, validade, status,
//              forma_pagamento, desconto_percentual, valor_total, observacoes,
//              motivo_perda, pedido_id.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Orcamento
{
    /// <summary>Também é o número do orçamento.</summary>
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    /// <summary>Opcional; vai para o pedido gerado se ainda estiver ativo (GP2).</summary>
    public int? VendedorId { get; set; }

    public Vendedor? Vendedor { get; set; }

    /// <summary>Data/hora de criação em UTC; não é editável.</summary>
    public DateTime DataOrcamento { get; set; }

    /// <summary>Último dia em que o orçamento vale. Aberto com validade passada = vencido (OR4).</summary>
    public DateOnly Validade { get; set; }

    public StatusOrcamento Status { get; set; } = StatusOrcamento.Aberto;

    public FormaPagamento? FormaPagamento { get; set; }

    /// <summary>Desconto em % sobre a soma dos itens (0 a 100).</summary>
    public decimal DescontoPercentual { get; set; }

    /// <summary>Total em reais, recalculado a cada gravação (CalculoPedido).</summary>
    public decimal ValorTotal { get; set; }

    public string? Observacoes { get; set; }

    /// <summary>Preenchido (opcional) ao marcar como Perdido.</summary>
    public string? MotivoPerda { get; set; }

    /// <summary>Pedido gerado a partir deste orçamento (só no Aprovado).</summary>
    public int? PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    public List<OrcamentoItem> Itens { get; set; } = [];

    /// <summary>OR4: Aberto com a validade antes de hoje. Calculado, não gravado.</summary>
    public bool EstaVencido(DateOnly hoje) => Status == StatusOrcamento.Aberto && Validade < hoje;
}
