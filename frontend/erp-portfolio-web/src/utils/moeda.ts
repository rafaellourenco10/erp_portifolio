/**
 * =====================================================================
 * Arquivo....: moeda.ts
 * Versão.....: 1.1.0
 * Data.......: 22/09/2026
 * Descrição..: Formatação de valores em reais, percentual, cálculo de
 *              margem e formatação de quantidade (até 3 casas).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 22/09/2026 - formatarQuantidade, para o saldo/extrato de Estoque.
 * =====================================================================
 */

const formatoReal = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const formatoPercentual = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })
const formatoQuantidade = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 0, maximumFractionDigits: 3 })

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

export function formatarQuantidade(valor: number): string {
  return formatoQuantidade.format(valor)
}
