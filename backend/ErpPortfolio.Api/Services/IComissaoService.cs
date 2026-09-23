// =====================================================================================
// Arquivo....: IComissaoService.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Contrato do serviço de comissões: listagem com totais e fechamento em
//              conta a pagar. A geração fica no ContasReceberService (ao receber a parcela).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por ComissaoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - GerarContaAsync no lugar de PagarAsync (etapa 12).
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IComissaoService
{
    /// <summary>Página de comissões (mais recente primeiro) e os totais de TODO o filtro (CM6).</summary>
    Task<ComissaoListaDto> ListarAsync(ComissaoFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>
    /// Fecha comissões Pendentes de um vendedor numa conta a pagar (CC1/CC2): 1 parcela com a soma; as comissões
    /// ficam Em pagamento. Pagar/cancelar a conta propaga (ContasPagarService).
    /// </summary>
    /// <exception cref="DadoInvalidoException">Id inexistente, comissão não Pendente ou vendedores misturados (nada é alterado).</exception>
    Task<ComissaoContaGeradaDto> GerarContaAsync(IReadOnlyCollection<int> ids, DateOnly vencimento, CancellationToken cancelamento);
}
