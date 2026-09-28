/**
 * =====================================================================
 * Arquivo....: useEmpresa.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Hooks (React Query) dos dados da empresa emitente (etapa 16).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { empresaApi } from '../api/empresaApi'
import type { Empresa } from '../types/empresa'

const CHAVE_EMPRESA = ['empresa'] as const

export function useEmpresa() {
  return useQuery({ queryKey: CHAVE_EMPRESA, queryFn: empresaApi.obter })
}

export function useSalvarEmpresa() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dados: Empresa) => empresaApi.salvar(dados),
    onSuccess: (empresa) => queryClient.setQueryData(CHAVE_EMPRESA, empresa),
  })
}
