// =====================================================================================
// Arquivo....: EstoqueResumoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de uma linha da listagem de estoque: produto e saldo atual.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.produtos com o saldo agregado de
//              public.estoque_movimentacoes (Σ Entrada − Σ Saída).
// Fontes.....: Montado direto na consulta do EstoqueService.ListarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record EstoqueResumoDto(
    int ProdutoId,
    string ProdutoNome,
    string Sku,
    string Unidade,
    decimal Saldo);
