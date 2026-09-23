// =====================================================================================
// Arquivo....: EstoqueService.cs
// Versão.....: 1.5.0
// Data.......: 22/09/2026
// Descrição..: Consulta de estoque (listagem com saldo, extrato por produto), entrada
//              manual, a baixa/estorno usados pelo PedidoService (pedido de venda) e o
//              receber/estornar usados pelo PedidoCompraService (pedido de compra). O
//              saldo nunca é gravado: é sempre Σ Entrada − Σ Saída, calculado na consulta
//              (SPEC.md, E1).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.produtos
//                - SELECT : listagem (ILIKE em nome ou sku, ORDER BY nome, id, LIMIT/OFFSET)
//                           e validação do produto na entrada manual (existe e está ativo)
//              public.estoque_movimentacoes
//                - SELECT : saldo agregado por produto (subconsulta correlacionada e,
//                           em BaixarAsync/EstornarCompraAsync, uma consulta por item) e
//                           extrato paginado de um produto (ORDER BY data_movimentacao DESC)
//                - INSERT : entrada manual (E4), saída por venda (E2), entrada de estorno
//                           de venda (E3), entrada por compra (PC6), saída de estorno de
//                           compra (PC7)
// Fontes.....: ErpPortfolioDbContext.Produtos / EstoqueMovimentacoes (EF Core / Npgsql).
//              BaixarAsync, Estornar, Receber e EstornarCompraAsync não chamam
//              SaveChanges: ficam na mesma transação de quem chamar (PedidoService ou
//              PedidoCompraService), que salva tudo de uma vez.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e extrato).
//   1.1.0 - 22/09/2026 - Entrada manual (RegistrarEntradaAsync).
//   1.2.0 - 22/09/2026 - BaixarAsync (confirmar) e Estornar (cancelar de confirmado).
//   1.3.0 - 22/09/2026 - ObterResumoAsync (quantidade de produtos com saldo baixo), para o Dashboard.
//   1.4.0 - 22/09/2026 - Estoque mínimo passa a ser por produto (produtos.estoque_minimo);
//                        ObterResumoAsync devolve a lista dos produtos baixos, não só a contagem.
//   1.5.0 - 22/09/2026 - Receber e EstornarCompraAsync, para o Pedido de Compra (PC6/PC7).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class EstoqueService(ErpPortfolioDbContext contexto) : IEstoqueService
{
    private const string CampoItens = "Itens";

    public async Task<ResultadoPaginadoDto<EstoqueResumoDto>> ListarAsync(EstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Produtos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var padrao = $"%{ClienteService.EscaparCuringasLike(filtro.Busca.Trim())}%";
            consulta = consulta.Where(p => EF.Functions.ILike(p.Nome, padrao) || EF.Functions.ILike(p.Sku, padrao));
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(p => p.Nome)
            .ThenBy(p => p.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            // Subconsulta correlacionada, traduzida em SQL: COALESCE(SUM(CASE WHEN tipo = 'Entrada' THEN quantidade ELSE -quantidade END), 0).
            // Precisa ficar inline no Select: um método de instância separado não é traduzido pelo EF (InvalidOperationException).
            .Select(p => new EstoqueResumoDto(
                p.Id,
                p.Nome,
                p.Sku,
                p.Unidade,
                contexto.EstoqueMovimentacoes
                    .Where(m => m.ProdutoId == p.Id)
                    .Sum(m => m.Tipo == TipoMovimentacao.Entrada ? m.Quantidade : -m.Quantidade)))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<EstoqueResumoDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<ResultadoPaginadoDto<MovimentacaoRespostaDto>?> ObterExtratoAsync(
        int produtoId, EstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        var produtoExiste = await contexto.Produtos.AsNoTracking().AnyAsync(p => p.Id == produtoId, cancelamento);
        if (!produtoExiste)
            return null;

        var consulta = contexto.EstoqueMovimentacoes.AsNoTracking().Where(m => m.ProdutoId == produtoId);

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(m => new MovimentacaoRespostaDto(m.Tipo, m.Quantidade, m.Motivo, m.PedidoId, m.PedidoCompraId, m.DataMovimentacao))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<MovimentacaoRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<MovimentacaoRespostaDto?> RegistrarEntradaAsync(EstoqueEntradaDto dados, CancellationToken cancelamento)
    {
        var produto = await contexto.Produtos.FirstOrDefaultAsync(p => p.Id == dados.ProdutoId, cancelamento);
        if (produto is null)
            return null;

        if (!produto.Ativo)
            throw new DadoInvalidoException(nameof(EstoqueEntradaDto.ProdutoId), $"O produto \"{produto.Nome}\" está inativo.");

        var movimentacao = new EstoqueMovimentacao
        {
            ProdutoId = produto.Id,
            Tipo = TipoMovimentacao.Entrada,
            Quantidade = dados.Quantidade!.Value,
            Motivo = dados.Motivo,
            DataMovimentacao = DateTime.UtcNow
        };

        contexto.EstoqueMovimentacoes.Add(movimentacao);
        await contexto.SaveChangesAsync(cancelamento);

        return new MovimentacaoRespostaDto(
            movimentacao.Tipo, movimentacao.Quantidade, movimentacao.Motivo, movimentacao.PedidoId, movimentacao.PedidoCompraId, movimentacao.DataMovimentacao);
    }

    public async Task BaixarAsync(int pedidoId, IEnumerable<PedidoItem> itens, CancellationToken cancelamento)
    {
        var lista = itens.ToList();
        var insuficientes = new List<string>();

        // Confere TODOS os itens antes de enfileirar qualquer movimentação: se um item não tiver saldo,
        // nenhuma Saída é gravada, nem a dos itens que tinham saldo (E2).
        foreach (var item in lista)
        {
            var saldo = await SaldoAsync(item.ProdutoId, cancelamento);
            if (saldo < item.Quantidade)
                insuficientes.Add($"\"{item.Produto!.Nome}\" (saldo {saldo}, pedido pede {item.Quantidade})");
        }

        if (insuficientes.Count > 0)
            throw new DadoInvalidoException(CampoItens, $"Estoque insuficiente: {string.Join("; ", insuficientes)}.");

        foreach (var item in lista)
        {
            contexto.EstoqueMovimentacoes.Add(
                NovaMovimentacao(item.ProdutoId, TipoMovimentacao.Saida, item.Quantidade, pedidoId, $"Venda pedido #{pedidoId}"));
        }
    }

    public void Estornar(int pedidoId, IEnumerable<PedidoItem> itens)
    {
        foreach (var item in itens)
        {
            contexto.EstoqueMovimentacoes.Add(
                NovaMovimentacao(item.ProdutoId, TipoMovimentacao.Entrada, item.Quantidade, pedidoId, $"Estorno cancelamento pedido #{pedidoId}"));
        }
    }

    public void Receber(int pedidoCompraId, IEnumerable<PedidoCompraItem> itens)
    {
        foreach (var item in itens)
        {
            contexto.EstoqueMovimentacoes.Add(
                NovaMovimentacaoCompra(item.ProdutoId, TipoMovimentacao.Entrada, item.Quantidade, pedidoCompraId, $"Compra pedido #{pedidoCompraId}"));
        }
    }

    public async Task EstornarCompraAsync(int pedidoCompraId, IEnumerable<PedidoCompraItem> itens, CancellationToken cancelamento)
    {
        var lista = itens.ToList();
        var insuficientes = new List<string>();

        // Confere TODOS os itens antes de enfileirar qualquer movimentação: se um item não tiver saldo (já foi
        // vendido/consumido), nenhuma Saída é gravada, nem a dos itens que tinham saldo (PC7).
        foreach (var item in lista)
        {
            var saldo = await SaldoAsync(item.ProdutoId, cancelamento);
            if (saldo < item.Quantidade)
                insuficientes.Add($"\"{item.Produto!.Nome}\" (saldo {saldo}, a compra tinha entrado com {item.Quantidade})");
        }

        if (insuficientes.Count > 0)
            throw new DadoInvalidoException(CampoItens, $"Estoque insuficiente para estornar: {string.Join("; ", insuficientes)}.");

        foreach (var item in lista)
        {
            contexto.EstoqueMovimentacoes.Add(
                NovaMovimentacaoCompra(item.ProdutoId, TipoMovimentacao.Saida, item.Quantidade, pedidoCompraId, $"Estorno cancelamento pedido de compra #{pedidoCompraId}"));
        }
    }

    public async Task<EstoqueResumoDashboardDto> ObterResumoAsync(CancellationToken cancelamento)
    {
        // Traz o saldo de todos os produtos ativos pra comparar com o próprio EstoqueMinimo de cada um em
        // memória (a comparação entre duas colunas do mesmo produto não precisa virar SQL; escala de portfólio).
        var produtos = await contexto.Produtos.AsNoTracking()
            .Where(p => p.Ativo)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                p.EstoqueMinimo,
                Saldo = contexto.EstoqueMovimentacoes
                    .Where(m => m.ProdutoId == p.Id)
                    .Sum(m => m.Tipo == TipoMovimentacao.Entrada ? m.Quantidade : -m.Quantidade)
            })
            .ToListAsync(cancelamento);

        var baixos = produtos
            .Where(p => p.Saldo <= p.EstoqueMinimo)
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoSaldoBaixoDto(p.Id, p.Nome, p.Saldo, p.EstoqueMinimo))
            .ToList();

        return new EstoqueResumoDashboardDto(baixos.Count, baixos);
    }

    private async Task<decimal> SaldoAsync(int produtoId, CancellationToken cancelamento) =>
        await contexto.EstoqueMovimentacoes
            .Where(m => m.ProdutoId == produtoId)
            .SumAsync(m => m.Tipo == TipoMovimentacao.Entrada ? m.Quantidade : -m.Quantidade, cancelamento);

    private static EstoqueMovimentacao NovaMovimentacao(int produtoId, TipoMovimentacao tipo, decimal quantidade, int pedidoId, string motivo) => new()
    {
        ProdutoId = produtoId,
        Tipo = tipo,
        Quantidade = quantidade,
        Motivo = motivo,
        PedidoId = pedidoId,
        DataMovimentacao = DateTime.UtcNow
    };

    private static EstoqueMovimentacao NovaMovimentacaoCompra(int produtoId, TipoMovimentacao tipo, decimal quantidade, int pedidoCompraId, string motivo) => new()
    {
        ProdutoId = produtoId,
        Tipo = tipo,
        Quantidade = quantidade,
        Motivo = motivo,
        PedidoCompraId = pedidoCompraId,
        DataMovimentacao = DateTime.UtcNow
    };
}
