// =====================================================================================
// Arquivo....: EstoqueEntradaDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de entrada para lançar uma entrada manual de estoque (compra/ajuste):
//              produto, quantidade e motivo opcional (texto livre). Sem fornecedor (E4).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.estoque_movimentacoes pelo EstoqueService.
// Fontes.....: Corpo (JSON) da requisição HTTP (POST /api/estoque/entradas).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class EstoqueEntradaDto : IValidatableObject
{
    /// <example>5</example>
    [Range(1, int.MaxValue, ErrorMessage = "O produto é obrigatório.")]
    public int ProdutoId { get; set; }

    /// <summary>De 0,001 a 999.999,999, com até 3 casas.</summary>
    /// <example>10</example>
    [Required(ErrorMessage = "A quantidade é obrigatória.")]
    [Range(0.001, 999_999.999, ErrorMessage = "A quantidade deve estar entre 0,001 e 999.999,999.")]
    public decimal? Quantidade { get; set; }

    /// <summary>Opcional, texto livre (ex.: "Compra NF 1234"). Até 200 caracteres.</summary>
    /// <example>Compra NF 1234</example>
    [StringLength(200, ErrorMessage = "O motivo deve ter no máximo 200 caracteres.")]
    public string? Motivo
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    // A coluna é numeric(12,3): mais casas seriam arredondadas em silêncio pelo banco.
    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Quantidade is { } quantidade && decimal.Round(quantidade, 3) != quantidade)
            yield return new("A quantidade deve ter no máximo 3 casas decimais.", [nameof(Quantidade)]);
    }
}
