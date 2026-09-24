// =====================================================================================
// Arquivo....: FluxoCaixaController.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Endpoint REST do fluxo de caixa (SPEC.md etapa 15). Devolve os dados (JSON)
//              para a tela; com ?formato=xlsx|pdf devolve o arquivo, gerado do mesmo
//              cálculo (FC8).
//                GET /api/fluxo-caixa?dataInicio=&dataFim=&agrupamento=Dia|Mes&formato=
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IFluxoCaixaService), só leitura.
// Tabelas....: public.parcelas_receber, public.parcelas_pagar.
// Fontes.....: IFluxoCaixaService -> FluxoCaixaService -> FluxoCaixaCalculo.
//              Período inválido (FC7) vira 400 pelo [ApiController] (IValidatableObject).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/fluxo-caixa")]
public class FluxoCaixaController(IFluxoCaixaService fluxoCaixaService) : ControllerBase
{
    /// <summary>Entradas e saídas realizadas e previstas por dia (até 93 dias) ou mês, com saldo acumulado e menor saldo.</summary>
    [HttpGet]
    [ProducesResponseType<FluxoCaixaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Obter([FromQuery] FluxoCaixaFiltroDto filtro, CancellationToken cancelamento) =>
        filtro.Formato == FormatoRelatorio.Json
            ? Ok(await fluxoCaixaService.ObterAsync(filtro, cancelamento))
            : ExportadorRelatorio.Arquivo(await fluxoCaixaService.ModeloAsync(filtro, cancelamento), filtro.Formato);
}
