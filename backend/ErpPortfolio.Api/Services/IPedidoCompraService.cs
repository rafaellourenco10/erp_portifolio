// =====================================================================================
// Arquivo....: IPedidoCompraService.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de pedidos de compra (criação, consulta, listagem,
//              edição do rascunho, confirmação e cancelamento). Espelho de
//              IPedidoService, sem forma de pagamento/parcelas.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedidos_compra, public.pedido_compra_itens
// Fontes.....: Implementado por PedidoCompraService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IPedidoCompraService
{
    /// <summary>Lista paginada (mais recentes primeiro), com busca por número/fornecedor e filtro de status.</summary>
    Task<ResultadoPaginadoDto<PedidoCompraResumoDto>> ListarAsync(PedidoCompraFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>O pedido de compra com fornecedor e itens, ou null se não existir.</returns>
    Task<PedidoCompraRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <summary>Cria um pedido de compra em Rascunho, copiando o Custo de cada produto e calculando o total.</summary>
    /// <exception cref="DadoInvalidoException">Fornecedor/produto inexistente ou inativo, quantidade inválida para a unidade ou total acima do limite.</exception>
    Task<PedidoCompraRespostaDto> CriarAsync(PedidoCompraCriacaoDto dados, CancellationToken cancelamento);

    /// <summary>
    /// Substitui fornecedor, itens e desconto de um rascunho. O item que já existia mantém o preço congelado;
    /// produto novo copia o Custo atual.
    /// </summary>
    /// <returns>O pedido atualizado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">O pedido não está em Rascunho.</exception>
    /// <exception cref="DadoInvalidoException">Fornecedor trocado por inexistente/inativo, produto novo inválido, quantidade inválida para a unidade ou total acima do limite.</exception>
    Task<PedidoCompraRespostaDto?> AtualizarAsync(int id, PedidoCompraCriacaoDto dados, CancellationToken cancelamento);

    /// <summary>
    /// Rascunho -> Confirmado (PC5). Exige fornecedor e produtos ativos; gera uma Entrada de estoque por item
    /// (PC6), atualiza o Custo de cada produto para o preço pago no item e gera as parcelas a pagar (P1).
    /// </summary>
    /// <returns>O pedido confirmado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">O pedido não está em Rascunho.</exception>
    /// <exception cref="DadoInvalidoException">Fornecedor ou algum produto está inativo.</exception>
    Task<PedidoCompraRespostaDto?> ConfirmarAsync(int id, int numeroParcelas, int intervaloDias, CancellationToken cancelamento);

    /// <summary>
    /// Rascunho ou Confirmado -> Cancelado. Cancelar um pedido já cancelado também é sucesso (PC8). Cancelar um
    /// Confirmado exige saldo suficiente em cada item (PC7): bloqueia se o saldo já foi consumido; se passar,
    /// cancela as parcelas a pagar ainda Pendentes (P5).
    /// </summary>
    /// <returns>false se o pedido não existir.</returns>
    /// <exception cref="DadoInvalidoException">Saldo insuficiente para estornar algum item.</exception>
    Task<bool> CancelarAsync(int id, CancellationToken cancelamento);
}
