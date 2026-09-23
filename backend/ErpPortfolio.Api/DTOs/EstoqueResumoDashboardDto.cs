// =====================================================================================
// Arquivo....: EstoqueResumoDashboardDto.cs
// Versão.....: 1.1.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de GET /api/dashboard/estoque: quantidade e lista de
//              produtos ativos com saldo baixo (saldo ≤ o próprio estoque mínimo do
//              produto). Distinto de EstoqueResumoDto, que é a linha da listagem de
//              Estoque (produto + saldo individual).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Agregação de public.produtos e public.estoque_movimentacoes.
// Fontes.....: Montado por EstoqueService.ObterResumoAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo (limite fixo, só a contagem).
//   1.1.0 - 22/09/2026 - Estoque mínimo passa a ser por produto; devolve a lista
//                        (produto, saldo, mínimo) em vez de só o número.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record EstoqueResumoDashboardDto(int QuantidadeSaldoBaixo, IReadOnlyList<ProdutoSaldoBaixoDto> Produtos);
