/**
 * =====================================================================
 * Arquivo....: empresaApi.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Chamadas de GET/PUT /api/empresa (emitente da NF-e, etapa 16).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import axios from 'axios'
import type { Empresa } from '../types/empresa'
import { axiosClient } from './axiosClient'

export const empresaApi = {
  /** null enquanto a empresa não foi cadastrada (a API responde 404). */
  async obter(): Promise<Empresa | null> {
    try {
      return (await axiosClient.get<Empresa>('/empresa')).data
    } catch (erro) {
      if (axios.isAxiosError(erro) && erro.response?.status === 404) return null
      throw erro
    }
  },

  async salvar(dados: Empresa): Promise<Empresa> {
    return (await axiosClient.put<Empresa>('/empresa', dados)).data
  },
}
