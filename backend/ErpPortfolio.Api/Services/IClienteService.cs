// =====================================================================================
// Arquivo....: IClienteService.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: Contrato do serviço de clientes (listagem, consulta, inclusão,
//              edição e inativação).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.clientes
// Fontes.....: Implementado por ClienteService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IClienteService
{
    Task<ResultadoPaginadoDto<ClienteRespostaDto>> ListarAsync(ClienteFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>O cliente, ou null se não existir.</returns>
    Task<ClienteRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <exception cref="ConflitoException">Documento já cadastrado.</exception>
    Task<ClienteRespostaDto> CriarAsync(ClienteCriacaoDto dados, CancellationToken cancelamento);

    /// <returns>O cliente atualizado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">Documento já cadastrado em outro cliente.</exception>
    Task<ClienteRespostaDto?> AtualizarAsync(int id, ClienteAtualizacaoDto dados, CancellationToken cancelamento);

    /// <returns>false se o cliente não existir.</returns>
    Task<bool> InativarAsync(int id, CancellationToken cancelamento);
}
