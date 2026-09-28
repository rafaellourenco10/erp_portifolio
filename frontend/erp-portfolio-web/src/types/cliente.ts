/**
 * =====================================================================
 * Arquivo....: cliente.ts
 * Versão.....: 1.3.0
 * Data.......: 21/09/2026
 * Descrição..: Tipos do módulo de Clientes, espelhando os DTOs da API
 *              (ClienteRespostaDto, ClienteCriacaoDto, ClienteAtualizacaoDto,
 *              ClienteFiltroDto e ResultadoPaginadoDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Filtros ufs e ativo em ClienteFiltro.
 *   1.2.0 - 21/09/2026 - ResultadoPaginado movido para paginacao.ts (reexportado aqui).
 *   1.3.0 - 28/09/2026 - Dados fiscais: endereço, CEP, código IBGE e IE (NF-e, etapa 16).
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
  logradouro: string | null
  numero: string | null
  complemento: string | null
  bairro: string | null
  /** 8 dígitos, sem máscara. */
  cep: string | null
  /** Código IBGE do município (7 dígitos). */
  codigoMunicipio: string | null
  /** Dígitos ou "ISENTO". */
  inscricaoEstadual: string | null
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
  logradouro: string | null
  numero: string | null
  complemento: string | null
  bairro: string | null
  cep: string | null
  codigoMunicipio: string | null
  inscricaoEstadual: string | null
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
