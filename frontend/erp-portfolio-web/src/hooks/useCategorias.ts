/**
 * =====================================================================
 * Arquivo....: useCategorias.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Hooks do TanStack Query para Categorias (consulta paginada,
 *              categorias ativas para o seletor de produtos, salvar e
 *              inativar). As mutações invalidam também o cache "produtos",
 *              porque a lista de produtos mostra o nome da categoria.
 * ---------------------------------------------------------------------
 * Fontes.....: categoriasApi.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { categoriasApi } from '../api/categoriasApi'
import type { CategoriaAtualizacao, CategoriaFiltro } from '../types/categoria'

const CHAVE_CATEGORIAS = ['categorias'] as const
const CHAVE_PRODUTOS = ['produtos'] as const

export function useListaCategorias(filtro: CategoriaFiltro) {
  return useQuery({
    queryKey: [...CHAVE_CATEGORIAS, 'lista', filtro],
    queryFn: () => categoriasApi.listar(filtro),
    placeholderData: keepPreviousData,
  })
}

// ponytail: o seletor do produto lê uma página de até 100 categorias ativas (limite da API);
// com mais que isso, trocar por busca no servidor (Select com onSearch).
const FILTRO_ATIVAS: CategoriaFiltro = { ativo: true, pagina: 1, tamanhoPagina: 100 }

export function useCategoriasAtivas() {
  return useQuery({
    queryKey: [...CHAVE_CATEGORIAS, 'lista', FILTRO_ATIVAS],
    queryFn: () => categoriasApi.listar(FILTRO_ATIVAS),
  })
}

interface SalvarCategoriaParametros {
  /** Ausente na inclusão. */
  id?: number
  dados: CategoriaAtualizacao
}

function useInvalidarCategoriasEProdutos() {
  const queryClient = useQueryClient()
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: CHAVE_CATEGORIAS }),
      queryClient.invalidateQueries({ queryKey: CHAVE_PRODUTOS }),
    ])
}

export function useSalvarCategoria() {
  const invalidar = useInvalidarCategoriasEProdutos()

  return useMutation({
    mutationFn: ({ id, dados }: SalvarCategoriaParametros) =>
      id === undefined ? categoriasApi.criar(dados) : categoriasApi.atualizar(id, dados),
    onSuccess: invalidar,
  })
}

export function useInativarCategoria() {
  const invalidar = useInvalidarCategoriasEProdutos()

  return useMutation({
    mutationFn: (id: number) => categoriasApi.inativar(id),
    onSuccess: invalidar,
  })
}
