// =====================================================================================
// Arquivo....: IContasReceberService.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de contas a receber: consulta e marcar recebido.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por ContasReceberService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IContasReceberService
{
    Task<ResultadoPaginadoDto<ParcelaRespostaDto>> ListarAsync(ParcelaFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Marca a parcela como recebida (C7). Nulo se não existir; ConflitoException se estiver Cancelado.</summary>
    Task<ParcelaRespostaDto?> MarcarRecebidaAsync(int id, CancellationToken cancelamento);
}
