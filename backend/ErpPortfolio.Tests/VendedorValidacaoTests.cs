// =====================================================================================
// Arquivo....: VendedorValidacaoTests.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Testes da validação de CPF só-pessoa-física ([Cpf]) usada no cadastro de
//              vendedores (SPEC.md, V1).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: DTOs/Validacoes/CpfAttribute.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

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
}
