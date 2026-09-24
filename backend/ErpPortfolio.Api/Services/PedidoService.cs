// =====================================================================================
// Arquivo....: PedidoService.cs
// Versão.....: 1.9.0
// Data.......: 23/09/2026
// Descrição..: Regras de negócio e persistência de pedidos de venda. O servidor decide o
//              preço (copiado do produto, R3) e o total (CalculoPedido); a API nunca
//              aceita preço nem total vindos do cliente. Confirmar baixa o estoque dos
//              itens (E2) e gera as parcelas a receber (C1); cancelar um pedido que
//              estava Confirmado devolve o estoque (E3) e cancela as parcelas pendentes (C6).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.pedidos, public.pedido_itens
//                - SELECT : listagem (busca por id ou ILIKE no nome do cliente, status,
//                           ORDER BY data_pedido DESC, id DESC, LIMIT/OFFSET),
//                           consulta por id (JOIN com clientes, vendedores e produtos) e
//                           validação de cliente, vendedor e produtos (existem e estão ativos)
//                - INSERT : criação do rascunho com os itens
//                - UPDATE : edição do rascunho (cabeçalho, valor_total e itens existentes)
//                - INSERT / DELETE em pedido_itens: itens novos e itens removidos na edição
//                - UPDATE : status (Rascunho -> Confirmado; Rascunho/Confirmado -> Cancelado) e
//                           percentual_comissao (copiado do vendedor ao confirmar)
//              public.estoque_movimentacoes (indiretamente, via IEstoqueService)
//                - INSERT : Saída por item ao confirmar; Entrada de estorno por item ao
//                           cancelar um pedido que estava Confirmado
//              public.parcelas_receber (indiretamente, via IContasReceberService)
//                - INSERT : parcelas geradas ao confirmar
//                - UPDATE : parcelas Pendentes viram Cancelado ao cancelar um Confirmado
// Fontes.....: ErpPortfolioDbContext.Pedidos / PedidoItens / Clientes / Produtos.
//              IEstoqueService.BaixarAsync / Estornar e IContasReceberService.GerarParcelas /
//              CancelarPendentesAsync não chamam SaveChanges: ficam na mesma transação do
//              SaveChangesAsync deste serviço.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo (criar e obter).
//   1.1.0 - 21/09/2026 - Listagem paginada com busca e filtro de status.
//   1.2.0 - 21/09/2026 - Edição do rascunho (PUT) com atualização dos itens no lugar.
//   1.3.0 - 21/09/2026 - Confirmar (R6) e cancelar (R7, idempotente).
//   1.4.0 - 22/09/2026 - Confirmar baixa estoque (E2); cancelar de Confirmado estorna (E3).
//   1.5.0 - 22/09/2026 - Confirmar gera parcelas a receber (C1).
//   1.6.0 - 22/09/2026 - Cancelar de Confirmado cancela as parcelas Pendentes (C6).
//   1.7.0 - 22/09/2026 - ObterResumoVendasAsync (faturamento, ticket médio, por status,
//                        faturamento diário do mês atual), para o Dashboard.
//   1.8.0 - 23/09/2026 - Vendedor no pedido (PV1) e confirmar exige vendedor ativo e congela a %
//                        de comissão (PV2/PV3), etapa 10.
//   1.9.0 - 23/09/2026 - Validações de cliente/vendedor/produto/item internal static, reaproveitadas
//                        pelo OrcamentoService (etapa 13).
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class PedidoService(ErpPortfolioDbContext contexto, IEstoqueService estoqueService, IContasReceberService contasReceberService) : IPedidoService
{
    internal const string CampoItens = "Itens";

    public async Task<ResultadoPaginadoDto<PedidoResumoDto>> ListarAsync(PedidoFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Pedidos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var texto = filtro.Busca.Trim();
            var padrao = $"%{ClienteService.EscaparCuringasLike(texto)}%";

            // Número do pedido (com ou sem #) OU trecho do nome do cliente: nomes com dígitos continuam achando pelo nome.
            consulta = int.TryParse(texto.TrimStart('#'), out var numero)
                ? consulta.Where(p => p.Id == numero || EF.Functions.ILike(p.Cliente!.Nome, padrao))
                : consulta.Where(p => EF.Functions.ILike(p.Cliente!.Nome, padrao));
        }

        if (filtro.Status is { } status)
        {
            consulta = consulta.Where(p => p.Status == status);
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderByDescending(p => p.DataPedido)
            .ThenByDescending(p => p.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(p => new PedidoResumoDto(p.Id, p.ClienteId, p.Cliente!.Nome, p.DataPedido, p.Status, p.ValorTotal, p.Itens.Count))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<PedidoResumoDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<PedidoRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        var pedido = await contexto.Pedidos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Vendedor)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id, cancelamento);

        return pedido is null ? null : PedidoRespostaDto.DeEntidade(pedido);
    }

    public async Task<PedidoRespostaDto> CriarAsync(PedidoCriacaoDto dados, CancellationToken cancelamento)
    {
        var cliente = await ObterClienteAtivoAsync(contexto, dados.ClienteId!.Value, cancelamento);
        // PV1: vendedor é opcional no rascunho, mas se vier precisa existir e estar ativo.
        var vendedor = dados.VendedorId is int vendedorId ? await ObterVendedorAtivoAsync(contexto, vendedorId, cancelamento) : null;
        var produtos = await ObterProdutosAsync(contexto, dados.Itens.Select(i => i.ProdutoId), cancelamento);

        var pedido = new Pedido
        {
            Cliente = cliente,
            ClienteId = cliente.Id,
            Status = StatusPedido.Rascunho,
            DataPedido = DateTime.UtcNow,
            FormaPagamento = dados.FormaPagamento,
            Vendedor = vendedor,
            VendedorId = vendedor?.Id,
            DescontoPercentual = dados.DescontoPercentual
        };

        foreach (var entrada in dados.Itens)
        {
            var produto = ValidarProdutoDoItem(produtos, entrada);
            pedido.Itens.Add(NovoItem(produto, entrada));
        }

        Recalcular(pedido);

        contexto.Pedidos.Add(pedido);
        await contexto.SaveChangesAsync(cancelamento);

        return (await ObterPorIdAsync(pedido.Id, cancelamento))!;
    }

    public async Task<PedidoRespostaDto?> AtualizarAsync(int id, PedidoCriacaoDto dados, CancellationToken cancelamento)
    {
        var pedido = await contexto.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (pedido is null)
            return null;

        if (!TransicoesPedido.PodeEditar(pedido.Status))
            throw new ConflitoException($"O pedido {id} está {pedido.Status} e não pode ser editado; só o rascunho pode.");

        // Cliente: só exige "ativo" se foi trocado (um rascunho antigo com cliente inativado depois continua editável).
        var clienteId = dados.ClienteId!.Value;
        if (clienteId != pedido.ClienteId)
        {
            var cliente = await ObterClienteAtivoAsync(contexto, clienteId, cancelamento);
            pedido.Cliente = cliente;
            pedido.ClienteId = cliente.Id;
        }

        pedido.FormaPagamento = dados.FormaPagamento;
        pedido.DescontoPercentual = dados.DescontoPercentual;

        // PV1: mesma regra do cliente — só exige "ativo" se o vendedor foi trocado; pode voltar a ficar vazio.
        if (dados.VendedorId != pedido.VendedorId)
        {
            var vendedor = dados.VendedorId is int vendedorId ? await ObterVendedorAtivoAsync(contexto, vendedorId, cancelamento) : null;
            pedido.Vendedor = vendedor;
            pedido.VendedorId = vendedor?.Id;
        }

        var produtos = await ObterProdutosAsync(contexto, dados.Itens.Select(i => i.ProdutoId), cancelamento);

        // Itens atualizados NO LUGAR, casando por produto: o item que já existia mantém o preço congelado (R3) e
        // não há DELETE + INSERT da mesma chave (pedido, produto) no mesmo salvamento, que quebraria o índice único.
        foreach (var entrada in dados.Itens)
        {
            var existente = pedido.Itens.FirstOrDefault(i => i.ProdutoId == entrada.ProdutoId);
            if (existente is null)
            {
                var produto = ValidarProdutoDoItem(produtos, entrada);
                pedido.Itens.Add(NovoItem(produto, entrada));
            }
            else
            {
                // Produto já no pedido: não precisa estar ativo (inativado depois; a confirmação é que barra), mas a quantidade segue a unidade.
                ValidarQuantidadeNaUnidade(produtos[entrada.ProdutoId], entrada);
                existente.Quantidade = entrada.Quantidade!.Value;
                existente.DescontoPercentual = entrada.DescontoPercentual;
            }
        }

        // Itens que saíram do pedido: apagados no banco E tirados da coleção. Só o RemoveRange não basta: o item
        // continua em pedido.Itens até o SaveChanges e o Recalcular somaria o item removido no total.
        var idsRecebidos = dados.Itens.Select(i => i.ProdutoId).ToHashSet();
        var removidos = pedido.Itens.Where(i => !idsRecebidos.Contains(i.ProdutoId)).ToList();
        contexto.PedidoItens.RemoveRange(removidos);
        foreach (var removido in removidos)
            pedido.Itens.Remove(removido);

        Recalcular(pedido);
        await contexto.SaveChangesAsync(cancelamento);

        return await ObterPorIdAsync(id, cancelamento);
    }

    public async Task<PedidoRespostaDto?> ConfirmarAsync(int id, int numeroParcelas, int intervaloDias, CancellationToken cancelamento)
    {
        var pedido = await contexto.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Vendedor)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (pedido is null)
            return null;

        if (!TransicoesPedido.PodeConfirmar(pedido.Status))
            throw new ConflitoException($"O pedido {id} está {pedido.Status} e não pode ser confirmado; só o rascunho pode.");

        // R6: confirmar exige forma de pagamento, cliente ativo e produtos ativos. O que foi inativado depois de o item
        // entrar no rascunho é barrado aqui (e não na edição), porque a venda só se fecha com cadastros ativos.
        if (pedido.FormaPagamento is null)
            throw new DadoInvalidoException(nameof(PedidoCriacaoDto.FormaPagamento), "Informe a forma de pagamento para confirmar o pedido.");

        if (!pedido.Cliente!.Ativo)
            throw new DadoInvalidoException(nameof(PedidoCriacaoDto.ClienteId), "O cliente do pedido está inativo.");

        // PV2: vendedor obrigatório e ativo para confirmar.
        if (pedido.Vendedor is null)
            throw new DadoInvalidoException(nameof(PedidoCriacaoDto.VendedorId), "Informe o vendedor para confirmar o pedido.");

        if (!pedido.Vendedor.Ativo)
            throw new DadoInvalidoException(nameof(PedidoCriacaoDto.VendedorId), "O vendedor do pedido está inativo.");

        var inativos = pedido.Itens.Where(i => !i.Produto!.Ativo).Select(i => $"\"{i.Produto!.Nome}\"").ToList();
        if (inativos.Count > 0)
            throw new DadoInvalidoException(CampoItens, $"Produto(s) inativo(s) no pedido: {string.Join(", ", inativos)}.");

        // E2: confere saldo de todos os itens e enfileira as Saídas; se faltar saldo em algum item, nada é
        // gravado (nem aqui nem no SaveChanges abaixo, que ainda não foi chamado).
        await estoqueService.BaixarAsync(pedido.Id, pedido.Itens, cancelamento);

        pedido.Status = StatusPedido.Confirmado;

        // PV3: congela a % de comissão do vendedor neste momento (não muda mais, mesmo que a % do vendedor mude).
        pedido.PercentualComissao = pedido.Vendedor.PercentualComissao;

        // C1: gera as parcelas a receber (não chama SaveChanges; entra no mesmo SaveChangesAsync abaixo).
        contasReceberService.GerarParcelas(pedido, numeroParcelas, intervaloDias);

        await contexto.SaveChangesAsync(cancelamento);

        return PedidoRespostaDto.DeEntidade(pedido);
    }

    public async Task<bool> CancelarAsync(int id, CancellationToken cancelamento)
    {
        var pedido = await contexto.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (pedido is null)
            return false;

        // Cancelar de novo é sucesso (R7): o resultado desejado já é o estado atual.
        if (pedido.Status == StatusPedido.Cancelado)
            return true;

        if (!TransicoesPedido.PodeCancelar(pedido.Status))
            throw new ConflitoException($"O pedido {id} está {pedido.Status} e não pode ser cancelado.");

        // E3/C6: só devolve estoque e cancela parcelas se o pedido JÁ TINHA baixado/gerado (estava Confirmado);
        // um rascunho cancelado nunca baixou estoque nem gerou parcela.
        if (pedido.Status == StatusPedido.Confirmado)
        {
            estoqueService.Estornar(pedido.Id, pedido.Itens);
            await contasReceberService.CancelarPendentesAsync(pedido.Id, cancelamento);
        }

        pedido.Status = StatusPedido.Cancelado;
        await contexto.SaveChangesAsync(cancelamento);

        return true;
    }

    public async Task<VendasResumoDto> ObterResumoVendasAsync(CancellationToken cancelamento)
    {
        var hoje = DateTime.UtcNow;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var inicioProximoMes = inicioMes.AddMonths(1);

        var pedidosDoMes = await contexto.Pedidos.AsNoTracking()
            .Where(p => p.DataPedido >= inicioMes && p.DataPedido < inicioProximoMes)
            .Select(p => new { p.Status, p.DataPedido, p.ValorTotal })
            .ToListAsync(cancelamento);

        var confirmados = pedidosDoMes.Where(p => p.Status == StatusPedido.Confirmado).ToList();
        var faturamento = confirmados.Sum(p => p.ValorTotal);

        var porStatus = new PedidosPorStatusDto(
            pedidosDoMes.Count(p => p.Status == StatusPedido.Rascunho),
            confirmados.Count,
            pedidosDoMes.Count(p => p.Status == StatusPedido.Cancelado));

        var valoresPorDia = confirmados
            .GroupBy(p => DateOnly.FromDateTime(p.DataPedido))
            .Select(g => (Dia: g.Key, Valor: g.Sum(p => p.ValorTotal)));

        var primeiroDia = DateOnly.FromDateTime(inicioMes);
        var ultimoDia = DateOnly.FromDateTime(inicioProximoMes.AddDays(-1));
        var faturamentoPorDia = DashboardCalculo.PreencherDias(valoresPorDia, primeiroDia, ultimoDia)
            .Select(d => new FaturamentoDiaDto(d.Dia, d.Valor))
            .ToList();

        return new VendasResumoDto(
            faturamento, DashboardCalculo.TicketMedio(faturamento, confirmados.Count), confirmados.Count, porStatus, faturamentoPorDia);
    }

    // Preço copiado do produto agora e congelado no item (R3); o cliente da API não envia preço.
    private static PedidoItem NovoItem(Produto produto, PedidoItemEntradaDto entrada) => new()
    {
        Produto = produto,
        ProdutoId = produto.Id,
        Quantidade = entrada.Quantidade!.Value,
        PrecoUnitario = produto.PrecoVenda,
        DescontoPercentual = entrada.DescontoPercentual
    };

    // Único ponto que grava valor_total: criar e editar chamam este método (SPEC.md, plano).
    private static void Recalcular(Pedido pedido)
    {
        var subtotais = pedido.Itens.Select(i => CalculoPedido.Subtotal(i.Quantidade, i.PrecoUnitario, i.DescontoPercentual));
        pedido.ValorTotal = CalculoPedido.Total(subtotais, pedido.DescontoPercentual);

        if (pedido.ValorTotal > CalculoPedido.LimiteTotal)
            throw new DadoInvalidoException(CampoItens, "O total do pedido não pode passar de R$ 9.999.999.999,99.");
    }

    internal static async Task<Cliente> ObterClienteAtivoAsync(ErpPortfolioDbContext contexto, int clienteId, CancellationToken cancelamento)
    {
        var cliente = await contexto.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, cancelamento);
        if (cliente is null || !cliente.Ativo)
            throw new DadoInvalidoException(nameof(PedidoCriacaoDto.ClienteId), "Cliente inexistente ou inativo.");

        return cliente;
    }

    internal static async Task<Vendedor> ObterVendedorAtivoAsync(ErpPortfolioDbContext contexto, int vendedorId, CancellationToken cancelamento)
    {
        var vendedor = await contexto.Vendedores.FirstOrDefaultAsync(v => v.Id == vendedorId, cancelamento);
        if (vendedor is null || !vendedor.Ativo)
            throw new DadoInvalidoException(nameof(PedidoCriacaoDto.VendedorId), "Vendedor inexistente ou inativo.");

        return vendedor;
    }

    internal static async Task<Dictionary<int, Produto>> ObterProdutosAsync(ErpPortfolioDbContext contexto, IEnumerable<int> ids, CancellationToken cancelamento)
    {
        var lista = ids.Distinct().ToList();
        return await contexto.Produtos.Where(p => lista.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancelamento);
    }

    // Item novo: o produto precisa existir, estar ativo e aceitar a quantidade na sua unidade (R5 e R8).
    internal static Produto ValidarProdutoDoItem(Dictionary<int, Produto> produtos, PedidoItemEntradaDto entrada)
    {
        if (!produtos.TryGetValue(entrada.ProdutoId, out var produto))
            throw new DadoInvalidoException(CampoItens, $"Produto {entrada.ProdutoId} inexistente.");

        if (!produto.Ativo)
            throw new DadoInvalidoException(CampoItens, $"O produto \"{produto.Nome}\" está inativo.");

        ValidarQuantidadeNaUnidade(produto, entrada);

        return produto;
    }

    internal static void ValidarQuantidadeNaUnidade(Produto produto, PedidoItemEntradaDto entrada)
    {
        if (!CalculoPedido.QuantidadeValidaParaUnidade(produto.Unidade, entrada.Quantidade!.Value))
            throw new DadoInvalidoException(CampoItens, $"A quantidade de \"{produto.Nome}\" deve ser inteira (unidade {produto.Unidade}).");
    }
}
