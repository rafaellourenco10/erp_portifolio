/**
 * =====================================================================
 * Arquivo....: useContasPagar.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Contas a Pagar (lista
 *              paginada, pagar, cancelar e lançar conta avulsa). Toda mutação
 *              invalida "contas-pagar" e também "comissoes" (pagar/cancelar
 *              uma conta de comissão muda o status das comissões ligadas).
 * ---------------------------------------------------------------------
 * Fontes.....: contasPagarApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - useCancelarParcela e useCriarContaAvulsa; invalida comissões (etapa 12).
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { contasPagarApi } from '../api/contasPagarApi'
import type { ContaAvulsaEntrada, ParcelaPagarFiltro } from '../types/contaPagar'

const CHAVE_CONTAS_PAGAR = ['contas-pagar'] as const

function useInvalidar() {
  const queryClient = useQueryClient()
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: CHAVE_CONTAS_PAGAR }),
      queryClient.invalidateQueries({ queryKey: ['comissoes'] }),
    ])
}

export function useListaContasPagar(filtro: ParcelaPagarFiltro) {
  return useQuery({
    queryKey: [...CHAVE_CONTAS_PAGAR, 'lista', filtro],
    queryFn: () => contasPagarApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

export function usePagarParcela() {
  const invalidar = useInvalidar()
  return useMutation({ mutationFn: (id: number) => contasPagarApi.pagar(id), onSuccess: invalidar })
}

export function useCancelarParcela() {
  const invalidar = useInvalidar()
  return useMutation({ mutationFn: (id: number) => contasPagarApi.cancelar(id), onSuccess: invalidar })
}

export function useCriarContaAvulsa() {
  const invalidar = useInvalidar()
  return useMutation({ mutationFn: (dados: ContaAvulsaEntrada) => contasPagarApi.criarAvulsa(dados), onSuccess: invalidar })
}
