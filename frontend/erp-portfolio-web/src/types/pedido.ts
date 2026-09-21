/**
 * =====================================================================
 * Arquivo....: pedido.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Tipos do módulo de Pedidos, espelhando os DTOs da API
 *              (PedidoRespostaDto, PedidoResumoDto, PedidoCriacaoDto,
 *              PedidoItemEntradaDto e PedidoFiltroDto). Status e forma de
 *              pagamento chegam e saem como TEXTO.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export type StatusPedido = 'Rascunho' | 'Confirmado' | 'Cancelado'

export type FormaPagamento = 'Dinheiro' | 'Pix' | 'Boleto' | 'Cartao'

export const OPCOES_FORMA_PAGAMENTO: { value: FormaPagamento; label: string }[] = [
  { value: 'Dinheiro', label: 'Dinheiro' },
  { value: 'Pix', label: 'Pix' },
  { value: 'Boleto', label: 'Boleto' },
  { value: 'Cartao', label: 'Cartão' },
]

export const OPCOES_STATUS_PEDIDO: { value: StatusPedido; label: string }[] = [
  { value: 'Rascunho', label: 'Rascunho' },
  { value: 'Confirmado', label: 'Confirmado' },
  { value: 'Cancelado', label: 'Cancelado' },
]

export interface PedidoItem {
  id: number
  produtoId: number
  produtoNome: string
  sku: string
  unidade: string
  quantidade: number
  /** Preço do produto no momento em que o item foi adicionado (congelado). */
  precoUnitario: number
  descontoPercentual: number
  subtotal: number
}

export interface Pedido {
  /** Também é o número do pedido. */
  id: number
  clienteId: number
  clienteNome: string
  clienteDocumento: string
  /** Data/hora ISO 8601 em UTC. */
  dataPedido: string
  status: StatusPedido
  formaPagamento: FormaPagamento | null
  descontoPercentual: number
  subtotalItens: number
  valorTotal: number
  itens: PedidoItem[]
}

/** Uma linha da listagem (sem os itens). */
export interface PedidoResumo {
  id: number
  clienteId: number
  clienteNome: string
  dataPedido: string
  status: StatusPedido
  valorTotal: number
  quantidadeItens: number
}

/** Item enviado à API: sem preço (o servidor copia do produto). */
export interface PedidoItemEntrada {
  produtoId: number
  quantidade: number
  descontoPercentual: number
}

/** Corpo do POST e do PUT (o PUT substitui o rascunho inteiro). */
export interface PedidoEntrada {
  clienteId: number
  formaPagamento: FormaPagamento | null
  descontoPercentual: number
  itens: PedidoItemEntrada[]
}

export interface PedidoFiltro {
  /** Número do pedido (ex.: 12 ou #12) ou trecho do nome do cliente. */
  busca?: string
  status?: StatusPedido
  pagina: number
  tamanhoPagina: number
}
