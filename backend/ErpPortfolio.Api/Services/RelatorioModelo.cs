// =====================================================================================
// Arquivo....: RelatorioModelo.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Modelo genérico de um relatório para exportação: título, filtros já
//              descritos, campos do resumo, colunas e linhas (valores tipados). Os três
//              relatórios viram este modelo e o ExportadorRelatorio gera .xlsx/.pdf a
//              partir dele — um exportador para todos (SPEC.md, "Arquitetura").
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Montado pelo RelatorioService (Modelo*Async); lido pelo ExportadorRelatorio.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 24/09/2026 - "Hoje" e limites de dia/mês em horário de Brasília (HorarioBrasilia).
// =====================================================================================

using System.Globalization;

namespace ErpPortfolio.Api.Services;

/// <summary>Como um valor é formatado no Excel (formato de número) e no PDF (texto).</summary>
public enum TipoValor
{
    Texto,
    Inteiro,
    Quantidade,
    Moeda,
    DataHora,
    SimNao
}

public record ColunaRelatorio(string Titulo, TipoValor Tipo);

public record CampoRelatorio(string Rotulo, object Valor, TipoValor Tipo);

// NomeArquivo: sem extensão (ex.: relatorio-vendas-2026-09-01_2026-09-30). GeradoEm: horário de Brasília.
public record RelatorioModelo(
    string Titulo,
    string NomeArquivo,
    DateTime GeradoEm,
    IReadOnlyList<string> Filtros,
    IReadOnlyList<CampoRelatorio> Resumo,
    IReadOnlyList<ColunaRelatorio> Colunas,
    IReadOnlyList<object?[]> Linhas);

public static class FormatoRelatorioTexto
{
    public static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Texto de um valor para o PDF (e para os filtros/resumo descritos), em pt-BR.</summary>
    public static string Formatar(object? valor, TipoValor tipo) => valor switch
    {
        null => "",
        decimal d when tipo == TipoValor.Moeda => d.ToString("C2", PtBr),
        decimal d => d.ToString("#,##0.###", PtBr),
        int i => i.ToString("N0", PtBr),
        DateTime dt => dt.ToString("dd/MM/yyyy HH:mm", PtBr),
        bool b => b ? "Sim" : "Não",
        _ => Convert.ToString(valor, PtBr) ?? ""
    };
}
