// =====================================================================================
// Arquivo....: NfeCalculo.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Contas puras da NF-e simulada (SPEC.md etapa 16), sem banco: CFOP (NF3),
//              alíquota de ICMS interna/interestadual, rateio do desconto do pedido entre os
//              itens (NF2), ICMS/PIS/COFINS por item e totais (NF4) e a chave de acesso de 44
//              posições com o dígito verificador módulo 11, aceitando CNPJ alfanumérico (NF5).
//              Arredondamento a 2 casas, meio para cima, igual ao CalculoPedido.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Usado pelo NotaFiscalService. Testado em NfeCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public static class NfeCalculo
{
    public const decimal AliquotaPis = 1.65m;
    public const decimal AliquotaCofins = 7.6m;

    /// <summary>Código IBGE de cada UF (o cUF da chave e do XML).</summary>
    public static readonly IReadOnlyDictionary<string, string> CodigoUf = new Dictionary<string, string>
    {
        ["RO"] = "11", ["AC"] = "12", ["AM"] = "13", ["RR"] = "14", ["PA"] = "15", ["AP"] = "16", ["TO"] = "17",
        ["MA"] = "21", ["PI"] = "22", ["CE"] = "23", ["RN"] = "24", ["PB"] = "25", ["PE"] = "26", ["AL"] = "27",
        ["SE"] = "28", ["BA"] = "29", ["MG"] = "31", ["ES"] = "32", ["RJ"] = "33", ["SP"] = "35", ["PR"] = "41",
        ["SC"] = "42", ["RS"] = "43", ["MS"] = "50", ["MT"] = "51", ["GO"] = "52", ["DF"] = "53"
    };

    // ponytail: alíquota modal de cada UF em 2025, sem FCP; atualizar à mão se algum estado mudar.
    private static readonly Dictionary<string, decimal> AliquotaInterna = new()
    {
        ["AC"] = 19, ["AL"] = 19, ["AP"] = 18, ["AM"] = 20, ["BA"] = 20.5m, ["CE"] = 20, ["DF"] = 20,
        ["ES"] = 17, ["GO"] = 19, ["MA"] = 23, ["MT"] = 17, ["MS"] = 17, ["MG"] = 18, ["PA"] = 19,
        ["PB"] = 20, ["PR"] = 19.5m, ["PE"] = 20.5m, ["PI"] = 22.5m, ["RJ"] = 20, ["RN"] = 20, ["RS"] = 17,
        ["RO"] = 19.5m, ["RR"] = 20, ["SC"] = 17, ["SP"] = 18, ["SE"] = 20, ["TO"] = 20
    };

    /// <summary>Sul e Sudeste menos o ES: daqui para as outras UFs a alíquota é 7% (Resolução do Senado 22/89).</summary>
    private static readonly HashSet<string> SulSudesteSemEs = ["SP", "RJ", "MG", "PR", "SC", "RS"];

    /// <summary>Item antes do cálculo: quantidade, preço e o valor já com o desconto do próprio item.</summary>
    public record ItemEntrada(decimal Quantidade, decimal ValorUnitario, decimal ValorLiquido);

    /// <summary>Item calculado. ValorBruto = vProd; ValorDesconto = vDesc (item + rateio do pedido); BaseIcms = bruto − desconto.</summary>
    public record ItemCalculado(
        decimal ValorBruto, decimal ValorDesconto, decimal BaseIcms,
        decimal AliquotaIcms, decimal ValorIcms, decimal ValorPis, decimal ValorCofins);

    public record Totais(
        decimal ValorProdutos, decimal ValorDesconto, decimal BaseIcms,
        decimal ValorIcms, decimal ValorPis, decimal ValorCofins, decimal ValorTotal);

    public static bool UfValida(string? uf) => uf is not null && CodigoUf.ContainsKey(uf);

    /// <summary>NF3: 5102/6102 na venda, 1202/2202 na devolução.</summary>
    public static string Cfop(bool devolucao, string ufEmitente, string ufDestinatario) =>
        (devolucao, ufEmitente == ufDestinatario) switch
        {
            (false, true) => "5102",
            (false, false) => "6102",
            (true, true) => "1202",
            (true, false) => "2202"
        };

    /// <summary>Mesma UF: alíquota interna do emitente. Outra UF: 7% do Sul/Sudeste (menos ES) para as demais; senão 12%.</summary>
    public static decimal AliquotaIcms(string ufEmitente, string ufDestinatario)
    {
        if (ufEmitente == ufDestinatario)
            return AliquotaInterna[ufEmitente];

        return SulSudesteSemEs.Contains(ufEmitente) && !SulSudesteSemEs.Contains(ufDestinatario) ? 7m : 12m;
    }

    /// <summary>
    /// NF2 + NF4: reparte <paramref name="valorTotal"/> entre os itens na proporção do valor líquido de cada um
    /// (a sobra de centavos fica no último), e calcula os impostos sobre a parte de cada item.
    /// Na venda, <paramref name="valorTotal"/> é o total do pedido; na devolução, a soma dos itens (sem rateio).
    /// </summary>
    public static IReadOnlyList<ItemCalculado> Itens(IReadOnlyList<ItemEntrada> itens, decimal valorTotal, decimal aliquotaIcms)
    {
        var somaLiquidos = itens.Sum(i => i.ValorLiquido);
        var resultado = new List<ItemCalculado>(itens.Count);
        var jaRepartido = 0m;

        for (var i = 0; i < itens.Count; i++)
        {
            var item = itens[i];
            var bruto = Arredondar(item.Quantidade * item.ValorUnitario);
            var parte = i == itens.Count - 1
                ? valorTotal - jaRepartido
                : somaLiquidos == 0 ? 0 : Arredondar(item.ValorLiquido * valorTotal / somaLiquidos);
            jaRepartido += parte;

            resultado.Add(new ItemCalculado(
                bruto, bruto - parte, parte, aliquotaIcms,
                Arredondar(parte * aliquotaIcms / 100),
                Arredondar(parte * AliquotaPis / 100),
                Arredondar(parte * AliquotaCofins / 100)));
        }

        return resultado;
    }

    public static Totais Somar(IEnumerable<ItemCalculado> itens)
    {
        var lista = itens.ToList();
        var produtos = lista.Sum(i => i.ValorBruto);
        var desconto = lista.Sum(i => i.ValorDesconto);
        return new Totais(
            produtos, desconto, lista.Sum(i => i.BaseIcms), lista.Sum(i => i.ValorIcms),
            lista.Sum(i => i.ValorPis), lista.Sum(i => i.ValorCofins), produtos - desconto);
    }

    /// <summary>NF5: cUF + AAMM + CNPJ + 55 + série + número + tpEmis 1 + código numérico + DV.</summary>
    public static string Chave(string uf, DateTime emissao, string cnpj, int serie, int numero, int codigoNumerico)
    {
        var semDv = $"{CodigoUf[uf]}{emissao:yyMM}{cnpj}55{serie:D3}{numero:D9}1{codigoNumerico:D8}";
        return semDv + DigitoVerificador(semDv);
    }

    /// <summary>Módulo 11 com pesos 2 a 9 da direita para a esquerda; resto 0 ou 1 → 0. Letras valem ASCII − 48.</summary>
    public static int DigitoVerificador(string chaveSemDv)
    {
        var soma = 0;
        var peso = 2;
        for (var i = chaveSemDv.Length - 1; i >= 0; i--)
        {
            soma += (chaveSemDv[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public static bool NcmValido(string? ncm) => ncm is { Length: 8 } && ncm.All(char.IsAsciiDigit);

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
