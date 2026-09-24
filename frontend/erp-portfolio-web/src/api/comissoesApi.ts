/**
 * =====================================================================
 * Arquivo....: comissoesApi.ts
 * Versão.....: 1.2.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Comissões.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ComissoesController)
 *                GET  /comissoes?vendedorId=&status=&dataInicio=&dataFim=&pagina=&tamanhoPagina=
 *                GET  /comissoes/exportar?(filtro)&formato=xlsx|pdf
 *                POST /comissoes/gerar-conta   { ids, vencimento }
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - gerarConta no lugar de pagar (etapa 12).
 *   1.2.0 - 24/09/2026 - exportar (Excel/PDF com todo o filtro).
 * =====================================================================
 */

import type { ComissaoFiltro, ComissaoLista, ContaDeComissaoGerada } from '../types/comissao'
import type { FormatoArquivo } from '../types/relatorio'
import { axiosClient } from './axiosClient'
import { baixarArquivo } from './relatoriosApi'

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

  /** Baixa todas as comissões do filtro (sem paginação) em Excel ou PDF. */
  exportar(filtro: ComissaoFiltro, formato: FormatoArquivo) {
    const { pagina: _pagina, tamanhoPagina: _tamanho, ...semPaginacao } = filtro
    return baixarArquivo('/comissoes/exportar', { ...semPaginacao, formato }, `comissoes.${formato}`)
  },
}
