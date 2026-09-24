/**
 * =====================================================================
 * Arquivo....: pedido.ts
 * Versão.....: 1.2.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Pedidos, espelhando os DTOs da API
 *              (PedidoRespostaDto, PedidoResumoDto, PedidoCriacaoDto,
 *              PedidoItemEntradaDto e PedidoFiltroDto). Status e forma de
 *              pagamento chegam e saem como TEXTO.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - Vendedor e % de comissão congelada (etapa 10).
 *   1.2.0 - 24/09/2026 - quantidadeDevolvida por item e valorDevolvido (etapa 14).
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
  /** Quanto deste item já voltou em devoluções (0 no orçamento e em pedido sem devolução). */
  quantidadeDevolvida: number
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
  vendedorId: number | null
  vendedorNome: string | null
  /** % de comissão do vendedor congelada ao confirmar; nula no rascunho e em pedidos antigos. */
  percentualComissao: number | null
  descontoPercentual: number
  subtotalItens: number
  valorTotal: number
  itens: PedidoItem[]
  /** Soma das devoluções do pedido (etapa 14). */
  valorDevolvido: number
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
  /** Opcional no rascunho; obrigatório para confirmar. */
  vendedorId: number | null
  descontoPercentual: number
  itens: PedidoItemEntrada[]
}

/** Corpo do PATCH /confirmar: número de parcelas e intervalo entre elas, para as contas a receber. */
export interface PedidoConfirmarEntrada {
  numeroParcelas: number
  intervaloDias: number
}

export interface PedidoFiltro {
  /** Número do pedido (ex.: 12 ou #12) ou trecho do nome do cliente. */
  busca?: string
  status?: StatusPedido
  pagina: number
  tamanhoPagina: number
}
