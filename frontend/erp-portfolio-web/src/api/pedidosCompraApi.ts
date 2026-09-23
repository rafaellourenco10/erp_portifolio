/**
 * =====================================================================
 * Arquivo....: pedidosCompraApi.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Pedidos de Compra. Espelho de
 *              pedidosApi.ts (confirmar envia as parcelas a pagar).
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (PedidosCompraController)
 *                GET    /pedidos-compra?busca=&status=&pagina=&tamanhoPagina=
 *                GET    /pedidos-compra/{id}
 *                POST   /pedidos-compra
 *                PUT    /pedidos-compra/{id}
 *                PATCH  /pedidos-compra/{id}/confirmar
 *                PATCH  /pedidos-compra/{id}/cancelar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - confirmar envia numeroParcelas/intervaloDias (contas a pagar, etapa 8).
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type {
  PedidoCompra,
  PedidoCompraEntrada,
  PedidoCompraFiltro,
  PedidoCompraResumo,
} from '../types/pedidoCompra'
import type { PedidoConfirmarEntrada } from '../types/pedido'
import { axiosClient } from './axiosClient'

export const pedidosCompraApi = {
  async listar(filtro: PedidoCompraFiltro): Promise<ResultadoPaginado<PedidoCompraResumo>> {
    const resposta = await axiosClient.get<ResultadoPaginado<PedidoCompraResumo>>('/pedidos-compra', { params: filtro })
    return resposta.data
  },

  async obterPorId(id: number): Promise<PedidoCompra> {
    const resposta = await axiosClient.get<PedidoCompra>(`/pedidos-compra/${id}`)
    return resposta.data
  },

  async criar(dados: PedidoCompraEntrada): Promise<PedidoCompra> {
    const resposta = await axiosClient.post<PedidoCompra>('/pedidos-compra', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: PedidoCompraEntrada): Promise<PedidoCompra> {
    const resposta = await axiosClient.put<PedidoCompra>(`/pedidos-compra/${id}`, dados)
    return resposta.data
  },

  async confirmar(id: number, dados: PedidoConfirmarEntrada): Promise<PedidoCompra> {
    const resposta = await axiosClient.patch<PedidoCompra>(`/pedidos-compra/${id}/confirmar`, dados)
    return resposta.data
  },

  async cancelar(id: number): Promise<void> {
    await axiosClient.patch(`/pedidos-compra/${id}/cancelar`)
  },
}
