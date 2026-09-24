/**
 * =====================================================================
 * Arquivo....: orcamento.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Orçamentos (etapa 13), espelhando os DTOs da API
 *              (OrcamentoRespostaDto, OrcamentoResumoDto, OrcamentoCriacaoDto e
 *              OrcamentoFiltroDto). Os itens são os mesmos do pedido. "Vencido" não é
 *              status: vem calculado no campo `vencido` e existe só como filtro.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { FormaPagamento, PedidoEntrada, PedidoItem } from './pedido'

export type StatusOrcamento = 'Aberto' | 'Aprovado' | 'Perdido'

/** Filtro da lista: "Aberto" traz só os dentro da validade; "Vencido" os abertos já vencidos. */
export type FiltroStatusOrcamento = StatusOrcamento | 'Vencido'

export const OPCOES_STATUS_ORCAMENTO: { value: FiltroStatusOrcamento; label: string }[] = [
  { value: 'Aberto', label: 'Aberto' },
  { value: 'Vencido', label: 'Vencido' },
  { value: 'Aprovado', label: 'Aprovado' },
  { value: 'Perdido', label: 'Perdido' },
]

export interface Orcamento {
  /** Também é o número do orçamento. */
  id: number
  clienteId: number
  clienteNome: string
  clienteDocumento: string
  /** Data/hora ISO 8601 em UTC. */
  dataOrcamento: string
  /** Data (AAAA-MM-DD): último dia em que o orçamento vale. */
  validade: string
  status: StatusOrcamento
  vencido: boolean
  formaPagamento: FormaPagamento | null
  vendedorId: number | null
  vendedorNome: string | null
  descontoPercentual: number
  subtotalItens: number
  valorTotal: number
  observacoes: string | null
  motivoPerda: string | null
  /** Pedido gerado (só no Aprovado). */
  pedidoId: number | null
  itens: PedidoItem[]
}

/** Uma linha da listagem (sem os itens). */
export interface OrcamentoResumo {
  id: number
  clienteId: number
  clienteNome: string
  dataOrcamento: string
  validade: string
  status: StatusOrcamento
  vencido: boolean
  valorTotal: number
  quantidadeItens: number
  pedidoId: number | null
}

/** Corpo do POST e do PUT: o do pedido + validade e observações. */
export interface OrcamentoEntrada extends PedidoEntrada {
  validade: string
  observacoes: string | null
}

export interface OrcamentoFiltro {
  /** Número do orçamento (ex.: 12 ou #12) ou trecho do nome do cliente. */
  busca?: string
  status?: FiltroStatusOrcamento
  pagina: number
  tamanhoPagina: number
}
