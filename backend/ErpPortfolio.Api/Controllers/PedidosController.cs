// =====================================================================================
// Arquivo....: PedidosController.cs
// Versão.....: 1.3.0
// Data.......: 21/09/2026
// Descrição..: Endpoints REST do módulo de Pedidos.
//                GET    /api/pedidos        -> listagem paginada (filtros: busca, status)
//                GET    /api/pedidos/{id}   -> consulta por id (com itens)
//                POST   /api/pedidos        -> criação de um rascunho
//                PUT    /api/pedidos/{id}   -> edição do rascunho
//                PATCH  /api/pedidos/{id}/confirmar -> Rascunho -> Confirmado
//                PATCH  /api/pedidos/{id}/cancelar  -> Rascunho/Confirmado -> Cancelado
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IPedidoService)
// Tabelas....: public.pedidos, public.pedido_itens
// Fontes.....: IPedidoService -> PedidoService -> ErpPortfolioDbContext.
//              Erros de validação (400) vêm do [ApiController] (DataAnnotations dos DTOs)
//              ou de DadoInvalidoException (regras que dependem do banco).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo (criar e obter).
//   1.1.0 - 21/09/2026 - Listagem paginada.
//   1.2.0 - 21/09/2026 - Edição do rascunho (PUT); transição inválida vira 409.
//   1.3.0 - 21/09/2026 - Confirmar e cancelar (PATCH).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Produces("application/json")]
public class PedidosController(IPedidoService pedidoService) : ControllerBase
{
    /// <summary>Lista pedidos (mais recentes primeiro) com paginação e filtros opcionais por número/cliente e status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<PedidoResumoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<PedidoResumoDto>>> Listar(
        [FromQuery] PedidoFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await pedidoService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém um pedido pelo id, com cliente e itens.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<PedidoRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var pedido = await pedidoService.ObterPorIdAsync(id, cancelamento);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    /// <summary>Cria um pedido em rascunho. O preço de cada item é copiado do produto e o total é calculado pelo servidor.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<PedidoRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PedidoRespostaDto>> Criar(PedidoCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var pedido = await pedidoService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
        }
        catch (DadoInvalidoException ex)
        {
            return ProblemaDeCampo(ex);
        }
    }

    /// <summary>Substitui cliente, itens, desconto e pagamento de um pedido em rascunho. Itens que já estavam no pedido mantêm o preço congelado.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<PedidoRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoRespostaDto>> Atualizar(int id, PedidoCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var pedido = await pedidoService.AtualizarAsync(id, dados, cancelamento);
            return pedido is null ? NotFound() : Ok(pedido);
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

    /// <summary>Confirma um rascunho (fica travado). Exige forma de pagamento, cliente ativo e produtos ativos.</summary>
    [HttpPatch("{id:int}/confirmar")]
    [ProducesResponseType<PedidoRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoRespostaDto>> Confirmar(int id, CancellationToken cancelamento)
    {
        try
        {
            var pedido = await pedidoService.ConfirmarAsync(id, cancelamento);
            return pedido is null ? NotFound() : Ok(pedido);
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

    /// <summary>Cancela um pedido (rascunho ou confirmado). Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancelamento)
    {
        var encontrado = await pedidoService.CancelarAsync(id, cancelamento);
        return encontrado ? NoContent() : NotFound();
    }

    // Mesmo formato dos erros de validação do [ApiController] (400), com o erro associado ao campo.
    private ActionResult ProblemaDeCampo(DadoInvalidoException ex) =>
        ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [ex.Campo] = [ex.Message] }));
}
