// =====================================================================================
// Arquivo....: ContasPagarService.cs
// Versão.....: 2.0.0
// Data.......: 23/09/2026
// Descrição..: Contas a pagar de três origens (Compra, Comissao, Avulsa): listagem com
//              favorecido/descrição e "atrasado" calculados no servidor, marcar como paga,
//              cancelar (avulsa/comissão), lançar conta avulsa em parcelas, gerar/cancelar
//              as parcelas da compra. Pagar ou cancelar uma conta de comissão propaga para
//              as comissões ligadas (SPEC.md etapa 12, CC3/CC4).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.parcelas_pagar
//                - SELECT : listagem (LEFT JOIN pedidos_compra/fornecedores e vendedores;
//                           busca por nº da compra ou ILIKE em fornecedor/vendedor/
//                           favorecido/descrição; filtros de status e origem; ORDER BY
//                           vencimento, id, LIMIT/OFFSET) e Pendentes de uma compra
//                - INSERT : parcelas da compra (GerarParcelas) e da conta avulsa
//                - UPDATE : pagar (status + data_pagamento); cancelar (avulsa/comissão e
//                           Pendentes da compra cancelada)
//              public.comissoes
//                - UPDATE : Paga ao pagar a conta de comissão; Pendente (desligada) ao cancelar
// Fontes.....: ErpPortfolioDbContext (EF Core / Npgsql). GerarParcelas e
//              CancelarPendentesAsync não chamam SaveChanges: ficam na mesma transação do
//              PedidoCompraService.ConfirmarAsync/CancelarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   2.0.0 - 23/09/2026 - Origem Compra/Comissao/Avulsa: favorecido e descrição na lista,
//                        filtro de origem, conta avulsa, cancelar e propagação para as
//                        comissões (etapa 12).
// =====================================================================================

using System.Linq.Expressions;
using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class ContasPagarService(ErpPortfolioDbContext contexto) : IContasPagarService
{
    public async Task<ResultadoPaginadoDto<ParcelaPagarRespostaDto>> ListarAsync(ParcelaPagarFiltroDto filtro, CancellationToken cancelamento)
    {
        // "Hoje" do servidor (UTC), igual ao Contas a Receber: nunca o relógio do navegador.
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var consulta = contexto.ParcelasPagar.AsNoTracking();

        // CP2: nº da compra ("4" ou "#4") ou trecho do favorecido (fornecedor, vendedor ou texto) ou da descrição.
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var texto = filtro.Busca.Trim();
            var padrao = $"%{ClienteService.EscaparCuringasLike(texto)}%";
            // Número só casa com compra de verdade: um "numero" nulo compararia com pedido_compra_id IS NULL
            // e traria todas as avulsas/comissões.
            var ehNumero = int.TryParse(texto.TrimStart('#'), out var numero);
            consulta = consulta.Where(p =>
                (ehNumero && p.PedidoCompraId == numero)
                || EF.Functions.ILike(p.PedidoCompra!.Fornecedor!.Nome, padrao)
                || EF.Functions.ILike(p.Vendedor!.Nome, padrao)
                || EF.Functions.ILike(p.Favorecido!, padrao)
                || EF.Functions.ILike(p.Descricao!, padrao));
        }

        if (filtro.Origem is OrigemContaPagar origem)
            consulta = consulta.Where(p => p.Origem == origem);

        consulta = filtro.Status switch
        {
            FiltroStatusParcelaPagar.Atrasado => consulta.Where(p => p.Status == StatusParcelaPagar.Pendente && p.Vencimento < hoje),
            FiltroStatusParcelaPagar.Pendente => consulta.Where(p => p.Status == StatusParcelaPagar.Pendente),
            FiltroStatusParcelaPagar.Pago => consulta.Where(p => p.Status == StatusParcelaPagar.Pago),
            FiltroStatusParcelaPagar.Cancelado => consulta.Where(p => p.Status == StatusParcelaPagar.Cancelado),
            _ => consulta,
        };

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(p => p.Vencimento)
            .ThenBy(p => p.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(Projecao(hoje))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<ParcelaPagarRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<ParcelaPagarRespostaDto?> MarcarPagaAsync(int id, CancellationToken cancelamento)
    {
        var parcela = await contexto.ParcelasPagar.FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (parcela is null)
            return null;

        if (parcela.Status == StatusParcelaPagar.Cancelado)
            throw new ConflitoException("Esta parcela foi cancelada e não pode ser paga.");

        // Pagar de novo é sucesso (P4): o resultado desejado já é o estado atual.
        if (parcela.Status != StatusParcelaPagar.Pago)
        {
            parcela.Status = StatusParcelaPagar.Pago;
            parcela.DataPagamento = DateTime.UtcNow;

            // CC3: as comissões desta conta ficam pagas junto, com a mesma data.
            if (parcela.Origem == OrigemContaPagar.Comissao)
            {
                var comissoes = await contexto.Comissoes.Where(c => c.ParcelaPagarId == id).ToListAsync(cancelamento);
                foreach (var comissao in comissoes)
                {
                    comissao.Status = StatusComissao.Paga;
                    comissao.DataPagamento = parcela.DataPagamento;
                }
            }

            await contexto.SaveChangesAsync(cancelamento);
        }

        return await ObterAsync(id, cancelamento);
    }

    public async Task<ParcelaPagarRespostaDto?> CancelarAsync(int id, CancellationToken cancelamento)
    {
        var parcela = await contexto.ParcelasPagar.FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (parcela is null)
            return null;

        // CP4: parcela de compra só é cancelada junto com o pedido de compra.
        if (parcela.Origem == OrigemContaPagar.Compra)
            throw new ConflitoException("Esta parcela é de um pedido de compra: cancele o pedido de compra.");

        if (parcela.Status == StatusParcelaPagar.Pago)
            throw new ConflitoException("Esta parcela já foi paga e não pode ser cancelada.");

        // Cancelar de novo é sucesso.
        if (parcela.Status != StatusParcelaPagar.Cancelado)
        {
            parcela.Status = StatusParcelaPagar.Cancelado;

            // CC4: as comissões voltam a Pendente, desligadas, para poderem gerar outra conta.
            if (parcela.Origem == OrigemContaPagar.Comissao)
            {
                var comissoes = await contexto.Comissoes.Where(c => c.ParcelaPagarId == id).ToListAsync(cancelamento);
                foreach (var comissao in comissoes)
                {
                    comissao.Status = StatusComissao.Pendente;
                    comissao.ParcelaPagarId = null;
                }
            }

            await contexto.SaveChangesAsync(cancelamento);
        }

        return await ObterAsync(id, cancelamento);
    }

    public async Task<IReadOnlyList<ParcelaPagarRespostaDto>> CriarAvulsaAsync(ContaAvulsaCriacaoDto dados, CancellationToken cancelamento)
    {
        var parcelas = ContasPagarCalculo.ParcelasAvulsa(dados.ValorTotal, dados.NumeroParcelas, dados.PrimeiroVencimento!.Value, dados.IntervaloDias)
            .Select((p, i) => new ParcelaPagar
            {
                Origem = OrigemContaPagar.Avulsa,
                Descricao = dados.Descricao.Trim(),
                Favorecido = dados.Favorecido,
                NumeroParcela = i + 1,
                TotalParcelas = dados.NumeroParcelas,
                Valor = p.Valor,
                Vencimento = p.Vencimento,
                Status = StatusParcelaPagar.Pendente
            })
            .ToList();

        contexto.ParcelasPagar.AddRange(parcelas);
        await contexto.SaveChangesAsync(cancelamento);

        var ids = parcelas.Select(p => p.Id).ToList();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return await contexto.ParcelasPagar.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.NumeroParcela)
            .Select(Projecao(hoje))
            .ToListAsync(cancelamento);
    }

    public void GerarParcelas(PedidoCompra pedido, int numeroParcelas, int intervaloDias)
    {
        // Mesma divisão do Contas a Receber (P1): centavos para baixo, resto na última.
        var valores = ContasReceberCalculo.Dividir(pedido.ValorTotal, numeroParcelas);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var i = 0; i < numeroParcelas; i++)
        {
            contexto.ParcelasPagar.Add(new ParcelaPagar
            {
                Origem = OrigemContaPagar.Compra,
                PedidoCompraId = pedido.Id,
                NumeroParcela = i + 1,
                TotalParcelas = numeroParcelas,
                Valor = valores[i],
                Vencimento = hoje.AddDays((i + 1) * intervaloDias),
                Status = StatusParcelaPagar.Pendente
            });
        }
    }

    public async Task CancelarPendentesAsync(int pedidoCompraId, CancellationToken cancelamento)
    {
        var pendentes = await contexto.ParcelasPagar
            .Where(p => p.PedidoCompraId == pedidoCompraId && p.Status == StatusParcelaPagar.Pendente)
            .ToListAsync(cancelamento);

        foreach (var parcela in pendentes)
            parcela.Status = StatusParcelaPagar.Cancelado;
    }

    public async Task<ContasPagarResumoDto> ObterResumoAsync(CancellationToken cancelamento)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var pendentes = await contexto.ParcelasPagar.AsNoTracking()
            .Where(p => p.Status == StatusParcelaPagar.Pendente)
            .Select(p => new { p.Valor, p.Vencimento })
            .ToListAsync(cancelamento);

        var atrasadas = pendentes.Where(p => p.Vencimento < hoje).ToList();
        return new ContasPagarResumoDto(
            pendentes.Sum(p => p.Valor), pendentes.Count,
            atrasadas.Sum(p => p.Valor), atrasadas.Count);
    }

    private async Task<ParcelaPagarRespostaDto?> ObterAsync(int id, CancellationToken cancelamento) =>
        await contexto.ParcelasPagar.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(Projecao(DateOnly.FromDateTime(DateTime.UtcNow)))
            .FirstOrDefaultAsync(cancelamento);

    // CP2: o favorecido vem do fornecedor (compra), do vendedor (comissão) ou do texto (avulsa).
    // Uma projeção só para a lista e para as respostas de pagar/cancelar/criar (traduzida em SQL).
    private static Expression<Func<ParcelaPagar, ParcelaPagarRespostaDto>> Projecao(DateOnly hoje) => p =>
        new ParcelaPagarRespostaDto(
            p.Id,
            p.Origem,
            p.PedidoCompraId,
            p.VendedorId,
            p.PedidoCompra != null ? p.PedidoCompra.Fornecedor!.Nome : p.Vendedor != null ? p.Vendedor.Nome : p.Favorecido,
            p.Descricao,
            p.NumeroParcela,
            p.TotalParcelas,
            p.Valor,
            p.Vencimento,
            p.Status,
            p.DataPagamento,
            p.Status == StatusParcelaPagar.Pendente && p.Vencimento < hoje);
}
