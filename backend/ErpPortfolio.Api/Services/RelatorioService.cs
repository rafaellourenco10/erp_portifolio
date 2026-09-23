// =====================================================================================
// Arquivo....: RelatorioService.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Consultas dos relatórios (somente leitura): vendas e compras por período
//              (um pedido por linha + resumo) e posição atual de estoque (saldo, valor em
//              estoque, abaixo do mínimo). O resultado alimenta a tela e a exportação.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.pedidos, public.clientes, public.pedido_itens
//                - SELECT : pedidos do período [início 00:00 UTC, fim + 1 dia 00:00 UTC),
//                           filtro de status e cliente, com nome do cliente e nº de itens
//              public.pedidos_compra, public.fornecedores, public.pedido_compra_itens
//                - SELECT : idem, para compras e fornecedor
//              public.produtos, public.categorias, public.estoque_movimentacoes
//                - SELECT : produtos ativos (filtro de categoria) com saldo agregado das
//                           movimentações (subconsulta correlacionada)
// Fontes.....: ErpPortfolioDbContext (EF Core / Npgsql), AsNoTracking.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Modelo*Async: mesma consulta no modelo de exportação (T2).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class RelatorioService(ErpPortfolioDbContext contexto) : IRelatorioService
{
    public async Task<RelatorioPedidosDto> VendasAsync(RelatorioVendasFiltroDto filtro, CancellationToken cancelamento)
    {
        var (inicio, fimExclusivo) = Intervalo(filtro);
        var consulta = contexto.Pedidos.AsNoTracking()
            .Where(p => p.DataPedido >= inicio && p.DataPedido < fimExclusivo);

        if (filtro.Status is not null)
            consulta = consulta.Where(p => p.Status == filtro.Status);
        if (filtro.ClienteId is not null)
            consulta = consulta.Where(p => p.ClienteId == filtro.ClienteId);

        var linhas = await consulta
            .OrderBy(p => p.DataPedido).ThenBy(p => p.Id)
            .Select(p => new RelatorioPedidoLinhaDto(p.Id, p.DataPedido, p.Cliente!.Nome, p.Itens.Count, p.ValorTotal, p.Status))
            .ToListAsync(cancelamento);

        return ComResumo(linhas);
    }

    public async Task<RelatorioPedidosDto> ComprasAsync(RelatorioComprasFiltroDto filtro, CancellationToken cancelamento)
    {
        var (inicio, fimExclusivo) = Intervalo(filtro);
        var consulta = contexto.PedidosCompra.AsNoTracking()
            .Where(p => p.DataPedido >= inicio && p.DataPedido < fimExclusivo);

        if (filtro.Status is not null)
            consulta = consulta.Where(p => p.Status == filtro.Status);
        if (filtro.FornecedorId is not null)
            consulta = consulta.Where(p => p.FornecedorId == filtro.FornecedorId);

        var linhas = await consulta
            .OrderBy(p => p.DataPedido).ThenBy(p => p.Id)
            .Select(p => new RelatorioPedidoLinhaDto(p.Id, p.DataPedido, p.Fornecedor!.Nome, p.Itens.Count, p.ValorTotal, p.Status))
            .ToListAsync(cancelamento);

        return ComResumo(linhas);
    }

    public async Task<RelatorioEstoqueDto> EstoqueAsync(RelatorioEstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Produtos.AsNoTracking().Where(p => p.Ativo);
        if (filtro.CategoriaId is not null)
            consulta = consulta.Where(p => p.CategoriaId == filtro.CategoriaId);

        // Saldo inline no Select (subconsulta correlacionada), mesmo padrão do EstoqueService.ListarAsync.
        var produtos = await consulta
            .OrderBy(p => p.Nome).ThenBy(p => p.Id)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                p.Sku,
                CategoriaNome = p.Categoria != null ? p.Categoria.Nome : null,
                p.Unidade,
                p.EstoqueMinimo,
                p.Custo,
                Saldo = contexto.EstoqueMovimentacoes
                    .Where(m => m.ProdutoId == p.Id)
                    .Sum(m => m.Tipo == TipoMovimentacao.Entrada ? m.Quantidade : -m.Quantidade)
            })
            .ToListAsync(cancelamento);

        // Comparação entre colunas do mesmo produto em memória, como no resumo do Dashboard (escala de portfólio).
        var linhas = produtos
            .Select(p => new RelatorioEstoqueLinhaDto(
                p.Id, p.Nome, p.Sku, p.CategoriaNome, p.Unidade, p.Saldo, p.EstoqueMinimo, p.Custo,
                Math.Round(p.Saldo * p.Custo, 2), p.Saldo <= p.EstoqueMinimo))
            .Where(l => !filtro.SomenteAbaixoMinimo || l.AbaixoMinimo)
            .ToList();

        return new RelatorioEstoqueDto(linhas, linhas.Count, linhas.Sum(l => l.ValorEstoque), linhas.Count(l => l.AbaixoMinimo));
    }

    public async Task<RelatorioModelo> ModeloVendasAsync(RelatorioVendasFiltroDto filtro, CancellationToken cancelamento)
    {
        var dados = await VendasAsync(filtro, cancelamento);
        var cliente = filtro.ClienteId is null
            ? "Todos"
            : await contexto.Clientes.Where(c => c.Id == filtro.ClienteId).Select(c => c.Nome).FirstOrDefaultAsync(cancelamento) ?? $"#{filtro.ClienteId}";
        return ModeloPedidos("Relatório de Vendas", "vendas", "Cliente", cliente, filtro, dados);
    }

    public async Task<RelatorioModelo> ModeloComprasAsync(RelatorioComprasFiltroDto filtro, CancellationToken cancelamento)
    {
        var dados = await ComprasAsync(filtro, cancelamento);
        var fornecedor = filtro.FornecedorId is null
            ? "Todos"
            : await contexto.Fornecedores.Where(f => f.Id == filtro.FornecedorId).Select(f => f.Nome).FirstOrDefaultAsync(cancelamento) ?? $"#{filtro.FornecedorId}";
        return ModeloPedidos("Relatório de Compras", "compras", "Fornecedor", fornecedor, filtro, dados);
    }

    public async Task<RelatorioModelo> ModeloEstoqueAsync(RelatorioEstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        var dados = await EstoqueAsync(filtro, cancelamento);
        var categoria = filtro.CategoriaId is null
            ? "Todas"
            : await contexto.Categorias.Where(c => c.Id == filtro.CategoriaId).Select(c => c.Nome).FirstOrDefaultAsync(cancelamento) ?? $"#{filtro.CategoriaId}";
        var agora = FormatoRelatorioTexto.ParaBrasilia(DateTime.UtcNow);

        return new RelatorioModelo(
            "Relatório de Estoque",
            $"relatorio-estoque-{agora:yyyy-MM-dd}",
            agora,
            [$"Categoria: {categoria}", $"Somente abaixo do mínimo: {(filtro.SomenteAbaixoMinimo ? "Sim" : "Não")}", "Somente produtos ativos"],
            [
                new CampoRelatorio("Produtos", dados.QuantidadeProdutos, TipoValor.Inteiro),
                new CampoRelatorio("Valor total em estoque", dados.ValorTotalEstoque, TipoValor.Moeda),
                new CampoRelatorio("Abaixo do mínimo", dados.QuantidadeAbaixoMinimo, TipoValor.Inteiro),
            ],
            [
                new ColunaRelatorio("Produto", TipoValor.Texto),
                new ColunaRelatorio("SKU", TipoValor.Texto),
                new ColunaRelatorio("Categoria", TipoValor.Texto),
                new ColunaRelatorio("Unidade", TipoValor.Texto),
                new ColunaRelatorio("Saldo", TipoValor.Quantidade),
                new ColunaRelatorio("Estoque mínimo", TipoValor.Quantidade),
                new ColunaRelatorio("Custo", TipoValor.Moeda),
                new ColunaRelatorio("Valor em estoque", TipoValor.Moeda),
                new ColunaRelatorio("Abaixo do mínimo", TipoValor.SimNao),
            ],
            dados.Linhas
                .Select(l => new object?[] { l.Nome, l.Sku, l.CategoriaNome ?? "—", l.Unidade, l.Saldo, l.EstoqueMinimo, l.Custo, l.ValorEstoque, l.AbaixoMinimo })
                .ToList());
    }

    private static RelatorioModelo ModeloPedidos(
        string titulo, string chave, string rotuloParceiro, string parceiro, RelatorioPedidosFiltroDto filtro, RelatorioPedidosDto dados)
    {
        var inicio = filtro.DataInicio!.Value;
        var fim = filtro.DataFim!.Value;

        return new RelatorioModelo(
            titulo,
            $"relatorio-{chave}-{inicio:yyyy-MM-dd}_{fim:yyyy-MM-dd}",
            FormatoRelatorioTexto.ParaBrasilia(DateTime.UtcNow),
            [
                $"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}",
                $"Status: {filtro.Status?.ToString() ?? "Todos"}",
                $"{rotuloParceiro}: {parceiro}",
            ],
            [
                new CampoRelatorio("Pedidos", dados.QuantidadePedidos, TipoValor.Inteiro),
                new CampoRelatorio("Valor total", dados.ValorTotal, TipoValor.Moeda),
                new CampoRelatorio("Ticket médio", dados.TicketMedio, TipoValor.Moeda),
            ],
            [
                new ColunaRelatorio("Nº", TipoValor.Inteiro),
                new ColunaRelatorio("Data", TipoValor.DataHora),
                new ColunaRelatorio(rotuloParceiro, TipoValor.Texto),
                new ColunaRelatorio("Itens", TipoValor.Inteiro),
                new ColunaRelatorio("Total", TipoValor.Moeda),
                new ColunaRelatorio("Status", TipoValor.Texto),
            ],
            dados.Linhas
                .Select(l => new object?[] { l.Id, FormatoRelatorioTexto.ParaBrasilia(l.DataPedido), l.Nome, l.QuantidadeItens, l.ValorTotal, l.Status.ToString() })
                .ToList());
    }

    // Datas inclusivas em UTC (R1/R2): o fim vale o dia inteiro, então compara com o dia seguinte exclusivo.
    private static (DateTime Inicio, DateTime FimExclusivo) Intervalo(RelatorioPedidosFiltroDto filtro) =>
        (filtro.DataInicio!.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
         filtro.DataFim!.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

    private static RelatorioPedidosDto ComResumo(List<RelatorioPedidoLinhaDto> linhas)
    {
        var (quantidade, total, ticketMedio) = RelatorioCalculo.ResumoPedidos(linhas.Select(l => l.ValorTotal));
        return new RelatorioPedidosDto(linhas, quantidade, total, ticketMedio);
    }
}
