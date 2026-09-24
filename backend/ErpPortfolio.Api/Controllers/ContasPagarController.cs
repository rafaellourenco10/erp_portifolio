// =====================================================================================
// Arquivo....: ContasPagarController.cs
// Versão.....: 1.2.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST do módulo de Contas a Pagar.
//                GET   /api/contas-pagar            -> listagem paginada
//                GET   /api/contas-pagar/exportar?formato=xlsx|pdf -> arquivo com todo o filtro
//                PATCH /api/contas-pagar/{id}/pagar  -> marca a parcela como paga
//                POST  /api/contas-pagar             -> lança conta avulsa em parcelas
//                PATCH /api/contas-pagar/{id}/cancelar -> cancela parcela avulsa/de comissão
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IContasPagarService)
// Tabelas....: public.parcelas_pagar
// Fontes.....: IContasPagarService -> ContasPagarService -> ErpPortfolioDbContext.
//              Transição inválida (pagar cancelada, cancelar paga ou de compra) vira 409.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - POST (conta avulsa) e PATCH cancelar (etapa 12).
//   1.2.0 - 24/09/2026 - GET exportar (Excel/PDF pelo ExportadorRelatorio).
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

    /// <summary>Parcelas do filtro (todas as páginas) em Excel ou PDF, no mesmo layout dos relatórios.</summary>
    [HttpGet("exportar")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, ExportadorRelatorio.TipoConteudoXlsx, ExportadorRelatorio.TipoConteudoPdf)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Exportar([FromQuery] ParcelaPagarFiltroDto filtro, [FromQuery] FormatoRelatorio formato = FormatoRelatorio.Xlsx, CancellationToken cancelamento = default) =>
        ExportadorRelatorio.Arquivo(await contasPagarService.ModeloAsync(filtro, cancelamento), formato);

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

    /// <summary>Lança uma conta avulsa (aluguel, luz...) em 1 a 12 parcelas; devolve as parcelas criadas.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<IReadOnlyList<ParcelaPagarRespostaDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ParcelaPagarRespostaDto>>> CriarAvulsa(ContaAvulsaCriacaoDto dados, CancellationToken cancelamento)
    {
        var parcelas = await contasPagarService.CriarAvulsaAsync(dados, cancelamento);
        return StatusCode(StatusCodes.Status201Created, parcelas);
    }

    /// <summary>Cancela uma parcela avulsa ou de comissão pendente (de comissão devolve as comissões para Pendente). Repetir também retorna 200.</summary>
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType<ParcelaPagarRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParcelaPagarRespostaDto>> Cancelar(int id, CancellationToken cancelamento)
    {
        try
        {
            var parcela = await contasPagarService.CancelarAsync(id, cancelamento);
            return parcela is null ? NotFound() : Ok(parcela);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }
}
