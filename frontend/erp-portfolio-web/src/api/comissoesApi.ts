/**
 * =====================================================================
 * Arquivo....: comissoesApi.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Comissões.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ComissoesController)
 *                GET  /comissoes?vendedorId=&status=&dataInicio=&dataFim=&pagina=&tamanhoPagina=
 *                POST /comissoes/pagar   { ids }
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ComissaoFiltro, ComissaoLista } from '../types/comissao'
import { axiosClient } from './axiosClient'

export const comissoesApi = {
  async listar(filtro: ComissaoFiltro): Promise<ComissaoLista> {
    const resposta = await axiosClient.get<ComissaoLista>('/comissoes', { params: filtro })
    return resposta.data
  },

  async pagar(ids: number[]): Promise<void> {
    await axiosClient.post('/comissoes/pagar', { ids })
  },
}
