/**
 * =====================================================================
 * Arquivo....: useContasReceber.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Contas a Receber (lista
 *              paginada e marcar como recebida). Marcar como recebida invalida
 *              o cache "contas-receber" para o saldo/status recarregar.
 * ---------------------------------------------------------------------
 * Fontes.....: contasReceberApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { contasReceberApi } from '../api/contasReceberApi'
import type { ParcelaFiltro } from '../types/contaReceber'

const CHAVE_CONTAS_RECEBER = ['contas-receber'] as const

export function useListaContasReceber(filtro: ParcelaFiltro) {
  return useQuery({
    queryKey: [...CHAVE_CONTAS_RECEBER, 'lista', filtro],
    queryFn: () => contasReceberApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

export function useReceberParcela() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => contasReceberApi.receber(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_CONTAS_RECEBER }),
  })
}
