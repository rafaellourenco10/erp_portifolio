// =====================================================================================
// Arquivo....: ComissaoService.cs
// Versão.....: 1.2.0
// Data.......: 23/09/2026
// Descrição..: Consulta de comissões (filtro por vendedor, status e período da data do
//              recebimento, com totais do filtro calculados no servidor) e pagamento ao
//              vendedor em lote (etapa 11); na etapa 12 o pagamento passa pelo Contas a
//              Pagar: GerarContaAsync fecha as comissões numa conta a pagar (CC1/CC2).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.comissoes
//                - SELECT : listagem (JOIN vendedores, pedidos/clientes, parcelas_receber;
//                           ORDER BY data_geracao DESC, id DESC, LIMIT/OFFSET), contagem de
//                           parcelas irmãs (X de Y) e somas por status do filtro
//                - UPDATE : status Pendente -> EmPagamento e parcela_pagar_id (gerar conta)
//              public.parcelas_pagar
//                - INSERT : a conta a pagar da comissão (origem Comissao, 1/1, soma)
// Fontes.....: ErpPortfolioDbContext.Comissoes (EF Core / Npgsql), AsNoTracking na leitura.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - GerarContaAsync substitui PagarAsync; total Em pagamento (etapa 12).
//   1.2.0 - 24/09/2026 - Estorno de devolução na lista; gerar conta inclui os estornos pendentes do
//                        vendedor e recusa soma <= 0 (etapa 14, CC6/CC7).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class ComissaoService(ErpPortfolioDbContext contexto) : IComissaoService
{
    public async Task<ComissaoListaDto> ListarAsync(ComissaoFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Comissoes.AsNoTracking();

        if (filtro.VendedorId is int vendedorId)
            consulta = consulta.Where(c => c.VendedorId == vendedorId);

        // Datas inclusivas em UTC, mesma convenção dos relatórios e do Dashboard.
        if (filtro.DataInicio is DateOnly inicio)
        {
            var desde = inicio.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            consulta = consulta.Where(c => c.DataGeracao >= desde);
        }
        if (filtro.DataFim is DateOnly fim)
        {
            var ate = fim.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            consulta = consulta.Where(c => c.DataGeracao < ate);
        }

        // Totais antes do filtro de status: os cards mostram o quadro do período/vendedor inteiro.
        var somas = await consulta
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Total = g.Sum(c => c.Valor) })
            .ToListAsync(cancelamento);
        decimal Soma(StatusComissao status) => somas.Where(s => s.Status == status).Sum(s => s.Total);
        var (pendente, emPagamento, pago) = (Soma(StatusComissao.Pendente), Soma(StatusComissao.EmPagamento), Soma(StatusComissao.Paga));

        if (filtro.Status is StatusComissao status)
            consulta = consulta.Where(c => c.Status == status);

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderByDescending(c => c.DataGeracao)
            .ThenByDescending(c => c.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(c => new ComissaoRespostaDto(
                c.Id,
                c.VendedorId,
                c.Vendedor!.Nome,
                c.PedidoId,
                c.Pedido!.Cliente!.Nome,
                c.ParcelaReceberId == null ? null : (int?)c.ParcelaReceber!.NumeroParcela,
                contexto.ParcelasReceber.Count(p => p.PedidoId == c.PedidoId),
                c.ValorBase,
                c.Percentual,
                c.Valor,
                c.DataGeracao,
                c.Status,
                c.DataPagamento,
                c.ParcelaPagarId,
                c.DevolucaoId))
            .ToListAsync(cancelamento);

        return new ComissaoListaDto(
            new ResultadoPaginadoDto<ComissaoRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens),
            new ComissaoTotaisDto(pendente + emPagamento + pago, pendente, emPagamento, pago));
    }

    public async Task<ComissaoContaGeradaDto> GerarContaAsync(IReadOnlyCollection<int> ids, DateOnly vencimento, CancellationToken cancelamento)
    {
        var distintos = ids.Distinct().ToList();
        var comissoes = await contexto.Comissoes
            .Include(c => c.Vendedor)
            .Where(c => distintos.Contains(c.Id))
            .ToListAsync(cancelamento);

        // CC1: tudo ou nada — nada é alterado se alguma regra falhar.
        var inexistentes = distintos.Except(comissoes.Select(c => c.Id)).ToList();
        if (inexistentes.Count > 0)
            throw new DadoInvalidoException(nameof(ComissaoGerarContaDto.Ids), $"Comissão(ões) inexistente(s): {string.Join(", ", inexistentes.Select(id => $"#{id}"))}.");

        var naoPendentes = comissoes.Where(c => c.Status != StatusComissao.Pendente).Select(c => $"#{c.Id}").ToList();
        if (naoPendentes.Count > 0)
            throw new DadoInvalidoException(nameof(ComissaoGerarContaDto.Ids), $"Só comissões a pagar podem gerar conta; já em pagamento ou pagas: {string.Join(", ", naoPendentes)}.");

        if (comissoes.Select(c => c.VendedorId).Distinct().Count() > 1)
            throw new DadoInvalidoException(nameof(ComissaoGerarContaDto.Ids), "Selecione comissões de um único vendedor.");

        // CC6: os estornos de devolução pendentes do vendedor entram sempre, mesmo sem selecionar.
        var vendedorId = comissoes[0].VendedorId;
        var idsSelecionados = comissoes.Select(c => c.Id).ToList();
        comissoes.AddRange(await contexto.Comissoes
            .Where(c => c.VendedorId == vendedorId && c.Status == StatusComissao.Pendente && c.Valor < 0 && !idsSelecionados.Contains(c.Id))
            .ToListAsync(cancelamento));

        var total = comissoes.Sum(c => c.Valor);
        if (total <= 0)
            throw new DadoInvalidoException(nameof(ComissaoGerarContaDto.Ids),
                $"Os estornos de devolução do vendedor ({comissoes.Where(c => c.Valor < 0).Sum(c => c.Valor):N2}) superam as comissões selecionadas; selecione mais comissões.");

        // CC2: uma parcela (1/1) com a soma; o vínculo e o status mudam no mesmo SaveChanges.
        var vendedor = comissoes[0].Vendedor!;
        var parcela = new ParcelaPagar
        {
            Origem = OrigemContaPagar.Comissao,
            VendedorId = vendedor.Id,
            Descricao = $"Comissões — {vendedor.Nome} ({comissoes.Count})",
            NumeroParcela = 1,
            TotalParcelas = 1,
            Valor = total,
            Vencimento = vencimento,
            Status = StatusParcelaPagar.Pendente
        };
        contexto.ParcelasPagar.Add(parcela);

        foreach (var comissao in comissoes)
        {
            comissao.ParcelaPagar = parcela;
            comissao.Status = StatusComissao.EmPagamento;
        }

        await contexto.SaveChangesAsync(cancelamento);

        return new ComissaoContaGeradaDto(parcela.Id, comissoes.Count, parcela.Valor, parcela.Vencimento);
    }
}
