// =====================================================================================
// Arquivo....: EnderecoFiscal.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Normalização e validação dos campos de endereço usados na NF-e (etapa 16),
//              compartilhadas pelo cadastro de clientes e pelos dados da empresa.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: ClienteCriacaoDto, EmpresaDto.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Api.DTOs.Validacoes;

public static class EnderecoFiscal
{
    /// <summary>Texto vazio vira null; o resto, sem espaços nas pontas.</summary>
    public static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    /// <summary>Sem pontos, traços, barras e espaços, em maiúsculas; letras ficam para a validação recusar.</summary>
    public static string? SemMascara(string? valor) =>
        Opcional(valor) is { } texto ? new string(texto.Where(c => c is not ('.' or '-' or '/' or ' ')).ToArray()).ToUpperInvariant() : null;

    /// <summary>Tem exatamente <paramref name="tamanho"/> dígitos.</summary>
    public static bool Digitos(string? valor, int tamanho) => valor is not null && valor.Length == tamanho && valor.All(char.IsAsciiDigit);

    /// <summary>CEP com 8 dígitos; código IBGE com 7 dígitos começando pelo código da UF.</summary>
    public static IEnumerable<ValidationResult> Validar(string? cep, string? codigoMunicipio, string? uf)
    {
        if (cep is not null && !Digitos(cep, 8))
            yield return new("O CEP deve ter 8 dígitos.", ["Cep"]);

        if (codigoMunicipio is null)
            yield break;

        if (!Digitos(codigoMunicipio, 7))
            yield return new("O código IBGE do município deve ter 7 dígitos.", ["CodigoMunicipio"]);
        else if (uf is not null && NfeCalculo.CodigoUf.TryGetValue(uf.Trim().ToUpperInvariant(), out var codigoUf) && !codigoMunicipio.StartsWith(codigoUf))
            yield return new($"O código IBGE não é de um município de {uf.Trim().ToUpperInvariant()} (deveria começar com {codigoUf}).", ["CodigoMunicipio"]);
    }
}
