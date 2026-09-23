// =====================================================================================
// Arquivo....: ParcelaPagarRespostaDto.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: DTO de saída de uma linha da listagem de contas a pagar: pedido de compra,
//              fornecedor, número da parcela (X de Y), valor, vencimento, status e se está
//              atrasada. "Atrasado" é calculado no servidor (SPEC.md, P3).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.parcelas_pagar, com JOIN em public.pedidos_compra e
//              public.fornecedores, e contagem de parcelas irmãs do mesmo pedido.
// Fontes.....: Montado direto na consulta do ContasPagarService.ListarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record ParcelaPagarRespostaDto(
    int Id,
    int PedidoCompraId,
    string FornecedorNome,
    int NumeroParcela,
    int TotalParcelas,
    decimal Valor,
    DateOnly Vencimento,
    StatusParcelaPagar Status,
    DateTime? DataPagamento,
    bool Atrasado);
