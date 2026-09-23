// =====================================================================================
// Arquivo....: RelatoriosController.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST dos relatórios. Cada rota devolve os dados (JSON) para a
//              tela; a exportação (.xlsx/.pdf) usa a mesma consulta (SPEC.md, R3).
//                GET /api/relatorios/vendas   -> pedidos de venda do período
//                GET /api/relatorios/compras  -> pedidos de compra do período
//                GET /api/relatorios/estoque  -> posição atual de estoque
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IRelatorioService), só leitura.
// Tabelas....: public.pedidos, public.pedidos_compra, public.produtos (e relacionadas).
// Fontes.....: IRelatorioService -> RelatorioService -> ErpPortfolioDbContext.
//              Período inválido (R1) vira 400 pelo [ApiController] (IValidatableObject).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo (JSON).
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
public class RelatoriosController(IRelatorioService relatorioService) : ControllerBase
{
    /// <summary>Pedidos de venda do período (datas obrigatórias, no máximo 366 dias), um por linha, com resumo.</summary>
    [HttpGet("vendas")]
    [ProducesResponseType<RelatorioPedidosDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Vendas([FromQuery] RelatorioVendasFiltroDto filtro, CancellationToken cancelamento) =>
        Ok(await relatorioService.VendasAsync(filtro, cancelamento));

    /// <summary>Pedidos de compra do período (datas obrigatórias, no máximo 366 dias), um por linha, com resumo.</summary>
    [HttpGet("compras")]
    [ProducesResponseType<RelatorioPedidosDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Compras([FromQuery] RelatorioComprasFiltroDto filtro, CancellationToken cancelamento) =>
        Ok(await relatorioService.ComprasAsync(filtro, cancelamento));

    /// <summary>Posição atual de estoque dos produtos ativos: saldo, valor em estoque e abaixo do mínimo.</summary>
    [HttpGet("estoque")]
    [ProducesResponseType<RelatorioEstoqueDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Estoque([FromQuery] RelatorioEstoqueFiltroDto filtro, CancellationToken cancelamento) =>
        Ok(await relatorioService.EstoqueAsync(filtro, cancelamento));
}
