// =====================================================================================
// Arquivo....: PedidoConfirmarDto.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: DTO de entrada de PATCH /api/pedidos/{id}/confirmar e
//              /api/pedidos-compra/{id}/confirmar: número de parcelas e intervalo em dias
//              entre vencimentos, usados para gerar as contas a receber / a pagar. Corpo opcional; ausente = padrão (1 parcela, 30 dias).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado por ContasReceberService.GerarParcelas (public.parcelas_receber) e
//              ContasPagarService.GerarParcelas (public.parcelas_pagar).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Reaproveitado no confirmar do pedido de compra (etapa 8).
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class PedidoConfirmarDto
{
    /// <summary>De 1 a 12. Padrão 1 (à vista).</summary>
    /// <example>1</example>
    [Range(1, 12, ErrorMessage = "O número de parcelas deve estar entre 1 e 12.")]
    public int NumeroParcelas { get; set; } = 1;

    /// <summary>Dias entre uma parcela e a próxima. Padrão 30.</summary>
    /// <example>30</example>
    [Range(1, 180, ErrorMessage = "O intervalo entre parcelas deve estar entre 1 e 180 dias.")]
    public int IntervaloDias { get; set; } = 30;
}
