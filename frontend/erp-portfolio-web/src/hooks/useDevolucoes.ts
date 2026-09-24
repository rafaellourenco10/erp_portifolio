/**
 * =====================================================================
 * Arquivo....: useDevolucoes.ts
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Hooks do TanStack Query da devolução de venda (etapa 14). Registrar uma
 *              devolução mexe em pedido, parcelas, contas a pagar, comissões, estoque e
 *              Dashboard: invalida o cache de todos eles.
 * ---------------------------------------------------------------------
 * Fontes.....: devolucoesApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { devolucoesApi } from '../api/devolucoesApi'
import type { DevolucaoEntrada } from '../types/devolucao'

// Debaixo de "pedidos": invalidar os pedidos recarrega também o histórico.
const chaveDevolucoes = (pedidoId: number) => ['pedidos', 'devolucoes', pedidoId] as const

const AFETADOS = ['pedidos', 'contas-receber', 'contas-pagar', 'comissoes', 'estoque', 'dashboard', 'relatorios']

export function useDevolucoes(pedidoId: number | undefined) {
  return useQuery({
    queryKey: chaveDevolucoes(pedidoId ?? 0),
    queryFn: () => devolucoesApi.listar(pedidoId!),
    enabled: pedidoId !== undefined,
  })
}

export function useRegistrarDevolucao() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ pedidoId, dados }: { pedidoId: number; dados: DevolucaoEntrada }) => devolucoesApi.registrar(pedidoId, dados),
    onSuccess: () => Promise.all(AFETADOS.map((chave) => queryClient.invalidateQueries({ queryKey: [chave] }))),
  })
}
