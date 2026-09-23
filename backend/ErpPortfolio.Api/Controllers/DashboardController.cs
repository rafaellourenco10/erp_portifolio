// =====================================================================================
// Arquivo....: DashboardController.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Endpoints REST do Dashboard. Cada rota só delega pro service do módulo
//              de origem (D7) — sem service ou lógica própria do Dashboard.
//                GET /api/dashboard/vendas          -> PedidoService.ObterResumoVendasAsync
//                GET /api/dashboard/contas-receber   -> ContasReceberService.ObterResumoAsync
//                GET /api/dashboard/estoque           -> EstoqueService.ObterResumoAsync
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IPedidoService, IContasReceberService, IEstoqueService)
// Tabelas....: public.pedidos, public.parcelas_receber, public.produtos, public.estoque_movimentacoes
// Fontes.....: Sem erro de validação esperado (sem parâmetros; o mês é sempre o atual, calculado no servidor).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController(IPedidoService pedidoService, IContasReceberService contasReceberService, IEstoqueService estoqueService)
    : ControllerBase
{
    /// <summary>Faturamento e ticket médio do mês atual (só pedidos Confirmados), pedidos por status e faturamento diário.</summary>
    [HttpGet("vendas")]
    [ProducesResponseType<VendasResumoDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VendasResumoDto>> ObterVendas(CancellationToken cancelamento) =>
        Ok(await pedidoService.ObterResumoVendasAsync(cancelamento));

    /// <summary>Total e quantidade de parcelas pendentes/atrasadas, sem filtro de mês.</summary>
    [HttpGet("contas-receber")]
    [ProducesResponseType<ContasReceberResumoDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ContasReceberResumoDto>> ObterContasReceber(CancellationToken cancelamento) =>
        Ok(await contasReceberService.ObterResumoAsync(cancelamento));

    /// <summary>Quantidade de produtos ativos com saldo de estoque baixo.</summary>
    [HttpGet("estoque")]
    [ProducesResponseType<EstoqueResumoDashboardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstoqueResumoDashboardDto>> ObterEstoque(CancellationToken cancelamento) =>
        Ok(await estoqueService.ObterResumoAsync(cancelamento));
}
