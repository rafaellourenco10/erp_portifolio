/**
 * =====================================================================
 * Arquivo....: pedidosApi.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Chamadas HTTP do módulo de Pedidos.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (PedidosController)
 *                GET    /pedidos?busca=&status=&pagina=&tamanhoPagina=
 *                GET    /pedidos/{id}
 *                POST   /pedidos
 *                PUT    /pedidos/{id}
 *                PATCH  /pedidos/{id}/confirmar
 *                PATCH  /pedidos/{id}/cancelar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { Pedido, PedidoEntrada, PedidoFiltro, PedidoResumo } from '../types/pedido'
import { axiosClient } from './axiosClient'

export const pedidosApi = {
  async listar(filtro: PedidoFiltro): Promise<ResultadoPaginado<PedidoResumo>> {
    const resposta = await axiosClient.get<ResultadoPaginado<PedidoResumo>>('/pedidos', { params: filtro })
    return resposta.data
  },

  async obterPorId(id: number): Promise<Pedido> {
    const resposta = await axiosClient.get<Pedido>(`/pedidos/${id}`)
    return resposta.data
  },

  async criar(dados: PedidoEntrada): Promise<Pedido> {
    const resposta = await axiosClient.post<Pedido>('/pedidos', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: PedidoEntrada): Promise<Pedido> {
    const resposta = await axiosClient.put<Pedido>(`/pedidos/${id}`, dados)
    return resposta.data
  },

  async confirmar(id: number): Promise<Pedido> {
    const resposta = await axiosClient.patch<Pedido>(`/pedidos/${id}/confirmar`)
    return resposta.data
  },

  async cancelar(id: number): Promise<void> {
    await axiosClient.patch(`/pedidos/${id}/cancelar`)
  },
}
