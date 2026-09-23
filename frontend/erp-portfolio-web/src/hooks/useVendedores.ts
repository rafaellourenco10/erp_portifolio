/**
 * =====================================================================
 * Arquivo....: useVendedores.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o cadastro de vendedores (lista
 *              paginada, salvar e inativar). Toda mutação invalida o cache
 *              "vendedores". Espelho de useFornecedores.ts.
 * ---------------------------------------------------------------------
 * Fontes.....: vendedoresApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { vendedoresApi } from '../api/vendedoresApi'
import type { VendedorAtualizacao, VendedorFiltro } from '../types/vendedor'

const CHAVE_VENDEDORES = ['vendedores'] as const

export function useListaVendedores(filtro: VendedorFiltro) {
  return useQuery({
    queryKey: [...CHAVE_VENDEDORES, 'lista', filtro],
    queryFn: () => vendedoresApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

interface SalvarVendedorParametros {
  /** Ausente na inclusão. */
  id?: number
  dados: VendedorAtualizacao
}

export function useSalvarVendedor() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: SalvarVendedorParametros) =>
      id === undefined ? vendedoresApi.criar(dados) : vendedoresApi.atualizar(id, dados),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_VENDEDORES }),
  })
}

export function useInativarVendedor() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => vendedoresApi.inativar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_VENDEDORES }),
  })
}
