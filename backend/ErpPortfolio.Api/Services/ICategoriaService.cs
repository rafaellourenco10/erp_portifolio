// =====================================================================================
// Arquivo....: ICategoriaService.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Contrato do serviço de categorias (listagem, consulta, inclusão, edição
//              e inativação).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.categorias
// Fontes.....: Implementado por CategoriaService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface ICategoriaService
{
    Task<ResultadoPaginadoDto<CategoriaRespostaDto>> ListarAsync(CategoriaFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>A categoria, ou null se não existir.</returns>
    Task<CategoriaRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <exception cref="ConflitoException">Nome já cadastrado.</exception>
    Task<CategoriaRespostaDto> CriarAsync(CategoriaCriacaoDto dados, CancellationToken cancelamento);

    /// <returns>A categoria atualizada, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">Nome já cadastrado em outra categoria.</exception>
    Task<CategoriaRespostaDto?> AtualizarAsync(int id, CategoriaAtualizacaoDto dados, CancellationToken cancelamento);

    /// <returns>false se a categoria não existir.</returns>
    Task<bool> InativarAsync(int id, CancellationToken cancelamento);
}
