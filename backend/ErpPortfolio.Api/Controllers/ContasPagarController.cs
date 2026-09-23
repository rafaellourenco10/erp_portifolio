// =====================================================================================
// Arquivo....: ContasPagarController.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST do módulo de Contas a Pagar.
//                GET   /api/contas-pagar            -> listagem paginada
//                PATCH /api/contas-pagar/{id}/pagar  -> marca a parcela como paga
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IContasPagarService)
// Tabelas....: public.parcelas_pagar
// Fontes.....: IContasPagarService -> ContasPagarService -> ErpPortfolioDbContext.
//              Transição inválida (pagar parcela cancelada) vira 409 (ConflitoException).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/contas-pagar")]
[Produces("application/json")]
public class ContasPagarController(IContasPagarService contasPagarService) : ControllerBase
{
    /// <summary>Lista as parcelas a pagar, paginadas, ordenadas por vencimento. Filtro "Atrasado" é calculado no servidor.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<ParcelaPagarRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<ParcelaPagarRespostaDto>>> Listar(
        [FromQuery] ParcelaPagarFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await contasPagarService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Marca a parcela como paga. Chamadas repetidas também retornam 200; parcela cancelada retorna 409.</summary>
    [HttpPatch("{id:int}/pagar")]
    [ProducesResponseType<ParcelaPagarRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParcelaPagarRespostaDto>> Pagar(int id, CancellationToken cancelamento)
    {
        try
        {
            var parcela = await contasPagarService.MarcarPagaAsync(id, cancelamento);
            return parcela is null ? NotFound() : Ok(parcela);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }
}
