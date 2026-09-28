// =====================================================================================
// Arquivo....: NotasFiscaisController.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: NF-e simulada (etapa 16): emissão pelo pedido (POST /api/pedidos/{id}/nfe).
//              Falta de dado fiscal → 400 com a lista em "Pendencias"; pedido não confirmado
//              ou já com nota → 409.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (pelo NotaFiscalService)
// Tabelas....: public.notas_fiscais, public.nota_fiscal_itens
// Fontes.....: Services/NotaFiscalService.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
[Produces("application/json")]
public class NotasFiscaisController(NotaFiscalService notaFiscalService) : ControllerBase
{
    /// <summary>Emite a NF-e de saída de um pedido confirmado (uma por pedido).</summary>
    [HttpPost("/api/pedidos/{id:int}/nfe")]
    [ProducesResponseType<NotaFiscalDetalheDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<ActionResult<NotaFiscalDetalheDto>> EmitirDoPedido(int id, CancellationToken cancelamento) =>
        Emitir(() => notaFiscalService.EmitirDoPedidoAsync(id, cancelamento));

    private async Task<ActionResult<NotaFiscalDetalheDto>> Emitir(Func<Task<NotaFiscalDetalheDto?>> emitir)
    {
        try
        {
            var nota = await emitir();
            return nota is null ? NotFound() : StatusCode(StatusCodes.Status201Created, nota);
        }
        catch (NfePendenteException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Pendencias"] = [.. ex.Pendencias]
            }));
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }
}
