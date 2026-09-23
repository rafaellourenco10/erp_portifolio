// =====================================================================================
// Arquivo....: VendedoresController.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST do cadastro de vendedores (espelho de FornecedoresController).
//                GET   /api/vendedores                -> listagem paginada (filtros: nome, ativo)
//                GET   /api/vendedores/{id}           -> consulta por id
//                POST  /api/vendedores                -> inclusão
//                PUT   /api/vendedores/{id}           -> edição (e reativação via "ativo")
//                PATCH /api/vendedores/{id}/inativar  -> inativação (exclusão lógica)
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IVendedorService)
// Tabelas....: public.vendedores
// Fontes.....: IVendedorService -> VendedorService -> ErpPortfolioDbContext.
//              CPF duplicado vira 409 (ConflitoException); validação, 400 do [ApiController].
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/vendedores")]
[Produces("application/json")]
public class VendedoresController(IVendedorService vendedorService) : ControllerBase
{
    /// <summary>Lista vendedores com paginação e filtros opcionais por nome e status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<VendedorRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<VendedorRespostaDto>>> Listar(
        [FromQuery] VendedorFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await vendedorService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém um vendedor pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<VendedorRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendedorRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var vendedor = await vendedorService.ObterPorIdAsync(id, cancelamento);
        return vendedor is null ? NotFound() : Ok(vendedor);
    }

    /// <summary>Cadastra um novo vendedor.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<VendedorRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VendedorRespostaDto>> Criar(VendedorCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var vendedor = await vendedorService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = vendedor.Id }, vendedor);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Atualiza os dados de um vendedor (inclusive reativação via campo "ativo").</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<VendedorRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VendedorRespostaDto>> Atualizar(
        int id, VendedorAtualizacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var vendedor = await vendedorService.AtualizarAsync(id, dados, cancelamento);
            return vendedor is null ? NotFound() : Ok(vendedor);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Inativa um vendedor (exclusão lógica). Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancelamento)
    {
        var encontrado = await vendedorService.InativarAsync(id, cancelamento);
        return encontrado ? NoContent() : NotFound();
    }
}
