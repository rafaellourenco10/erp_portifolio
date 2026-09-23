/**
 * =====================================================================
 * Arquivo....: contaPagar.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Contas a Pagar, espelhando os DTOs da API
 *              (ParcelaPagarRespostaDto, ParcelaPagarFiltroDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export type StatusParcelaPagar = 'Pendente' | 'Pago' | 'Cancelado'

/** Status para filtrar a listagem; "Atrasado" não é gravado, é calculado no servidor. */
export type FiltroStatusParcelaPagar = StatusParcelaPagar | 'Atrasado'

export interface ParcelaPagar {
  id: number
  pedidoCompraId: number
  fornecedorNome: string
  numeroParcela: number
  totalParcelas: number
  valor: number
  /** Data no formato AAAA-MM-DD (sem hora). */
  vencimento: string
  status: StatusParcelaPagar
  /** Data/hora ISO 8601 em UTC; nulo até ser paga. */
  dataPagamento: string | null
  atrasado: boolean
}

export interface ParcelaPagarFiltro {
  /** Trecho do nome do fornecedor ou número do pedido de compra (com ou sem #). */
  busca?: string
  status?: FiltroStatusParcelaPagar
  pagina: number
  tamanhoPagina: number
}
