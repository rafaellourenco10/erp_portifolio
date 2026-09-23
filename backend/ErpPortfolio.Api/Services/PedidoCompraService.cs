// =====================================================================================
// Arquivo....: PedidoCompraService.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Regras de negócio e persistência de pedidos de compra. Espelho de
//              PedidoService, reaproveitando StatusPedido, TransicoesPedido e
//              CalculoPedido do Pedido de Venda. O servidor decide o preço (copiado do
//              Custo do produto, PC2) e o total (CalculoPedido); a API nunca aceita preço
//              nem total vindos do cliente. Confirmar dá entrada no estoque dos itens
//              (PC6) e atualiza o Custo de cada produto; cancelar um pedido que estava
//              Confirmado estorna o estoque, bloqueando se o saldo já foi consumido (PC7).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.pedidos_compra, public.pedido_compra_itens
//                - SELECT : listagem (busca por id ou ILIKE no nome do fornecedor, status,
//                           ORDER BY data_pedido DESC, id DESC, LIMIT/OFFSET),
//                           consulta por id (JOIN com fornecedores e produtos) e validação
//                           de fornecedor e produtos (existem e estão ativos)
//                - INSERT : criação do rascunho com os itens
//                - UPDATE : edição do rascunho (cabeçalho, valor_total e itens existentes)
//                - INSERT / DELETE em pedido_compra_itens: itens novos e itens removidos
//                           na edição
//                - UPDATE : status (Rascunho -> Confirmado; Rascunho/Confirmado -> Cancelado)
//              public.produtos
//                - UPDATE : custo do produto, ao confirmar (PC6)
//              public.estoque_movimentacoes (indiretamente, via IEstoqueService)
//                - INSERT : Entrada por item ao confirmar (PC6); Saída de estorno por item
//                           ao cancelar um pedido que estava Confirmado (PC7)
// Fontes.....: ErpPortfolioDbContext.PedidosCompra / PedidoCompraItens / Fornecedores /
//              Produtos. IEstoqueService.Receber / EstornarCompraAsync não chamam
//              SaveChanges: ficam na mesma transação do SaveChangesAsync deste serviço.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

public class PedidoCompraService(ErpPortfolioDbContext contexto, IEstoqueService estoqueService) : IPedidoCompraService
{
    private const string CampoItens = "Itens";

    public async Task<ResultadoPaginadoDto<PedidoCompraResumoDto>> ListarAsync(PedidoCompraFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.PedidosCompra.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var texto = filtro.Busca.Trim();
            var padrao = $"%{ClienteService.EscaparCuringasLike(texto)}%";

            // Número do pedido (com ou sem #) OU trecho do nome do fornecedor: nomes com dígitos continuam achando pelo nome.
            consulta = int.TryParse(texto.TrimStart('#'), out var numero)
                ? consulta.Where(p => p.Id == numero || EF.Functions.ILike(p.Fornecedor!.Nome, padrao))
                : consulta.Where(p => EF.Functions.ILike(p.Fornecedor!.Nome, padrao));
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
            .Select(p => new PedidoCompraResumoDto(p.Id, p.FornecedorId, p.Fornecedor!.Nome, p.DataPedido, p.Status, p.ValorTotal, p.Itens.Count))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<PedidoCompraResumoDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<PedidoCompraRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        var pedido = await contexto.PedidosCompra.AsNoTracking()
            .Include(p => p.Fornecedor)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id, cancelamento);

        return pedido is null ? null : PedidoCompraRespostaDto.DeEntidade(pedido);
    }

    public async Task<PedidoCompraRespostaDto> CriarAsync(PedidoCompraCriacaoDto dados, CancellationToken cancelamento)
    {
        var fornecedor = await ObterFornecedorAtivoAsync(dados.FornecedorId!.Value, cancelamento);
        var produtos = await ObterProdutosAsync(dados.Itens.Select(i => i.ProdutoId), cancelamento);

        var pedido = new PedidoCompra
        {
            Fornecedor = fornecedor,
            FornecedorId = fornecedor.Id,
            Status = StatusPedido.Rascunho,
            DataPedido = DateTime.UtcNow,
            DescontoPercentual = dados.DescontoPercentual
        };

        foreach (var entrada in dados.Itens)
        {
            var produto = ValidarProdutoDoItem(produtos, entrada);
            pedido.Itens.Add(NovoItem(produto, entrada));
        }

        Recalcular(pedido);

        contexto.PedidosCompra.Add(pedido);
        await contexto.SaveChangesAsync(cancelamento);

        return (await ObterPorIdAsync(pedido.Id, cancelamento))!;
    }

    public async Task<PedidoCompraRespostaDto?> AtualizarAsync(int id, PedidoCompraCriacaoDto dados, CancellationToken cancelamento)
    {
        var pedido = await contexto.PedidosCompra.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (pedido is null)
            return null;

        if (!TransicoesPedido.PodeEditar(pedido.Status))
            throw new ConflitoException($"O pedido de compra {id} está {pedido.Status} e não pode ser editado; só o rascunho pode.");

        // Fornecedor: só exige "ativo" se foi trocado (um rascunho antigo com fornecedor inativado depois continua editável).
        var fornecedorId = dados.FornecedorId!.Value;
        if (fornecedorId != pedido.FornecedorId)
        {
            var fornecedor = await ObterFornecedorAtivoAsync(fornecedorId, cancelamento);
            pedido.Fornecedor = fornecedor;
            pedido.FornecedorId = fornecedor.Id;
        }

        pedido.DescontoPercentual = dados.DescontoPercentual;

        var produtos = await ObterProdutosAsync(dados.Itens.Select(i => i.ProdutoId), cancelamento);

        // Itens atualizados NO LUGAR, casando por produto: o item que já existia mantém o preço congelado (PC2) e
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
        contexto.PedidoCompraItens.RemoveRange(removidos);
        foreach (var removido in removidos)
            pedido.Itens.Remove(removido);

        Recalcular(pedido);
        await contexto.SaveChangesAsync(cancelamento);

        return await ObterPorIdAsync(id, cancelamento);
    }

    public async Task<PedidoCompraRespostaDto?> ConfirmarAsync(int id, CancellationToken cancelamento)
    {
        var pedido = await contexto.PedidosCompra
            .Include(p => p.Fornecedor)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (pedido is null)
            return null;

        if (!TransicoesPedido.PodeConfirmar(pedido.Status))
            throw new ConflitoException($"O pedido de compra {id} está {pedido.Status} e não pode ser confirmado; só o rascunho pode.");

        // PC5: confirmar exige fornecedor ativo e produtos ativos. O que foi inativado depois de o item entrar
        // no rascunho é barrado aqui (e não na edição), porque a compra só se fecha com cadastros ativos.
        if (!pedido.Fornecedor!.Ativo)
            throw new DadoInvalidoException(nameof(PedidoCompraCriacaoDto.FornecedorId), "O fornecedor do pedido está inativo.");

        var inativos = pedido.Itens.Where(i => !i.Produto!.Ativo).Select(i => $"\"{i.Produto!.Nome}\"").ToList();
        if (inativos.Count > 0)
            throw new DadoInvalidoException(CampoItens, $"Produto(s) inativo(s) no pedido: {string.Join(", ", inativos)}.");

        // PC6: entrada por item (não checa saldo, uma compra sempre pode entrar) e atualiza o custo de cada
        // produto para o preço pago no item (a entidade já está tracked pelo Include acima).
        estoqueService.Receber(pedido.Id, pedido.Itens);
        foreach (var item in pedido.Itens)
            item.Produto!.Custo = item.PrecoUnitario;

        pedido.Status = StatusPedido.Confirmado;

        await contexto.SaveChangesAsync(cancelamento);

        return PedidoCompraRespostaDto.DeEntidade(pedido);
    }

    public async Task<bool> CancelarAsync(int id, CancellationToken cancelamento)
    {
        var pedido = await contexto.PedidosCompra
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id, cancelamento);
        if (pedido is null)
            return false;

        // Cancelar de novo é sucesso (PC8): o resultado desejado já é o estado atual.
        if (pedido.Status == StatusPedido.Cancelado)
            return true;

        if (!TransicoesPedido.PodeCancelar(pedido.Status))
            throw new ConflitoException($"O pedido de compra {id} está {pedido.Status} e não pode ser cancelado.");

        // PC7: só estorna estoque se o pedido JÁ TINHA dado entrada (estava Confirmado); um rascunho cancelado
        // nunca deu entrada. EstornarCompraAsync bloqueia (sem gravar nada) se o saldo já foi consumido.
        if (pedido.Status == StatusPedido.Confirmado)
        {
            await estoqueService.EstornarCompraAsync(pedido.Id, pedido.Itens, cancelamento);
        }

        pedido.Status = StatusPedido.Cancelado;
        await contexto.SaveChangesAsync(cancelamento);

        return true;
    }

    // Preço copiado do Custo do produto agora e congelado no item (PC2); o cliente da API não envia preço.
    private static PedidoCompraItem NovoItem(Produto produto, PedidoCompraItemEntradaDto entrada) => new()
    {
        Produto = produto,
        ProdutoId = produto.Id,
        Quantidade = entrada.Quantidade!.Value,
        PrecoUnitario = produto.Custo,
        DescontoPercentual = entrada.DescontoPercentual
    };

    // Único ponto que grava valor_total: criar e editar chamam este método (reaproveita CalculoPedido do Pedido de Venda).
    private static void Recalcular(PedidoCompra pedido)
    {
        var subtotais = pedido.Itens.Select(i => CalculoPedido.Subtotal(i.Quantidade, i.PrecoUnitario, i.DescontoPercentual));
        pedido.ValorTotal = CalculoPedido.Total(subtotais, pedido.DescontoPercentual);

        if (pedido.ValorTotal > CalculoPedido.LimiteTotal)
            throw new DadoInvalidoException(CampoItens, "O total do pedido não pode passar de R$ 9.999.999.999,99.");
    }

    private async Task<Fornecedor> ObterFornecedorAtivoAsync(int fornecedorId, CancellationToken cancelamento)
    {
        var fornecedor = await contexto.Fornecedores.FirstOrDefaultAsync(f => f.Id == fornecedorId, cancelamento);
        if (fornecedor is null || !fornecedor.Ativo)
            throw new DadoInvalidoException(nameof(PedidoCompraCriacaoDto.FornecedorId), "Fornecedor inexistente ou inativo.");

        return fornecedor;
    }

    private async Task<Dictionary<int, Produto>> ObterProdutosAsync(IEnumerable<int> ids, CancellationToken cancelamento)
    {
        var lista = ids.Distinct().ToList();
        return await contexto.Produtos.Where(p => lista.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancelamento);
    }

    // Item novo: o produto precisa existir, estar ativo e aceitar a quantidade na sua unidade.
    private static Produto ValidarProdutoDoItem(Dictionary<int, Produto> produtos, PedidoCompraItemEntradaDto entrada)
    {
        if (!produtos.TryGetValue(entrada.ProdutoId, out var produto))
            throw new DadoInvalidoException(CampoItens, $"Produto {entrada.ProdutoId} inexistente.");

        if (!produto.Ativo)
            throw new DadoInvalidoException(CampoItens, $"O produto \"{produto.Nome}\" está inativo.");

        ValidarQuantidadeNaUnidade(produto, entrada);

        return produto;
    }

    private static void ValidarQuantidadeNaUnidade(Produto produto, PedidoCompraItemEntradaDto entrada)
    {
        if (!CalculoPedido.QuantidadeValidaParaUnidade(produto.Unidade, entrada.Quantidade!.Value))
            throw new DadoInvalidoException(CampoItens, $"A quantidade de \"{produto.Nome}\" deve ser inteira (unidade {produto.Unidade}).");
    }
}
