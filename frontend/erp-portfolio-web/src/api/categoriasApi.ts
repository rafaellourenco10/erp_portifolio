/**
 * =====================================================================
 * Arquivo....: categoriasApi.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Chamadas HTTP do cadastro de Categorias.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (CategoriasController)
 *                GET    /categorias?busca=&ativo=&pagina=&tamanhoPagina=
 *                POST   /categorias
 *                PUT    /categorias/{id}
 *                PATCH  /categorias/{id}/inativar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { Categoria, CategoriaAtualizacao, CategoriaCriacao, CategoriaFiltro } from '../types/categoria'
import type { ResultadoPaginado } from '../types/paginacao'
import { axiosClient } from './axiosClient'

export const categoriasApi = {
  async listar(filtro: CategoriaFiltro): Promise<ResultadoPaginado<Categoria>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Categoria>>('/categorias', { params: filtro })
    return resposta.data
  },

  async criar(dados: CategoriaCriacao): Promise<Categoria> {
    const resposta = await axiosClient.post<Categoria>('/categorias', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: CategoriaAtualizacao): Promise<Categoria> {
    const resposta = await axiosClient.put<Categoria>(`/categorias/${id}`, dados)
    return resposta.data
  },

  async inativar(id: number): Promise<void> {
    await axiosClient.patch(`/categorias/${id}/inativar`)
  },
}
