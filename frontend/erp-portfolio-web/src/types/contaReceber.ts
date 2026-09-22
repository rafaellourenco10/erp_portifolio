/**
 * =====================================================================
 * Arquivo....: contaReceber.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Tipos do módulo de Contas a Receber, espelhando os DTOs da API
 *              (ParcelaRespostaDto, ParcelaFiltroDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export type StatusParcela = 'Pendente' | 'Recebido' | 'Cancelado'

/** Status para filtrar a listagem; "Atrasado" não é gravado, é calculado no servidor. */
export type FiltroStatusParcela = StatusParcela | 'Atrasado'

export interface Parcela {
  id: number
  pedidoId: number
  clienteNome: string
  numeroParcela: number
  totalParcelas: number
  valor: number
  /** Data no formato AAAA-MM-DD (sem hora). */
  vencimento: string
  status: StatusParcela
  /** Data/hora ISO 8601 em UTC; nulo até ser recebida. */
  dataRecebimento: string | null
  atrasado: boolean
}

export interface ParcelaFiltro {
  /** Trecho do nome do cliente ou número do pedido (com ou sem #). */
  busca?: string
  status?: FiltroStatusParcela
  pagina: number
  tamanhoPagina: number
}
