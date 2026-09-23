// =====================================================================================
// Arquivo....: MovimentacaoRespostaDto.cs
// Versão.....: 1.1.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de uma movimentação de estoque, usado no extrato por produto.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.estoque_movimentacoes.
// Fontes.....: Montado direto na consulta do EstoqueService.ObterExtratoAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
//   1.1.0 - 22/09/2026 - PedidoCompraId, para distinguir a origem "Compra #N" (etapa 7).
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record MovimentacaoRespostaDto(
    TipoMovimentacao Tipo,
    decimal Quantidade,
    string? Motivo,
    int? PedidoId,
    int? PedidoCompraId,
    DateTime DataMovimentacao);
