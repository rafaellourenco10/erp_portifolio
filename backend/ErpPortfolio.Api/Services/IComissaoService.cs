// =====================================================================================
// Arquivo....: IComissaoService.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Contrato do serviço de comissões: listagem com totais e pagamento ao
//              vendedor. A geração fica no ContasReceberService (ao receber a parcela).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por ComissaoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IComissaoService
{
    /// <summary>Página de comissões (mais recente primeiro) e os totais de TODO o filtro (CM6).</summary>
    Task<ComissaoListaDto> ListarAsync(ComissaoFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Marca como pagas ao vendedor (CM4); as já pagas não mudam.</summary>
    /// <exception cref="DadoInvalidoException">Algum id não existe (nada é alterado).</exception>
    Task PagarAsync(IReadOnlyCollection<int> ids, CancellationToken cancelamento);
}
