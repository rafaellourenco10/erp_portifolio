// =====================================================================================
// Arquivo....: DadosFiscaisValidacaoTests.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Validação dos dados fiscais da etapa 16: endereço/IE do cliente, NCM do
//              produto e dados da empresa emitente (CEP, código IBGE × UF, CNPJ).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: ClienteCriacaoDto, ProdutoCriacaoDto, EmpresaDto, Validacoes/EnderecoFiscal.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Tests;

public class DadosFiscaisValidacaoTests
{
    private static List<string> Erros(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        // Como no [ApiController]: o Validate só roda se os atributos passarem.
        if (resultados.Count == 0 && objeto is IValidatableObject validavel)
            resultados.AddRange(validavel.Validate(new ValidationContext(objeto)));
        return resultados.Select(r => r.ErrorMessage!).ToList();
    }

    private static ClienteCriacaoDto Cliente() => new()
    {
        Nome = "Maria da Silva", Documento = "529.982.247-25", Cidade = "São Paulo", Uf = "SP"
    };

    private static EmpresaDto Empresa() => new()
    {
        RazaoSocial = "Ambition Ltda", Cnpj = "11.222.333/0001-81", InscricaoEstadual = "110.042.490.114",
        Logradouro = "Av. Paulista", Numero = "1000", Bairro = "Bela Vista", Cep = "01310-100",
        Municipio = "São Paulo", CodigoMunicipio = "3550308", Uf = "sp"
    };

    [Fact]
    public void Cliente_sem_dados_fiscais_continua_valido() => Assert.Empty(Erros(Cliente()));

    [Fact]
    public void Cliente_normaliza_cep_ie_e_vazios()
    {
        var cliente = Cliente();
        cliente.Cep = "01310-100";
        cliente.InscricaoEstadual = " isento ";
        cliente.Complemento = "  ";
        cliente.CodigoMunicipio = "3550308";

        Assert.Empty(Erros(cliente));
        Assert.Equal("01310100", cliente.Cep);
        Assert.Equal("ISENTO", cliente.InscricaoEstadual);
        Assert.Null(cliente.Complemento);
    }

    [Theory]
    [InlineData("0131010", null, null, "O CEP deve ter 8 dígitos.")]
    [InlineData(null, "355030", null, "O código IBGE do município deve ter 7 dígitos.")]
    [InlineData(null, "3304557", null, "O código IBGE não é de um município de SP (deveria começar com 35).")]
    [InlineData(null, null, "12.A", "Inscrição Estadual inválida: use de 2 a 14 dígitos ou ISENTO.")]
    public void Cliente_rejeita_dado_fiscal_invalido(string? cep, string? ibge, string? ie, string erro)
    {
        var cliente = Cliente();
        cliente.Cep = cep;
        cliente.CodigoMunicipio = ibge;
        cliente.InscricaoEstadual = ie;

        Assert.Contains(erro, Erros(cliente));
    }

    [Theory]
    [InlineData("8544.42.00", true)]
    [InlineData("854442", false)]
    [InlineData(null, true)]
    public void Ncm_do_produto_e_opcional_mas_tem_8_digitos(string? ncm, bool valido)
    {
        var produto = new ProdutoCriacaoDto
        {
            Nome = "Cabo HDMI", Sku = "10012345", Unidade = "UN", PrecoVenda = 10, Custo = 5, EstoqueMinimo = 0, Ncm = ncm
        };

        Assert.Equal(valido, Erros(produto).Count == 0);
    }

    [Fact]
    public void Empresa_completa_e_valida_e_normalizada()
    {
        var empresa = Empresa();

        Assert.Empty(Erros(empresa));
        Assert.Equal("11222333000181", empresa.Cnpj);
        Assert.Equal("110042490114", empresa.InscricaoEstadual);
        Assert.Equal("SP", empresa.Uf);
    }

    [Fact]
    public void Empresa_exige_cnpj_e_nao_cpf()
    {
        var empresa = Empresa();
        empresa.Cnpj = "529.982.247-25";

        Assert.Contains("O emitente precisa de CNPJ (14 caracteres).", Erros(empresa));
    }

    [Fact]
    public void Empresa_confere_ibge_com_a_uf()
    {
        var empresa = Empresa();
        empresa.Uf = "RJ";

        Assert.Contains("O código IBGE não é de um município de RJ (deveria começar com 33).", Erros(empresa));
    }
}
