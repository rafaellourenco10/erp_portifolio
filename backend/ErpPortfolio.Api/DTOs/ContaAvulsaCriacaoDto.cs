// =====================================================================================
// Arquivo....: ContaAvulsaCriacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: DTO de entrada de POST /api/contas-pagar: conta avulsa (aluguel, luz...)
//              em 1 a 12 parcelas (SPEC.md etapa 12, AV1).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo ContasPagarService para gravar public.parcelas_pagar (origem Avulsa).
// Fontes.....: Corpo de POST /api/contas-pagar.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class ContaAvulsaCriacaoDto : IValidatableObject
{
    /// <example>Aluguel outubro</example>
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 200 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Opcional (ex.: imobiliária, companhia de luz).</summary>
    /// <example>Imobiliária Central</example>
    [StringLength(150, ErrorMessage = "O favorecido deve ter no máximo 150 caracteres.")]
    public string? Favorecido
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <example>2500</example>
    [Range(0.01, 9_999_999_999.99, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal ValorTotal { get; set; }

    /// <example>2026-10-05</example>
    [Required(ErrorMessage = "Informe o vencimento da primeira parcela.")]
    public DateOnly? PrimeiroVencimento { get; set; }

    [Range(1, 12, ErrorMessage = "O número de parcelas deve estar entre 1 e 12.")]
    public int NumeroParcelas { get; set; } = 1;

    [Range(1, 180, ErrorMessage = "O intervalo entre parcelas deve estar entre 1 e 180 dias.")]
    public int IntervaloDias { get; set; } = 30;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (decimal.Round(ValorTotal, 2) != ValorTotal)
            yield return new("O valor deve ter no máximo 2 casas decimais.", [nameof(ValorTotal)]);

        // A menor parcela precisa ser de pelo menos 1 centavo (CHECK valor > 0 no banco).
        if (ValorTotal > 0 && NumeroParcelas > 0 && ValorTotal < NumeroParcelas * 0.01m)
            yield return new("O valor é pequeno demais para esse número de parcelas.", [nameof(ValorTotal)]);
    }
}
