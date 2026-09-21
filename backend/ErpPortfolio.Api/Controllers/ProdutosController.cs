// =====================================================================================
// Arquivo....: ProdutosController.cs
// Versão.....: 1.1.0
// Data.......: 21/09/2026
// Descrição..: Endpoints REST do módulo de Produtos.
//                GET    /api/produtos                 -> listagem paginada (filtros: busca,
//                                                        ativo)
//                GET    /api/produtos/{id}            -> consulta por id
//                POST   /api/produtos                 -> inclusão
//                PUT    /api/produtos/{id}            -> edição
//                PATCH  /api/produtos/{id}/inativar   -> inativação
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IProdutoService)
// Tabelas....: public.produtos
// Fontes.....: IProdutoService -> ProdutoService -> ErpPortfolioDbContext.Produtos.
//              Erros de validação (400) são gerados automaticamente pelo [ApiController]
//              a partir das DataAnnotations dos DTOs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 21/09/2026 - Categoria inválida (DadoInvalidoException) vira 400 no campo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/produtos")]
[Produces("application/json")]
public class ProdutosController(IProdutoService produtoService) : ControllerBase
{
    /// <summary>Lista produtos com paginação e filtros opcionais por nome/SKU e status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<ProdutoRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<ProdutoRespostaDto>>> Listar(
        [FromQuery] ProdutoFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await produtoService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém um produto pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ProdutoRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var produto = await produtoService.ObterPorIdAsync(id, cancelamento);
        return produto is null ? NotFound() : Ok(produto);
    }

    /// <summary>Cadastra um novo produto.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ProdutoRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoRespostaDto>> Criar(ProdutoCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var produto = await produtoService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
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

    /// <summary>Atualiza os dados de um produto (inclusive reativação via campo "ativo").</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<ProdutoRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoRespostaDto>> Atualizar(
        int id, ProdutoAtualizacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var produto = await produtoService.AtualizarAsync(id, dados, cancelamento);
            return produto is null ? NotFound() : Ok(produto);
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

    /// <summary>Inativa um produto (exclusão lógica). Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancelamento)
    {
        var encontrado = await produtoService.InativarAsync(id, cancelamento);
        return encontrado ? NoContent() : NotFound();
    }

    // Mesmo formato dos erros de validação do [ApiController] (400), com o erro associado ao campo.
    private ActionResult ProblemaDeCampo(DadoInvalidoException ex) =>
        ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [ex.Campo] = [ex.Message] }));
}
