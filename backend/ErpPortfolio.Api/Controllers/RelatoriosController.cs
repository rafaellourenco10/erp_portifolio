// =====================================================================================
// Arquivo....: RelatoriosController.cs
// Versão.....: 1.2.0
// Data.......: 23/09/2026
// Descrição..: Endpoints REST dos relatórios. Cada rota devolve os dados (JSON) para a
//              tela; com ?formato=xlsx|pdf devolve o arquivo, gerado da mesma consulta
//              (SPEC.md, R3), com o nome definido em R4 (Content-Disposition).
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
//   1.1.0 - 23/09/2026 - formato=xlsx devolve o arquivo Excel (T2).
//   1.2.0 - 23/09/2026 - formato=pdf devolve o arquivo PDF (T3).
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
    public Task<IActionResult> Vendas([FromQuery] RelatorioVendasFiltroDto filtro, CancellationToken cancelamento) =>
        Responder(filtro.Formato,
            async () => await relatorioService.VendasAsync(filtro, cancelamento),
            () => relatorioService.ModeloVendasAsync(filtro, cancelamento));

    /// <summary>Pedidos de compra do período (datas obrigatórias, no máximo 366 dias), um por linha, com resumo.</summary>
    [HttpGet("compras")]
    [ProducesResponseType<RelatorioPedidosDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Compras([FromQuery] RelatorioComprasFiltroDto filtro, CancellationToken cancelamento) =>
        Responder(filtro.Formato,
            async () => await relatorioService.ComprasAsync(filtro, cancelamento),
            () => relatorioService.ModeloComprasAsync(filtro, cancelamento));

    /// <summary>Posição atual de estoque dos produtos ativos: saldo, valor em estoque e abaixo do mínimo.</summary>
    [HttpGet("estoque")]
    [ProducesResponseType<RelatorioEstoqueDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Estoque([FromQuery] RelatorioEstoqueFiltroDto filtro, CancellationToken cancelamento) =>
        Responder(filtro.Formato,
            async () => await relatorioService.EstoqueAsync(filtro, cancelamento),
            () => relatorioService.ModeloEstoqueAsync(filtro, cancelamento));

    // JSON para a tela; senão o arquivo no formato pedido, montado a partir do mesmo relatório.
    private async Task<IActionResult> Responder(FormatoRelatorio formato, Func<Task<object>> dados, Func<Task<RelatorioModelo>> modelo)
    {
        if (formato == FormatoRelatorio.Json)
            return Ok(await dados());

        var relatorio = await modelo();
        return formato == FormatoRelatorio.Xlsx
            ? File(ExportadorRelatorio.GerarXlsx(relatorio), ExportadorRelatorio.TipoConteudoXlsx, $"{relatorio.NomeArquivo}.xlsx")
            : File(ExportadorRelatorio.GerarPdf(relatorio), ExportadorRelatorio.TipoConteudoPdf, $"{relatorio.NomeArquivo}.pdf");
    }
}
