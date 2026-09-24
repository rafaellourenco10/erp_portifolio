/**
 * =====================================================================
 * Arquivo....: contasReceberApi.ts
 * Versão.....: 1.1.0
 * Data.......: 22/09/2026
 * Descrição..: Chamadas HTTP do módulo de Contas a Receber.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ContasReceberController)
 *                GET   /contas-receber?busca=&status=&pagina=&tamanhoPagina=
 *                GET   /contas-receber/exportar?busca=&status=&formato=xlsx|pdf
 *                PATCH /contas-receber/{id}/receber
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 24/09/2026 - exportar (Excel/PDF com todo o filtro).
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { Parcela, ParcelaFiltro } from '../types/contaReceber'
import type { FormatoArquivo } from '../types/relatorio'
import { axiosClient } from './axiosClient'
import { baixarArquivo } from './relatoriosApi'

export const contasReceberApi = {
  async listar(filtro: ParcelaFiltro): Promise<ResultadoPaginado<Parcela>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Parcela>>('/contas-receber', { params: filtro })
    return resposta.data
  },

  async receber(id: number): Promise<Parcela> {
    const resposta = await axiosClient.patch<Parcela>(`/contas-receber/${id}/receber`)
    return resposta.data
  },

  /** Baixa todas as parcelas do filtro (sem paginação) em Excel ou PDF. */
  exportar(filtro: ParcelaFiltro, formato: FormatoArquivo) {
    const { pagina: _pagina, tamanhoPagina: _tamanho, ...semPaginacao } = filtro
    return baixarArquivo('/contas-receber/exportar', { ...semPaginacao, formato }, `contas-receber.${formato}`)
  },
}
