// =====================================================================================
// Arquivo....: EstoqueResumoDashboardDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de GET /api/dashboard/estoque: quantidade de produtos ativos
//              com saldo baixo (SPEC.md, D6). Distinto de EstoqueResumoDto, que é a linha
//              da listagem de Estoque (produto + saldo individual).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Agregação de public.produtos e public.estoque_movimentacoes.
// Fontes.....: Montado por EstoqueService.ObterResumoAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record EstoqueResumoDashboardDto(int QuantidadeSaldoBaixo, decimal LimiteSaldoBaixo);
