// =====================================================================================
// Arquivo....: IContasPagarService.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Contrato do serviço de contas a pagar: consulta, marcar pago, geração e
//              cancelamento de parcelas usados pelo PedidoCompraService, e o resumo do
//              Dashboard. Espelho de IContasReceberService.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por ContasPagarService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public interface IContasPagarService
{
    Task<ResultadoPaginadoDto<ParcelaPagarRespostaDto>> ListarAsync(ParcelaPagarFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Marca a parcela como paga (P4). Nulo se não existir; ConflitoException se estiver Cancelado.</summary>
    Task<ParcelaPagarRespostaDto?> MarcarPagaAsync(int id, CancellationToken cancelamento);

    /// <summary>
    /// Gera as parcelas do pedido de compra confirmado (P1, P2); não chama SaveChanges (fica na
    /// mesma transação de quem chamar).
    /// </summary>
    void GerarParcelas(PedidoCompra pedido, int numeroParcelas, int intervaloDias);

    /// <summary>Cancela as parcelas ainda Pendentes do pedido de compra (P5); não chama SaveChanges.</summary>
    Task CancelarPendentesAsync(int pedidoCompraId, CancellationToken cancelamento);

    /// <summary>Total e quantidade pendente/atrasado, sem filtro de mês (Dashboard).</summary>
    Task<ContasPagarResumoDto> ObterResumoAsync(CancellationToken cancelamento);
}
