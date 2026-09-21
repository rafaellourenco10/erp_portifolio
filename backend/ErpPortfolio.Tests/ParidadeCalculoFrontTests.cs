// =====================================================================================
// Arquivo....: ParidadeCalculoFrontTests.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Teste de PARIDADE entre o cálculo do servidor (CalculoPedido, decimal) e o da tela
//              (utils/calculoPedido.ts, aritmética inteira). Lê Dados/paridade-calculo.json, um
//              arquivo com ~2.000 pedidos gerados com semente fixa e calculados pelo front, e exige
//              que o servidor dê exatamente os mesmos subtotais e totais (texto com 2 casas).
//              Se um lado mudar a regra de arredondamento, este teste falha.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Dados/paridade-calculo.json (gerado por paridade-calculo.mts, guardado em
//              .claude/ferramentas-locais/), Services/CalculoPedido.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Globalization;
using System.Text.Json;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class ParidadeCalculoFrontTests
{
    private record ItemJson(string Quantidade, string Preco, string Desconto);

    private record CasoJson(List<ItemJson> Itens, string DescontoPedido, List<string> Subtotais, string Total);

    private record ArquivoJson(string ComoGerar, List<CasoJson> Casos);

    private static decimal D(string texto) => decimal.Parse(texto, CultureInfo.InvariantCulture);

    private static string Texto(decimal valor) => valor.ToString("F2", CultureInfo.InvariantCulture);

    private static ArquivoJson Carregar()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Dados", "paridade-calculo.json");
        var json = File.ReadAllText(caminho);
        return JsonSerializer.Deserialize<ArquivoJson>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    [Fact]
    public void Arquivo_de_casos_existe_e_tem_muitos_pedidos()
    {
        var arquivo = Carregar();

        Assert.True(arquivo.Casos.Count >= 2000, $"esperava ao menos 2000 casos, achei {arquivo.Casos.Count}");
        Assert.All(arquivo.Casos, c => Assert.Equal(c.Itens.Count, c.Subtotais.Count));
    }

    [Fact]
    public void Servidor_calcula_exatamente_o_mesmo_que_o_front_em_todos_os_casos()
    {
        var arquivo = Carregar();
        var divergencias = new List<string>();

        for (var i = 0; i < arquivo.Casos.Count; i++)
        {
            var caso = arquivo.Casos[i];
            var subtotais = new List<decimal>();

            for (var j = 0; j < caso.Itens.Count; j++)
            {
                var item = caso.Itens[j];
                var subtotal = CalculoPedido.Subtotal(D(item.Quantidade), D(item.Preco), D(item.Desconto));
                subtotais.Add(subtotal);

                if (Texto(subtotal) != caso.Subtotais[j])
                    divergencias.Add($"caso {i} item {j}: {item.Quantidade} x {item.Preco} com {item.Desconto}% -> servidor {Texto(subtotal)}, front {caso.Subtotais[j]}");
            }

            var total = CalculoPedido.Total(subtotais, D(caso.DescontoPedido));
            if (Texto(total) != caso.Total)
                divergencias.Add($"caso {i} total: soma {Texto(subtotais.Sum())} com {caso.DescontoPedido}% no pedido -> servidor {Texto(total)}, front {caso.Total}");
        }

        Assert.True(divergencias.Count == 0,
            $"{divergencias.Count} divergência(s) entre servidor e front. Primeiras:\n" + string.Join("\n", divergencias.Take(10)));
    }
}
