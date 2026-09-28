// =====================================================================================
// Arquivo....: NotasFiscaisController.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: NF-e simulada (etapa 16): emissão pelo pedido (POST /api/pedidos/{id}/nfe);
//              lista com filtros (JSON, xlsx ou pdf), detalhe, XML e DANFE. Falta de dado
//              fiscal → 400 com a lista em "Pendencias"; pedido não confirmado ou já com
//              nota → 409.
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
    /// <summary>Lista as notas (mais recentes primeiro) com filtros; formato=xlsx|pdf exporta todas as do filtro.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<NotaFiscalResumoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] NotaFiscalFiltroDto filtro, CancellationToken cancelamento) =>
        filtro.Formato == FormatoRelatorio.Json
            ? Ok(await notaFiscalService.ListarAsync(filtro, cancelamento))
            : ExportadorRelatorio.Arquivo(await notaFiscalService.ModeloAsync(filtro, cancelamento), filtro.Formato);

    /// <summary>Detalhe da nota com itens e impostos.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<NotaFiscalDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaFiscalDetalheDto>> Obter(int id, CancellationToken cancelamento) =>
        await notaFiscalService.ObterAsync(id, cancelamento) is { } nota ? Ok(nota) : NotFound();

    /// <summary>XML da nota (nfeProc), como arquivo NFe{chave}.xml.</summary>
    [HttpGet("{id:int}/xml")]
    [Produces("application/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Xml(int id, CancellationToken cancelamento) =>
        await notaFiscalService.ObterXmlAsync(id, cancelamento) is { } nota
            ? File(System.Text.Encoding.UTF8.GetBytes(nota.Xml), "application/xml", $"NFe{nota.Chave}.xml")
            : NotFound();

    /// <summary>DANFE em PDF (gerado do XML gravado), como arquivo NFe{chave}.pdf.</summary>
    [HttpGet("{id:int}/danfe")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Danfe(int id, CancellationToken cancelamento) =>
        await notaFiscalService.ObterXmlAsync(id, cancelamento) is { } nota
            ? File(ExportadorDanfe.GerarPdf(nota.Xml), "application/pdf", $"NFe{nota.Chave}.pdf")
            : NotFound();

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
