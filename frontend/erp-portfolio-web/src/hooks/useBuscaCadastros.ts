/**
 * =====================================================================
 * Arquivo....: useBuscaCadastros.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Hooks de busca no SERVIDOR para os seletores do pedido: clientes e
 *              produtos ATIVOS que combinam com o texto digitado, no máximo 20 por
 *              consulta. A consulta só dispara 300 ms depois da última tecla, para não
 *              chamar a API a cada letra. As chaves ficam sob "clientes" e "produtos",
 *              então cadastrar ou inativar em Clientes/Produtos atualiza as buscas.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/clientes?nome=&ativo=true&tamanhoPagina=20
 *              GET /api/produtos?busca=&ativo=true&tamanhoPagina=20
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { clientesApi } from '../api/clientesApi'
import { produtosApi } from '../api/produtosApi'
import type { Cliente } from '../types/cliente'
import type { ResultadoPaginado } from '../types/paginacao'
import type { Produto } from '../types/produto'

const ATRASO_MS = 300
const LIMITE_RESULTADOS = 20

/** Devolve o valor só depois de ele ficar parado por `atrasoMs` (debounce). */
export function useValorComAtraso<T>(valor: T, atrasoMs = ATRASO_MS): T {
  const [atrasado, setAtrasado] = useState(valor)

  useEffect(() => {
    const temporizador = setTimeout(() => setAtrasado(valor), atrasoMs)
    return () => clearTimeout(temporizador)
  }, [valor, atrasoMs])

  return atrasado
}

function useBusca<T>(cadastro: 'clientes' | 'produtos', texto: string, buscar: (busca: string) => Promise<ResultadoPaginado<T>>) {
  const busca = useValorComAtraso(texto.trim())
  const consulta = useQuery({
    queryKey: [cadastro, 'busca', busca],
    queryFn: () => buscar(busca),
    // Mantém a lista anterior na tela enquanto a nova chega (o seletor não "pisca" vazio).
    placeholderData: keepPreviousData,
  })

  return {
    itens: consulta.data?.itens ?? [],
    /** true enquanto a consulta roda ou o texto digitado ainda não virou consulta. */
    buscando: consulta.isFetching || busca !== texto.trim(),
    erro: consulta.isError,
  }
}

export function useBuscaClientes(texto: string) {
  return useBusca<Cliente>('clientes', texto, (busca) =>
    clientesApi.listar({ nome: busca || undefined, ativo: true, pagina: 1, tamanhoPagina: LIMITE_RESULTADOS }),
  )
}

export function useBuscaProdutos(texto: string) {
  return useBusca<Produto>('produtos', texto, (busca) =>
    produtosApi.listar({ busca: busca || undefined, ativo: true, pagina: 1, tamanhoPagina: LIMITE_RESULTADOS }),
  )
}
