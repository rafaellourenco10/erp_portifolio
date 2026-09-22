/**
 * =====================================================================
 * Arquivo....: contasReceberApi.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Chamadas HTTP do módulo de Contas a Receber.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ContasReceberController)
 *                GET   /contas-receber?busca=&status=&pagina=&tamanhoPagina=
 *                PATCH /contas-receber/{id}/receber
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { Parcela, ParcelaFiltro } from '../types/contaReceber'
import { axiosClient } from './axiosClient'

export const contasReceberApi = {
  async listar(filtro: ParcelaFiltro): Promise<ResultadoPaginado<Parcela>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Parcela>>('/contas-receber', { params: filtro })
    return resposta.data
  },

  async receber(id: number): Promise<Parcela> {
    const resposta = await axiosClient.patch<Parcela>(`/contas-receber/${id}/receber`)
    return resposta.data
  },
}
