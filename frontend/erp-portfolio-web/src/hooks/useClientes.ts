/**
 * =====================================================================
 * Arquivo....: useClientes.ts
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Clientes
 *              (consulta paginada, salvar e inativar). As mutações
 *              invalidam o cache "clientes" para recarregar a tabela.
 * ---------------------------------------------------------------------
 * Fontes.....: clientesApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { clientesApi } from '../api/clientesApi'
import type { ClienteAtualizacao, ClienteFiltro } from '../types/cliente'

const CHAVE_CLIENTES = ['clientes'] as const

export function useListaClientes(filtro: ClienteFiltro) {
  return useQuery({
    queryKey: [...CHAVE_CLIENTES, 'lista', filtro],
    queryFn: () => clientesApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

interface SalvarClienteParametros {
  /** Ausente na inclusão. */
  id?: number
  dados: ClienteAtualizacao
}

export function useSalvarCliente() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: SalvarClienteParametros) =>
      id === undefined ? clientesApi.criar(dados) : clientesApi.atualizar(id, dados),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_CLIENTES }),
  })
}

export function useInativarCliente() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => clientesApi.inativar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_CLIENTES }),
  })
}
