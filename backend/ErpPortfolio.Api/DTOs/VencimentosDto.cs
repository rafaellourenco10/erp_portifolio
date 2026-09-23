// =====================================================================================
// Arquivo....: VencimentosDto.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Saída dos quadros de vencimentos do Dashboard (a pagar e a receber):
//              parcelas Pendentes atrasadas ou que vencem nos próximos 7 dias, as mais
//              urgentes primeiro. "Dias" é calculado no servidor (negativo = atrasada).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeções de public.parcelas_pagar e public.parcelas_receber.
// Fontes.....: ContasPagarService / ContasReceberService.ObterVencimentosAsync;
//              GET /api/dashboard/vencimentos-pagar | vencimentos-receber.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

// Titulo: favorecido (a pagar) ou cliente (a receber). Detalhe: de onde vem ("Compra #N", descrição
// da conta ou "Pedido #N") com a parcela X/Y. Dias: até o vencimento (0 = hoje; negativo = atrasada).
public record VencimentoItemDto(int Id, string Titulo, string Detalhe, decimal Valor, DateOnly Vencimento, int Dias);

// Quantidade e Total contam a janela inteira; Itens vem cortado em VencimentosCalculo.LimiteItens.
public record VencimentosDto(int Quantidade, decimal Total, int QuantidadeAtrasadas, IReadOnlyList<VencimentoItemDto> Itens);
