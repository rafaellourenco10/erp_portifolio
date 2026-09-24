/**
 * =====================================================================
 * Arquivo....: orcamentosApi.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Orçamentos (etapa 13).
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (OrcamentosController)
 *                GET    /orcamentos?busca=&status=&pagina=&tamanhoPagina=
 *                GET    /orcamentos/{id}
 *                POST   /orcamentos
 *                PUT    /orcamentos/{id}
 *                POST   /orcamentos/{id}/gerar-pedido
 *                PATCH  /orcamentos/{id}/perder
 *                GET    /orcamentos/{id}/pdf (link direto, ver urlPdf)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from '../types/paginacao'
import type { Orcamento, OrcamentoEntrada, OrcamentoFiltro, OrcamentoResumo } from '../types/orcamento'
import { axiosClient } from './axiosClient'

export const orcamentosApi = {
  async listar(filtro: OrcamentoFiltro): Promise<ResultadoPaginado<OrcamentoResumo>> {
    const resposta = await axiosClient.get<ResultadoPaginado<OrcamentoResumo>>('/orcamentos', { params: filtro })
    return resposta.data
  },

  async obterPorId(id: number): Promise<Orcamento> {
    const resposta = await axiosClient.get<Orcamento>(`/orcamentos/${id}`)
    return resposta.data
  },

  async criar(dados: OrcamentoEntrada): Promise<Orcamento> {
    const resposta = await axiosClient.post<Orcamento>('/orcamentos', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: OrcamentoEntrada): Promise<Orcamento> {
    const resposta = await axiosClient.put<Orcamento>(`/orcamentos/${id}`, dados)
    return resposta.data
  },

  /** Devolve o número do pedido de venda (rascunho) gerado. */
  async gerarPedido(id: number): Promise<number> {
    const resposta = await axiosClient.post<{ pedidoId: number }>(`/orcamentos/${id}/gerar-pedido`)
    return resposta.data.pedidoId
  },

  async perder(id: number, motivo: string | null): Promise<void> {
    await axiosClient.patch(`/orcamentos/${id}/perder`, { motivo })
  },

  /** A API responde com Content-Disposition: attachment, então um link comum já baixa o arquivo. */
  urlPdf(id: number): string {
    return `${axiosClient.defaults.baseURL}/orcamentos/${id}/pdf`
  },
}
