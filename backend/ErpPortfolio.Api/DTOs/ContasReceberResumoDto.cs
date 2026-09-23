// =====================================================================================
// Arquivo....: ContasReceberResumoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de GET /api/dashboard/contas-receber: total e quantidade de
//              parcelas Pendentes, e o subconjunto Atrasado (vencimento no passado),
//              sem filtro de mês (SPEC.md, D5).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Agregação de public.parcelas_receber.
// Fontes.....: Montado por ContasReceberService.ObterResumoAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record ContasReceberResumoDto(
    decimal TotalPendente,
    int QuantidadePendente,
    decimal TotalAtrasado,
    int QuantidadeAtrasado);
