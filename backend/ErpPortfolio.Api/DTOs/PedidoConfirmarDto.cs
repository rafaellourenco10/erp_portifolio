// =====================================================================================
// Arquivo....: PedidoConfirmarDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de entrada de PATCH /api/pedidos/{id}/confirmar: número de parcelas e
//              intervalo em dias entre vencimentos, usados para gerar as contas a
//              receber (SPEC.md, C1). Corpo opcional; ausente = padrão (1 parcela, 30 dias).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado por ContasReceberService.GerarParcelas para gravar public.parcelas_receber.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
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
