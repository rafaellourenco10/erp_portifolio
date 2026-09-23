/**
 * =====================================================================
 * Arquivo....: contaPagar.ts
 * Versão.....: 2.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Contas a Pagar, espelhando os DTOs da API
 *              (ParcelaPagarRespostaDto, ParcelaPagarFiltroDto,
 *              ContaAvulsaCriacaoDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   2.0.0 - 23/09/2026 - Origem (Compra/Comissao/Avulsa), favorecido e descrição;
 *                        fornecedorNome vira favorecido; conta avulsa (etapa 12).
 * =====================================================================
 */

export type StatusParcelaPagar = 'Pendente' | 'Pago' | 'Cancelado'

/** Status para filtrar a listagem; "Atrasado" não é gravado, é calculado no servidor. */
export type FiltroStatusParcelaPagar = StatusParcelaPagar | 'Atrasado'

export type OrigemContaPagar = 'Compra' | 'Comissao' | 'Avulsa'

export interface ParcelaPagar {
  id: number
  origem: OrigemContaPagar
  /** Só nas parcelas de compra. */
  pedidoCompraId: number | null
  /** Só nas parcelas de comissão. */
  vendedorId: number | null
  /** Fornecedor (compra), vendedor (comissão) ou o texto digitado (avulsa; pode faltar). */
  favorecido: string | null
  /** Nula nas de compra (a tela mostra "Compra #N"). */
  descricao: string | null
  numeroParcela: number
  totalParcelas: number
  valor: number
  /** Data no formato AAAA-MM-DD (sem hora). */
  vencimento: string
  status: StatusParcelaPagar
  /** Data/hora ISO 8601 em UTC; nulo até ser paga. */
  dataPagamento: string | null
  atrasado: boolean
}

export interface ParcelaPagarFiltro {
  /** Nº da compra (com ou sem #) ou trecho do favorecido/descrição. */
  busca?: string
  status?: FiltroStatusParcelaPagar
  origem?: OrigemContaPagar
  pagina: number
  tamanhoPagina: number
}

/** Corpo do POST /contas-pagar (conta avulsa). */
export interface ContaAvulsaEntrada {
  descricao: string
  favorecido: string | null
  valorTotal: number
  /** AAAA-MM-DD. */
  primeiroVencimento: string
  numeroParcelas: number
  intervaloDias: number
}
