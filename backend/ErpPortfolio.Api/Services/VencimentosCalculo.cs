// =====================================================================================
// Arquivo....: VencimentosCalculo.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Regras puras dos quadros de vencimentos do Dashboard (sem banco): a janela
//              (atrasadas + próximos 7 dias), o limite de itens da lista e a montagem do
//              resumo a partir das parcelas já lidas.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Usado por ContasPagarService e ContasReceberService. Testado em
//              VencimentosCalculoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public static class VencimentosCalculo
{
    /// <summary>"Perto de vencer" = de hoje até hoje + 7 dias (decisão do Rafael, 23/09/2026).</summary>
    public const int DiasJanela = 7;

    /// <summary>Quantas linhas cada quadro mostra; o resto aparece só no total.</summary>
    public const int LimiteItens = 10;

    /// <summary>Último vencimento que entra no quadro (as atrasadas entram todas).</summary>
    public static DateOnly FimDaJanela(DateOnly hoje) => hoje.AddDays(DiasJanela);

    /// <summary>Ordena (mais urgente primeiro), corta no limite e soma o total da janela inteira.</summary>
    public static VencimentosDto Montar(
        IEnumerable<(int Id, string Titulo, string Detalhe, decimal Valor, DateOnly Vencimento)> parcelas, DateOnly hoje)
    {
        var itens = parcelas
            .OrderBy(p => p.Vencimento).ThenBy(p => p.Id)
            .Select(p => new VencimentoItemDto(p.Id, p.Titulo, p.Detalhe, p.Valor, p.Vencimento, p.Vencimento.DayNumber - hoje.DayNumber))
            .ToList();

        return new VencimentosDto(itens.Count, itens.Sum(i => i.Valor), itens.Count(i => i.Dias < 0), itens.Take(LimiteItens).ToList());
    }
}
