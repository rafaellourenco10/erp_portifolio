// =====================================================================================
// Arquivo....: IContasReceberService.cs
// Versão.....: 1.2.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de contas a receber: consulta, marcar recebido e a
//              geração de parcelas usada pelo PedidoService ao confirmar um pedido.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por ContasReceberService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
//   1.1.0 - 22/09/2026 - GerarParcelas (usado pelo PedidoService ao confirmar).
//   1.2.0 - 22/09/2026 - CancelarPendentesAsync (usado pelo PedidoService ao cancelar).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public interface IContasReceberService
{
    Task<ResultadoPaginadoDto<ParcelaRespostaDto>> ListarAsync(ParcelaFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Marca a parcela como recebida (C7). Nulo se não existir; ConflitoException se estiver Cancelado.</summary>
    Task<ParcelaRespostaDto?> MarcarRecebidaAsync(int id, CancellationToken cancelamento);

    /// <summary>
    /// Gera as parcelas do pedido confirmado (C1, C2, C3); não chama SaveChanges (fica na mesma
    /// transação de quem chamar).
    /// </summary>
    void GerarParcelas(Pedido pedido, int numeroParcelas, int intervaloDias);

    /// <summary>Cancela as parcelas ainda Pendentes do pedido (C6); não chama SaveChanges.</summary>
    Task CancelarPendentesAsync(int pedidoId, CancellationToken cancelamento);
}
