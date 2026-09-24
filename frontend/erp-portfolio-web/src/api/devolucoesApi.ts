/**
 * =====================================================================
 * Arquivo....: devolucoesApi.ts
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Chamadas HTTP da devolução de venda (etapa 14).
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (DevolucoesController)
 *                GET  /pedidos/{id}/devolucoes
 *                POST /pedidos/{id}/devolucoes
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { Devolucao, DevolucaoEntrada } from '../types/devolucao'
import { axiosClient } from './axiosClient'

export const devolucoesApi = {
  async listar(pedidoId: number): Promise<Devolucao[]> {
    const resposta = await axiosClient.get<Devolucao[]>(`/pedidos/${pedidoId}/devolucoes`)
    return resposta.data
  },

  async registrar(pedidoId: number, dados: DevolucaoEntrada): Promise<Devolucao> {
    const resposta = await axiosClient.post<Devolucao>(`/pedidos/${pedidoId}/devolucoes`, dados)
    return resposta.data
  },
}
