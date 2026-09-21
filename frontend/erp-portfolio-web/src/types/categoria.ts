/**
 * =====================================================================
 * Arquivo....: categoria.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Tipos do cadastro de Categorias, espelhando os DTOs da API
 *              (CategoriaRespostaDto, CategoriaCriacaoDto,
 *              CategoriaAtualizacaoDto e CategoriaFiltroDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export interface Categoria {
  id: number
  nome: string
  ativo: boolean
  /** Data/hora ISO 8601 em UTC. */
  dataCadastro: string
}

export interface CategoriaCriacao {
  nome: string
}

export interface CategoriaAtualizacao extends CategoriaCriacao {
  ativo: boolean
}

export interface CategoriaFiltro {
  /** Trecho do nome. */
  busca?: string
  /** true = só ativas, false = só inativas, ausente = todas. */
  ativo?: boolean
  pagina: number
  tamanhoPagina: number
}
