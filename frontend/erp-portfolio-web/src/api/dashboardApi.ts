/**
 * =====================================================================
 * Arquivo....: dashboardApi.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Chamadas HTTP do Dashboard. Três endpoints independentes, um por
 *              módulo de origem (SPEC.md, D7) — sem parâmetros; o mês é sempre
 *              o atual, calculado no servidor.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (DashboardController)
 *                GET /dashboard/vendas
 *                GET /dashboard/contas-receber
 *                GET /dashboard/estoque
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ContasReceberResumo, EstoqueResumoDashboard, VendasResumo } from '../types/dashboard'
import { axiosClient } from './axiosClient'

export const dashboardApi = {
  async obterVendas(): Promise<VendasResumo> {
    const resposta = await axiosClient.get<VendasResumo>('/dashboard/vendas')
    return resposta.data
  },

  async obterContasReceber(): Promise<ContasReceberResumo> {
    const resposta = await axiosClient.get<ContasReceberResumo>('/dashboard/contas-receber')
    return resposta.data
  },

  async obterEstoque(): Promise<EstoqueResumoDashboard> {
    const resposta = await axiosClient.get<EstoqueResumoDashboard>('/dashboard/estoque')
    return resposta.data
  },
}
