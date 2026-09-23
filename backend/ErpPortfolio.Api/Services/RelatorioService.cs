// =====================================================================================
// Arquivo....: RelatorioService.cs
// Versão.....: 1.0.0
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
