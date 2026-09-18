// =====================================================================================
// Arquivo....: ClientesController.cs
// Versão.....: 1.1.0
// Data.......: 18/09/2026
// Descrição..: Endpoints REST do módulo de Clientes.
//                GET    /api/clientes                 -> listagem paginada (filtros: nome,
//                                                        ufs, ativo)
//                GET    /api/clientes/{id}            -> consulta por id
//                POST   /api/clientes                 -> inclusão
//                PUT    /api/clientes/{id}            -> edição
//                PATCH  /api/clientes/{id}/inativar   -> inativação
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IClienteService)
// Tabelas....: public.clientes
// Fontes.....: IClienteService -> ClienteService -> ErpPortfolioDbContext.Clientes.
//              Erros de validação (400) são gerados automaticamente pelo [ApiController]
//              a partir das DataAnnotations dos DTOs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
//   1.1.0 - 18/09/2026 - Documentação dos novos filtros da listagem (ufs, ativo).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
public class ClientesController(IClienteService clienteService) : ControllerBase
{
    /// <summary>Lista clientes com paginação e filtros opcionais por nome, UFs e status.</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginadoDto<ClienteRespostaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginadoDto<ClienteRespostaDto>>> Listar(
        [FromQuery] ClienteFiltroDto filtro, CancellationToken cancelamento)
    {
        return Ok(await clienteService.ListarAsync(filtro, cancelamento));
    }

    /// <summary>Obtém um cliente pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClienteRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteRespostaDto>> ObterPorId(int id, CancellationToken cancelamento)
    {
        var cliente = await clienteService.ObterPorIdAsync(id, cancelamento);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    /// <summary>Cadastra um novo cliente.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ClienteRespostaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteRespostaDto>> Criar(ClienteCriacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var cliente = await clienteService.CriarAsync(dados, cancelamento);
            return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Atualiza os dados de um cliente (inclusive reativação via campo "ativo").</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<ClienteRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteRespostaDto>> Atualizar(
        int id, ClienteAtualizacaoDto dados, CancellationToken cancelamento)
    {
        try
        {
            var cliente = await clienteService.AtualizarAsync(id, dados, cancelamento);
            return cliente is null ? NotFound() : Ok(cliente);
        }
        catch (ConflitoException ex)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflito", detail: ex.Message);
        }
    }

    /// <summary>Inativa um cliente (exclusão lógica). Chamadas repetidas também retornam 204.</summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancelamento)
    {
        var encontrado = await clienteService.InativarAsync(id, cancelamento);
        return encontrado ? NoContent() : NotFound();
    }
}
