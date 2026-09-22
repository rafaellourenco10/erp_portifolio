/**
 * =====================================================================
 * Arquivo....: estoqueApi.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Chamadas HTTP do módulo de Estoque.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (EstoqueController)
 *                GET  /estoque?busca=&pagina=&tamanhoPagina=
 *                GET  /estoque/{produtoId}/movimentacoes?pagina=&tamanhoPagina=
 *                POST /estoque/entradas
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { EstoqueEntrada, EstoqueFiltro, EstoqueResumo, Movimentacao } from '../types/estoque'
import { axiosClient } from './axiosClient'

export const estoqueApi = {
  async listar(filtro: EstoqueFiltro): Promise<ResultadoPaginado<EstoqueResumo>> {
    const resposta = await axiosClient.get<ResultadoPaginado<EstoqueResumo>>('/estoque', { params: filtro })
    return resposta.data
  },

  async obterMovimentacoes(
    produtoId: number,
    filtro: EstoqueFiltro,
  ): Promise<ResultadoPaginado<Movimentacao>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Movimentacao>>(`/estoque/${produtoId}/movimentacoes`, {
      params: filtro,
    })
    return resposta.data
  },

  async registrarEntrada(dados: EstoqueEntrada): Promise<Movimentacao> {
    const resposta = await axiosClient.post<Movimentacao>('/estoque/entradas', dados)
    return resposta.data
  },
}
