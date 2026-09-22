// =====================================================================================
// Arquivo....: ParcelaRespostaDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída de uma linha da listagem de contas a receber: pedido,
//              cliente, número da parcela (X de Y), valor, vencimento, status e se está
//              atrasada. "Atrasado" é calculado no servidor (SPEC.md, C5) — nunca
//              confiar no relógio/fuso do navegador.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.parcelas_receber, com JOIN em public.pedidos e
//              public.clientes, e contagem de parcelas irmãs do mesmo pedido.
// Fontes.....: Montado direto na consulta do ContasReceberService.ListarAsync.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record ParcelaRespostaDto(
    int Id,
    int PedidoId,
    string ClienteNome,
    int NumeroParcela,
    int TotalParcelas,
    decimal Valor,
    DateOnly Vencimento,
    StatusParcela Status,
    DateTime? DataRecebimento,
    bool Atrasado);
