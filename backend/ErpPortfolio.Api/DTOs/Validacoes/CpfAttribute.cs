// =====================================================================================
// Arquivo....: CpfAttribute.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Validação de CPF (só pessoa física: CNPJ é recusado), com ou sem máscara.
//              Reaproveita o DocumentoValidador do Cliente; usado no cadastro de
//              vendedores (SPEC.md, V1).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: DocumentoValidador. Testado em ErpPortfolio.Tests/VendedorValidacaoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs.Validacoes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CpfAttribute : ValidationAttribute
{
    public CpfAttribute() : base("CPF inválido.") { }

    // Valores nulos/vazios ficam a cargo do [Required].
    public override bool IsValid(object? value) =>
        value is not string texto
        || string.IsNullOrWhiteSpace(texto)
        || (DocumentoValidador.Normalizar(texto).Length == 11 && DocumentoValidador.EhValido(texto));
}
