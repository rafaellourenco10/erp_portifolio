// =====================================================================================
// Arquivo....: ParcelaPagarRespostaDto.cs
// Versão.....: 2.0.0
// Data.......: 23/09/2026
// Descrição..: DTO de saída de uma parcela a pagar: origem (Compra/Comissao/Avulsa),
//              favorecido (fornecedor, vendedor ou texto da avulsa), descrição, parcela
//              X de Y, valor, vencimento, status e se está atrasada. "Atrasado" é calculado
//              no servidor (SPEC.md etapa 8, P3); favorecido na etapa 12 (CP2).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.parcelas_pagar com LEFT JOIN em pedidos_compra/
//              fornecedores e vendedores.
// Fontes.....: Montado na projeção do ContasPagarService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   2.0.0 - 23/09/2026 - Origem, vendedor, favorecido e descrição; FornecedorNome vira
//                        Favorecido e PedidoCompraId passa a ser opcional (etapa 12).
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record ParcelaPagarRespostaDto(
    int Id,
    OrigemContaPagar Origem,
    int? PedidoCompraId,
    int? VendedorId,
    string? Favorecido,
    string? Descricao, // nula nas parcelas de compra (a tela mostra "Compra #N")
    int NumeroParcela,
    int TotalParcelas,
    decimal Valor,
    DateOnly Vencimento,
    StatusParcelaPagar Status,
    DateTime? DataPagamento,
    bool Atrasado);
