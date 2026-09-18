/**
 * =====================================================================
 * Arquivo....: clientesApi.ts
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Chamadas HTTP do módulo de Clientes.
 * ---------------------------------------------------------------------
 * Fontes.....: API ErpPortfolio (ClientesController)
 *                GET    /clientes?nome=&pagina=&tamanhoPagina=
 *                GET    /clientes/{id}
 *                POST   /clientes
 *                PUT    /clientes/{id}
 *                PATCH  /clientes/{id}/inativar
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type {
  Cliente,
  ClienteAtualizacao,
  ClienteCriacao,
  ClienteFiltro,
  ResultadoPaginado,
} from '../types/cliente'
import { axiosClient } from './axiosClient'

export const clientesApi = {
  async listar(filtro: ClienteFiltro): Promise<ResultadoPaginado<Cliente>> {
    const resposta = await axiosClient.get<ResultadoPaginado<Cliente>>('/clientes', { params: filtro })
    return resposta.data
  },

  async obterPorId(id: number): Promise<Cliente> {
    const resposta = await axiosClient.get<Cliente>(`/clientes/${id}`)
    return resposta.data
  },

  async criar(dados: ClienteCriacao): Promise<Cliente> {
    const resposta = await axiosClient.post<Cliente>('/clientes', dados)
    return resposta.data
  },

  async atualizar(id: number, dados: ClienteAtualizacao): Promise<Cliente> {
    const resposta = await axiosClient.put<Cliente>(`/clientes/${id}`, dados)
    return resposta.data
  },

  async inativar(id: number): Promise<void> {
    await axiosClient.patch(`/clientes/${id}/inativar`)
  },
}
