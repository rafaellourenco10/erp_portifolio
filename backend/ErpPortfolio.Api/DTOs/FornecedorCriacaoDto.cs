// =====================================================================================
// Arquivo....: FornecedorCriacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de entrada para inclusão de fornecedor (POST /api/fornecedores),
//              com as regras de validação via DataAnnotations. Espelho de
//              ClienteCriacaoDto (SPEC.md, F1).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.fornecedores pelo FornecedorService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs.Validacoes;

namespace ErpPortfolio.Api.DTOs;

public class FornecedorCriacaoDto
{
    /// <example>Distribuidora ABC Ltda</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF ou CNPJ, com ou sem máscara.</summary>
    /// <example>12.345.678/0001-90</example>
    [Required(ErrorMessage = "O documento (CPF/CNPJ) é obrigatório.")]
    [StringLength(18, ErrorMessage = "O documento deve ter no máximo 18 caracteres.")]
    [CpfCnpj]
    public string Documento { get; set; } = string.Empty;

    // Texto vazio vira null antes da validação: [EmailAddress] rejeitaria "".
    /// <summary>Opcional.</summary>
    /// <example>contato@abc.com.br</example>
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string? Email
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>Opcional.</summary>
    /// <example>(11) 3456-7890</example>
    [RegularExpression(@"^[0-9()+\-\s]{8,20}$", ErrorMessage = "Telefone inválido.")]
    public string? Telefone
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <example>São Paulo</example>
    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A cidade deve ter entre 2 e 100 caracteres.")]
    public string Cidade { get; set; } = string.Empty;

    /// <example>SP</example>
    [Required(ErrorMessage = "A UF é obrigatória.")]
    [Uf]
    public string Uf { get; set; } = string.Empty;
}
