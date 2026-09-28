// =====================================================================================
// Arquivo....: EmpresaDto.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Dados da empresa emitente (etapa 16), iguais na leitura e na gravação
//              (GET/PUT /api/empresa).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: EmpresaController, EmpresaService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs.Validacoes;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public class EmpresaDto : IValidatableObject
{
    /// <example>Ambition Comércio de Eletrônicos Ltda</example>
    [Required(ErrorMessage = "A razão social é obrigatória.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "A razão social deve ter entre 3 e 60 caracteres.")]
    public string RazaoSocial { get; set => field = value?.Trim() ?? string.Empty; } = string.Empty;

    /// <example>Ambition</example>
    [StringLength(60, ErrorMessage = "O nome fantasia deve ter no máximo 60 caracteres.")]
    public string? NomeFantasia { get; set => field = EnderecoFiscal.Opcional(value); }

    /// <summary>Com ou sem máscara; aceita o CNPJ alfanumérico.</summary>
    /// <example>11.222.333/0001-81</example>
    [Required(ErrorMessage = "O CNPJ é obrigatório.")]
    [CpfCnpj]
    public string Cnpj { get; set => field = DocumentoValidador.Normalizar(value ?? string.Empty); } = string.Empty;

    /// <example>110042490114</example>
    [Required(ErrorMessage = "A Inscrição Estadual é obrigatória.")]
    [RegularExpression("^[0-9]{2,14}$", ErrorMessage = "A Inscrição Estadual deve ter de 2 a 14 dígitos.")]
    public string InscricaoEstadual { get; set => field = EnderecoFiscal.SemMascara(value) ?? string.Empty; } = string.Empty;

    /// <example>Avenida Paulista</example>
    [Required(ErrorMessage = "O logradouro é obrigatório.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "O logradouro deve ter entre 2 e 60 caracteres.")]
    public string Logradouro { get; set => field = value?.Trim() ?? string.Empty; } = string.Empty;

    /// <example>1000</example>
    [Required(ErrorMessage = "O número é obrigatório.")]
    [StringLength(10, ErrorMessage = "O número deve ter no máximo 10 caracteres.")]
    public string Numero { get; set => field = value?.Trim() ?? string.Empty; } = string.Empty;

    [StringLength(60, ErrorMessage = "O complemento deve ter no máximo 60 caracteres.")]
    public string? Complemento { get; set => field = EnderecoFiscal.Opcional(value); }

    /// <example>Bela Vista</example>
    [Required(ErrorMessage = "O bairro é obrigatório.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "O bairro deve ter entre 2 e 60 caracteres.")]
    public string Bairro { get; set => field = value?.Trim() ?? string.Empty; } = string.Empty;

    /// <example>01310-100</example>
    [Required(ErrorMessage = "O CEP é obrigatório.")]
    public string Cep { get; set => field = EnderecoFiscal.SemMascara(value) ?? string.Empty; } = string.Empty;

    /// <example>São Paulo</example>
    [Required(ErrorMessage = "O município é obrigatório.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "O município deve ter entre 2 e 60 caracteres.")]
    public string Municipio { get; set => field = value?.Trim() ?? string.Empty; } = string.Empty;

    /// <example>3550308</example>
    [Required(ErrorMessage = "O código IBGE do município é obrigatório.")]
    public string CodigoMunicipio { get; set => field = EnderecoFiscal.SemMascara(value) ?? string.Empty; } = string.Empty;

    /// <example>SP</example>
    [Required(ErrorMessage = "A UF é obrigatória.")]
    [Uf]
    public string Uf { get; set => field = value?.Trim().ToUpperInvariant() ?? string.Empty; } = string.Empty;

    [RegularExpression(@"^[0-9()+\-\s]{8,20}$", ErrorMessage = "Telefone inválido.")]
    public string? Telefone { get; set => field = EnderecoFiscal.Opcional(value); }

    /// <example>1</example>
    [Range(0, 999, ErrorMessage = "A série deve estar entre 0 e 999.")]
    public int SerieNfe { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Cnpj.Length != 14)
            yield return new("O emitente precisa de CNPJ (14 caracteres).", [nameof(Cnpj)]);

        foreach (var erro in EnderecoFiscal.Validar(Cep, CodigoMunicipio, Uf))
            yield return erro;
    }

    public static EmpresaDto DeEntidade(Empresa e) => new()
    {
        RazaoSocial = e.RazaoSocial, NomeFantasia = e.NomeFantasia, Cnpj = e.Cnpj, InscricaoEstadual = e.InscricaoEstadual,
        Logradouro = e.Logradouro, Numero = e.Numero, Complemento = e.Complemento, Bairro = e.Bairro, Cep = e.Cep,
        Municipio = e.Municipio, CodigoMunicipio = e.CodigoMunicipio, Uf = e.Uf, Telefone = e.Telefone, SerieNfe = e.SerieNfe
    };

    public void Aplicar(Empresa e)
    {
        e.RazaoSocial = RazaoSocial; e.NomeFantasia = NomeFantasia; e.Cnpj = Cnpj; e.InscricaoEstadual = InscricaoEstadual;
        e.Logradouro = Logradouro; e.Numero = Numero; e.Complemento = Complemento; e.Bairro = Bairro; e.Cep = Cep;
        e.Municipio = Municipio; e.CodigoMunicipio = CodigoMunicipio; e.Uf = Uf; e.Telefone = Telefone; e.SerieNfe = SerieNfe;
    }
}
