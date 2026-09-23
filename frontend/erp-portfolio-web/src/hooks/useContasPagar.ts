/**
 * =====================================================================
 * Arquivo....: useContasPagar.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Contas a Pagar (lista
 *              paginada e marcar como paga). Marcar como paga invalida o cache
 *              "contas-pagar" para o status recarregar.
 * ---------------------------------------------------------------------
 * Fontes.....: contasPagarApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { contasPagarApi } from '../api/contasPagarApi'
import type { ParcelaPagarFiltro } from '../types/contaPagar'

const CHAVE_CONTAS_PAGAR = ['contas-pagar'] as const

export function useListaContasPagar(filtro: ParcelaPagarFiltro) {
  return useQuery({
    queryKey: [...CHAVE_CONTAS_PAGAR, 'lista', filtro],
    queryFn: () => contasPagarApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

export function usePagarParcela() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => contasPagarApi.pagar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_CONTAS_PAGAR }),
  })
}
