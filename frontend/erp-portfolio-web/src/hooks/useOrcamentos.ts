/**
 * =====================================================================
 * Arquivo....: useOrcamentos.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para Orçamentos (lista, um orçamento, salvar,
 *              gerar pedido e marcar como perdido). Toda mutação invalida o cache
 *              "orcamentos"; gerar pedido invalida também "pedidos" (nasce um rascunho).
 * ---------------------------------------------------------------------
 * Fontes.....: orcamentosApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { orcamentosApi } from '../api/orcamentosApi'
import type { OrcamentoEntrada, OrcamentoFiltro } from '../types/orcamento'

const CHAVE_ORCAMENTOS = ['orcamentos'] as const

export function useListaOrcamentos(filtro: OrcamentoFiltro) {
  return useQuery({
    queryKey: [...CHAVE_ORCAMENTOS, 'lista', filtro],
    queryFn: () => orcamentosApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

/** Um orçamento pelo número; com `id` indefinido (orçamento novo) não consulta nada. */
export function useOrcamento(id: number | undefined) {
  return useQuery({
    queryKey: [...CHAVE_ORCAMENTOS, 'detalhe', id],
    queryFn: () => orcamentosApi.obterPorId(id!),
    enabled: id !== undefined,
    retry: (tentativas, erro) => !(isAxiosError(erro) && erro.response?.status === 404) && tentativas < 1,
  })
}

function useInvalidarOrcamentos() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: CHAVE_ORCAMENTOS })
}

export function useSalvarOrcamento() {
  const invalidar = useInvalidarOrcamentos()

  return useMutation({
    mutationFn: ({ id, dados }: { id?: number; dados: OrcamentoEntrada }) =>
      id === undefined ? orcamentosApi.criar(dados) : orcamentosApi.atualizar(id, dados),
    onSuccess: invalidar,
  })
}

export function useGerarPedido() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => orcamentosApi.gerarPedido(id),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: CHAVE_ORCAMENTOS }),
        queryClient.invalidateQueries({ queryKey: ['pedidos'] }),
      ]),
  })
}

export function usePerderOrcamento() {
  const invalidar = useInvalidarOrcamentos()

  return useMutation({
    mutationFn: ({ id, motivo }: { id: number; motivo: string | null }) => orcamentosApi.perder(id, motivo),
    onSuccess: invalidar,
  })
}
