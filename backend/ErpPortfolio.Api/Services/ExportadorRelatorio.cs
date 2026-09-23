// =====================================================================================
// Arquivo....: ExportadorRelatorio.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Gera o arquivo de um RelatorioModelo: Excel (.xlsx, ClosedXML). O mesmo
//              modelo serve aos três relatórios. Layout: título, "gerado em", filtros,
//              resumo (rótulo | valor) e a tabela com cabeçalho destacado, filtro
//              automático e valores tipados (moeda, data, número) — não texto.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: ClosedXML (MIT). Chamado pelo RelatoriosController.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo (Excel).
// =====================================================================================

using ClosedXML.Excel;

namespace ErpPortfolio.Api.Services;

public static class ExportadorRelatorio
{
    public const string TipoConteudoXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

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
