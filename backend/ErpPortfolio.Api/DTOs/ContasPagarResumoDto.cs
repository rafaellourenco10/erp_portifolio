// =====================================================================================
// Arquivo....: ContasPagarResumoDto.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: DTO de saída de GET /api/dashboard/contas-pagar: total e quantidade de
//              parcelas Pendentes, e o subconjunto Atrasado (vencimento no passado).
//              Espelho de ContasReceberResumoDto.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Agregação de public.parcelas_pagar.
// Fontes.....: Montado por ContasPagarService.ObterResumoAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record ContasPagarResumoDto(
    decimal TotalPendente,
    int QuantidadePendente,
    decimal TotalAtrasado,
    int QuantidadeAtrasado);
