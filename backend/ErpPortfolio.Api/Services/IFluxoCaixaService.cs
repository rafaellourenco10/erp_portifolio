// =====================================================================================
// Arquivo....: IFluxoCaixaService.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Contrato do fluxo de caixa (SPEC.md etapa 15): os dados da tela e o mesmo
//              resultado no modelo do ExportadorRelatorio.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (via implementação), só leitura.
// Tabelas....: public.parcelas_receber, public.parcelas_pagar.
// Fontes.....: Implementado por FluxoCaixaService; usado pelo FluxoCaixaController.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.DTOs;

namespace ErpPortfolio.Api.Services;

public interface IFluxoCaixaService
{
    /// <summary>Realizado + previsto do período, com saldo acumulado desde o início (FC1-FC7).</summary>
    Task<FluxoCaixaDto> ObterAsync(FluxoCaixaFiltroDto filtro, CancellationToken cancelamento);

    /// <summary>O mesmo fluxo como relatório (resumo + tabela) para Excel/PDF (FC8).</summary>
    Task<RelatorioModelo> ModeloAsync(FluxoCaixaFiltroDto filtro, CancellationToken cancelamento);
}
