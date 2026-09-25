/**
 * =====================================================================
 * Arquivo....: useFluxoCaixa.ts
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Hook do TanStack Query para o Fluxo de Caixa. A consulta roda
 *              com o filtro de "Gerar"; mudar os campos sem gerar não refaz.
 * ---------------------------------------------------------------------
 * Fontes.....: fluxoCaixaApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { fluxoCaixaApi } from '../api/fluxoCaixaApi'
import type { FluxoCaixaFiltro } from '../types/fluxoCaixa'

export function useFluxoCaixa(filtro: FluxoCaixaFiltro) {
  return useQuery({
    queryKey: ['fluxo-caixa', filtro],
    queryFn: () => fluxoCaixaApi.obter(filtro),
    placeholderData: keepPreviousData,
  })
}
