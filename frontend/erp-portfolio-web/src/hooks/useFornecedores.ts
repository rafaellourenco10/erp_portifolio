/**
 * =====================================================================
 * Arquivo....: useFornecedores.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Fornecedores
 *              (consulta paginada, salvar e inativar). As mutações
 *              invalidam o cache "fornecedores" para recarregar a tabela.
 *              Espelho de useClientes.ts.
 * ---------------------------------------------------------------------
 * Fontes.....: fornecedoresApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { fornecedoresApi } from '../api/fornecedoresApi'
import type { FornecedorAtualizacao, FornecedorFiltro } from '../types/fornecedor'

const CHAVE_FORNECEDORES = ['fornecedores'] as const

export function useListaFornecedores(filtro: FornecedorFiltro) {
  return useQuery({
    queryKey: [...CHAVE_FORNECEDORES, 'lista', filtro],
    queryFn: () => fornecedoresApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

interface SalvarFornecedorParametros {
  /** Ausente na inclusão. */
  id?: number
  dados: FornecedorAtualizacao
}

export function useSalvarFornecedor() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: SalvarFornecedorParametros) =>
      id === undefined ? fornecedoresApi.criar(dados) : fornecedoresApi.atualizar(id, dados),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_FORNECEDORES }),
  })
}

export function useInativarFornecedor() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => fornecedoresApi.inativar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_FORNECEDORES }),
  })
}
