/**
 * =====================================================================
 * Arquivo....: moeda.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Formatação de valores em reais e cálculo de margem.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

const formatoReal = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const formatoPercentual = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })

export function formatarReal(valor: number): string {
  return formatoReal.format(valor)
}

/** Margem sobre o preço de venda, em %: (preço - custo) / preço. Null quando o preço é zero. */
export function calcularMargem(precoVenda: number, custo: number): number | null {
  return precoVenda > 0 ? ((precoVenda - custo) / precoVenda) * 100 : null
}

export function formatarPercentual(valor: number): string {
  return `${formatoPercentual.format(valor)}%`
}
