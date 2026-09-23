// =====================================================================================
// Arquivo....: IFornecedorService.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de fornecedores (listagem, consulta, inclusão,
//              edição e inativação). Espelho de IClienteService.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.fornecedores
// Fontes.....: Implementado por FornecedorService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IFornecedorService
{
    Task<ResultadoPaginadoDto<FornecedorRespostaDto>> ListarAsync(FornecedorFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>O fornecedor, ou null se não existir.</returns>
    Task<FornecedorRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <exception cref="ConflitoException">Documento já cadastrado.</exception>
    Task<FornecedorRespostaDto> CriarAsync(FornecedorCriacaoDto dados, CancellationToken cancelamento);

    /// <returns>O fornecedor atualizado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">Documento já cadastrado em outro fornecedor.</exception>
    Task<FornecedorRespostaDto?> AtualizarAsync(int id, FornecedorAtualizacaoDto dados, CancellationToken cancelamento);

    /// <returns>false se o fornecedor não existir.</returns>
    Task<bool> InativarAsync(int id, CancellationToken cancelamento);
}
