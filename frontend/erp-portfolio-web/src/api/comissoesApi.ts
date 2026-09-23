/**
 * =====================================================================
 * Arquivo....: comissoesApi.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Comissões.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ComissoesController)
 *                GET  /comissoes?vendedorId=&status=&dataInicio=&dataFim=&pagina=&tamanhoPagina=
 *                POST /comissoes/gerar-conta   { ids, vencimento }
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - gerarConta no lugar de pagar (etapa 12).
 * =====================================================================
 */

import type { ComissaoFiltro, ComissaoLista, ContaDeComissaoGerada } from '../types/comissao'
import { axiosClient } from './axiosClient'

export const comissoesApi = {
  async listar(filtro: ComissaoFiltro): Promise<ComissaoLista> {
    const resposta = await axiosClient.get<ComissaoLista>('/comissoes', { params: filtro })
    return resposta.data
  },

  /** Fecha comissões pendentes de UM vendedor numa conta a pagar; vencimento em AAAA-MM-DD. */
  async gerarConta(ids: number[], vencimento: string): Promise<ContaDeComissaoGerada> {
    const resposta = await axiosClient.post<ContaDeComissaoGerada>('/comissoes/gerar-conta', { ids, vencimento })
    return resposta.data
  },
}
