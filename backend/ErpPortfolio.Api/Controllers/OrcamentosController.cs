// =====================================================================================
// Arquivo....: OrcamentosController.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Endpoints de orçamentos (/api/orcamentos, SPEC.md etapa 13): listar,
//              obter, criar e editar o aberto.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IOrcamentoService).
// Tabelas....: public.orcamentos, public.orcamento_itens
// Fontes.....: IOrcamentoService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
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

    // Mesmo formato dos erros de validação do [ApiController] (400), com o erro associado ao campo.
    private ActionResult ProblemaDeCampo(DadoInvalidoException ex) =>
        ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [ex.Campo] = [ex.Message] }));
}
