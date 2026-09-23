/**
 * =====================================================================
 * Arquivo....: contasPagarApi.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Contas a Pagar.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ContasPagarController)
 *                GET   /contas-pagar?busca=&status=&pagina=&tamanhoPagina=
 *                PATCH /contas-pagar/{id}/pagar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { ParcelaPagar, ParcelaPagarFiltro } from '../types/contaPagar'
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
}
