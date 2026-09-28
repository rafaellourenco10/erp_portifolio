// =====================================================================================
// Arquivo....: EmpresaController.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Dados da empresa emitente da NF-e (etapa 16): GET e PUT /api/empresa. A
//              tabela tem uma linha só; o PUT cria ou atualiza.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.empresa
// Fontes.....: Lê e grava direto pelo ErpPortfolioDbContext (sem regra além da validação do DTO).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/empresa")]
[Produces("application/json")]
public class EmpresaController(ErpPortfolioDbContext contexto) : ControllerBase
{
    /// <summary>Dados da empresa; 404 enquanto não forem cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType<EmpresaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpresaDto>> Obter(CancellationToken cancelamento)
    {
        var empresa = await contexto.Empresa.AsNoTracking().FirstOrDefaultAsync(cancelamento);
        return empresa is null ? NotFound() : Ok(EmpresaDto.DeEntidade(empresa));
    }

    /// <summary>Cria ou atualiza os dados da empresa.</summary>
    [HttpPut]
    [Consumes("application/json")]
    [ProducesResponseType<EmpresaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmpresaDto>> Salvar(EmpresaDto dados, CancellationToken cancelamento)
    {
        var empresa = await contexto.Empresa.FirstOrDefaultAsync(cancelamento);
        if (empresa is null)
        {
            empresa = new Empresa { Id = 1 };
            contexto.Empresa.Add(empresa);
        }

        dados.Aplicar(empresa);
        await contexto.SaveChangesAsync(cancelamento);

        return Ok(EmpresaDto.DeEntidade(empresa));
    }
}
