// =====================================================================================
// Arquivo....: IRelatorioService.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Contrato das consultas dos relatórios (vendas, compras, estoque).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente (contrato).
// Tabelas....: Não se aplica.
// Fontes.....: Implementado por RelatorioService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Modelo*Async, para a exportação (T2).
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IRelatorioService
{
    /// <summary>Pedidos de venda do período (filtro já validado), um por linha, com resumo das linhas listadas.</summary>
    Task<RelatorioPedidosDto> VendasAsync(RelatorioVendasFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Pedidos de compra do período (filtro já validado), um por linha, com resumo das linhas listadas.</summary>
    Task<RelatorioPedidosDto> ComprasAsync(RelatorioComprasFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Posição atual de estoque dos produtos ativos.</summary>
    Task<RelatorioEstoqueDto> EstoqueAsync(RelatorioEstoqueFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Mesma consulta de VendasAsync, já no modelo de exportação (filtros descritos, resumo, colunas).</summary>
    Task<RelatorioModelo> ModeloVendasAsync(RelatorioVendasFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Mesma consulta de ComprasAsync, no modelo de exportação.</summary>
    Task<RelatorioModelo> ModeloComprasAsync(RelatorioComprasFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>Mesma consulta de EstoqueAsync, no modelo de exportação.</summary>
    Task<RelatorioModelo> ModeloEstoqueAsync(RelatorioEstoqueFiltroDto filtro, CancellationToken cancelamento);
}
