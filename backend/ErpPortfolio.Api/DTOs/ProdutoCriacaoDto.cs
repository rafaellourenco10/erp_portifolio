// =====================================================================================
// Arquivo....: ProdutoCriacaoDto.cs
// Versão.....: 1.3.0
// Data.......: 22/09/2026
// Descrição..: DTO de entrada para inclusão de produto (POST /api/produtos), com as
//              regras de validação via DataAnnotations. Nome, SKU e unidade são
//              normalizados (trim; maiúsculas na unidade) antes de validar.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.produtos pelo ProdutoService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 21/09/2026 - SKU restrito a dígitos (número de série).
//   1.2.0 - 21/09/2026 - Categoria texto livre trocada por CategoriaId.
//   1.3.0 - 22/09/2026 - EstoqueMinimo obrigatório (usado pelo Dashboard).
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class ProdutoCriacaoDto : IValidatableObject
{
    /// <example>Cabo HDMI 2 metros</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>Número de série do produto, único; somente dígitos (2 a 30).</summary>
    /// <example>10012345</example>
    [Required(ErrorMessage = "O SKU é obrigatório.")]
    [RegularExpression(@"^[0-9]{2,30}$", ErrorMessage = "O SKU deve ter de 2 a 30 dígitos (somente números).")]
    public string Sku
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>Opcional. Id de uma categoria ativa (cadastrada em /api/categorias).</summary>
    /// <example>1</example>
    public int? CategoriaId { get; set; }

    /// <summary>UN, KG, L, M ou CX.</summary>
    /// <example>UN</example>
    [Required(ErrorMessage = "A unidade é obrigatória.")]
    [AllowedValues("UN", "KG", "L", "M", "CX", ErrorMessage = "Unidade inválida. Use UN, KG, L, M ou CX.")]
    public string Unidade
    {
        get;
        set => field = value?.Trim().ToUpperInvariant() ?? string.Empty;
    } = string.Empty;

    /// <example>39.90</example>
    [Required(ErrorMessage = "O preço de venda é obrigatório.")]
    [Range(0, 9_999_999_999.99, ErrorMessage = "O preço de venda deve estar entre 0 e 9.999.999.999,99.")]
    public decimal? PrecoVenda { get; set; }

    /// <example>18.50</example>
    [Required(ErrorMessage = "O custo é obrigatório.")]
    [Range(0, 9_999_999_999.99, ErrorMessage = "O custo deve estar entre 0 e 9.999.999.999,99.")]
    public decimal? Custo { get; set; }

    /// <summary>Saldo de estoque igual ou abaixo disso conta como "baixo" no Dashboard. 0 = sem mínimo definido.</summary>
    /// <example>5</example>
    [Required(ErrorMessage = "O estoque mínimo é obrigatório.")]
    [Range(0, 999_999.999, ErrorMessage = "O estoque mínimo deve estar entre 0 e 999.999,999.")]
    public decimal? EstoqueMinimo { get; set; }

    // As colunas são numeric(12,2)/(12,3): mais casas seriam arredondadas em silêncio pelo banco.
    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (PrecoVenda is { } preco && decimal.Round(preco, 2) != preco)
            yield return new("O preço de venda deve ter no máximo 2 casas decimais.", [nameof(PrecoVenda)]);

        if (Custo is { } custo && decimal.Round(custo, 2) != custo)
            yield return new("O custo deve ter no máximo 2 casas decimais.", [nameof(Custo)]);

        if (EstoqueMinimo is { } minimo && decimal.Round(minimo, 3) != minimo)
            yield return new("O estoque mínimo deve ter no máximo 3 casas decimais.", [nameof(EstoqueMinimo)]);
    }
}
