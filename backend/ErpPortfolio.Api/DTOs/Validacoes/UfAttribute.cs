// =====================================================================================
// Arquivo....: UfAttribute.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: Atributo de validação que aceita apenas siglas de UF brasileiras
//              (maiúsculas ou minúsculas).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco de dados.
// Tabelas....: Nenhuma (o valor é gravado em public.clientes.uf).
// Fontes.....: Lista fixa das 27 unidades federativas.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs.Validacoes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class UfAttribute : ValidationAttribute
{
    private static readonly HashSet<string> UfsValidas =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public UfAttribute() : base("UF inválida.") { }

    // Valores nulos/vazios ficam a cargo do [Required].
    public override bool IsValid(object? value) =>
        value is not string texto || string.IsNullOrWhiteSpace(texto) || UfsValidas.Contains(texto.Trim().ToUpperInvariant());
}
