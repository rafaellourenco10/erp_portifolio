// =====================================================================================
// Arquivo....: IEstoqueService.cs
// Versão.....: 1.2.0
// Data.......: 22/09/2026
// Descrição..: Contrato do serviço de estoque: consulta (saldo por produto e extrato),
//              entrada manual e a baixa/estorno usados pelo PedidoService ao confirmar
//              e cancelar um pedido.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por EstoqueService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e extrato).
//   1.1.0 - 22/09/2026 - Entrada manual (RegistrarEntradaAsync).
//   1.2.0 - 22/09/2026 - BaixarAsync (confirmar) e Estornar (cancelar de confirmado).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public interface IEstoqueService
{
    Task<ResultadoPaginadoDto<EstoqueResumoDto>> ListarAsync(EstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Extrato de movimentações de um produto, mais recente primeiro. Nulo se o produto não existir.</summary>
    Task<ResultadoPaginadoDto<MovimentacaoRespostaDto>?> ObterExtratoAsync(
        int produtoId, EstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Lança uma entrada manual (E4). Nulo se o produto não existir; DadoInvalidoException se estiver inativo.</summary>
    Task<MovimentacaoRespostaDto?> RegistrarEntradaAsync(EstoqueEntradaDto dados, CancellationToken cancelamento);

    /// <summary>
    /// Verifica o saldo de todos os itens e enfileira uma Saída por item (E2); não chama SaveChanges (fica na
    /// mesma transação de quem chamar). DadoInvalidoException se algum item não tiver saldo suficiente — nesse
    /// caso nenhuma movimentação é enfileirada.
    /// </summary>
    Task BaixarAsync(int pedidoId, IEnumerable<PedidoItem> itens, CancellationToken cancelamento);

    /// <summary>Enfileira uma Entrada de estorno por item (E3); não chama SaveChanges.</summary>
    void Estornar(int pedidoId, IEnumerable<PedidoItem> itens);
}
