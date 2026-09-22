// =====================================================================================
// Arquivo....: EstoqueController.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Endpoints REST do módulo de Estoque.
//                GET  /api/estoque                        -> listagem paginada com saldo
//                GET  /api/estoque/{produtoId}/movimentacoes -> extrato paginado do produto
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IEstoqueService)
// Tabelas....: public.produtos, public.estoque_movimentacoes
// Fontes.....: IEstoqueService -> EstoqueService -> ErpPortfolioDbContext.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (listar e extrato).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/estoque")]
[Produces("application/json")]
public class EstoqueController(IEstoqueService estoqueService) : ControllerBase
{
    /// <summary>Lista produtos com o saldo de estoque atual, com busca por nome ou SKU.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<EstoqueResumoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<EstoqueResumoDto>>> Listar(
        [FromQuery] EstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await estoqueService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Extrato de movimentações de um produto, mais recente primeiro.</summary>
    [HttpGet("{produtoId:int}/movimentacoes")]
    [ProducesResponseType<ResultadoPaginadoDto<MovimentacaoRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultadoPaginadoDto<MovimentacaoRespostaDto>>> ObterMovimentacoes(
        int produtoId, [FromQuery] EstoqueFiltroDto filtro, CancellationToken cancelamento)
    {
        var extrato = await estoqueService.ObterExtratoAsync(produtoId, filtro, cancelamento);
        return extrato is null ? NotFound() : Ok(extrato);
    }
}
