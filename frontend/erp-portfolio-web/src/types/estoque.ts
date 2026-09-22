/**
 * =====================================================================
 * Arquivo....: estoque.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Tipos do módulo de Estoque, espelhando os DTOs da API
 *              (EstoqueResumoDto, MovimentacaoRespostaDto, EstoqueFiltroDto,
 *              EstoqueEntradaDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export type TipoMovimentacao = 'Entrada' | 'Saida'

export interface EstoqueResumo {
  produtoId: number
  produtoNome: string
  sku: string
  unidade: string
  saldo: number
}

export interface Movimentacao {
  tipo: TipoMovimentacao
  quantidade: number
  motivo: string | null
  pedidoId: number | null
  /** Data/hora ISO 8601 em UTC. */
  dataMovimentacao: string
}

export interface EstoqueFiltro {
  /** Trecho do nome ou do SKU do produto. */
  busca?: string
  pagina: number
  tamanhoPagina: number
}

export interface EstoqueEntrada {
  produtoId: number
  quantidade: number
  motivo?: string
}
