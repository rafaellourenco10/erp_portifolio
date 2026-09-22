/**
 * =====================================================================
 * Arquivo....: useEstoque.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Estoque (lista com
 *              saldo, extrato por produto e entrada manual). A entrada
 *              manual invalida o cache "estoque" para o saldo e o extrato
 *              recarregarem.
 * ---------------------------------------------------------------------
 * Fontes.....: estoqueApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { estoqueApi } from '../api/estoqueApi'
import type { EstoqueEntrada, EstoqueFiltro } from '../types/estoque'

const CHAVE_ESTOQUE = ['estoque'] as const

export function useListaEstoque(filtro: EstoqueFiltro) {
  return useQuery({
    queryKey: [...CHAVE_ESTOQUE, 'lista', filtro],
    queryFn: () => estoqueApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

export function useMovimentacoesProduto(produtoId: number | null, filtro: EstoqueFiltro) {
  return useQuery({
    queryKey: [...CHAVE_ESTOQUE, 'movimentacoes', produtoId, filtro],
    queryFn: () => estoqueApi.obterMovimentacoes(produtoId!, filtro),
    enabled: produtoId !== null,
    placeholderData: keepPreviousData,
  })
}

export function useRegistrarEntrada() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dados: EstoqueEntrada) => estoqueApi.registrarEntrada(dados),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: CHAVE_ESTOQUE }),
  })
}
