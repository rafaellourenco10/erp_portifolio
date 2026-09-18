// =====================================================================================
// Arquivo....: DocumentoValidador.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: Normalização e validação de CPF e CNPJ (dígitos verificadores).
//              Suporta o CNPJ alfanumérico (12 caracteres [0-9A-Z] + 2 dígitos),
//              em que o valor de cada caractere é (código ASCII - 48).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco de dados.
// Tabelas....: Nenhuma (o valor normalizado é gravado em public.clientes.documento).
// Fontes.....: Regras da Receita Federal para cálculo de DV de CPF/CNPJ.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.RegularExpressions;

namespace ErpPortfolio.Api.DTOs.Validacoes;

public static partial class DocumentoValidador
{
    private static readonly int[] PesosCnpjPrimeiroDigito = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] PesosCnpjSegundoDigito = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    [GeneratedRegex("[^0-9A-Za-z]")]
    private static partial Regex CaracteresDeMascara();

    [GeneratedRegex("^[0-9A-Z]{12}[0-9]{2}$")]
    private static partial Regex FormatoCnpj();

    /// <summary>Remove máscara (pontos, barras, traços e espaços) e converte para maiúsculas.</summary>
    public static string Normalizar(string documento) =>
        CaracteresDeMascara().Replace(documento, string.Empty).ToUpperInvariant();

    public static bool EhValido(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return false;

        var normalizado = Normalizar(documento);

        return normalizado.Length switch
        {
            11 => EhCpfValido(normalizado),
            14 => EhCnpjValido(normalizado),
            _ => false
        };
    }

    private static bool EhCpfValido(string cpf)
    {
        if (!cpf.All(char.IsAsciiDigit) || cpf.Distinct().Count() == 1)
            return false;

        var digitos = cpf.Select(c => c - '0').ToArray();

        var primeiroDv = CalcularDvCpf(digitos, 9);
        var segundoDv = CalcularDvCpf(digitos, 10);

        return digitos[9] == primeiroDv && digitos[10] == segundoDv;
    }

    private static int CalcularDvCpf(int[] digitos, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += digitos[i] * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static bool EhCnpjValido(string cnpj)
    {
        if (!FormatoCnpj().IsMatch(cnpj) || cnpj.Distinct().Count() == 1)
            return false;

        var valores = cnpj.Select(c => c - '0').ToArray();

        var primeiroDv = CalcularDvCnpj(valores, PesosCnpjPrimeiroDigito);
        var segundoDv = CalcularDvCnpj(valores, PesosCnpjSegundoDigito);

        return valores[12] == primeiroDv && valores[13] == segundoDv;
    }

    private static int CalcularDvCnpj(int[] valores, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < pesos.Length; i++)
            soma += valores[i] * pesos[i];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
