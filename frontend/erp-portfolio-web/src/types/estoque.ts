/**
 * =====================================================================
 * Arquivo....: estoque.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Estoque, espelhando os DTOs da API
 *              (EstoqueResumoDto, MovimentacaoRespostaDto, EstoqueFiltroDto,
 *              EstoqueEntradaDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - Movimentacao ganha pedidoCompraId (etapa 7).
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
  /** Preenchido só nas movimentações de um pedido de compra (entrada ou estorno). */
  pedidoCompraId: number | null
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
