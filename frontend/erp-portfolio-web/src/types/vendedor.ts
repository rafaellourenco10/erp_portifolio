/**
 * =====================================================================
 * Arquivo....: vendedor.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do cadastro de vendedores, espelhando os DTOs da API
 *              (VendedorRespostaDto, VendedorCriacaoDto, VendedorAtualizacaoDto,
 *              VendedorFiltroDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export interface Vendedor {
  id: number
  nome: string
  /** 11 dígitos, sem máscara. */
  cpf: string
  email: string | null
  telefone: string | null
  /** % de comissão padrão (0 a 100), copiada para o pedido ao confirmar. */
  percentualComissao: number
  ativo: boolean
  /** Data/hora ISO 8601 em UTC. */
  dataCadastro: string
}

export interface VendedorAtualizacao {
  nome: string
  cpf: string
  email: string | null
  telefone: string | null
  percentualComissao: number
  ativo: boolean
}

export interface VendedorFiltro {
  nome?: string
  /** true = só ativos, false = só inativos, ausente = todos. */
  ativo?: boolean
  pagina: number
  tamanhoPagina: number
}
