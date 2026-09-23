// =====================================================================================
// Arquivo....: ExportadorRelatorio.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Gera o arquivo de um RelatorioModelo: Excel (.xlsx, ClosedXML) ou PDF
//              (QuestPDF). O mesmo modelo serve aos três relatórios e aos dois formatos.
//              Excel: título, "gerado em", filtros, resumo (rótulo | valor) e a tabela com
//              cabeçalho destacado, filtro automático e valores tipados. PDF: A4 (paisagem
//              quando há mais de 6 colunas), cabeçalho com filtros e cards de resumo,
//              tabela com cabeçalho repetido a cada página e rodapé "Página X de Y".
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: ClosedXML (MIT), QuestPDF (licença Community, declarada no Program.cs).
//              Chamado pelo RelatoriosController.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo (Excel).
//   1.1.0 - 23/09/2026 - GerarPdf (T3).
// =====================================================================================

using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ErpPortfolio.Api.Services;

public static class ExportadorRelatorio
{
    public const string TipoConteudoXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string TipoConteudoPdf = "application/pdf";

    private const string FormatoMoeda = "\"R$\" #,##0.00";
    private const string FormatoDataHora = "dd/mm/yyyy hh:mm";

    public static byte[] GerarXlsx(RelatorioModelo modelo)
    {
        using var pasta = new XLWorkbook();
        var planilha = pasta.Worksheets.Add("Relatório");

        var linha = 1;
        planilha.Cell(linha, 1).Value = modelo.Titulo;
        planilha.Cell(linha, 1).Style.Font.SetBold().Font.SetFontSize(14);
        linha++;
        planilha.Cell(linha++, 1).Value = $"Gerado em {FormatoRelatorioTexto.Formatar(modelo.GeradoEm, TipoValor.DataHora)}";
        foreach (var filtro in modelo.Filtros)
            planilha.Cell(linha++, 1).Value = filtro;

        linha++;
        foreach (var campo in modelo.Resumo)
        {
            planilha.Cell(linha, 1).Value = campo.Rotulo;
            planilha.Cell(linha, 1).Style.Font.SetBold();
            Escrever(planilha.Cell(linha, 2), campo.Valor, campo.Tipo);
            linha++;
        }

        linha++;
        var linhaCabecalho = linha;
        for (var c = 0; c < modelo.Colunas.Count; c++)
            planilha.Cell(linhaCabecalho, c + 1).Value = modelo.Colunas[c].Titulo;

        var cabecalho = planilha.Range(linhaCabecalho, 1, linhaCabecalho, modelo.Colunas.Count);
        cabecalho.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#DCFCE7"));

        foreach (var valores in modelo.Linhas)
        {
            linha++;
            for (var c = 0; c < modelo.Colunas.Count; c++)
                Escrever(planilha.Cell(linha, c + 1), valores[c], modelo.Colunas[c].Tipo);
        }

        if (modelo.Linhas.Count > 0)
            planilha.Range(linhaCabecalho, 1, linha, modelo.Colunas.Count).SetAutoFilter();

        planilha.Columns(1, modelo.Colunas.Count).AdjustToContents(linhaCabecalho, linha);
        planilha.SheetView.FreezeRows(linhaCabecalho);

        using var memoria = new MemoryStream();
        pasta.SaveAs(memoria);
        return memoria.ToArray();
    }

    public static byte[] GerarPdf(RelatorioModelo modelo)
    {
        const string corDestaque = "#DCFCE7";
        var cinza = Colors.Grey.Darken1;

        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(modelo.Colunas.Count > 6 ? PageSizes.A4.Landscape() : PageSizes.A4);
            pagina.Margin(1.5f, Unit.Centimetre);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(9));

            pagina.Header().Column(cabecalho =>
            {
                cabecalho.Item().Text(modelo.Titulo).FontSize(16).Bold();
                cabecalho.Item().Text($"Gerado em {FormatoRelatorioTexto.Formatar(modelo.GeradoEm, TipoValor.DataHora)}").FontColor(cinza);
                cabecalho.Item().Text(string.Join("  ·  ", modelo.Filtros)).FontColor(cinza);

                cabecalho.Item().PaddingVertical(8).Row(resumo =>
                {
                    resumo.Spacing(8);
                    foreach (var campo in modelo.Resumo)
                    {
                        resumo.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(card =>
                        {
                            card.Item().Text(campo.Rotulo).FontSize(8).FontColor(cinza);
                            card.Item().Text(FormatoRelatorioTexto.Formatar(campo.Valor, campo.Tipo)).FontSize(12).Bold();
                        });
                    }
                });
            });

            pagina.Content().Element(conteudo =>
            {
                if (modelo.Linhas.Count == 0)
                {
                    conteudo.PaddingTop(12).Text("Nenhum registro para os filtros informados.").FontColor(cinza);
                    return;
                }

                conteudo.Table(tabela =>
                {
                    // Texto ganha o dobro da largura das numéricas; data-hora o suficiente para não quebrar a hora.
                    tabela.ColumnsDefinition(colunas =>
                    {
                        foreach (var coluna in modelo.Colunas)
                            colunas.RelativeColumn(coluna.Tipo switch { TipoValor.Texto => 2f, TipoValor.DataHora => 1.6f, _ => 1f });
                    });

                    tabela.Header(cabecalho =>
                    {
                        foreach (var coluna in modelo.Colunas)
                            Alinhar(cabecalho.Cell().Background(corDestaque).Padding(4), coluna.Tipo).Text(coluna.Titulo).Bold();
                    });

                    foreach (var valores in modelo.Linhas)
                    {
                        for (var c = 0; c < modelo.Colunas.Count; c++)
                        {
                            var celula = tabela.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                            Alinhar(celula, modelo.Colunas[c].Tipo).Text(FormatoRelatorioTexto.Formatar(valores[c], modelo.Colunas[c].Tipo));
                        }
                    }
                });
            });

            pagina.Footer().AlignCenter().Text(rodape =>
            {
                rodape.DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(cinza));
                rodape.Span("Ambition ERP  ·  Página ");
                rodape.CurrentPageNumber();
                rodape.Span(" de ");
                rodape.TotalPages();
            });
        })).GeneratePdf();
    }

    // Números à direita (como no Excel e na tela); texto e data à esquerda.
    private static IContainer Alinhar(IContainer celula, TipoValor tipo) =>
        tipo is TipoValor.Inteiro or TipoValor.Quantidade or TipoValor.Moeda ? celula.AlignRight() : celula;

    // Valor tipado na célula (o Excel soma e ordena direito); o formato só muda a exibição.
    private static void Escrever(IXLCell celula, object? valor, TipoValor tipo)
    {
        switch (valor)
        {
            case null:
                return;
            case decimal d:
                celula.Value = d;
                if (tipo == TipoValor.Moeda)
                    celula.Style.NumberFormat.Format = FormatoMoeda;
                break;
            case int i:
                celula.Value = i;
                break;
            case DateTime dt:
                celula.Value = dt;
                celula.Style.NumberFormat.Format = FormatoDataHora;
                break;
            default:
                celula.Value = FormatoRelatorioTexto.Formatar(valor, tipo);
                break;
        }
    }
}
