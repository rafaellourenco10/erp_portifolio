// =====================================================================================
// Arquivo....: Devolucao.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Devolução (parcial ou total) de um pedido de venda confirmado (SPEC.md
//              etapa 14). Guarda os totais já decididos na hora: quanto abateu das parcelas
//              pendentes e quanto virou reembolso ao cliente (valor_total = abatido +
//              reembolso). É definitiva: não se edita nem se exclui.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.devolucoes (FK pedido_id -> public.pedidos)
// Fontes.....: Mapeada em ErpPortfolioDbContext.Devolucoes. Criada pelo DevolucaoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Devolucao
{
    /// <summary>Também é o número da devolução.</summary>
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    /// <summary>Data/hora (UTC) do registro.</summary>
    public DateTime DataDevolucao { get; set; }

    public string? Motivo { get; set; }

    /// <summary>Valor dos itens devolvidos, com os descontos do pedido (DV3).</summary>
    public decimal ValorTotal { get; set; }

    /// <summary>Parte descontada das parcelas pendentes (DV4).</summary>
    public decimal ValorAbatido { get; set; }

    /// <summary>Parte que o cliente já tinha pago e volta como conta a pagar (DV5).</summary>
    public decimal ValorReembolso { get; set; }

    public List<DevolucaoItem> Itens { get; set; } = [];
}
