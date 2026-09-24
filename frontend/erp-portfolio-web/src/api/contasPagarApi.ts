/**
 * =====================================================================
 * Arquivo....: contasPagarApi.ts
 * Versão.....: 1.2.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Contas a Pagar.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ContasPagarController)
 *                GET   /contas-pagar?busca=&status=&origem=&pagina=&tamanhoPagina=
 *                GET   /contas-pagar/exportar?busca=&status=&origem=&formato=xlsx|pdf
 *                PATCH /contas-pagar/{id}/pagar
 *                POST  /contas-pagar             (conta avulsa)
 *                PATCH /contas-pagar/{id}/cancelar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - criarAvulsa e cancelar (etapa 12).
 *   1.2.0 - 24/09/2026 - exportar (Excel/PDF com todo o filtro).
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { ContaAvulsaEntrada, ParcelaPagar, ParcelaPagarFiltro } from '../types/contaPagar'
import type { FormatoArquivo } from '../types/relatorio'
import { axiosClient } from './axiosClient'
import { baixarArquivo } from './relatoriosApi'

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

  /** Baixa todas as parcelas do filtro (sem paginação) em Excel ou PDF. */
  exportar(filtro: ParcelaPagarFiltro, formato: FormatoArquivo) {
    const { pagina: _pagina, tamanhoPagina: _tamanho, ...semPaginacao } = filtro
    return baixarArquivo('/contas-pagar/exportar', { ...semPaginacao, formato }, `contas-pagar.${formato}`)
  },
}
