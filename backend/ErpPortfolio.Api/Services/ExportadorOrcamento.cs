// =====================================================================================
// Arquivo....: ExportadorOrcamento.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: PDF do orçamento para mandar ao cliente (SPEC.md etapa 13, PD1): A4 retrato
//              com número, data e validade; dados do cliente e vendedor; tabela de itens;
//              subtotal, desconto e total; forma de pagamento; observações; rodapé com a
//              validade e "Página X de Y". Layout de documento, por isso separado do
//              ExportadorRelatorio (tabular).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica (recebe o Orcamento já carregado com cliente, vendedor e itens).
// Fontes.....: QuestPDF (licença Community, declarada no Program.cs). Chamado pelo
//              OrcamentoService.GerarPdfAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ErpPortfolio.Api.Services;

public static class ExportadorOrcamento
{
    private const string CorDestaque = "#DCFCE7";

    public static byte[] GerarPdf(Orcamento orcamento, DateOnly hoje)
    {
        var ptBr = FormatoRelatorioTexto.PtBr;
        var cinza = Colors.Grey.Darken1;
        var cliente = orcamento.Cliente!;
        var validade = orcamento.Validade.ToString("dd/MM/yyyy", ptBr);
        var itens = orcamento.Itens.OrderBy(i => i.Id).ToList();
        var subtotais = itens.Select(i => CalculoPedido.Subtotal(i.Quantidade, i.PrecoUnitario, i.DescontoPercentual)).ToList();
        var subtotalItens = subtotais.Sum();

        string Moeda(decimal valor) => valor.ToString("C2", ptBr);

        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(1.5f, Unit.Centimetre);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

            pagina.Header().PaddingBottom(10).Row(linha =>
            {
                linha.RelativeItem().Column(esquerda =>
                {
                    esquerda.Item().Text("Ambition ERP").FontSize(18).Bold();
                    esquerda.Item().Text($"Orçamento Nº {orcamento.Id}").FontSize(13).SemiBold();
                });
                linha.ConstantItem(190).AlignRight().Column(direita =>
                {
                    direita.Item().AlignRight().Text($"Data: {FormatoRelatorioTexto.ParaBrasilia(orcamento.DataOrcamento):dd/MM/yyyy}");
                    direita.Item().AlignRight().Text($"Válido até: {validade}").Bold();
                    if (Situacao(orcamento, hoje) is { } situacao)
                        direita.Item().AlignRight().Text(situacao).FontColor(cinza);
                });
            });

            pagina.Content().Column(conteudo =>
            {
                conteudo.Spacing(10);

                conteudo.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(dados =>
                {
                    dados.Item().Text("Cliente").FontSize(8).FontColor(cinza);
                    dados.Item().Text(cliente.Nome).Bold();
                    dados.Item().Text($"{(cliente.Documento.Length == 11 ? "CPF" : "CNPJ")}: {FormatarDocumento(cliente.Documento)}");
                    var contato = string.Join("  ·  ", new[] { cliente.Email, cliente.Telefone }.Where(t => !string.IsNullOrWhiteSpace(t)));
                    if (contato.Length > 0)
                        dados.Item().Text(contato);
                    dados.Item().Text($"{cliente.Cidade}/{cliente.Uf}");
                    if (orcamento.Vendedor is { } vendedor)
                        dados.Item().PaddingTop(4).Text(texto =>
                        {
                            texto.Span("Vendedor: ").FontColor(cinza);
                            texto.Span(vendedor.Nome);
                        });
                });

                conteudo.Item().Table(tabela =>
                {
                    tabela.ColumnsDefinition(colunas =>
                    {
                        colunas.RelativeColumn(4f);
                        colunas.RelativeColumn(1.2f);
                        colunas.RelativeColumn(0.7f);
                        colunas.RelativeColumn(1.6f);
                        colunas.RelativeColumn(1f);
                        colunas.RelativeColumn(1.7f);
                    });

                    tabela.Header(cabecalho =>
                    {
                        string[] titulos = ["Produto", "Qtd", "Un", "Preço unit.", "Desc. %", "Subtotal"];
                        for (var c = 0; c < titulos.Length; c++)
                        {
                            var celula = cabecalho.Cell().Background(CorDestaque).Padding(4);
                            (c == 0 || c == 2 ? celula : celula.AlignRight()).Text(titulos[c]).Bold();
                        }
                    });

                    for (var i = 0; i < itens.Count; i++)
                    {
                        var item = itens[i];
                        var produto = item.Produto!;
                        IContainer Celula() => tabela.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);

                        Celula().Column(nome =>
                        {
                            nome.Item().Text(produto.Nome);
                            nome.Item().Text($"SKU {produto.Sku}").FontSize(8).FontColor(cinza);
                        });
                        Celula().AlignRight().Text(item.Quantidade.ToString("#,##0.###", ptBr));
                        Celula().Text(produto.Unidade);
                        Celula().AlignRight().Text(Moeda(item.PrecoUnitario));
                        Celula().AlignRight().Text(item.DescontoPercentual == 0 ? "-" : item.DescontoPercentual.ToString("0.##", ptBr));
                        Celula().AlignRight().Text(Moeda(subtotais[i]));
                    }
                });

                conteudo.Item().AlignRight().Width(240).Column(totais =>
                {
                    void Linha(string rotulo, string valor, bool destaque = false) => totais.Item().Row(linha =>
                    {
                        var r = linha.RelativeItem().Text(rotulo);
                        var v = linha.ConstantItem(110).AlignRight().Text(valor);
                        if (destaque) { r.Bold().FontSize(12); v.Bold().FontSize(12); }
                    });

                    Linha("Subtotal dos itens", Moeda(subtotalItens));
                    if (orcamento.DescontoPercentual > 0)
                        Linha($"Desconto ({orcamento.DescontoPercentual.ToString("0.##", ptBr)}%)", "- " + Moeda(subtotalItens - orcamento.ValorTotal));
                    totais.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    Linha("Total", Moeda(orcamento.ValorTotal), destaque: true);
                });

                if (orcamento.FormaPagamento is { } forma)
                    conteudo.Item().Text(texto =>
                    {
                        texto.Span("Forma de pagamento: ").FontColor(cinza);
                        texto.Span(forma == FormaPagamento.Cartao ? "Cartão" : forma.ToString());
                    });

                if (!string.IsNullOrWhiteSpace(orcamento.Observacoes))
                    conteudo.Item().Column(obs =>
                    {
                        obs.Item().Text("Observações").FontSize(8).FontColor(cinza);
                        obs.Item().Text(orcamento.Observacoes);
                    });
            });

            pagina.Footer().AlignCenter().Text(rodape =>
            {
                rodape.DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(cinza));
                rodape.Span($"Orçamento válido até {validade}  ·  Ambition ERP  ·  Página ");
                rodape.CurrentPageNumber();
                rodape.Span(" de ");
                rodape.TotalPages();
            });
        })).GeneratePdf();
    }

    // Só aparece quando não é um orçamento aberto e válido (o cliente normalmente recebe esse).
    private static string? Situacao(Orcamento orcamento, DateOnly hoje) => orcamento.Status switch
    {
        StatusOrcamento.Aprovado => $"Aprovado (pedido Nº {orcamento.PedidoId})",
        StatusOrcamento.Perdido => "Perdido",
        _ when orcamento.EstaVencido(hoje) => "Vencido",
        _ => null
    };

    // Gravado sem máscara (11 = CPF, 14 = CNPJ, inclusive o alfanumérico).
    public static string FormatarDocumento(string documento) => documento.Length switch
    {
        11 => $"{documento[..3]}.{documento[3..6]}.{documento[6..9]}-{documento[9..]}",
        14 => $"{documento[..2]}.{documento[2..5]}.{documento[5..8]}/{documento[8..12]}-{documento[12..]}",
        _ => documento
    };
}
