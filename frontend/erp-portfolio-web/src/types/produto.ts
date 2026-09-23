/**
 * =====================================================================
 * Arquivo....: produto.ts
 * Versão.....: 1.3.0
 * Data.......: 22/09/2026
 * Descrição..: Tipos do módulo de Produtos, espelhando os DTOs da API
 *              (ProdutoRespostaDto, ProdutoCriacaoDto, ProdutoAtualizacaoDto
 *              e ProdutoFiltroDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 21/09/2026 - categoria (texto) trocada por categoriaId / categoriaNome.
 *   1.2.0 - 22/09/2026 - Filtro por categoriaId.
 *   1.3.0 - 22/09/2026 - estoqueMinimo.
 * =====================================================================
 */

export interface Produto {
  id: number
  nome: string
  sku: string
  categoriaId: number | null
  categoriaNome: string | null
  unidade: string
  precoVenda: number
  custo: number
  /** Saldo de estoque igual ou abaixo disso conta como "baixo" no Dashboard. 0 = sem mínimo definido. */
  estoqueMinimo: number
  ativo: boolean
  /** Data/hora ISO 8601 em UTC. */
  dataCadastro: string
}

export interface ProdutoCriacao {
  nome: string
  sku: string
  categoriaId: number | null
  unidade: string
  precoVenda: number
  custo: number
  estoqueMinimo: number
}

export interface ProdutoAtualizacao extends ProdutoCriacao {
  ativo: boolean
}

export interface ProdutoFiltro {
  /** Trecho do nome ou do SKU. */
  busca?: string
  /** true = só ativos, false = só inativos, ausente = todos. */
  ativo?: boolean
  /** Id da categoria; ausente = todas. */
  categoriaId?: number
  pagina: number
  tamanhoPagina: number
}
