/**
 * =====================================================================
 * Arquivo....: comissao.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tipos do módulo de Comissões, espelhando os DTOs da API
 *              (ComissaoRespostaDto, ComissaoListaDto, ComissaoFiltroDto).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { ResultadoPaginado } from './paginacao'

export type StatusComissao = 'Pendente' | 'Paga'

export interface Comissao {
  id: number
  vendedorId: number
  vendedorNome: string
  pedidoId: number
  clienteNome: string
  numeroParcela: number
  totalParcelas: number
  /** Valor da parcela recebida. */
  valorBase: number
  /** % congelada no pedido. */
  percentual: number
  valor: number
  /** Data/hora ISO 8601 (UTC) do recebimento da parcela. */
  dataGeracao: string
  status: StatusComissao
  dataPagamento: string | null
}

export interface ComissaoLista {
  resultado: ResultadoPaginado<Comissao>
  /** Somas de todo o filtro (vendedor/período), sem considerar o status. */
  totais: { totalGerado: number; totalPendente: number; totalPago: number }
}

export interface ComissaoFiltro {
  vendedorId?: number
  status?: StatusComissao
  /** AAAA-MM-DD, pela data do recebimento (inclusiva). */
  dataInicio?: string
  dataFim?: string
  pagina: number
  tamanhoPagina: number
}
