/**
 * =====================================================================
 * Arquivo....: useComissoes.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Comissões (lista com
 *              totais e marcar como pagas). Pagar invalida o cache "comissoes".
 * ---------------------------------------------------------------------
 * Fontes.....: comissoesApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { comissoesApi } from '../api/comissoesApi'
import type { ComissaoFiltro } from '../types/comissao'

const CHAVE_COMISSOES = ['comissoes'] as const

export function useListaComissoes(filtro: ComissaoFiltro) {
  return useQuery({
    queryKey: [...CHAVE_COMISSOES, 'lista', filtro],
    queryFn: () => comissoesApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

export function usePagarComissoes() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (ids: number[]) => comissoesApi.pagar(ids),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_COMISSOES }),
  })
}
