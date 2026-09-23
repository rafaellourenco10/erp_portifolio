// =====================================================================================
// Arquivo....: IPedidoService.cs
// Versão.....: 1.2.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de pedidos de venda (criação, consulta, listagem,
//              edição do rascunho, confirmação e cancelamento).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.pedidos, public.pedido_itens
// Fontes.....: Implementado por PedidoService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo (criar e obter).
//   1.1.0 - 22/09/2026 - ConfirmarAsync recebe numeroParcelas/intervaloDias (gera parcelas).
//   1.2.0 - 22/09/2026 - ObterResumoVendasAsync, para o Dashboard.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IPedidoService
{
    /// <summary>Lista paginada (mais recentes primeiro), com busca por número/cliente e filtro de status.</summary>
    Task<ResultadoPaginadoDto<PedidoResumoDto>> ListarAsync(PedidoFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>O pedido com cliente e itens, ou null se não existir.</returns>
    Task<PedidoRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <summary>
    /// Substitui cliente, itens, desconto e pagamento de um rascunho. O item que já existia mantém o preço
    /// congelado; produto novo copia o preço atual.
    /// </summary>
    /// <returns>O pedido atualizado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">O pedido não está em Rascunho.</exception>
    /// <exception cref="DadoInvalidoException">Cliente trocado por inexistente/inativo, produto novo inválido, quantidade inválida para a unidade ou total acima do limite.</exception>
    Task<PedidoRespostaDto?> AtualizarAsync(int id, PedidoCriacaoDto dados, CancellationToken cancelamento);

    /// <summary>
    /// Rascunho -> Confirmado. Exige forma de pagamento, cliente ativo, produtos ativos e saldo de
    /// estoque suficiente (R6, E2); gera <paramref name="numeroParcelas"/> parcelas a receber, a
    /// primeira vencendo em <paramref name="intervaloDias"/> dias (C1).
    /// </summary>
    /// <returns>O pedido confirmado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">O pedido não está em Rascunho.</exception>
    /// <exception cref="DadoInvalidoException">Falta a forma de pagamento, cliente/produto está inativo, ou estoque insuficiente.</exception>
    Task<PedidoRespostaDto?> ConfirmarAsync(int id, int numeroParcelas, int intervaloDias, CancellationToken cancelamento);

    /// <summary>Rascunho ou Confirmado -> Cancelado. Cancelar um pedido já cancelado também é sucesso (R7).</summary>
    /// <returns>false se o pedido não existir.</returns>
    Task<bool> CancelarAsync(int id, CancellationToken cancelamento);

    /// <summary>Cria um pedido em Rascunho, copiando o preço de cada produto e calculando o total.</summary>
    /// <exception cref="DadoInvalidoException">Cliente/produto inexistente ou inativo, quantidade inválida para a unidade ou total acima do limite.</exception>
    Task<PedidoRespostaDto> CriarAsync(PedidoCriacaoDto dados, CancellationToken cancelamento);

    /// <summary>Faturamento, ticket médio, pedidos por status e faturamento diário do mês atual (D1-D4, Dashboard).</summary>
    Task<VendasResumoDto> ObterResumoVendasAsync(CancellationToken cancelamento);
}
