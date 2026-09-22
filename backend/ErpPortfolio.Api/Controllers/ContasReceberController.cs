// =====================================================================================
// Arquivo....: ContasReceberController.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Endpoints REST do módulo de Contas a Receber.
//                GET   /api/contas-receber              -> listagem paginada
//                PATCH /api/contas-receber/{id}/receber  -> marca a parcela como recebida
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IContasReceberService)
// Tabelas....: public.parcelas_receber
// Fontes.....: IContasReceberService -> ContasReceberService -> ErpPortfolioDbContext.
//              Transição inválida (receber parcela cancelada) vira 409 (ConflitoException).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e marcar recebido).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/contas-receber")]
[Produces("application/json")]
public class ContasReceberController(IContasReceberService contasReceberService) : ControllerBase
{
    /// <summary>Lista as parcelas a receber (vencimento mais próximo primeiro), com busca e filtro de status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<ParcelaRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<ParcelaRespostaDto>>> Listar(
        [FromQuery] ParcelaFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await contasReceberService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Marca a parcela como recebida. Chamadas repetidas também retornam 200 (idempotente).</summary>
    [HttpPatch("{id:int}/receber")]
    [ProducesResponseType<ParcelaRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParcelaRespostaDto>> Receber(int id, CancellationToken cancelamento)
    {
        try
        {
            var parcela = await contasReceberService.MarcarRecebidaAsync(id, cancelamento);
            return parcela is null ? NotFound() : Ok(parcela);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }
}
