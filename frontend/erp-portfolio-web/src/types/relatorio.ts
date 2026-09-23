/**
 * =====================================================================
 * Arquivo....: relatorio.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Relatórios, espelhando os DTOs da API
 *              (RelatorioPedidosDto, RelatorioEstoqueDto e os filtros).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { StatusPedido } from './pedido'

export type FormatoArquivo = 'xlsx' | 'pdf'

/** Filtro de vendas ou compras; datas no formato AAAA-MM-DD (inclusivas). */
export interface RelatorioPedidosFiltro {
  dataInicio: string
  dataFim: string
  /** Ausente = todos os status. */
  status?: StatusPedido
  clienteId?: number
  fornecedorId?: number
}

export interface RelatorioPedidoLinha {
  id: number
  /** Data/hora ISO 8601 em UTC. */
  dataPedido: string
  /** Cliente (vendas) ou fornecedor (compras). */
  nome: string
  quantidadeItens: number
  valorTotal: number
  status: StatusPedido
}

export interface RelatorioPedidos {
  linhas: RelatorioPedidoLinha[]
  quantidadePedidos: number
  valorTotal: number
  ticketMedio: number
}

export interface RelatorioEstoqueFiltro {
  categoriaId?: number
  somenteAbaixoMinimo?: boolean
}

export interface RelatorioEstoqueLinha {
  produtoId: number
  nome: string
  sku: string
  categoriaNome: string | null
  unidade: string
  saldo: number
  estoqueMinimo: number
  custo: number
  valorEstoque: number
  abaixoMinimo: boolean
}

export interface RelatorioEstoque {
  linhas: RelatorioEstoqueLinha[]
  quantidadeProdutos: number
  valorTotalEstoque: number
  quantidadeAbaixoMinimo: number
}
