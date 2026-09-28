/**
 * =====================================================================
 * Arquivo....: notaFiscal.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Tipos da NF-e simulada (etapa 16), espelhando os DTOs da API
 *              (NotaFiscalResumoDto, NotaFiscalDetalheDto, NotaFiscalFiltroDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

/** Saída = venda; Entrada = devolução de venda. */
export type TipoNotaFiscal = 'Saida' | 'Entrada'

export interface NotaFiscalResumo {
  id: number
  tipo: TipoNotaFiscal
  serie: number
  numero: number
  /** 44 dígitos. */
  chave: string
  /** Data/hora ISO 8601 em UTC. */
  dataEmissao: string
  clienteId: number
  destinatarioNome: string
  destinatarioDocumento: string
  destinatarioUf: string
  pedidoId: number
  devolucaoId: number | null
  valorTotal: number
}

export interface NotaFiscalItem {
  numeroItem: number
  produtoId: number
  codigo: string
  descricao: string
  ncm: string
  cfop: string
  unidade: string
  quantidade: number
  valorUnitario: number
  valorBruto: number
  valorDesconto: number
  baseIcms: number
  aliquotaIcms: number
  valorIcms: number
  valorPis: number
  valorCofins: number
}

export interface NotaFiscalDetalhe extends NotaFiscalResumo {
  protocolo: string
  notaReferenciadaId: number | null
  chaveReferenciada: string | null
  valorProdutos: number
  valorDesconto: number
  baseIcms: number
  valorIcms: number
  valorPis: number
  valorCofins: number
  itens: NotaFiscalItem[]
}

export interface NotaFiscalFiltro {
  /** AAAA-MM-DD. */
  dataInicio?: string
  dataFim?: string
  clienteId?: number
  tipo?: TipoNotaFiscal
  /** Número da nota ou trecho da chave. */
  busca?: string
  pagina: number
  tamanhoPagina: number
}
