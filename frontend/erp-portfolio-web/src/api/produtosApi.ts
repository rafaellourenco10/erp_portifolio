/**
 * =====================================================================
 * Arquivo....: produtosApi.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Chamadas HTTP do módulo de Produtos.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (ProdutosController)
 *                GET    /produtos?busca=&ativo=&pagina=&tamanhoPagina=
 *                POST   /produtos
 *                PUT    /produtos/{id}
 *                PATCH  /produtos/{id}/inativar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { Produto, ProdutoAtualizacao, ProdutoCriacao, ProdutoFiltro } from '../types/produto'
import { axiosClient } from './axiosClient'

export const produtosApi = {
  async listar(filtro: ProdutoFiltro): Promise<ResultadoPaginado<Produto>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Produto>>('/produtos', { params: filtro })
    return resposta.data
  },

  async criar(dados: ProdutoCriacao): Promise<Produto> {
    const resposta = await axiosClient.post<Produto>('/produtos', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: ProdutoAtualizacao): Promise<Produto> {
    const resposta = await axiosClient.put<Produto>(`/produtos/${id}`, dados)
    return resposta.data
  },

  async inativar(id: number): Promise<void> {
    await axiosClient.patch(`/produtos/${id}/inativar`)
  },
}
