// =====================================================================================
// Arquivo....: IEstoqueService.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de estoque: consulta (saldo por produto e extrato).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por EstoqueService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e extrato).
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IEstoqueService
{
    Task<ResultadoPaginadoDto<EstoqueResumoDto>> ListarAsync(EstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Extrato de movimentações de um produto, mais recente primeiro. Nulo se o produto não existir.</summary>
    Task<ResultadoPaginadoDto<MovimentacaoRespostaDto>?> ObterExtratoAsync(
        int produtoId, EstoqueFiltroDto filtro, CancellationToken cancelamento);
}
