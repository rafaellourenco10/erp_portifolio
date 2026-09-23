// =====================================================================================
// Arquivo....: PedidosCompraController.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST do módulo de Pedidos de Compra. Espelho de
//              PedidosController (confirmar recebe as parcelas a pagar).
//                GET    /api/pedidos-compra        -> listagem paginada (filtros: busca, status)
//                GET    /api/pedidos-compra/{id}   -> consulta por id (com itens)
//                POST   /api/pedidos-compra        -> criação de um rascunho
//                PUT    /api/pedidos-compra/{id}   -> edição do rascunho
//                PATCH  /api/pedidos-compra/{id}/confirmar -> Rascunho -> Confirmado (entrada de estoque + parcelas a pagar)
//                PATCH  /api/pedidos-compra/{id}/cancelar  -> Rascunho/Confirmado -> Cancelado
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IPedidoCompraService)
// Tabelas....: public.pedidos_compra, public.pedido_compra_itens
// Fontes.....: IPedidoCompraService -> PedidoCompraService -> ErpPortfolioDbContext.
//              Erros de validação (400) vêm do [ApiController] (DataAnnotations dos DTOs)
//              ou de DadoInvalidoException (regras que dependem do banco).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Confirmar recebe numeroParcelas/intervaloDias (gera parcelas a pagar, etapa 8).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/pedidos-compra")]
[Produces("application/json")]
public class PedidosCompraController(IPedidoCompraService pedidoCompraService) : ControllerBase
{
    /// <summary>Lista pedidos de compra (mais recentes primeiro) com paginação e filtros opcionais por número/fornecedor e status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<PedidoCompraResumoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<PedidoCompraResumoDto>>> Listar(
        [FromQuery] PedidoCompraFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await pedidoCompraService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém um pedido de compra pelo id, com fornecedor e itens.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<PedidoCompraRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoCompraRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var pedido = await pedidoCompraService.ObterPorIdAsync(id, cancelamento);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    /// <summary>Cria um pedido de compra em rascunho. O preço de cada item é copiado do custo do produto e o total é calculado pelo servidor.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<PedidoCompraRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PedidoCompraRespostaDto>> Criar(PedidoCompraCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var pedido = await pedidoCompraService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
        }
        catch (DadoInvalidoException ex)
        {
            return ProblemaDeCampo(ex);
        }
    }

    /// <summary>Substitui fornecedor, itens e desconto de um pedido em rascunho. Itens que já estavam no pedido mantêm o preço congelado.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<PedidoCompraRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoCompraRespostaDto>> Atualizar(int id, PedidoCompraCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var pedido = await pedidoCompraService.AtualizarAsync(id, dados, cancelamento);
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

    /// <summary>Confirma um rascunho (fica travado). Exige fornecedor ativo e produtos ativos; dá entrada no estoque, atualiza o custo dos produtos e gera as parcelas a pagar. Corpo opcional (padrão: 1 parcela, 30 dias).</summary>
    [HttpPatch("{id:int}/confirmar")]
    [ProducesResponseType<PedidoCompraRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoCompraRespostaDto>> Confirmar(int id, PedidoConfirmarDto? dados, CancellationToken cancelamento)
    {
        dados ??= new PedidoConfirmarDto();

        try
        {
            var pedido = await pedidoCompraService.ConfirmarAsync(id, dados.NumeroParcelas, dados.IntervaloDias, cancelamento);
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

    /// <summary>Cancela um pedido (rascunho ou confirmado). Cancelar um confirmado exige saldo suficiente em cada item. Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancelamento)
    {
        try
        {
            var encontrado = await pedidoCompraService.CancelarAsync(id, cancelamento);
            return encontrado ? NoContent() : NotFound();
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
