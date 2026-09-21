// =====================================================================================
// Arquivo....: IProdutoService.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Contrato do serviço de produtos (listagem, consulta, inclusão, edição
//              e inativação).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.produtos
// Fontes.....: Implementado por ProdutoService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IProdutoService
{
    Task<ResultadoPaginadoDto<ProdutoRespostaDto>> ListarAsync(ProdutoFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>O produto, ou null se não existir.</returns>
    Task<ProdutoRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <exception cref="ConflitoException">SKU já cadastrado.</exception>
    Task<ProdutoRespostaDto> CriarAsync(ProdutoCriacaoDto dados, CancellationToken cancelamento);

    /// <returns>O produto atualizado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">SKU já cadastrado em outro produto.</exception>
    Task<ProdutoRespostaDto?> AtualizarAsync(int id, ProdutoAtualizacaoDto dados, CancellationToken cancelamento);

    /// <returns>false se o produto não existir.</returns>
    Task<bool> InativarAsync(int id, CancellationToken cancelamento);
}
