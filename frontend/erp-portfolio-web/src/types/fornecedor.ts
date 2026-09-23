/**
 * =====================================================================
 * Arquivo....: fornecedor.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Fornecedores, espelhando os DTOs da API
 *              (FornecedorRespostaDto, FornecedorCriacaoDto,
 *              FornecedorAtualizacaoDto, FornecedorFiltroDto e
 *              ResultadoPaginadoDto). Espelho de types/cliente.ts.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export interface Fornecedor {
  id: number
  nome: string
  documento: string
  email: string | null
  telefone: string | null
  cidade: string
  uf: string
  ativo: boolean
  /** Data/hora ISO 8601 em UTC. */
  dataCadastro: string
}

export interface FornecedorCriacao {
  nome: string
  documento: string
  email: string | null
  telefone: string | null
  cidade: string
  uf: string
}

export interface FornecedorAtualizacao extends FornecedorCriacao {
  ativo: boolean
}

export interface FornecedorFiltro {
  nome?: string
  ufs?: string[]
  /** true = só ativos, false = só inativos, ausente = todos. */
  ativo?: boolean
  pagina: number
  tamanhoPagina: number
}

export type { ResultadoPaginado } from './paginacao'
