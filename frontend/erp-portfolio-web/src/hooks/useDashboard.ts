/**
 * =====================================================================
 * Arquivo....: useDashboard.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Hooks do TanStack Query para o Dashboard. Três queries
 *              independentes (uma por módulo de origem) — se uma falhar,
 *              as outras continuam mostrando seus cards normalmente (D7).
 * ---------------------------------------------------------------------
 * Fontes.....: dashboardApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
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

export function useResumoEstoque() {
  return useQuery({ queryKey: ['dashboard', 'estoque'], queryFn: dashboardApi.obterEstoque })
}
