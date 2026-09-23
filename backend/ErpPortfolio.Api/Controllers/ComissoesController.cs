// =====================================================================================
// Arquivo....: ComissoesController.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST das comissões dos vendedores.
//                GET  /api/comissoes        -> listagem paginada + totais do filtro
//                POST /api/comissoes/gerar-conta -> fecha comissões numa conta a pagar
//              A geração é automática, ao receber a parcela (ContasReceberService).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IComissaoService)
// Tabelas....: public.comissoes
// Fontes.....: IComissaoService -> ComissaoService -> ErpPortfolioDbContext.
//              Regra violada ao gerar conta vira 400 no campo Ids (DadoInvalidoException).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - POST gerar-conta substitui POST pagar (etapa 12).
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

    /// <summary>Fecha comissões pendentes de UM vendedor numa conta a pagar (com a soma). Pague essa conta em Contas a Pagar para as comissões ficarem pagas.</summary>
    [HttpPost("gerar-conta")]
    [Consumes("application/json")]
    [ProducesResponseType<ComissaoContaGeradaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComissaoContaGeradaDto>> GerarConta(ComissaoGerarContaDto dados, CancellationToken cancelamento)
    {
        try
        {
            var conta = await comissaoService.GerarContaAsync(dados.Ids, dados.Vencimento!.Value, cancelamento);
            return StatusCode(StatusCodes.Status201Created, conta);
        }
        catch (DadoInvalidoException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [ex.Campo] = [ex.Message] }));
        }
    }
}
