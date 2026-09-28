/**
 * =====================================================================
 * Arquivo....: notasFiscaisApi.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Chamadas da NF-e simulada (etapa 16): emitir pelo pedido e
 *              pela devolução, listar, detalhar, baixar XML/DANFE e exportar.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import axios from 'axios'
import type { ResultadoPaginado } from '../types/paginacao'
import type { NotaFiscalDetalhe, NotaFiscalFiltro, NotaFiscalResumo } from '../types/notaFiscal'
import type { FormatoArquivo } from '../types/relatorio'
import { axiosClient } from './axiosClient'
import { baixarArquivo } from './relatoriosApi'

export const notasFiscaisApi = {
  async listar(filtro: NotaFiscalFiltro): Promise<ResultadoPaginado<NotaFiscalResumo>> {
    return (await axiosClient.get<ResultadoPaginado<NotaFiscalResumo>>('/notas-fiscais', { params: filtro })).data
  },

  async obter(id: number): Promise<NotaFiscalDetalhe> {
    return (await axiosClient.get<NotaFiscalDetalhe>(`/notas-fiscais/${id}`)).data
  },

  async emitirDoPedido(pedidoId: number): Promise<NotaFiscalDetalhe> {
    return (await axiosClient.post<NotaFiscalDetalhe>(`/pedidos/${pedidoId}/nfe`)).data
  },

  async emitirDaDevolucao(devolucaoId: number): Promise<NotaFiscalDetalhe> {
    return (await axiosClient.post<NotaFiscalDetalhe>(`/devolucoes/${devolucaoId}/nfe`)).data
  },

  baixarXml(id: number) {
    return baixarArquivo(`/notas-fiscais/${id}/xml`, {}, `nfe-${id}.xml`)
  },

  baixarDanfe(id: number) {
    return baixarArquivo(`/notas-fiscais/${id}/danfe`, {}, `nfe-${id}.pdf`)
  },

  exportar(filtro: NotaFiscalFiltro, formato: FormatoArquivo) {
    return baixarArquivo('/notas-fiscais', { ...filtro, formato }, `notas-fiscais.${formato}`)
  },
}

/** Tudo o que falta para emitir (400 com errors.Pendencias); vazio se o erro for outro. */
export function lerPendenciasNfe(erro: unknown): string[] {
  if (!axios.isAxiosError<{ errors?: Record<string, string[]> }>(erro) || erro.response?.status !== 400) return []
  return erro.response.data?.errors?.Pendencias ?? []
}
