// =====================================================================================
// Arquivo....: ContasReceberService.cs
// Versão.....: 1.2.0
// Data.......: 22/09/2026
// Descrição..: Consulta de contas a receber (listagem paginada com "atrasado" calculado
//              no servidor), marcar parcela como recebida, gerar as parcelas ao
//              confirmar um pedido e cancelar as pendentes ao cancelar um confirmado.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.parcelas_receber
//                - SELECT : listagem (JOIN pedidos/clientes, busca por nº do pedido ou
//                           nome do cliente, filtro de status incluindo "Atrasado",
//                           ORDER BY vencimento, id, LIMIT/OFFSET), contagem de parcelas
//                           irmãs do mesmo pedido (subconsulta correlacionada) e busca
//                           das parcelas Pendentes de um pedido (CancelarPendentesAsync)
//                - UPDATE : marcar como recebida (status + data_recebimento); marcar
//                           Pendentes como Cancelado ao cancelar o pedido
//                - INSERT : geração das parcelas (GerarParcelas)
// Fontes.....: ErpPortfolioDbContext.ParcelasReceber / Pedidos (EF Core / Npgsql).
//              GerarParcelas e CancelarPendentesAsync não chamam SaveChanges: ficam na
//              mesma transação do PedidoService.ConfirmarAsync/CancelarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e marcar recebido).
//   1.1.0 - 22/09/2026 - GerarParcelas (usado pelo PedidoService ao confirmar).
//   1.2.0 - 22/09/2026 - CancelarPendentesAsync (usado pelo PedidoService ao cancelar).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class ContasReceberService(ErpPortfolioDbContext contexto) : IContasReceberService
{
    public async Task<ResultadoPaginadoDto<ParcelaRespostaDto>> ListarAsync(ParcelaFiltroDto filtro, CancellationToken cancelamento)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var consulta = contexto.ParcelasReceber.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var texto = filtro.Busca.Trim();
            var padrao = $"%{ClienteService.EscaparCuringasLike(texto)}%";

            consulta = int.TryParse(texto.TrimStart('#'), out var numeroPedido)
                ? consulta.Where(p => p.PedidoId == numeroPedido || EF.Functions.ILike(p.Pedido!.Cliente!.Nome, padrao))
                : consulta.Where(p => EF.Functions.ILike(p.Pedido!.Cliente!.Nome, padrao));
        }

        consulta = filtro.Status switch
        {
            FiltroStatusParcela.Atrasado => consulta.Where(p => p.Status == StatusParcela.Pendente && p.Vencimento < hoje),
            FiltroStatusParcela.Pendente => consulta.Where(p => p.Status == StatusParcela.Pendente),
            FiltroStatusParcela.Recebido => consulta.Where(p => p.Status == StatusParcela.Recebido),
            FiltroStatusParcela.Cancelado => consulta.Where(p => p.Status == StatusParcela.Cancelado),
            _ => consulta,
        };

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(p => p.Vencimento)
            .ThenBy(p => p.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            // TotalParcelas por subconsulta correlacionada inline (um método de instância separado não é traduzido pelo EF).
            .Select(p => new ParcelaRespostaDto(
                p.Id,
                p.PedidoId,
                p.Pedido!.Cliente!.Nome,
                p.NumeroParcela,
                contexto.ParcelasReceber.Count(x => x.PedidoId == p.PedidoId),
                p.Valor,
                p.Vencimento,
                p.Status,
                p.DataRecebimento,
                p.Status == StatusParcela.Pendente && p.Vencimento < hoje))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<ParcelaRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<ParcelaRespostaDto?> MarcarRecebidaAsync(int id, CancellationToken cancelamento)
    {
        var parcela = await contexto.ParcelasReceber
            .Include(p => p.Pedido).ThenInclude(pedido => pedido!.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (parcela is null)
            return null;

        if (parcela.Status == StatusParcela.Cancelado)
            throw new ConflitoException("Esta parcela foi cancelada e não pode ser recebida.");

        // Idempotente (C7): marcar de novo uma já recebida não muda a data.
        if (parcela.Status != StatusParcela.Recebido)
        {
            parcela.Status = StatusParcela.Recebido;
            parcela.DataRecebimento = DateTime.UtcNow;
            await contexto.SaveChangesAsync(cancelamento);
        }

        var totalParcelas = await contexto.ParcelasReceber.CountAsync(p => p.PedidoId == parcela.PedidoId, cancelamento);

        return new ParcelaRespostaDto(
            parcela.Id, parcela.PedidoId, parcela.Pedido!.Cliente!.Nome, parcela.NumeroParcela, totalParcelas,
            parcela.Valor, parcela.Vencimento, parcela.Status, parcela.DataRecebimento, Atrasado: false);
    }

    public void GerarParcelas(Pedido pedido, int numeroParcelas, int intervaloDias)
    {
        var valores = ContasReceberCalculo.Dividir(pedido.ValorTotal, numeroParcelas);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var i = 0; i < numeroParcelas; i++)
        {
            contexto.ParcelasReceber.Add(new ParcelaReceber
            {
                PedidoId = pedido.Id,
                NumeroParcela = i + 1,
                Valor = valores[i],
                // Parcela 1 vence em intervaloDias, nunca no mesmo dia da confirmação (C3).
                Vencimento = hoje.AddDays((i + 1) * intervaloDias),
                Status = StatusParcela.Pendente
            });
        }
    }

    public async Task CancelarPendentesAsync(int pedidoId, CancellationToken cancelamento)
    {
        var pendentes = await contexto.ParcelasReceber
            .Where(p => p.PedidoId == pedidoId && p.Status == StatusParcela.Pendente)
            .ToListAsync(cancelamento);

        foreach (var parcela in pendentes)
            parcela.Status = StatusParcela.Cancelado;
    }
}
