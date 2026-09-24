// =====================================================================================
// Arquivo....: IDevolucaoService.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Contrato do serviço de devolução de venda (SPEC.md etapa 14).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.devolucoes, public.devolucao_itens (+ efeitos em parcelas_receber,
//              parcelas_pagar, comissoes e estoque_movimentacoes)
// Fontes.....: Implementado por DevolucaoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IDevolucaoService
{
    /// <summary>
    /// DV1-DV8: registra a devolução e, na mesma transação, abate as parcelas pendentes, gera a conta de
    /// reembolso e o estorno de comissão (se houver) e dá entrada no estoque dos itens que voltam.
    /// </summary>
    /// <returns>A devolução registrada, ou null se o pedido não existir.</returns>
    /// <exception cref="ConflitoException">O pedido não está Confirmado.</exception>
    /// <exception cref="DadoInvalidoException">Item de outro pedido, quantidade inválida ou acima do disponível, vencimento passado.</exception>
    Task<DevolucaoRespostaDto?> RegistrarAsync(int pedidoId, DevolucaoCriacaoDto dados, CancellationToken cancelamento);

    /// <returns>As devoluções do pedido (mais antigas primeiro), ou null se o pedido não existir.</returns>
    Task<IReadOnlyList<DevolucaoRespostaDto>?> ListarAsync(int pedidoId, CancellationToken cancelamento);
}
