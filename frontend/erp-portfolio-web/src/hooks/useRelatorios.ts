/**
 * =====================================================================
 * Arquivo....: useRelatorios.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Hooks do TanStack Query para os relatórios. A consulta só roda
 *              depois de "Gerar" (filtro definido); mudar os campos sem gerar
 *              não refaz a consulta. Download de arquivo é uma mutação (ação
 *              pontual, com estado de carregando).
 * ---------------------------------------------------------------------
 * Fontes.....: relatoriosApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { useMutation, useQuery } from '@tanstack/react-query'
import { relatoriosApi, type TipoRelatorio } from '../api/relatoriosApi'
import type { FormatoArquivo, RelatorioEstoqueFiltro, RelatorioPedidosFiltro } from '../types/relatorio'

export function useRelatorioPedidos(tipo: 'vendas' | 'compras', filtro: RelatorioPedidosFiltro | null) {
  return useQuery({
    queryKey: ['relatorios', tipo, filtro],
    queryFn: () => relatoriosApi.pedidos(tipo, filtro!),
    enabled: filtro !== null,
  })
}

export function useRelatorioEstoque(filtro: RelatorioEstoqueFiltro | null) {
  return useQuery({
    queryKey: ['relatorios', 'estoque', filtro],
    queryFn: () => relatoriosApi.estoque(filtro!),
    enabled: filtro !== null,
  })
}

interface BaixarParametros {
  tipo: TipoRelatorio
  filtro: RelatorioPedidosFiltro | RelatorioEstoqueFiltro
  formato: FormatoArquivo
}

export function useBaixarRelatorio() {
  return useMutation({
    mutationFn: ({ tipo, filtro, formato }: BaixarParametros) => relatoriosApi.baixar(tipo, filtro, formato),
  })
}
