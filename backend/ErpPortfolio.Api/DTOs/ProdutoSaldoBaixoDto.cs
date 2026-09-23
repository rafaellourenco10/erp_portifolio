// =====================================================================================
// Arquivo....: ProdutoSaldoBaixoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Um produto com saldo de estoque igual ou abaixo do seu próprio estoque
//              mínimo, para a lista do card "Saldo baixo" do Dashboard.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.produtos com o saldo agregado de
//              public.estoque_movimentacoes.
// Fontes.....: Montado por EstoqueService.ObterResumoAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record ProdutoSaldoBaixoDto(int ProdutoId, string ProdutoNome, decimal Saldo, decimal EstoqueMinimo);
