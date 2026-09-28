// =====================================================================================
// Arquivo....: ExportadorDanfe.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: DANFE (Documento Auxiliar da NF-e) em PDF, montado só a partir do XML gravado
//              (etapa 16): emitente, destinatário e valores são os da emissão (NF8). Retrato
//              A4 nos blocos do DANFE tradicional, com a marca "SEM VALOR FISCAL".
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: QuestPDF (já usado nos relatórios). Usado pelo NotaFiscalService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Globalization;
using System.Xml.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ErpPortfolio.Api.Services;

public static class ExportadorDanfe
{
    // ponytail: sem código de barras Code128 da chave (QuestPDF não traz); a chave vai em texto. Desenhar em SVG se pedirem.
    public static byte[] GerarPdf(string xml)
    {
        var ns = NfeXml.Ns;
        var raiz = XDocument.Parse(xml).Root!;
        var inf = raiz.Element(ns + "NFe")!.Element(ns + "infNFe")!;
        var ide = inf.Element(ns + "ide")!;
        var emit = inf.Element(ns + "emit")!;
        var dest = inf.Element(ns + "dest")!;
        var tot = inf.Element(ns + "total")!.Element(ns + "ICMSTot")!;
        var prot = raiz.Element(ns + "protNFe")!.Element(ns + "infProt")!;
        var chave = prot.Element(ns + "chNFe")!.Value;
        var ptBr = FormatoRelatorioTexto.PtBr;

        string V(XElement? pai, string nome) => pai?.Element(ns + nome)?.Value ?? "";
        string Dinheiro(XElement pai, string nome) =>
            decimal.Parse(V(pai, nome) is { Length: > 0 } t ? t : "0", CultureInfo.InvariantCulture).ToString("N2", ptBr);
        string Numero(string texto, string formato) =>
            decimal.Parse(texto, CultureInfo.InvariantCulture).ToString(formato, ptBr);
        string Cep(XElement e) => V(e, "CEP") is { Length: 8 } cep ? $"{cep[..5]}-{cep[5..]}" : V(e, "CEP");
        string Endereco(XElement e) =>
            $"{V(e, "xLgr")}, {V(e, "nro")}{(V(e, "xCpl") is { Length: > 0 } c ? " - " + c : "")}";

        var dhEmi = DateTimeOffset.Parse(V(ide, "dhEmi"), CultureInfo.InvariantCulture);
        var dhProt = DateTimeOffset.Parse(V(prot, "dhRecbto"), CultureInfo.InvariantCulture);
        var enderEmit = emit.Element(ns + "enderEmit")!;
        var enderDest = dest.Element(ns + "enderDest")!;
        var documentoDest = V(dest, "CNPJ") is { Length: > 0 } cnpj ? cnpj : V(dest, "CPF");
        var chaveFormatada = string.Join(" ", Enumerable.Range(0, 11).Select(i => chave.Substring(i * 4, 4)));
        var numero = int.Parse(V(ide, "nNF")).ToString("000,000,000", CultureInfo.InvariantCulture).Replace(',', '.');

        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(1, Unit.Centimetre);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(8));

            pagina.Foreground().AlignCenter().AlignMiddle().Rotate(-35)
                .Text("SEM VALOR FISCAL").FontSize(60).Bold().FontColor("#33B91C1C");

            pagina.Content().Column(c =>
            {
                c.Spacing(4);

                // Cabeçalho: emitente | DANFE | chave e protocolo.
                c.Item().Border(0.5f).Row(linha =>
                {
                    linha.RelativeItem(4).Padding(4).Column(e =>
                    {
                        e.Item().Text(V(emit, "xNome")).FontSize(10).Bold();
                        e.Item().Text(Endereco(enderEmit));
                        e.Item().Text($"{V(enderEmit, "xBairro")} - CEP {Cep(enderEmit)}");
                        e.Item().Text($"{V(enderEmit, "xMun")} - {V(enderEmit, "UF")}");
                    });
                    linha.RelativeItem(2).BorderLeft(0.5f).BorderRight(0.5f).Padding(4).AlignCenter().Column(d =>
                    {
                        d.Item().AlignCenter().Text("DANFE").FontSize(12).Bold();
                        d.Item().AlignCenter().Text("Documento Auxiliar da Nota Fiscal Eletrônica").FontSize(6);
                        d.Item().AlignCenter().Text($"0 - Entrada   1 - Saída   [ {V(ide, "tpNF")} ]");
                        d.Item().AlignCenter().Text($"Nº {numero}").Bold();
                        d.Item().AlignCenter().Text($"Série {V(ide, "serie").PadLeft(3, '0')}");
                        d.Item().AlignCenter().Text(t => { t.Span("Folha "); t.CurrentPageNumber(); t.Span("/"); t.TotalPages(); });
                    });
                    linha.RelativeItem(4).Padding(4).Column(k =>
                    {
                        Campo(k.Item(), "CHAVE DE ACESSO", chaveFormatada, negrito: true);
                        k.Item().PaddingTop(4).Text("Consulta de autenticidade no portal nacional da NF-e (ambiente de homologação: documento de teste).").FontSize(6);
                    });
                });

                c.Item().Border(0.5f).Row(linha =>
                {
                    Campo(linha.RelativeItem(5).Padding(3), "NATUREZA DA OPERAÇÃO", V(ide, "natOp"));
                    Campo(linha.RelativeItem(5).BorderLeft(0.5f).Padding(3), "PROTOCOLO DE AUTORIZAÇÃO DE USO",
                        $"{V(prot, "nProt")} - {dhProt:dd/MM/yyyy HH:mm:ss}");
                });
                c.Item().Border(0.5f).Row(linha =>
                {
                    Campo(linha.RelativeItem().Padding(3), "INSCRIÇÃO ESTADUAL", V(emit, "IE"));
                    Campo(linha.RelativeItem().BorderLeft(0.5f).Padding(3), "CNPJ", ExportadorOrcamento.FormatarDocumento(V(emit, "CNPJ")));
                });

                Titulo(c.Item(), "DESTINATÁRIO / REMETENTE");
                c.Item().Border(0.5f).Column(d =>
                {
                    d.Item().Row(linha =>
                    {
                        Campo(linha.RelativeItem(6).Padding(3), "NOME / RAZÃO SOCIAL", V(dest, "xNome"));
                        Campo(linha.RelativeItem(3).BorderLeft(0.5f).Padding(3), "CNPJ / CPF", ExportadorOrcamento.FormatarDocumento(documentoDest));
                        Campo(linha.RelativeItem(2).BorderLeft(0.5f).Padding(3), "DATA DA EMISSÃO", dhEmi.ToString("dd/MM/yyyy"));
                    });
                    d.Item().BorderTop(0.5f).Row(linha =>
                    {
                        Campo(linha.RelativeItem(5).Padding(3), "ENDEREÇO", Endereco(enderDest));
                        Campo(linha.RelativeItem(3).BorderLeft(0.5f).Padding(3), "BAIRRO", V(enderDest, "xBairro"));
                        Campo(linha.RelativeItem(2).BorderLeft(0.5f).Padding(3), "CEP", Cep(enderDest));
                    });
                    d.Item().BorderTop(0.5f).Row(linha =>
                    {
                        Campo(linha.RelativeItem(5).Padding(3), "MUNICÍPIO", V(enderDest, "xMun"));
                        Campo(linha.RelativeItem(1).BorderLeft(0.5f).Padding(3), "UF", V(enderDest, "UF"));
                        Campo(linha.RelativeItem(4).BorderLeft(0.5f).Padding(3), "INSCRIÇÃO ESTADUAL", V(dest, "IE"));
                    });
                });

                Titulo(c.Item(), "CÁLCULO DO IMPOSTO");
                c.Item().Border(0.5f).Row(linha =>
                {
                    (string Rotulo, string No)[] campos =
                    [
                        ("BASE DE CÁLC. DO ICMS", "vBC"), ("VALOR DO ICMS", "vICMS"), ("VALOR TOTAL DOS PRODUTOS", "vProd"),
                        ("DESCONTO", "vDesc"), ("VALOR DO PIS", "vPIS"), ("VALOR DA COFINS", "vCOFINS"), ("VALOR TOTAL DA NOTA", "vNF")
                    ];
                    for (var i = 0; i < campos.Length; i++)
                    {
                        var item = linha.RelativeItem();
                        if (i > 0) item = item.BorderLeft(0.5f);
                        Campo(item.Padding(3), campos[i].Rotulo, Dinheiro(tot, campos[i].No), direita: true, negrito: campos[i].No == "vNF");
                    }
                });

                Titulo(c.Item(), "DADOS DOS PRODUTOS / SERVIÇOS");
                c.Item().Table(tabela =>
                {
                    tabela.ColumnsDefinition(col =>
                    {
                        foreach (var largura in new[] { 1.3f, 4f, 1.1f, 0.6f, 0.7f, 0.5f, 1f, 1.2f, 1.1f, 1.3f, 1.2f, 1.1f, 0.8f })
                            col.RelativeColumn(largura);
                    });
                    string[] titulos = ["CÓDIGO", "DESCRIÇÃO", "NCM", "CST", "CFOP", "UN", "QTD", "V. UNIT.", "DESC.", "V. TOTAL", "BC ICMS", "V. ICMS", "ALÍQ."];
                    tabela.Header(h =>
                    {
                        for (var i = 0; i < titulos.Length; i++)
                            h.Cell().Border(0.5f).Background(Colors.Grey.Lighten3).Padding(2)
                                .Text(titulos[i]).FontSize(6).Bold();
                    });

                    foreach (var det in inf.Elements(ns + "det"))
                    {
                        var prod = det.Element(ns + "prod")!;
                        var icms = det.Element(ns + "imposto")!.Element(ns + "ICMS")!.Elements().First();
                        string[] valores =
                        [
                            V(prod, "cProd"), V(prod, "xProd"), V(prod, "NCM"), V(icms, "orig") + V(icms, "CST"), V(prod, "CFOP"),
                            V(prod, "uCom"), Numero(V(prod, "qCom"), "#,##0.###"), Numero(V(prod, "vUnCom"), "N2"),
                            Dinheiro(prod, "vDesc"), Dinheiro(prod, "vProd"), Dinheiro(icms, "vBC"), Dinheiro(icms, "vICMS"),
                            Numero(V(icms, "pICMS"), "0.##")
                        ];
                        for (var i = 0; i < valores.Length; i++)
                        {
                            var celula = tabela.Cell().BorderLeft(0.5f).BorderRight(0.5f).BorderBottom(0.25f).BorderColor(Colors.Grey.Medium).Padding(2);
                            (i >= 6 ? celula.AlignRight() : celula).Text(valores[i]).FontSize(7);
                        }
                    }
                });

                Titulo(c.Item(), "DADOS ADICIONAIS");
                c.Item().Border(0.5f).MinHeight(50).Padding(3).Column(a =>
                {
                    a.Item().Text("INFORMAÇÕES COMPLEMENTARES").FontSize(6).FontColor(Colors.Grey.Darken1);
                    a.Item().Text(V(inf.Element(ns + "infAdic"), "infCpl"));
                });
            });
        })).GeneratePdf();
    }

    private static void Titulo(IContainer container, string texto) =>
        container.PaddingTop(4).Text(texto).FontSize(7).Bold();

    private static void Campo(IContainer container, string rotulo, string valor, bool direita = false, bool negrito = false) =>
        container.Column(col =>
        {
            col.Item().Text(rotulo).FontSize(6).FontColor(Colors.Grey.Darken1);
            var texto = (direita ? col.Item().AlignRight() : col.Item()).Text(valor).FontSize(8);
            if (negrito) texto.Bold();
        });
}
