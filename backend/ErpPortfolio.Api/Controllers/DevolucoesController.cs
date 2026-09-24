// =====================================================================================
// Arquivo....: DevolucoesController.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Devoluções de um pedido de venda (/api/pedidos/{pedidoId}/devolucoes,
//              SPEC.md etapa 14): registrar e listar o histórico.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IDevolucaoService).
// Tabelas....: public.devolucoes, public.devolucao_itens (e os efeitos da devolução)
// Fontes.....: IDevolucaoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/pedidos/{pedidoId:int}/devolucoes")]
[Produces("application/json")]
public class DevolucoesController(IDevolucaoService devolucaoService) : ControllerBase
{
    /// <summary>Histórico de devoluções do pedido (mais antigas primeiro).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DevolucaoRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DevolucaoRespostaDto>>> Listar(int pedidoId, CancellationToken cancelamento)
    {
        var devolucoes = await devolucaoService.ListarAsync(pedidoId, cancelamento);
        return devolucoes is null ? NotFound() : Ok(devolucoes);
    }

    /// <summary>
    /// Registra uma devolução (parcial ou total) de um pedido confirmado: abate as parcelas pendentes, gera o
    /// reembolso do excedente (conta a pagar) e o estorno de comissão, e devolve ao estoque os itens marcados.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<DevolucaoRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DevolucaoRespostaDto>> Registrar(int pedidoId, DevolucaoCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var devolucao = await devolucaoService.RegistrarAsync(pedidoId, dados, cancelamento);
            return devolucao is null ? NotFound() : CreatedAtAction(nameof(Listar), new { pedidoId }, devolucao);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
        catch (DadoInvalidoException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [ex.Campo] = [ex.Message] }));
        }
    }
}
