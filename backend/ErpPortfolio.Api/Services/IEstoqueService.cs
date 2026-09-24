// =====================================================================================
// Arquivo....: IEstoqueService.cs
// Versão.....: 1.6.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de estoque: consulta (saldo por produto e extrato),
//              entrada manual, a baixa/estorno usados pelo PedidoService (pedido de
//              venda) e o receber/estornar usados pelo PedidoCompraService (pedido de
//              compra) ao confirmar e cancelar.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por EstoqueService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e extrato).
//   1.1.0 - 22/09/2026 - Entrada manual (RegistrarEntradaAsync).
//   1.2.0 - 22/09/2026 - BaixarAsync (confirmar) e Estornar (cancelar de confirmado).
//   1.3.0 - 22/09/2026 - ObterResumoAsync, para o Dashboard.
//   1.4.0 - 22/09/2026 - ObterResumoAsync devolve a lista de produtos baixos (estoque mínimo por produto).
//   1.5.0 - 22/09/2026 - ReceberAsync e EstornarCompra, para o Pedido de Compra (SPEC.md, PC6/PC7).
//   1.6.0 - 24/09/2026 - DevolverAoEstoque, para a devolução de venda (etapa 14, DV7).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public interface IEstoqueService
{
    Task<ResultadoPaginadoDto<EstoqueResumoDto>> ListarAsync(EstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Extrato de movimentações de um produto, mais recente primeiro. Nulo se o produto não existir.</summary>
    Task<ResultadoPaginadoDto<MovimentacaoRespostaDto>?> ObterExtratoAsync(
        int produtoId, EstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Lança uma entrada manual (E4). Nulo se o produto não existir; DadoInvalidoException se estiver inativo.</summary>
    Task<MovimentacaoRespostaDto?> RegistrarEntradaAsync(EstoqueEntradaDto dados, CancellationToken cancelamento);

    /// <summary>
    /// Verifica o saldo de todos os itens e enfileira uma Saída por item (E2); não chama SaveChanges (fica na
    /// mesma transação de quem chamar). DadoInvalidoException se algum item não tiver saldo suficiente — nesse
    /// caso nenhuma movimentação é enfileirada.
    /// </summary>
    Task BaixarAsync(int pedidoId, IEnumerable<PedidoItem> itens, CancellationToken cancelamento);

    /// <summary>Enfileira uma Entrada de estorno por item (E3); não chama SaveChanges.</summary>
    void Estornar(int pedidoId, IEnumerable<PedidoItem> itens);

    /// <summary>Enfileira uma Entrada por item devolvido que volta ao estoque (DV7); não chama SaveChanges.</summary>
    void DevolverAoEstoque(int pedidoId, int devolucaoId, IEnumerable<(int ProdutoId, decimal Quantidade)> itens);

    /// <summary>
    /// Enfileira uma Entrada por item ligada ao pedido de compra (PC6), sem checar saldo (uma compra sempre
    /// pode entrar); não chama SaveChanges.
    /// </summary>
    void Receber(int pedidoCompraId, IEnumerable<PedidoCompraItem> itens);

    /// <summary>
    /// Verifica o saldo de todos os itens e enfileira uma Saída de estorno por item, ligada ao pedido de
    /// compra (PC7); não chama SaveChanges. DadoInvalidoException se algum item não tiver saldo suficiente
    /// (já foi vendido/consumido) — nesse caso nenhuma movimentação é enfileirada.
    /// </summary>
    Task EstornarCompraAsync(int pedidoCompraId, IEnumerable<PedidoCompraItem> itens, CancellationToken cancelamento);

    /// <summary>Quantidade e lista de produtos ativos com saldo ≤ o próprio estoque mínimo (D6, Dashboard).</summary>
    Task<EstoqueResumoDashboardDto> ObterResumoAsync(CancellationToken cancelamento);
}
