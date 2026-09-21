// =====================================================================================
// Arquivo....: CategoriasController.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Endpoints REST do cadastro de categorias de produtos.
//                GET    /api/categorias                 -> listagem paginada (filtros: busca,
//                                                          ativo)
//                GET    /api/categorias/{id}            -> consulta por id
//                POST   /api/categorias                 -> inclusão
//                PUT    /api/categorias/{id}            -> edição
//                PATCH  /api/categorias/{id}/inativar   -> inativação
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via ICategoriaService)
// Tabelas....: public.categorias
// Fontes.....: ICategoriaService -> CategoriaService -> ErpPortfolioDbContext.Categorias.
//              Erros de validação (400) são gerados automaticamente pelo [ApiController]
//              a partir das DataAnnotations dos DTOs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    /// <summary>Lista categorias com paginação e filtros opcionais por nome e status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<CategoriaRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<CategoriaRespostaDto>>> Listar(
        [FromQuery] CategoriaFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await categoriaService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém uma categoria pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CategoriaRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var categoria = await categoriaService.ObterPorIdAsync(id, cancelamento);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    /// <summary>Cadastra uma nova categoria.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<CategoriaRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaRespostaDto>> Criar(CategoriaCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var categoria = await categoriaService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Atualiza os dados de uma categoria (inclusive reativação via campo "ativo").</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<CategoriaRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaRespostaDto>> Atualizar(
        int id, CategoriaAtualizacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var categoria = await categoriaService.AtualizarAsync(id, dados, cancelamento);
            return categoria is null ? NotFound() : Ok(categoria);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Inativa uma categoria (exclusão lógica; os produtos continuam ligados a ela). Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancelamento)
    {
        var encontrado = await categoriaService.InativarAsync(id, cancelamento);
        return encontrado ? NoContent() : NotFound();
    }
}
