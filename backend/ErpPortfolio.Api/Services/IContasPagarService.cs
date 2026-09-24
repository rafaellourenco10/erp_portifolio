// =====================================================================================
// Arquivo....: IContasPagarService.cs
// Versão.....: 1.3.0
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
//   1.1.0 - 23/09/2026 - CriarAvulsaAsync e CancelarAsync; pagar propaga para comissões (etapa 12).
//   1.2.0 - 23/09/2026 - ObterVencimentosAsync, para o Dashboard.
//   1.3.0 - 24/09/2026 - ModeloAsync (exportar Excel/PDF).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public interface IContasPagarService
{
    Task<ResultadoPaginadoDto<ParcelaPagarRespostaDto>> ListarAsync(ParcelaPagarFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Todas as parcelas do filtro (ignora a paginação) no modelo do ExportadorRelatorio.</summary>
    Task<RelatorioModelo> ModeloAsync(ParcelaPagarFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>
    /// Marca a parcela como paga (P4); se for de comissão, paga as comissões ligadas (CC3).
    /// Nulo se não existir; ConflitoException se estiver Cancelado.
    /// </summary>
    Task<ParcelaPagarRespostaDto?> MarcarPagaAsync(int id, CancellationToken cancelamento);

    /// <summary>
    /// Gera as parcelas do pedido de compra confirmado (P1, P2); não chama SaveChanges (fica na
    /// mesma transação de quem chamar).
    /// </summary>
    void GerarParcelas(PedidoCompra pedido, int numeroParcelas, int intervaloDias);

    /// <summary>Lança uma conta avulsa em N parcelas (AV1/AV2) e devolve as parcelas criadas.</summary>
    Task<IReadOnlyList<ParcelaPagarRespostaDto>> CriarAvulsaAsync(ContaAvulsaCriacaoDto dados, CancellationToken cancelamento);

    /// <summary>
    /// Cancela parcela Avulsa/Comissão pendente (CP4), idempotente; se for de comissão, devolve as comissões
    /// para Pendente (CC4). Nulo se não existir; ConflitoException se for de compra ou já estiver paga.
    /// </summary>
    Task<ParcelaPagarRespostaDto?> CancelarAsync(int id, CancellationToken cancelamento);

    /// <summary>Cancela as parcelas ainda Pendentes do pedido de compra (P5); não chama SaveChanges.</summary>
    Task CancelarPendentesAsync(int pedidoCompraId, CancellationToken cancelamento);

    /// <summary>Total e quantidade pendente/atrasado, sem filtro de mês (Dashboard).</summary>
    Task<ContasPagarResumoDto> ObterResumoAsync(CancellationToken cancelamento);

    /// <summary>Pendentes atrasadas ou que vencem nos próximos 7 dias, mais urgentes primeiro (Dashboard).</summary>
    Task<VencimentosDto> ObterVencimentosAsync(CancellationToken cancelamento);
}
