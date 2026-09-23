/**
 * =====================================================================
 * Arquivo....: dashboard.ts
 * Versão.....: 1.3.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do Dashboard, espelhando os DTOs da API (VendasResumoDto,
 *              PedidosPorStatusDto, FaturamentoDiaDto, ContasReceberResumoDto,
 *              ContasPagarResumoDto, EstoqueResumoDashboardDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 22/09/2026 - EstoqueResumoDashboard troca limiteSaldoBaixo pela
 *                        lista de produtos (estoque mínimo por produto).
 *   1.2.0 - 23/09/2026 - ContasPagarResumo (etapa 8).
 *   1.3.0 - 23/09/2026 - Vencimentos (quadros de a pagar e a receber).
 * =====================================================================
 */

export interface PedidosPorStatus {
  rascunho: number
  confirmado: number
  cancelado: number
}

export interface FaturamentoDia {
  /** Data no formato AAAA-MM-DD (sem hora). */
  dia: string
  valor: number
}

export interface VendasResumo {
  faturamento: number
  ticketMedio: number
  quantidadeConfirmados: number
  porStatus: PedidosPorStatus
  faturamentoPorDia: FaturamentoDia[]
}

export interface ContasReceberResumo {
  totalPendente: number
  quantidadePendente: number
  totalAtrasado: number
  quantidadeAtrasado: number
}

/** Mesmo formato do resumo de contas a receber. */
export type ContasPagarResumo = ContasReceberResumo

export interface ProdutoSaldoBaixo {
  produtoId: number
  produtoNome: string
  saldo: number
  estoqueMinimo: number
}

export interface EstoqueResumoDashboard {
  quantidadeSaldoBaixo: number
  produtos: ProdutoSaldoBaixo[]
}

/** Uma parcela pendente atrasada ou que vence nos próximos 7 dias. */
export interface VencimentoItem {
  id: number
  /** Favorecido (a pagar) ou cliente (a receber). */
  titulo: string
  /** De onde vem ("Compra #N", descrição, "Pedido #N") com a parcela X/Y. */
  detalhe: string
  valor: number
  /** AAAA-MM-DD. */
  vencimento: string
  /** Dias até o vencimento: 0 = hoje, negativo = atrasada. */
  dias: number
}

export interface Vencimentos {
  /** Quantidade e total contam a janela inteira; a lista vem limitada. */
  quantidade: number
  total: number
  quantidadeAtrasadas: number
  itens: VencimentoItem[]
}
