/**
 * =====================================================================
 * Arquivo....: cliente.ts
 * Versão.....: 1.2.0
 * Data.......: 21/09/2026
 * Descrição..: Tipos do módulo de Clientes, espelhando os DTOs da API
 *              (ClienteRespostaDto, ClienteCriacaoDto, ClienteAtualizacaoDto,
 *              ClienteFiltroDto e ResultadoPaginadoDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Filtros ufs e ativo em ClienteFiltro.
 *   1.2.0 - 21/09/2026 - ResultadoPaginado movido para paginacao.ts (reexportado aqui).
 * =====================================================================
 */

export interface Cliente {
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

export interface ClienteCriacao {
  nome: string
  documento: string
  email: string | null
  telefone: string | null
  cidade: string
  uf: string
}

export interface ClienteAtualizacao extends ClienteCriacao {
  ativo: boolean
}

export interface ClienteFiltro {
  nome?: string
  ufs?: string[]
  /** true = só ativos, false = só inativos, ausente = todos. */
  ativo?: boolean
  pagina: number
  tamanhoPagina: number
}

export type { ResultadoPaginado } from './paginacao'
