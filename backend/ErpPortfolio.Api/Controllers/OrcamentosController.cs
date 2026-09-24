// =====================================================================================
// Arquivo....: OrcamentosController.cs
// Versão.....: 1.2.0
// Data.......: 23/09/2026
// Descrição..: Endpoints de orçamentos (/api/orcamentos, SPEC.md etapa 13): listar,
//              obter, criar, editar o aberto, gerar pedido, marcar como perdido e PDF.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IOrcamentoService).
// Tabelas....: public.orcamentos, public.orcamento_itens
// Fontes.....: IOrcamentoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - POST gerar-pedido e PATCH perder (T3).
//   1.2.0 - 23/09/2026 - GET pdf (T4).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/orcamentos")]
[Produces("application/json")]
public class OrcamentosController(IOrcamentoService orcamentoService) : ControllerBase
{
    /// <summary>Lista orçamentos (mais recentes primeiro) com paginação, busca por número/cliente e status (Aberto, Vencido, Aprovado, Perdido).</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<OrcamentoResumoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<OrcamentoResumoDto>>> Listar(
        [FromQuery] OrcamentoFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await orcamentoService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém um orçamento pelo id, com cliente, vendedor e itens.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<OrcamentoRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrcamentoRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var orcamento = await orcamentoService.ObterPorIdAsync(id, cancelamento);
        return orcamento is null ? NotFound() : Ok(orcamento);
    }

    /// <summary>Cria um orçamento aberto. O preço de cada item é copiado do produto e o total é calculado pelo servidor.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<OrcamentoRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrcamentoRespostaDto>> Criar(OrcamentoCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var orcamento = await orcamentoService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = orcamento.Id }, orcamento);
        }
        catch (DadoInvalidoException ex)
        {
            return ProblemaDeCampo(ex);
        }
    }

    /// <summary>Substitui os dados de um orçamento aberto (inclusive vencido, para prorrogar a validade).</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<OrcamentoRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrcamentoRespostaDto>> Atualizar(int id, OrcamentoCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var orcamento = await orcamentoService.AtualizarAsync(id, dados, cancelamento);
            return orcamento is null ? NotFound() : Ok(orcamento);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
        catch (DadoInvalidoException ex)
        {
            return ProblemaDeCampo(ex);
        }
    }

    /// <summary>Gera um pedido de venda em rascunho com os preços do orçamento; o orçamento passa a Aprovado. Só orçamento aberto e dentro da validade.</summary>
    [HttpPost("{id:int}/gerar-pedido")]
    [ProducesResponseType<OrcamentoPedidoGeradoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrcamentoPedidoGeradoDto>> GerarPedido(int id, CancellationToken cancelamento)
    {
        try
        {
            var pedidoId = await orcamentoService.GerarPedidoAsync(id, cancelamento);
            return pedidoId is int numero
                ? CreatedAtAction(nameof(PedidosController.ObterPorId), "Pedidos", new { id = numero }, new OrcamentoPedidoGeradoDto(numero))
                : NotFound();
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
        catch (DadoInvalidoException ex)
        {
            return ProblemaDeCampo(ex);
        }
    }

    /// <summary>Marca um orçamento aberto como perdido (motivo opcional). Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/perder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Perder(int id, OrcamentoPerderDto? dados, CancellationToken cancelamento)
    {
        try
        {
            var encontrado = await orcamentoService.PerderAsync(id, dados?.Motivo, cancelamento);
            return encontrado ? NoContent() : NotFound();
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Baixa o PDF do orçamento (orcamento-N.pdf), em qualquer status.</summary>
    [HttpGet("{id:int}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, ExportadorRelatorio.TipoConteudoPdf)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pdf(int id, CancellationToken cancelamento)
    {
        var pdf = await orcamentoService.GerarPdfAsync(id, cancelamento);
        return pdf is null ? NotFound() : File(pdf, ExportadorRelatorio.TipoConteudoPdf, $"orcamento-{id}.pdf");
    }

    // Mesmo formato dos erros de validação do [ApiController] (400), com o erro associado ao campo.
    private ActionResult ProblemaDeCampo(DadoInvalidoException ex) =>
        ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [ex.Campo] = [ex.Message] }));
}
