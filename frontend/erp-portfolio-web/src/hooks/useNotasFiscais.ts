/**
 * =====================================================================
 * Arquivo....: useNotasFiscais.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Hooks (React Query) da NF-e simulada (etapa 16). Emitir
 *              recarrega as notas e os pedidos (o detalhe do pedido e as
 *              devoluções mostram a nota).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { notasFiscaisApi } from '../api/notasFiscaisApi'
import type { NotaFiscalFiltro } from '../types/notaFiscal'

const CHAVE_NOTAS = ['notas-fiscais'] as const

export function useListaNotasFiscais(filtro: NotaFiscalFiltro) {
  return useQuery({
    queryKey: [...CHAVE_NOTAS, 'lista', filtro],
    queryFn: () => notasFiscaisApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

export function useNotaFiscal(id: number | null) {
  return useQuery({
    queryKey: [...CHAVE_NOTAS, 'detalhe', id],
    queryFn: () => notasFiscaisApi.obter(id!),
    enabled: id !== null,
  })
}

/** origem: de um pedido (NF-e de saída) ou de uma devolução (NF-e de entrada). */
export function useEmitirNfe() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ origem, id }: { origem: 'pedido' | 'devolucao'; id: number }) =>
      origem === 'pedido' ? notasFiscaisApi.emitirDoPedido(id) : notasFiscaisApi.emitirDaDevolucao(id),
    onSuccess: () =>
      Promise.all([CHAVE_NOTAS, ['pedidos']].map((queryKey) => queryClient.invalidateQueries({ queryKey }))),
  })
}
