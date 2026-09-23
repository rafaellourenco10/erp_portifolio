// =====================================================================================
// Arquivo....: DashboardController.cs
// Versão.....: 1.2.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST do Dashboard. Cada rota só delega pro service do módulo
//              de origem (D7) — sem service ou lógica própria do Dashboard.
//                GET /api/dashboard/vendas          -> PedidoService.ObterResumoVendasAsync
//                GET /api/dashboard/contas-receber   -> ContasReceberService.ObterResumoAsync
//                GET /api/dashboard/contas-pagar     -> ContasPagarService.ObterResumoAsync
//                GET /api/dashboard/vencimentos-pagar   -> ContasPagarService.ObterVencimentosAsync
//                GET /api/dashboard/vencimentos-receber -> ContasReceberService.ObterVencimentosAsync
//                GET /api/dashboard/estoque           -> EstoqueService.ObterResumoAsync
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via IPedidoService, IContasReceberService, IContasPagarService, IEstoqueService)
// Tabelas....: public.pedidos, public.parcelas_receber, public.parcelas_pagar, public.produtos, public.estoque_movimentacoes
// Fontes.....: Sem erro de validação esperado (sem parâmetros; o mês é sempre o atual, calculado no servidor).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - GET /api/dashboard/contas-pagar (etapa 8).
//   1.2.0 - 23/09/2026 - GET vencimentos-pagar e vencimentos-receber.
// =====================================================================================

using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErpPortfolio.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController(IPedidoService pedidoService, IContasReceberService contasReceberService,
    IContasPagarService contasPagarService, IEstoqueService estoqueService)
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

    /// <summary>Total e quantidade de parcelas a pagar pendentes/atrasadas, sem filtro de mês.</summary>
    [HttpGet("contas-pagar")]
    [ProducesResponseType<ContasPagarResumoDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ContasPagarResumoDto>> ObterContasPagar(CancellationToken cancelamento) =>
        Ok(await contasPagarService.ObterResumoAsync(cancelamento));

    /// <summary>Contas a pagar pendentes atrasadas ou que vencem nos próximos 7 dias (mais urgentes primeiro).</summary>
    [HttpGet("vencimentos-pagar")]
    [ProducesResponseType<VencimentosDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VencimentosDto>> ObterVencimentosPagar(CancellationToken cancelamento) =>
        Ok(await contasPagarService.ObterVencimentosAsync(cancelamento));

    /// <summary>Contas a receber pendentes atrasadas ou que vencem nos próximos 7 dias (mais urgentes primeiro).</summary>
    [HttpGet("vencimentos-receber")]
    [ProducesResponseType<VencimentosDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VencimentosDto>> ObterVencimentosReceber(CancellationToken cancelamento) =>
        Ok(await contasReceberService.ObterVencimentosAsync(cancelamento));

    /// <summary>Quantidade de produtos ativos com saldo de estoque baixo.</summary>
    [HttpGet("estoque")]
    [ProducesResponseType<EstoqueResumoDashboardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstoqueResumoDashboardDto>> ObterEstoque(CancellationToken cancelamento) =>
        Ok(await estoqueService.ObterResumoAsync(cancelamento));
}
