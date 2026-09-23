/**
 * =====================================================================
 * Arquivo....: dashboardApi.ts
 * Versão.....: 1.2.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do Dashboard. Quatro endpoints independentes, um por
 *              módulo de origem (SPEC.md, D7) — sem parâmetros; o mês é sempre
 *              o atual, calculado no servidor.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (DashboardController)
 *                GET /dashboard/vendas
 *                GET /dashboard/contas-receber
 *                GET /dashboard/contas-pagar
 *                GET /dashboard/vencimentos-pagar | vencimentos-receber
 *                GET /dashboard/estoque
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - obterContasPagar (etapa 8).
 *   1.2.0 - 23/09/2026 - obterVencimentosPagar / obterVencimentosReceber.
 * =====================================================================
 */

import type { ContasPagarResumo, ContasReceberResumo, EstoqueResumoDashboard, Vencimentos, VendasResumo } from '../types/dashboard'
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

  async obterContasPagar(): Promise<ContasPagarResumo> {
    const resposta = await axiosClient.get<ContasPagarResumo>('/dashboard/contas-pagar')
    return resposta.data
  },

  async obterVencimentosPagar(): Promise<Vencimentos> {
    const resposta = await axiosClient.get<Vencimentos>('/dashboard/vencimentos-pagar')
    return resposta.data
  },

  async obterVencimentosReceber(): Promise<Vencimentos> {
    const resposta = await axiosClient.get<Vencimentos>('/dashboard/vencimentos-receber')
    return resposta.data
  },

  async obterEstoque(): Promise<EstoqueResumoDashboard> {
    const resposta = await axiosClient.get<EstoqueResumoDashboard>('/dashboard/estoque')
    return resposta.data
  },
}
