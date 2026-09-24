// =====================================================================================
// Arquivo....: DevolucaoDtos.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: DTOs da devolução de venda (SPEC.md etapa 14): entrada (itens do pedido com
//              quantidade e "volta ao estoque", motivo e vencimento do reembolso, DV2) e
//              resposta (totais abatido/reembolso, conta de reembolso e estorno de comissão).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usados pelo DevolucaoService para gravar public.devolucoes e
//              public.devolucao_itens.
// Fontes.....: Corpo de POST e resposta de GET /api/pedidos/{id}/devolucoes.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class DevolucaoItemEntradaDto : IValidatableObject
{
    /// <summary>Id do item do pedido (não do produto).</summary>
    /// <example>10</example>
    [Range(1, int.MaxValue, ErrorMessage = "O item do pedido é obrigatório.")]
    public int PedidoItemId { get; set; }

    /// <summary>De 0,001 a 999.999,999, com até 3 casas; inteira para UN e CX; até o que ainda não foi devolvido.</summary>
    /// <example>1</example>
    [Required(ErrorMessage = "A quantidade é obrigatória.")]
    [Range(0.001, 999_999.999, ErrorMessage = "A quantidade deve estar entre 0,001 e 999.999,999.")]
    public decimal? Quantidade { get; set; }

    /// <summary>true (padrão) = o item volta ao estoque; false = perda (ex.: defeito).</summary>
    public bool VoltaEstoque { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Quantidade is { } quantidade && decimal.Round(quantidade, 3) != quantidade)
            yield return new("A quantidade deve ter no máximo 3 casas decimais.", [nameof(Quantidade)]);
    }
}

public class DevolucaoCriacaoDto : IValidatableObject
{
    [Required(ErrorMessage = "Informe ao menos um item.")]
    [MinLength(1, ErrorMessage = "Informe ao menos um item.")]
    [MaxLength(100, ErrorMessage = "A devolução pode ter no máximo 100 itens.")]
    public List<DevolucaoItemEntradaDto> Itens { get; set; } = [];

    /// <example>Produto chegou com defeito</example>
    [StringLength(200, ErrorMessage = "O motivo deve ter no máximo 200 caracteres.")]
    public string? Motivo { get; set; }

    /// <summary>Vencimento da conta de reembolso, se houver (padrão: hoje).</summary>
    /// <example>2026-09-30</example>
    public DateOnly? VencimentoReembolso { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Itens.GroupBy(i => i.PedidoItemId).Any(g => g.Count() > 1))
            yield return new("O mesmo item não pode aparecer mais de uma vez na devolução (repetido).", [nameof(Itens)]);
    }
}

public record DevolucaoItemRespostaDto(
    int Id,
    int PedidoItemId,
    int ProdutoId,
    string ProdutoNome,
    string Unidade,
    decimal Quantidade,
    decimal Valor,
    bool VoltaEstoque);

// ParcelaPagarId: conta a pagar de reembolso (origem Devolucao), se houve reembolso.
// EstornoComissao: estorno de comissão gerado (negativo) ou 0.
public record DevolucaoRespostaDto(
    int Id,
    int PedidoId,
    DateTime DataDevolucao,
    string? Motivo,
    decimal ValorTotal,
    decimal ValorAbatido,
    decimal ValorReembolso,
    int? ParcelaPagarId,
    decimal EstornoComissao,
    IReadOnlyList<DevolucaoItemRespostaDto> Itens);
