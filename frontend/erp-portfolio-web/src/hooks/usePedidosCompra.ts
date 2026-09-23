/**
 * =====================================================================
 * Arquivo....: usePedidosCompra.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Pedidos de Compra
 *              (lista paginada, um pedido, salvar, confirmar e cancelar).
 *              Toda mutação invalida o cache "pedidos-compra" (lista e
 *              detalhe) para a tela recarregar. Espelho de usePedidos.ts.
 * ---------------------------------------------------------------------
 * Fontes.....: pedidosCompraApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - Confirmar recebe as parcelas a pagar (etapa 8).
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { pedidosCompraApi } from '../api/pedidosCompraApi'
import type { PedidoConfirmarEntrada } from '../types/pedido'
import type { PedidoCompraEntrada, PedidoCompraFiltro } from '../types/pedidoCompra'

const CHAVE_PEDIDOS_COMPRA = ['pedidos-compra'] as const

export function useListaPedidosCompra(filtro: PedidoCompraFiltro) {
  return useQuery({
    queryKey: [...CHAVE_PEDIDOS_COMPRA, 'lista', filtro],
    queryFn: () => pedidosCompraApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

/** Um pedido de compra pelo número; com `id` indefinido (tela de pedido novo) não consulta nada. */
export function usePedidoCompra(id: number | undefined) {
  return useQuery({
    queryKey: [...CHAVE_PEDIDOS_COMPRA, 'detalhe', id],
    queryFn: () => pedidosCompraApi.obterPorId(id!),
    enabled: id !== undefined,
    // Pedido inexistente (404) não melhora tentando de novo.
    retry: (tentativas, erro) => !(isAxiosError(erro) && erro.response?.status === 404) && tentativas < 1,
  })
}

interface SalvarPedidoCompraParametros {
  /** Ausente na inclusão. */
  id?: number
  dados: PedidoCompraEntrada
}

function useInvalidarPedidosCompra() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: CHAVE_PEDIDOS_COMPRA })
}

export function useSalvarPedidoCompra() {
  const invalidar = useInvalidarPedidosCompra()

  return useMutation({
    mutationFn: ({ id, dados }: SalvarPedidoCompraParametros) =>
      id === undefined ? pedidosCompraApi.criar(dados) : pedidosCompraApi.atualizar(id, dados),
    onSuccess: invalidar,
  })
}

interface ConfirmarPedidoCompraParametros {
  id: number
  dados: PedidoConfirmarEntrada
}

export function useConfirmarPedidoCompra() {
  const invalidar = useInvalidarPedidosCompra()

  return useMutation({
    mutationFn: ({ id, dados }: ConfirmarPedidoCompraParametros) => pedidosCompraApi.confirmar(id, dados),
    onSuccess: invalidar,
  })
}

export function useCancelarPedidoCompra() {
  const invalidar = useInvalidarPedidosCompra()

  return useMutation({
    mutationFn: (id: number) => pedidosCompraApi.cancelar(id),
    onSuccess: invalidar,
  })
}
