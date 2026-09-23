// =====================================================================================
// Arquivo....: IVendedorService.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Contrato do serviço de vendedores (espelho de IFornecedorService).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por VendedorService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IVendedorService
{
    Task<ResultadoPaginadoDto<VendedorRespostaDto>> ListarAsync(VendedorFiltroDto filtro, CancellationToken cancelamento);

    /// <returns>O vendedor, ou null se não existir.</returns>
    Task<VendedorRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento);

    /// <exception cref="ConflitoException">CPF já cadastrado.</exception>
    Task<VendedorRespostaDto> CriarAsync(VendedorCriacaoDto dados, CancellationToken cancelamento);

    /// <returns>O vendedor atualizado, ou null se não existir.</returns>
    /// <exception cref="ConflitoException">CPF já cadastrado em outro vendedor.</exception>
    Task<VendedorRespostaDto?> AtualizarAsync(int id, VendedorAtualizacaoDto dados, CancellationToken cancelamento);

    /// <returns>false se o vendedor não existir.</returns>
    Task<bool> InativarAsync(int id, CancellationToken cancelamento);
}
