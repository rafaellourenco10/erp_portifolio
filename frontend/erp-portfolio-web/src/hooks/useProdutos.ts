/**
 * =====================================================================
 * Arquivo....: useProdutos.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Produtos
 *              (consulta paginada, salvar e inativar). As mutações
 *              invalidam o cache "produtos" para recarregar a tabela.
 * ---------------------------------------------------------------------
 * Fontes.....: produtosApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { produtosApi } from '../api/produtosApi'
import type { ProdutoAtualizacao, ProdutoFiltro } from '../types/produto'

const CHAVE_PRODUTOS = ['produtos'] as const

export function useListaProdutos(filtro: ProdutoFiltro) {
  return useQuery({
    queryKey: [...CHAVE_PRODUTOS, 'lista', filtro],
    queryFn: () => produtosApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

interface SalvarProdutoParametros {
  /** Ausente na inclusão. */
  id?: number
  dados: ProdutoAtualizacao
}

export function useSalvarProduto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: SalvarProdutoParametros) =>
      id === undefined ? produtosApi.criar(dados) : produtosApi.atualizar(id, dados),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_PRODUTOS }),
  })
}

export function useInativarProduto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => produtosApi.inativar(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_PRODUTOS }),
  })
}
