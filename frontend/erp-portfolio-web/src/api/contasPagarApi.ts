/**
 * =====================================================================
 * Arquivo....: contasPagarApi.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Contas a Pagar.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ContasPagarController)
 *                GET   /contas-pagar?busca=&status=&origem=&pagina=&tamanhoPagina=
 *                PATCH /contas-pagar/{id}/pagar
 *                POST  /contas-pagar             (conta avulsa)
 *                PATCH /contas-pagar/{id}/cancelar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - criarAvulsa e cancelar (etapa 12).
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { ContaAvulsaEntrada, ParcelaPagar, ParcelaPagarFiltro } from '../types/contaPagar'
import { axiosClient } from './axiosClient'

export const contasPagarApi = {
  async listar(filtro: ParcelaPagarFiltro): Promise<ResultadoPaginado<ParcelaPagar>> {
    const resposta = await axiosClient.get<ResultadoPaginado<ParcelaPagar>>('/contas-pagar', { params: filtro })
    return resposta.data
  },

  async pagar(id: number): Promise<ParcelaPagar> {
    const resposta = await axiosClient.patch<ParcelaPagar>(`/contas-pagar/${id}/pagar`)
    return resposta.data
  },

  async criarAvulsa(dados: ContaAvulsaEntrada): Promise<ParcelaPagar[]> {
    const resposta = await axiosClient.post<ParcelaPagar[]>('/contas-pagar', dados)
    return resposta.data
  },

  async cancelar(id: number): Promise<ParcelaPagar> {
    const resposta = await axiosClient.patch<ParcelaPagar>(`/contas-pagar/${id}/cancelar`)
    return resposta.data
  },
}
