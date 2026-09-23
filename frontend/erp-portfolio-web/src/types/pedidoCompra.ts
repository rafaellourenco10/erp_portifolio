/**
 * =====================================================================
 * Arquivo....: pedidoCompra.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Pedidos de Compra, espelhando os DTOs da
 *              API (PedidoCompraRespostaDto, PedidoCompraResumoDto,
 *              PedidoCompraCriacaoDto, PedidoCompraItemEntradaDto e
 *              PedidoCompraFiltroDto). Status chega e sai como TEXTO;
 *              reaproveita os mesmos valores de StatusPedido do Pedido de
 *              Venda (sem forma de pagamento: não há Contas a Pagar ainda).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { StatusPedido } from './pedido'

export type { StatusPedido }

export interface PedidoCompraItem {
  id: number
  produtoId: number
  produtoNome: string
  sku: string
  unidade: string
  quantidade: number
  /** Custo do produto no momento em que o item foi adicionado (congelado). */
  precoUnitario: number
  descontoPercentual: number
  subtotal: number
}

export interface PedidoCompra {
  /** Também é o número do pedido de compra. */
  id: number
  fornecedorId: number
  fornecedorNome: string
  fornecedorDocumento: string
  /** Data/hora ISO 8601 em UTC. */
  dataPedido: string
  status: StatusPedido
  descontoPercentual: number
  subtotalItens: number
  valorTotal: number
  itens: PedidoCompraItem[]
}

/** Uma linha da listagem (sem os itens). */
export interface PedidoCompraResumo {
  id: number
  fornecedorId: number
  fornecedorNome: string
  dataPedido: string
  status: StatusPedido
  valorTotal: number
  quantidadeItens: number
}

/** Item enviado à API: sem preço (o servidor copia do Custo do produto). */
export interface PedidoCompraItemEntrada {
  produtoId: number
  quantidade: number
  descontoPercentual: number
}

/** Corpo do POST e do PUT (o PUT substitui o rascunho inteiro). */
export interface PedidoCompraEntrada {
  fornecedorId: number
  descontoPercentual: number
  itens: PedidoCompraItemEntrada[]
}

export interface PedidoCompraFiltro {
  /** Número do pedido (ex.: 12 ou #12) ou trecho do nome do fornecedor. */
  busca?: string
  status?: StatusPedido
  pagina: number
  tamanhoPagina: number
}
