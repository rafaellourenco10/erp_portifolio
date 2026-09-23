// =====================================================================================
// Arquivo....: FornecedoresController.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Endpoints REST do módulo de Fornecedores. Espelho de ClientesController.
//                GET    /api/fornecedores                 -> listagem paginada (filtros: nome,
//                                                            ufs, ativo)
//                GET    /api/fornecedores/{id}            -> consulta por id
//                POST   /api/fornecedores                 -> inclusão
//                PUT    /api/fornecedores/{id}            -> edição
//                PATCH  /api/fornecedores/{id}/inativar   -> inativação
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IFornecedorService)
// Tabelas....: public.fornecedores
// Fontes.....: IFornecedorService -> FornecedorService -> ErpPortfolioDbContext.Fornecedores.
//              Erros de validação (400) são gerados automaticamente pelo [ApiController]
//              a partir das DataAnnotations dos DTOs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/fornecedores")]
[Produces("application/json")]
public class FornecedoresController(IFornecedorService fornecedorService) : ControllerBase
{
    /// <summary>Lista fornecedores com paginação e filtros opcionais por nome, UFs e status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<FornecedorRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<FornecedorRespostaDto>>> Listar(
        [FromQuery] FornecedorFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await fornecedorService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém um fornecedor pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<FornecedorRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FornecedorRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var fornecedor = await fornecedorService.ObterPorIdAsync(id, cancelamento);
        return fornecedor is null ? NotFound() : Ok(fornecedor);
    }

    /// <summary>Cadastra um novo fornecedor.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<FornecedorRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FornecedorRespostaDto>> Criar(FornecedorCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var fornecedor = await fornecedorService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = fornecedor.Id }, fornecedor);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Atualiza os dados de um fornecedor (inclusive reativação via campo "ativo").</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<FornecedorRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FornecedorRespostaDto>> Atualizar(
        int id, FornecedorAtualizacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var fornecedor = await fornecedorService.AtualizarAsync(id, dados, cancelamento);
            return fornecedor is null ? NotFound() : Ok(fornecedor);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Inativa um fornecedor (exclusão lógica). Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancelamento)
    {
        var encontrado = await fornecedorService.InativarAsync(id, cancelamento);
        return encontrado ? NoContent() : NotFound();
    }
}
