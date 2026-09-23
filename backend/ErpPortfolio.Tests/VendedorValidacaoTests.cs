// =====================================================================================
// Arquivo....: VendedorValidacaoTests.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Testes da validação de CPF só-pessoa-física ([Cpf]) e da % de comissão do
//              cadastro de vendedores (SPEC.md, V1).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: DTOs/Validacoes/CpfAttribute.cs, DTOs/VendedorDtos.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo (CPF na T1; % de comissão do DTO na T2).
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.DTOs.Validacoes;

namespace ErpPortfolio.Tests;

public class VendedorValidacaoTests
{
    private readonly CpfAttribute cpf = new();

    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void Cpf_valido_com_ou_sem_mascara_e_aceito(string valor) => Assert.True(cpf.IsValid(valor));

    [Theory]
    [InlineData("52998224724")] // dígito verificador errado
    [InlineData("11111111111")] // todos iguais
    [InlineData("11.222.333/0001-81")] // CNPJ válido, mas não é CPF
    [InlineData("1234")]
    public void Cpf_invalido_ou_cnpj_e_recusado(string valor) => Assert.False(cpf.IsValid(valor));

    [Fact]
    public void Vazio_fica_para_o_Required() => Assert.True(cpf.IsValid(""));

    [Theory]
    [InlineData("0")]
    [InlineData("5.55")]
    [InlineData("100")]
    public void Comissao_de_0_a_100_com_2_casas_e_valida(string percentual) =>
        Assert.Empty(Validar(Vendedor(decimal.Parse(percentual, CultureInfo.InvariantCulture))));

    [Theory]
    [InlineData("-1")]
    [InlineData("100.01")]
    [InlineData("5.555")]
    public void Comissao_fora_da_faixa_ou_com_3_casas_e_recusada(string percentual) =>
        Assert.Contains(Validar(Vendedor(decimal.Parse(percentual, CultureInfo.InvariantCulture))),
            e => e.MemberNames.Contains(nameof(VendedorCriacaoDto.PercentualComissao)));

    [Fact]
    public void Cnpj_no_cadastro_do_vendedor_e_recusado() =>
        Assert.Contains(Validar(new VendedorCriacaoDto { Nome = "Ana Souza", Cpf = "11.222.333/0001-81" }),
            e => e.MemberNames.Contains(nameof(VendedorCriacaoDto.Cpf)));

    private static VendedorCriacaoDto Vendedor(decimal percentual) =>
        new() { Nome = "Ana Souza", Cpf = "529.982.247-25", PercentualComissao = percentual };

    private static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }
}
