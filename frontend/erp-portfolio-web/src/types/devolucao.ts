/**
 * =====================================================================
 * Arquivo....: devolucao.ts
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Tipos da devolução de venda (etapa 14), espelhando DevolucaoCriacaoDto,
 *              DevolucaoRespostaDto e DevolucoesResumoDto da API.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export interface DevolucaoItemEntrada {
  /** Id do item do pedido (não do produto). */
  pedidoItemId: number
  quantidade: number
  /** false = perda (ex.: defeito): não volta ao estoque. */
  voltaEstoque: boolean
}

export interface DevolucaoEntrada {
  itens: DevolucaoItemEntrada[]
  motivo: string | null
  /** AAAA-MM-DD; só usado se houver reembolso (padrão: hoje). */
  vencimentoReembolso: string | null
}

export interface DevolucaoItem {
  id: number
  pedidoItemId: number
  produtoId: number
  produtoNome: string
  unidade: string
  quantidade: number
  valor: number
  voltaEstoque: boolean
}

export interface Devolucao {
  /** Também é o número da devolução. */
  id: number
  pedidoId: number
  /** Data/hora ISO 8601 em UTC. */
  dataDevolucao: string
  motivo: string | null
  valorTotal: number
  /** Parte descontada das parcelas pendentes. */
  valorAbatido: number
  /** Parte que virou conta a pagar de reembolso ao cliente. */
  valorReembolso: number
  parcelaPagarId: number | null
  /** Estorno de comissão (negativo) ou 0. */
  estornoComissao: number
  itens: DevolucaoItem[]
}

/** Card "Devoluções do mês" do Dashboard. */
export interface DevolucoesResumo {
  valorTotal: number
  quantidade: number
}
