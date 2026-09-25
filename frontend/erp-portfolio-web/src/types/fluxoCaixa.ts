/**
 * =====================================================================
 * Arquivo....: fluxoCaixa.ts
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Tipos do Fluxo de Caixa, espelhando FluxoCaixaFiltroDto,
 *              FluxoCaixaDto e PeriodoFluxoCaixa da API (etapa 15).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export type AgrupamentoFluxoCaixa = 'Dia' | 'Mes'

export interface FluxoCaixaFiltro {
  /** AAAA-MM-DD, inclusivas. */
  dataInicio: string
  dataFim: string
  agrupamento: AgrupamentoFluxoCaixa
}

export interface PeriodoFluxoCaixa {
  /** AAAA-MM-DD: o dia, ou o 1º dia do mês na visão mensal. */
  inicio: string
  entradasRealizadas: number
  saidasRealizadas: number
  entradasPrevistas: number
  saidasPrevistas: number
  /** Saldo ao fim do período. */
  saldo: number
}

export interface FluxoCaixa {
  saldoInicial: number
  totalEntradas: number
  totalSaidas: number
  saldoFinal: number
  menorSaldo: number
  dataMenorSaldo: string
  /** Pendentes já vencidas; contam como previstas em hoje (0 se hoje está fora do período). */
  atrasadoReceber: number
  atrasadoPagar: number
  /** "Hoje" do servidor (Brasília), AAAA-MM-DD: daqui em diante é previsto. */
  hoje: string
  periodos: PeriodoFluxoCaixa[]
}
