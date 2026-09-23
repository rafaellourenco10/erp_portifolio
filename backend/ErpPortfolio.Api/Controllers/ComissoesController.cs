// =====================================================================================
// Arquivo....: ComissoesController.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST das comissões dos vendedores.
//                GET  /api/comissoes        -> listagem paginada + totais do filtro
//                POST /api/comissoes/pagar  -> marca comissões como pagas ao vendedor
//              A geração é automática, ao receber a parcela (ContasReceberService).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IComissaoService)
// Tabelas....: public.comissoes
// Fontes.....: IComissaoService -> ComissaoService -> ErpPortfolioDbContext.
//              Id inexistente ao pagar vira 400 no campo Ids (DadoInvalidoException).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/comissoes")]
[Produces("application/json")]
public class ComissoesController(IComissaoService comissaoService) : ControllerBase
{
    /// <summary>Lista comissões (mais recente primeiro) com os totais gerado/pendente/pago de todo o filtro.</summary>
    [HttpGet]
    [ProducesResponseType<ComissaoListaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComissaoListaDto>> Listar([FromQuery] ComissaoFiltroDto filtro, CancellationToken cancelamento) =>
        Ok(await comissaoService.ListarAsync(filtro, cancelamento));

    /// <summary>Marca as comissões informadas como pagas ao vendedor. As já pagas não mudam; id inexistente não altera nada (400).</summary>
    [HttpPost("pagar")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pagar(ComissaoPagarDto dados, CancellationToken cancelamento)
    {
        try
        {
            await comissaoService.PagarAsync(dados.Ids, cancelamento);
            return NoContent();
        }
        catch (DadoInvalidoException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [ex.Campo] = [ex.Message] }));
        }
    }
}
