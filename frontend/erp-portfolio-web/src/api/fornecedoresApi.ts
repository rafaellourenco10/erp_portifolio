/**
 * =====================================================================
 * Arquivo....: fornecedoresApi.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Fornecedores. Espelho de
 *              clientesApi.ts.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (FornecedoresController)
 *                GET    /fornecedores?nome=&ufs=&ativo=&pagina=&tamanhoPagina=
 *                GET    /fornecedores/{id}
 *                POST   /fornecedores
 *                PUT    /fornecedores/{id}
 *                PATCH  /fornecedores/{id}/inativar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type {
  Fornecedor,
  FornecedorAtualizacao,
  FornecedorCriacao,
  FornecedorFiltro,
  ResultadoPaginado,
} from '../types/fornecedor'
import { axiosClient } from './axiosClient'

export const fornecedoresApi = {
  async listar(filtro: FornecedorFiltro): Promise<ResultadoPaginado<Fornecedor>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Fornecedor>>('/fornecedores', { params: filtro })
    return resposta.data
  },

  async obterPorId(id: number): Promise<Fornecedor> {
    const resposta = await axiosClient.get<Fornecedor>(`/fornecedores/${id}`)
    return resposta.data
  },

  async criar(dados: FornecedorCriacao): Promise<Fornecedor> {
    const resposta = await axiosClient.post<Fornecedor>('/fornecedores', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: FornecedorAtualizacao): Promise<Fornecedor> {
    const resposta = await axiosClient.put<Fornecedor>(`/fornecedores/${id}`, dados)
    return resposta.data
  },

  async inativar(id: number): Promise<void> {
    await axiosClient.patch(`/fornecedores/${id}/inativar`)
  },
}
