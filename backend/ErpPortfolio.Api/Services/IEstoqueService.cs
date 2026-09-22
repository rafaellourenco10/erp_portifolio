// =====================================================================================
// Arquivo....: IEstoqueService.cs
// Versão.....: 1.1.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de estoque: consulta (saldo por produto e extrato) e
//              entrada manual.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por EstoqueService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e extrato).
//   1.1.0 - 22/09/2026 - Entrada manual (RegistrarEntradaAsync).
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IEstoqueService
{
    Task<ResultadoPaginadoDto<EstoqueResumoDto>> ListarAsync(EstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Extrato de movimentações de um produto, mais recente primeiro. Nulo se o produto não existir.</summary>
    Task<ResultadoPaginadoDto<MovimentacaoRespostaDto>?> ObterExtratoAsync(
        int produtoId, EstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Lança uma entrada manual (E4). Nulo se o produto não existir; DadoInvalidoException se estiver inativo.</summary>
    Task<MovimentacaoRespostaDto?> RegistrarEntradaAsync(EstoqueEntradaDto dados, CancellationToken cancelamento);
}
