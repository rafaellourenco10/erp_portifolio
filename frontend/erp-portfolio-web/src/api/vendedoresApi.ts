/**
 * =====================================================================
 * Arquivo....: vendedoresApi.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do cadastro de vendedores (espelho de
 *              fornecedoresApi.ts).
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (VendedoresController)
 *                GET   /vendedores?nome=&ativo=&pagina=&tamanhoPagina=
 *                GET   /vendedores/{id}
 *                POST  /vendedores
 *                PUT   /vendedores/{id}
 *                PATCH /vendedores/{id}/inativar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { Vendedor, VendedorAtualizacao, VendedorFiltro } from '../types/vendedor'
import { axiosClient } from './axiosClient'

export const vendedoresApi = {
  async listar(filtro: VendedorFiltro): Promise<ResultadoPaginado<Vendedor>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Vendedor>>('/vendedores', { params: filtro })
    return resposta.data
  },

  async criar(dados: VendedorAtualizacao): Promise<Vendedor> {
    const resposta = await axiosClient.post<Vendedor>('/vendedores', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: VendedorAtualizacao): Promise<Vendedor> {
    const resposta = await axiosClient.put<Vendedor>(`/vendedores/${id}`, dados)
    return resposta.data
  },

  async inativar(id: number): Promise<void> {
    await axiosClient.patch(`/vendedores/${id}/inativar`)
  },
}
