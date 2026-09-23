// =====================================================================================
// Arquivo....: ContasPagarService.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Consulta de contas a pagar (listagem paginada com "atrasado" calculado
//              no servidor), marcar parcela como paga, gerar as parcelas ao confirmar
//              um pedido de compra e cancelar as pendentes ao cancelar um confirmado.
//              Espelho de ContasReceberService (mesma divisão em parcelas).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.parcelas_pagar
//                - SELECT : listagem (JOIN pedidos_compra/fornecedores, busca por nº do
//                           pedido de compra ou nome do fornecedor, filtro de status
//                           incluindo "Atrasado", ORDER BY vencimento, id, LIMIT/OFFSET),
//                           contagem de parcelas irmãs do mesmo pedido e busca das
//                           parcelas Pendentes de um pedido (CancelarPendentesAsync)
//                - UPDATE : marcar como paga (status + data_pagamento); marcar Pendentes
//                           como Cancelado ao cancelar o pedido de compra
//                - INSERT : geração das parcelas (GerarParcelas)
// Fontes.....: ErpPortfolioDbContext.ParcelasPagar (EF Core / Npgsql).
//              GerarParcelas e CancelarPendentesAsync não chamam SaveChanges: ficam na
//              mesma transação do PedidoCompraService.ConfirmarAsync/CancelarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

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

        // Busca: número do pedido de compra ("4" ou "#4") ou parte do nome do fornecedor (P6).
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var texto = filtro.Busca.Trim();
            var padrao = $"%{ClienteService.EscaparCuringasLike(texto)}%";
            consulta = int.TryParse(texto.TrimStart('#'), out var numeroPedido)
                ? consulta.Where(p => p.PedidoCompraId == numeroPedido || EF.Functions.ILike(p.PedidoCompra!.Fornecedor!.Nome, padrao))
                : consulta.Where(p => EF.Functions.ILike(p.PedidoCompra!.Fornecedor!.Nome, padrao));
        }

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
            .Select(p => new ParcelaPagarRespostaDto(
                p.Id,
                p.PedidoCompraId,
                p.PedidoCompra!.Fornecedor!.Nome,
                p.NumeroParcela,
                p.TotalParcelas,
                p.Valor,
                p.Vencimento,
                p.Status,
                p.DataPagamento,
                p.Status == StatusParcelaPagar.Pendente && p.Vencimento < hoje))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<ParcelaPagarRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<ParcelaPagarRespostaDto?> MarcarPagaAsync(int id, CancellationToken cancelamento)
    {
        var parcela = await contexto.ParcelasPagar
            .Include(p => p.PedidoCompra).ThenInclude(pedido => pedido!.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (parcela is null)
            return null;

        if (parcela.Status == StatusParcelaPagar.Cancelado)
            throw new ConflitoException("Esta parcela foi cancelada e não pode ser paga.");

        // Pagar de novo é sucesso (P4): o resultado desejado já é o estado atual.
        if (parcela.Status != StatusParcelaPagar.Pago)
        {
            parcela.Status = StatusParcelaPagar.Pago;
            parcela.DataPagamento = DateTime.UtcNow;
            await contexto.SaveChangesAsync(cancelamento);
        }

        return new ParcelaPagarRespostaDto(
            parcela.Id, parcela.PedidoCompraId, parcela.PedidoCompra!.Fornecedor!.Nome, parcela.NumeroParcela, parcela.TotalParcelas,
            parcela.Valor, parcela.Vencimento, parcela.Status, parcela.DataPagamento, Atrasado: false);
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
}
