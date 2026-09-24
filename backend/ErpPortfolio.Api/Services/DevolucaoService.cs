// =====================================================================================
// Arquivo....: DevolucaoService.cs
// Versão.....: 1.2.0
// Data.......: 24/09/2026
// Descrição..: Devolução de venda (SPEC.md etapa 14). Valida os itens (DV1/DV2), calcula
//              valor, abatimento, reembolso e estorno pelo DevolucaoCalculo (DV3-DV6) e
//              aplica tudo numa transação (DV8): devolução + itens, parcelas pendentes
//              reduzidas/canceladas, conta a pagar de reembolso, comissão negativa de
//              estorno e entrada no estoque dos itens que voltam (DV7).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.devolucoes / public.devolucao_itens (INSERT; SELECT no histórico)
//              public.parcelas_receber (UPDATE valor/status das pendentes, DV4)
//              public.parcelas_pagar (INSERT do reembolso, origem Devolucao, DV5)
//              public.comissoes (INSERT do estorno negativo, DV6)
//              public.estoque_movimentacoes (INSERT de Entrada via IEstoqueService, DV7)
//              public.pedidos / public.pedido_itens (SELECT)
// Fontes.....: ErpPortfolioDbContext; IEstoqueService.DevolverAoEstoque. Uma transação
//              explícita com dois SaveChanges: o número da devolução entra na descrição do
//              reembolso e no motivo do estoque.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
//   1.1.0 - 24/09/2026 - ObterResumoMesAsync (card do Dashboard, DB1).
//   1.2.0 - 24/09/2026 - "Hoje" e limites de dia/mês em horário de Brasília (HorarioBrasilia).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class DevolucaoService(ErpPortfolioDbContext contexto, IEstoqueService estoqueService) : IDevolucaoService
{
    private const string CampoItens = nameof(DevolucaoCriacaoDto.Itens);

    public async Task<DevolucaoRespostaDto?> RegistrarAsync(int pedidoId, DevolucaoCriacaoDto dados, CancellationToken cancelamento)
    {
        var pedido = await contexto.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == pedidoId, cancelamento);
        if (pedido is null)
            return null;

        // DV1
        if (pedido.Status != StatusPedido.Confirmado)
            throw new ConflitoException($"O pedido {pedidoId} está {pedido.Status}; só pedido confirmado aceita devolução.");

        var hoje = HorarioBrasilia.Hoje();
        if (dados.VencimentoReembolso is { } vencimento && vencimento < hoje)
            throw new DadoInvalidoException(nameof(DevolucaoCriacaoDto.VencimentoReembolso), "O vencimento do reembolso não pode ser anterior a hoje.");

        var jaDevolvido = await QuantidadesDevolvidasAsync(pedidoId, cancelamento);

        // DV2: cada item precisa ser deste pedido, respeitar a unidade e caber no que ainda não voltou.
        var itensPedido = pedido.Itens.ToDictionary(i => i.Id);
        var novos = new List<DevolucaoItem>();
        foreach (var entrada in dados.Itens)
        {
            if (!itensPedido.TryGetValue(entrada.PedidoItemId, out var item))
                throw new DadoInvalidoException(CampoItens, $"O item {entrada.PedidoItemId} não pertence ao pedido {pedidoId}.");

            var quantidade = entrada.Quantidade!.Value;
            if (!CalculoPedido.QuantidadeValidaParaUnidade(item.Produto!.Unidade, quantidade))
                throw new DadoInvalidoException(CampoItens, $"A quantidade de \"{item.Produto.Nome}\" deve ser inteira (unidade {item.Produto.Unidade}).");

            var disponivel = item.Quantidade - jaDevolvido.GetValueOrDefault(item.Id);
            if (quantidade > disponivel)
                throw new DadoInvalidoException(CampoItens, $"\"{item.Produto.Nome}\": só {disponivel:0.###} ainda pode(m) ser devolvido(s).");

            novos.Add(new DevolucaoItem
            {
                PedidoItemId = item.Id,
                PedidoItem = item,
                Quantidade = quantidade,
                Valor = DevolucaoCalculo.ValorItem(quantidade, item.PrecoUnitario, item.DescontoPercentual, pedido.DescontoPercentual),
                VoltaEstoque = entrada.VoltaEstoque
            });
        }

        // DV3: a devolução que zera o pedido leva exatamente o que falta.
        var novasPorItem = novos.ToDictionary(n => n.PedidoItemId, n => n.Quantidade);
        var devolveTudo = pedido.Itens.All(i => jaDevolvido.GetValueOrDefault(i.Id) + novasPorItem.GetValueOrDefault(i.Id) == i.Quantidade);
        var valorJaDevolvido = await contexto.Devolucoes.Where(d => d.PedidoId == pedidoId).SumAsync(d => d.ValorTotal, cancelamento);
        var valorTotal = DevolucaoCalculo.ValorTotal(novos.Select(n => n.Valor), devolveTudo, pedido.ValorTotal, valorJaDevolvido);
        if (valorTotal <= 0)
            throw new DadoInvalidoException(CampoItens, "A devolução não tem valor a devolver (itens de preço zero).");

        // DV4/DV5
        var pendentes = await contexto.ParcelasReceber
            .Where(p => p.PedidoId == pedidoId && p.Status == StatusParcela.Pendente)
            .ToListAsync(cancelamento);
        var abatimento = DevolucaoCalculo.Abater(
            pendentes.Select(p => new DevolucaoCalculo.ParcelaPendente(p.Id, p.NumeroParcela, p.Valor)), valorTotal);

        await using var transacao = await contexto.Database.BeginTransactionAsync(cancelamento);

        var devolucao = new Devolucao
        {
            PedidoId = pedidoId,
            DataDevolucao = DateTime.UtcNow,
            Motivo = string.IsNullOrWhiteSpace(dados.Motivo) ? null : dados.Motivo.Trim(),
            ValorTotal = valorTotal,
            ValorAbatido = abatimento.Abatido,
            ValorReembolso = abatimento.Reembolso,
            Itens = novos
        };
        contexto.Devolucoes.Add(devolucao);

        var parcelasPorId = pendentes.ToDictionary(p => p.Id);
        foreach (var ajuste in abatimento.Ajustes)
        {
            var parcela = parcelasPorId[ajuste.Id];
            // Zerada: cancela e mantém o valor original como histórico (CHECK valor > 0).
            if (ajuste.Cancelar)
                parcela.Status = StatusParcela.Cancelado;
            else
                parcela.Valor = ajuste.NovoValor;
        }

        await contexto.SaveChangesAsync(cancelamento);

        ParcelaPagar? reembolso = null;
        if (abatimento.Reembolso > 0)
        {
            reembolso = new ParcelaPagar
            {
                Origem = OrigemContaPagar.Devolucao,
                DevolucaoId = devolucao.Id,
                Favorecido = pedido.Cliente!.Nome,
                Descricao = $"Reembolso devolução #{devolucao.Id} — pedido #{pedidoId}",
                NumeroParcela = 1,
                TotalParcelas = 1,
                Valor = abatimento.Reembolso,
                Vencimento = dados.VencimentoReembolso ?? hoje,
                Status = StatusParcelaPagar.Pendente
            };
            contexto.ParcelasPagar.Add(reembolso);
        }

        // DV6: só o reembolsado já gerou comissão; o abatido deixa de gerar quando a parcela for recebida.
        var estorno = DevolucaoCalculo.Estorno(abatimento.Reembolso, pedido.PercentualComissao);
        if (estorno < 0 && pedido.VendedorId is int vendedorId)
        {
            contexto.Comissoes.Add(new Comissao
            {
                DevolucaoId = devolucao.Id,
                PedidoId = pedidoId,
                VendedorId = vendedorId,
                ValorBase = abatimento.Reembolso,
                Percentual = pedido.PercentualComissao!.Value,
                Valor = estorno,
                DataGeracao = devolucao.DataDevolucao,
                Status = StatusComissao.Pendente
            });
        }

        // DV7
        estoqueService.DevolverAoEstoque(pedidoId, devolucao.Id,
            novos.Where(n => n.VoltaEstoque).Select(n => (n.PedidoItem!.ProdutoId, n.Quantidade)));

        await contexto.SaveChangesAsync(cancelamento);
        await transacao.CommitAsync(cancelamento);

        return ParaResposta(devolucao, reembolso?.Id, estorno);
    }

    public async Task<IReadOnlyList<DevolucaoRespostaDto>?> ListarAsync(int pedidoId, CancellationToken cancelamento)
    {
        if (!await contexto.Pedidos.AnyAsync(p => p.Id == pedidoId, cancelamento))
            return null;

        var devolucoes = await contexto.Devolucoes.AsNoTracking()
            .Include(d => d.Itens).ThenInclude(i => i.PedidoItem).ThenInclude(i => i!.Produto)
            .Where(d => d.PedidoId == pedidoId)
            .OrderBy(d => d.Id)
            .ToListAsync(cancelamento);

        var ids = devolucoes.Select(d => d.Id).ToList();
        var reembolsos = await contexto.ParcelasPagar.AsNoTracking()
            .Where(p => p.DevolucaoId != null && ids.Contains(p.DevolucaoId.Value))
            .ToDictionaryAsync(p => p.DevolucaoId!.Value, p => p.Id, cancelamento);
        var estornos = await contexto.Comissoes.AsNoTracking()
            .Where(c => c.DevolucaoId != null && ids.Contains(c.DevolucaoId.Value))
            .ToDictionaryAsync(c => c.DevolucaoId!.Value, c => c.Valor, cancelamento);

        return devolucoes
            .Select(d => ParaResposta(d, reembolsos.TryGetValue(d.Id, out var conta) ? conta : null, estornos.GetValueOrDefault(d.Id)))
            .ToList();
    }

    public async Task<DevolucoesResumoDto> ObterResumoMesAsync(CancellationToken cancelamento)
    {
        var hoje = HorarioBrasilia.Hoje();
        var primeiroDia = new DateOnly(hoje.Year, hoje.Month, 1);
        var (inicioMes, inicioProximoMes) = (HorarioBrasilia.InicioDoDiaUtc(primeiroDia), HorarioBrasilia.InicioDoDiaUtc(primeiroDia.AddMonths(1)));
        var doMes = contexto.Devolucoes.Where(d => d.DataDevolucao >= inicioMes && d.DataDevolucao < inicioProximoMes);

        return new DevolucoesResumoDto(await doMes.SumAsync(d => d.ValorTotal, cancelamento), await doMes.CountAsync(cancelamento));
    }

    /// <summary>Quantidade já devolvida por item do pedido (id do item → quantidade).</summary>
    private async Task<Dictionary<int, decimal>> QuantidadesDevolvidasAsync(int pedidoId, CancellationToken cancelamento) =>
        await contexto.DevolucaoItens
            .Where(i => i.Devolucao!.PedidoId == pedidoId)
            .GroupBy(i => i.PedidoItemId)
            .Select(g => new { g.Key, Total = g.Sum(i => i.Quantidade) })
            .ToDictionaryAsync(x => x.Key, x => x.Total, cancelamento);

    private static DevolucaoRespostaDto ParaResposta(Devolucao d, int? parcelaPagarId, decimal estorno) => new(
        d.Id, d.PedidoId, d.DataDevolucao, d.Motivo, d.ValorTotal, d.ValorAbatido, d.ValorReembolso, parcelaPagarId, estorno,
        d.Itens.OrderBy(i => i.Id).Select(i => new DevolucaoItemRespostaDto(
            i.Id, i.PedidoItemId, i.PedidoItem!.ProdutoId, i.PedidoItem.Produto!.Nome, i.PedidoItem.Produto.Unidade,
            i.Quantidade, i.Valor, i.VoltaEstoque)).ToList());
}
