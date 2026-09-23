// =====================================================================================
// Arquivo....: VendedorDtos.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: DTOs do cadastro de vendedores: criação, atualização (com "ativo" para
//              reativar), filtro da listagem e resposta. Mesmo desenho dos DTOs de
//              Fornecedor, sem cidade/UF e com a % de comissão (SPEC.md, V1).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usados pelo VendedorService para ler/gravar public.vendedores.
// Fontes.....: Corpo de POST/PUT e query string de GET /api/vendedores.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs.Validacoes;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public class VendedorCriacaoDto : IValidatableObject
{
    /// <example>Ana Souza</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF, com ou sem máscara (CNPJ não é aceito: vendedor é pessoa física).</summary>
    /// <example>529.982.247-25</example>
    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres.")]
    [Cpf]
    public string Cpf { get; set; } = string.Empty;

    // Texto vazio vira null antes da validação: [EmailAddress] rejeitaria "".
    /// <summary>Opcional.</summary>
    /// <example>ana@empresa.com.br</example>
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string? Email
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>Opcional.</summary>
    /// <example>(11) 98765-4321</example>
    [RegularExpression(@"^[0-9()+\-\s]{8,20}$", ErrorMessage = "Telefone inválido.")]
    public string? Telefone
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>% de comissão padrão, de 0 a 100, com até 2 casas.</summary>
    /// <example>5</example>
    // 0.0 e 100.0 (double) de propósito: o overload Range(int, int) deixaria passar 100,01.
    [Range(0.0, 100.0, ErrorMessage = "A comissão deve estar entre 0 e 100%.")]
    public decimal PercentualComissao { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (decimal.Round(PercentualComissao, 2) != PercentualComissao)
            yield return new("A comissão deve ter no máximo 2 casas decimais.", [nameof(PercentualComissao)]);
    }
}

public class VendedorAtualizacaoDto : VendedorCriacaoDto
{
    /// <summary>Opcional. Ausente = mantém o status atual do vendedor.</summary>
    /// <example>true</example>
    public bool? Ativo { get; set; }
}

public class VendedorFiltroDto
{
    /// <summary>Trecho do nome (busca sem diferenciar maiúsculas/minúsculas).</summary>
    [StringLength(150, ErrorMessage = "O filtro de nome deve ter no máximo 150 caracteres.")]
    public string? Nome { get; set; }

    /// <summary>true = só ativos, false = só inativos, ausente = todos.</summary>
    public bool? Ativo { get; set; }

    /// <summary>Número da página, começando em 1.</summary>
    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de registros por página (1 a 100).</summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}

public record VendedorRespostaDto(
    int Id,
    string Nome,
    string Cpf,
    string? Email,
    string? Telefone,
    decimal PercentualComissao,
    bool Ativo,
    DateTime DataCadastro)
{
    public static VendedorRespostaDto DeEntidade(Vendedor vendedor) => new(
        vendedor.Id,
        vendedor.Nome,
        vendedor.Cpf,
        vendedor.Email,
        vendedor.Telefone,
        vendedor.PercentualComissao,
        vendedor.Ativo,
        vendedor.DataCadastro);
}
