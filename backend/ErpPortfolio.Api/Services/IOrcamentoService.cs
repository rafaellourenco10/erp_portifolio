// =====================================================================================
// Arquivo....: IOrcamentoService.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Contrato do serviço de orçamentos (SPEC.md etapa 13).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.orcamentos, public.orcamento_itens
// Fontes.....: Implementado por OrcamentoService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo (listar, obter, criar, editar).
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IOrcamentoService
{
    /// <summary>Lista paginada (mais recentes primeiro), com busca por número/cliente e filtro de status (inclui Vencido).</summary>
    Task<ResultadoPaginadoDto<OrcamentoResumoDto>> ListarAsync(OrcamentoFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>O orçamento com cliente, vendedor e itens, ou null se não existir.</returns>
    Task<OrcamentoRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <summary>Cria um orçamento Aberto, copiando o preço de cada produto (OR1-OR2).</summary>
    /// <exception cref="DadoInvalidoException">Validade passada, cliente/vendedor/produto inexistente ou inativo, quantidade inválida ou total acima do limite.</exception>
    Task<OrcamentoRespostaDto> CriarAsync(OrcamentoCriacaoDto dados, CancellationToken cancelamento);

    /// <summary>Substitui os dados de um orçamento Aberto (OR5); itens que já estavam mantêm o preço congelado.</summary>
    /// <returns>O orçamento atualizado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">O orçamento está Aprovado ou Perdido.</exception>
    /// <exception cref="DadoInvalidoException">Mesmas regras da criação.</exception>
    Task<OrcamentoRespostaDto?> AtualizarAsync(int id, OrcamentoCriacaoDto dados, CancellationToken cancelamento);
}
