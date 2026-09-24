// =====================================================================================
// Arquivo....: OrcamentoService.cs
// Versão.....: 1.3.0
// Data.......: 23/09/2026
// Descrição..: Regras de negócio e persistência de orçamentos (SPEC.md etapa 13). Mesma
//              forma do pedido de venda: o servidor copia o preço do produto (OR2) e
//              calcula o total (CalculoPedido); as validações de cliente, vendedor, produto
//              e quantidade são as do PedidoService (métodos internal static).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.orcamentos
//                - SELECT : listagem (busca, status/vencido, paginação) e detalhe
//                - INSERT : criar
//                - UPDATE : editar o Aberto; status Aprovado + pedido_id (gerar pedido);
//                           status Perdido + motivo_perda
//              public.orcamento_itens
//                - INSERT/UPDATE/DELETE : itens atualizados no lugar ao editar
//              public.pedidos, public.pedido_itens
//                - INSERT : pedido Rascunho gerado a partir do orçamento (GP3)
//              public.clientes, public.vendedores, public.produtos (SELECT: validações)
// Fontes.....: ErpPortfolioDbContext.Orcamentos / OrcamentoItens / Pedidos.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo (listar, obter, criar, editar).
//   1.1.0 - 23/09/2026 - Gerar pedido (GP1-GP4) e marcar como perdido (PE1).
//   1.2.0 - 23/09/2026 - PDF do orçamento (PD1, ExportadorOrcamento).
//   1.3.0 - 24/09/2026 - "Hoje" e limites de dia/mês em horário de Brasília (HorarioBrasilia).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class OrcamentoService(ErpPortfolioDbContext contexto) : IOrcamentoService
{
    // Mesma referência de "hoje" de Contas a Receber (OR3).
    public async Task<ResultadoPaginadoDto<OrcamentoResumoDto>> ListarAsync(OrcamentoFiltroDto filtro, CancellationToken cancelamento)
    {
        var hoje = HorarioBrasilia.Hoje();
        var consulta = contexto.Orcamentos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var texto = filtro.Busca.Trim();
            var padrao = $"%{ClienteService.EscaparCuringasLike(texto)}%";

            consulta = int.TryParse(texto.TrimStart('#'), out var numero)
                ? consulta.Where(o => o.Id == numero || EF.Functions.ILike(o.Cliente!.Nome, padrao))
                : consulta.Where(o => EF.Functions.ILike(o.Cliente!.Nome, padrao));
        }

        consulta = filtro.Status switch
        {
            FiltroStatusOrcamento.Aberto => consulta.Where(o => o.Status == StatusOrcamento.Aberto && o.Validade >= hoje),
            FiltroStatusOrcamento.Vencido => consulta.Where(o => o.Status == StatusOrcamento.Aberto && o.Validade < hoje),
            FiltroStatusOrcamento.Aprovado => consulta.Where(o => o.Status == StatusOrcamento.Aprovado),
            FiltroStatusOrcamento.Perdido => consulta.Where(o => o.Status == StatusOrcamento.Perdido),
            _ => consulta
        };

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderByDescending(o => o.DataOrcamento)
            .ThenByDescending(o => o.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(o => new OrcamentoResumoDto(
                o.Id, o.ClienteId, o.Cliente!.Nome, o.DataOrcamento, o.Validade, o.Status,
                o.Status == StatusOrcamento.Aberto && o.Validade < hoje,
                o.ValorTotal, o.Itens.Count, o.PedidoId))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<OrcamentoResumoDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<OrcamentoRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        var orcamento = await contexto.Orcamentos.AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.Vendedor)
            .Include(o => o.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id, cancelamento);

        return orcamento is null ? null : OrcamentoRespostaDto.DeEntidade(orcamento, HorarioBrasilia.Hoje());
    }

    public async Task<OrcamentoRespostaDto> CriarAsync(OrcamentoCriacaoDto dados, CancellationToken cancelamento)
    {
        ValidarValidade(dados.Validade!.Value);

        var cliente = await PedidoService.ObterClienteAtivoAsync(contexto, dados.ClienteId!.Value, cancelamento);
        var vendedor = dados.VendedorId is int vendedorId ? await PedidoService.ObterVendedorAtivoAsync(contexto, vendedorId, cancelamento) : null;
        var produtos = await PedidoService.ObterProdutosAsync(contexto, dados.Itens.Select(i => i.ProdutoId), cancelamento);

        var orcamento = new Orcamento
        {
            Cliente = cliente,
            ClienteId = cliente.Id,
            Vendedor = vendedor,
            VendedorId = vendedor?.Id,
            DataOrcamento = DateTime.UtcNow,
            Status = StatusOrcamento.Aberto
        };
        CopiarCabecalho(orcamento, dados);

        foreach (var entrada in dados.Itens)
            orcamento.Itens.Add(NovoItem(PedidoService.ValidarProdutoDoItem(produtos, entrada), entrada));

        Recalcular(orcamento);

        contexto.Orcamentos.Add(orcamento);
        await contexto.SaveChangesAsync(cancelamento);

        return (await ObterPorIdAsync(orcamento.Id, cancelamento))!;
    }

    public async Task<OrcamentoRespostaDto?> AtualizarAsync(int id, OrcamentoCriacaoDto dados, CancellationToken cancelamento)
    {
        var orcamento = await contexto.Orcamentos.Include(o => o.Itens).FirstOrDefaultAsync(o => o.Id == id, cancelamento);
        if (orcamento is null)
            return null;

        // OR5: só o Aberto (vencido ou não) é editável.
        if (orcamento.Status != StatusOrcamento.Aberto)
            throw new ConflitoException($"O orçamento {id} está {orcamento.Status} e não pode ser editado; só o aberto pode.");

        ValidarValidade(dados.Validade!.Value);

        // Cliente e vendedor: só exigem "ativo" se foram trocados (mesma regra do rascunho de pedido).
        if (dados.ClienteId!.Value != orcamento.ClienteId)
        {
            var cliente = await PedidoService.ObterClienteAtivoAsync(contexto, dados.ClienteId.Value, cancelamento);
            orcamento.Cliente = cliente;
            orcamento.ClienteId = cliente.Id;
        }

        if (dados.VendedorId != orcamento.VendedorId)
        {
            var vendedor = dados.VendedorId is int vendedorId ? await PedidoService.ObterVendedorAtivoAsync(contexto, vendedorId, cancelamento) : null;
            orcamento.Vendedor = vendedor;
            orcamento.VendedorId = vendedor?.Id;
        }

        CopiarCabecalho(orcamento, dados);

        var produtos = await PedidoService.ObterProdutosAsync(contexto, dados.Itens.Select(i => i.ProdutoId), cancelamento);

        // Itens atualizados no lugar, casando por produto: o que já existia mantém o preço congelado (OR2)
        // e não há DELETE + INSERT da mesma chave (orçamento, produto) no mesmo salvamento (ver PedidoService).
        foreach (var entrada in dados.Itens)
        {
            var existente = orcamento.Itens.FirstOrDefault(i => i.ProdutoId == entrada.ProdutoId);
            if (existente is null)
            {
                orcamento.Itens.Add(NovoItem(PedidoService.ValidarProdutoDoItem(produtos, entrada), entrada));
            }
            else
            {
                PedidoService.ValidarQuantidadeNaUnidade(produtos[entrada.ProdutoId], entrada);
                existente.Quantidade = entrada.Quantidade!.Value;
                existente.DescontoPercentual = entrada.DescontoPercentual;
            }
        }

        var idsRecebidos = dados.Itens.Select(i => i.ProdutoId).ToHashSet();
        var removidos = orcamento.Itens.Where(i => !idsRecebidos.Contains(i.ProdutoId)).ToList();
        contexto.OrcamentoItens.RemoveRange(removidos);
        foreach (var removido in removidos)
            orcamento.Itens.Remove(removido);

        Recalcular(orcamento);
        await contexto.SaveChangesAsync(cancelamento);

        return await ObterPorIdAsync(id, cancelamento);
    }

    public async Task<int?> GerarPedidoAsync(int id, CancellationToken cancelamento)
    {
        var orcamento = await contexto.Orcamentos
            .Include(o => o.Cliente)
            .Include(o => o.Vendedor)
            .Include(o => o.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id, cancelamento);
        if (orcamento is null)
            return null;

        // GP1
        if (orcamento.Status != StatusOrcamento.Aberto)
            throw new ConflitoException($"O orçamento {id} está {orcamento.Status}; só o aberto gera pedido.");

        if (orcamento.EstaVencido(HorarioBrasilia.Hoje()))
            throw new DadoInvalidoException(nameof(OrcamentoCriacaoDto.Validade), "O orçamento está vencido; prorrogue a validade para gerar o pedido.");

        // GP2: cliente e produtos precisam estar ativos; vendedor inativo só não vai para o pedido (é opcional no rascunho).
        if (!orcamento.Cliente!.Ativo)
            throw new DadoInvalidoException(nameof(OrcamentoCriacaoDto.ClienteId), "O cliente do orçamento está inativo.");

        var inativos = orcamento.Itens.Where(i => !i.Produto!.Ativo).Select(i => $"\"{i.Produto!.Nome}\"").ToList();
        if (inativos.Count > 0)
            throw new DadoInvalidoException(PedidoService.CampoItens, $"Produto(s) inativo(s) no orçamento: {string.Join(", ", inativos)}.");

        var vendedor = orcamento.Vendedor is { Ativo: true } ? orcamento.Vendedor : null;

        // GP3: rascunho com os preços e descontos do orçamento (não os atuais do produto). Montado aqui e não
        // pelo PedidoService.CriarAsync, que copia o preço atual (R3).
        var pedido = new Pedido
        {
            ClienteId = orcamento.ClienteId,
            VendedorId = vendedor?.Id,
            DataPedido = DateTime.UtcNow,
            Status = StatusPedido.Rascunho,
            FormaPagamento = orcamento.FormaPagamento,
            DescontoPercentual = orcamento.DescontoPercentual,
            ValorTotal = orcamento.ValorTotal,
            Itens = [.. orcamento.Itens.OrderBy(i => i.Id).Select(i => new PedidoItem
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario,
                DescontoPercentual = i.DescontoPercentual
            })]
        };

        // GP4: um SaveChanges só (INSERT do pedido e UPDATE do orçamento na mesma transação).
        // ponytail: sem trava de concorrência (como o resto do ERP, monousuário); dois cliques simultâneos gerariam
        // dois pedidos. Se virar multiusuário: SELECT ... FOR UPDATE no orçamento ou token de concorrência (xmin).
        orcamento.Status = StatusOrcamento.Aprovado;
        orcamento.Pedido = pedido;
        contexto.Pedidos.Add(pedido);
        await contexto.SaveChangesAsync(cancelamento);

        return pedido.Id;
    }

    public async Task<bool> PerderAsync(int id, string? motivo, CancellationToken cancelamento)
    {
        var orcamento = await contexto.Orcamentos.FirstOrDefaultAsync(o => o.Id == id, cancelamento);
        if (orcamento is null)
            return false;

        // PE1: repetir é sucesso sem mudar nada (nem o motivo).
        if (orcamento.Status == StatusOrcamento.Perdido)
            return true;

        if (orcamento.Status == StatusOrcamento.Aprovado)
            throw new ConflitoException($"O orçamento {id} já virou o pedido {orcamento.PedidoId} e não pode ser marcado como perdido.");

        orcamento.Status = StatusOrcamento.Perdido;
        orcamento.MotivoPerda = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        await contexto.SaveChangesAsync(cancelamento);

        return true;
    }

    public async Task<byte[]?> GerarPdfAsync(int id, CancellationToken cancelamento)
    {
        var orcamento = await contexto.Orcamentos.AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.Vendedor)
            .Include(o => o.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(o => o.Id == id, cancelamento);

        return orcamento is null ? null : ExportadorOrcamento.GerarPdf(orcamento, HorarioBrasilia.Hoje());
    }

    // OR1: validade não pode ser antes de hoje, ao criar e ao editar (editar com data nova "prorroga" o vencido).
    private static void ValidarValidade(DateOnly validade)
    {
        if (validade < HorarioBrasilia.Hoje())
            throw new DadoInvalidoException(nameof(OrcamentoCriacaoDto.Validade), "A validade não pode ser anterior a hoje.");
    }

    private static void CopiarCabecalho(Orcamento orcamento, OrcamentoCriacaoDto dados)
    {
        orcamento.Validade = dados.Validade!.Value;
        orcamento.FormaPagamento = dados.FormaPagamento;
        orcamento.DescontoPercentual = dados.DescontoPercentual;
        orcamento.Observacoes = string.IsNullOrWhiteSpace(dados.Observacoes) ? null : dados.Observacoes.Trim();
    }

    // OR2: preço copiado do produto agora e congelado no item.
    private static OrcamentoItem NovoItem(Produto produto, PedidoItemEntradaDto entrada) => new()
    {
        Produto = produto,
        ProdutoId = produto.Id,
        Quantidade = entrada.Quantidade!.Value,
        PrecoUnitario = produto.PrecoVenda,
        DescontoPercentual = entrada.DescontoPercentual
    };

    private static void Recalcular(Orcamento orcamento)
    {
        var subtotais = orcamento.Itens.Select(i => CalculoPedido.Subtotal(i.Quantidade, i.PrecoUnitario, i.DescontoPercentual));
        orcamento.ValorTotal = CalculoPedido.Total(subtotais, orcamento.DescontoPercentual);

        if (orcamento.ValorTotal > CalculoPedido.LimiteTotal)
            throw new DadoInvalidoException(PedidoService.CampoItens, "O total do orçamento não pode passar de R$ 9.999.999.999,99.");
    }
}
