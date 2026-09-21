// =====================================================================================
// Arquivo....: TransicoesPedido.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Regras de status do pedido (R1 e R2 da SPEC.md), em funções puras:
//                Rascunho  -> pode editar, confirmar e cancelar
//                Confirmado -> só cancelar
//                Cancelado -> nada (estado final)
//              Cancelar um pedido que já está cancelado é tratado como sucesso (204) pelo
//              serviço, antes de consultar estas regras.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Models/StatusPedido.cs. Testes: ErpPortfolio.Tests/TransicoesPedidoTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public static class TransicoesPedido
{
    public static bool PodeEditar(StatusPedido status) => status == StatusPedido.Rascunho;

    public static bool PodeConfirmar(StatusPedido status) => status == StatusPedido.Rascunho;

    public static bool PodeCancelar(StatusPedido status) => status is StatusPedido.Rascunho or StatusPedido.Confirmado;
}
