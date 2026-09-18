// =====================================================================================
// Arquivo....: CpfCnpjAttribute.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: Atributo de validação (DataAnnotations) que aceita CPF ou CNPJ válidos,
//              com ou sem máscara. Delega a regra para DocumentoValidador.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco de dados.
// Tabelas....: Nenhuma.
// Fontes.....: DocumentoValidador.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs.Validacoes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CpfCnpjAttribute : ValidationAttribute
{
    public CpfCnpjAttribute() : base("Documento inválido. Informe um CPF ou CNPJ válido.") { }

    // Valores nulos/vazios ficam a cargo do [Required].
    public override bool IsValid(object? value) =>
        value is not string texto || string.IsNullOrWhiteSpace(texto) || DocumentoValidador.EhValido(texto);
}
