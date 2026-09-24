/**
 * =====================================================================
 * Arquivo....: useComissoes.ts
 * Versão.....: 1.2.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para o módulo de Comissões (lista com
 *              totais e gerar conta a pagar). Gerar a conta invalida
 *              "comissoes" e "contas-pagar" (a conta nova aparece lá).
 * ---------------------------------------------------------------------
 * Fontes.....: comissoesApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - useGerarContaComissoes no lugar de usePagarComissoes (etapa 12).
 *   1.2.0 - 24/09/2026 - useExportarComissoes (Excel/PDF).
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { comissoesApi } from '../api/comissoesApi'
import type { ComissaoFiltro } from '../types/comissao'
import type { FormatoArquivo } from '../types/relatorio'

const CHAVE_COMISSOES = ['comissoes'] as const

export function useListaComissoes(filtro: ComissaoFiltro) {
  return useQuery({
    queryKey: [...CHAVE_COMISSOES, 'lista', filtro],
    queryFn: () => comissoesApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

export function useGerarContaComissoes() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ ids, vencimento }: { ids: number[]; vencimento: string }) => comissoesApi.gerarConta(ids, vencimento),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: CHAVE_COMISSOES }),
        queryClient.invalidateQueries({ queryKey: ['contas-pagar'] }),
      ]),
  })
}

export function useExportarComissoes() {
  return useMutation({
    mutationFn: ({ filtro, formato }: { filtro: ComissaoFiltro; formato: FormatoArquivo }) => comissoesApi.exportar(filtro, formato),
  })
}
