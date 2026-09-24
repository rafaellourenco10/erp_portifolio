// =====================================================================================
// Arquivo....: IOrcamentoService.cs
// Versão.....: 1.2.0
// Data.......: 23/09/2026
// Descrição..: Contrato do serviço de orçamentos (SPEC.md etapa 13).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.orcamentos, public.orcamento_itens
// Fontes.....: Implementado por OrcamentoService (ErpPortfolioDbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo (listar, obter, criar, editar).
//   1.1.0 - 23/09/2026 - Gerar pedido (GP1-GP4) e marcar como perdido (PE1).
//   1.2.0 - 23/09/2026 - PDF do orçamento (PD1).
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

    /// <summary>
    /// GP1-GP4: cria um pedido de venda em Rascunho com os preços e descontos do orçamento e deixa o
    /// orçamento Aprovado, ligado ao pedido, na mesma transação.
    /// </summary>
    /// <returns>O id do pedido gerado, ou null se o orçamento não existir.</returns>
    /// <exception cref="ConflitoException">O orçamento já está Aprovado ou Perdido.</exception>
    /// <exception cref="DadoInvalidoException">Orçamento vencido, cliente inativo ou produto inativo.</exception>
    Task<int?> GerarPedidoAsync(int id, CancellationToken cancelamento);

    /// <summary>PE1: Aberto (vencido ou não) → Perdido, com motivo opcional. Repetir num Perdido é sucesso sem mudar nada.</summary>
    /// <returns>false se o orçamento não existir.</returns>
    /// <exception cref="ConflitoException">O orçamento está Aprovado.</exception>
    Task<bool> PerderAsync(int id, string? motivo, CancellationToken cancelamento);

    /// <summary>PD1: PDF do orçamento (qualquer status).</summary>
    /// <returns>Os bytes do PDF, ou null se o orçamento não existir.</returns>
    Task<byte[]?> GerarPdfAsync(int id, CancellationToken cancelamento);
}
