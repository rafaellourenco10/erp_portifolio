/**
 * =====================================================================
 * Arquivo....: useDashboard.ts
 * Versão.....: 1.2.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o Dashboard. Quatro queries
 *              independentes (uma por módulo de origem) — se uma falhar,
 *              as outras continuam mostrando seus cards normalmente (D7).
 * ---------------------------------------------------------------------
 * Fontes.....: dashboardApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - useResumoContasPagar (etapa 8).
 *   1.2.0 - 23/09/2026 - useVencimentosPagar / useVencimentosReceber.
 * =====================================================================
 */

import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from '../api/dashboardApi'

export function useResumoVendas() {
  return useQuery({ queryKey: ['dashboard', 'vendas'], queryFn: dashboardApi.obterVendas })
}

export function useResumoContasReceber() {
  return useQuery({ queryKey: ['dashboard', 'contas-receber'], queryFn: dashboardApi.obterContasReceber })
}

export function useResumoContasPagar() {
  return useQuery({ queryKey: ['dashboard', 'contas-pagar'], queryFn: dashboardApi.obterContasPagar })
}

export function useVencimentosPagar() {
  return useQuery({ queryKey: ['dashboard', 'vencimentos-pagar'], queryFn: dashboardApi.obterVencimentosPagar })
}

export function useVencimentosReceber() {
  return useQuery({ queryKey: ['dashboard', 'vencimentos-receber'], queryFn: dashboardApi.obterVencimentosReceber })
}

export function useResumoEstoque() {
  return useQuery({ queryKey: ['dashboard', 'estoque'], queryFn: dashboardApi.obterEstoque })
}
