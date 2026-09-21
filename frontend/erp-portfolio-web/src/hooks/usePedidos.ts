/**
 * =====================================================================
 * Arquivo....: usePedidos.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Pedidos (lista paginada,
 *              um pedido, salvar, confirmar e cancelar). Toda mutação invalida o
 *              cache "pedidos" (lista e detalhe) para a tela recarregar.
 * ---------------------------------------------------------------------
 * Fontes.....: pedidosApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { pedidosApi } from '../api/pedidosApi'
import type { PedidoEntrada, PedidoFiltro } from '../types/pedido'

const CHAVE_PEDIDOS = ['pedidos'] as const

export function useListaPedidos(filtro: PedidoFiltro) {
  return useQuery({
    queryKey: [...CHAVE_PEDIDOS, 'lista', filtro],
    queryFn: () => pedidosApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

/** Um pedido pelo número; com `id` indefinido (tela de pedido novo) não consulta nada. */
export function usePedido(id: number | undefined) {
  return useQuery({
    queryKey: [...CHAVE_PEDIDOS, 'detalhe', id],
    queryFn: () => pedidosApi.obterPorId(id!),
    enabled: id !== undefined,
    // Pedido inexistente (404) não melhora tentando de novo.
    retry: (tentativas, erro) => !(isAxiosError(erro) && erro.response?.status === 404) && tentativas < 1,
  })
}

interface SalvarPedidoParametros {
  /** Ausente na inclusão. */
  id?: number
  dados: PedidoEntrada
}

function useInvalidarPedidos() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: CHAVE_PEDIDOS })
}

export function useSalvarPedido() {
  const invalidar = useInvalidarPedidos()

  return useMutation({
    mutationFn: ({ id, dados }: SalvarPedidoParametros) =>
      id === undefined ? pedidosApi.criar(dados) : pedidosApi.atualizar(id, dados),
    onSuccess: invalidar,
  })
}

export function useConfirmarPedido() {
  const invalidar = useInvalidarPedidos()

  return useMutation({
    mutationFn: (id: number) => pedidosApi.confirmar(id),
    onSuccess: invalidar,
  })
}

export function useCancelarPedido() {
  const invalidar = useInvalidarPedidos()

  return useMutation({
    mutationFn: (id: number) => pedidosApi.cancelar(id),
    onSuccess: invalidar,
  })
}
