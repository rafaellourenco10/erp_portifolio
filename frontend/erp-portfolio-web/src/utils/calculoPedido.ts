/**
 * =====================================================================
 * Arquivo....: calculoPedido.ts
 * Versão.....: 1.1.0
 * Data.......: 21/09/2026
 * Descrição..: Cálculo de subtotal e total do pedido para a PRÉ-VISUALIZAÇÃO da tela,
 *              com a mesma fórmula do servidor (Services/CalculoPedido.cs):
 *                subtotal = arredonda2(quantidade × preço × (1 − desconto/100))
 *                total    = arredonda2(soma dos subtotais × (1 − descontoPedido/100))
 *              Arredondamento em 2 casas, metade para cima.
 *              Usa aritmética INTEIRA (BigInt: milésimos, centavos e centésimos de %),
 *              porque o número decimal do JavaScript erra em casos como 1,005 × 100.
 *              Vale sempre o valor que o servidor devolve ao salvar.
 * ---------------------------------------------------------------------
 * Fontes.....: SPEC.md (seção "Cálculo"). Paridade com o back verificada por
 *              backend/ErpPortfolio.Tests/ParidadeCalculoFrontTests.cs.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 24/09/2026 - valorDevolucaoCentavos: prévia do valor de um item devolvido (etapa 14).
 * =====================================================================
 */

export interface ItemCalculo {
  quantidade: number
  precoUnitario: number
  descontoPercentual: number
}

export interface ResumoPedido {
  /** Subtotal de cada item, na mesma ordem da entrada (em reais). */
  subtotais: number[]
  /** Soma dos subtotais dos itens (em reais). */
  subtotalItens: number
  /** Valor do desconto do pedido (soma − total), em reais. */
  valorDesconto: number
  total: number
  /** true se o total passa de R$ 9.999.999.999,99 (o servidor recusa: regra R10). */
  excedeLimite: boolean
}

/** Maior total que o servidor aceita (coluna numeric(12,2)), em centavos. */
export const LIMITE_TOTAL_CENTAVOS = 999_999_999_999n

/** Cem por cento em centésimos de percentual. */
const CEM_POR_CENTO = 10_000n

/** Número decimal → inteiro na escala dada. Vazio/NaN vira 0; o resultado nunca é negativo. */
function paraInteiro(valor: number, escala: number): bigint {
  const finito = Number.isFinite(valor) ? Math.max(0, valor) : 0
  return BigInt(Math.round(finito * escala))
}

function descontoEmCentesimos(percentual: number): bigint {
  const centesimos = paraInteiro(percentual, 100)
  return centesimos > CEM_POR_CENTO ? CEM_POR_CENTO : centesimos
}

/** Subtotal do item em CENTAVOS (exato). */
export function subtotalCentavos(item: ItemCalculo): bigint {
  const milesimos = paraInteiro(item.quantidade, 1000)
  const centavos = paraInteiro(item.precoUnitario, 100)
  const desconto = descontoEmCentesimos(item.descontoPercentual)

  // (milésimos/1000) × centavos × ((10000 − desconto)/10000), arredondado para cima na metade: divide por 1000 × 10000.
  return (milesimos * centavos * (CEM_POR_CENTO - desconto) + 5_000_000n) / 10_000_000n
}

/** Total do pedido em CENTAVOS (exato), aplicando o desconto do pedido sobre a soma dos subtotais. */
export function totalCentavos(subtotais: bigint[], descontoPedidoPercentual: number): bigint {
  const soma = subtotais.reduce((acumulado, subtotal) => acumulado + subtotal, 0n)
  const desconto = descontoEmCentesimos(descontoPedidoPercentual)

  return (soma * (CEM_POR_CENTO - desconto) + 5_000n) / 10_000n
}

const paraReais = (centavos: bigint): number => Number(centavos) / 100

export function calcularPedido(itens: ItemCalculo[], descontoPedidoPercentual: number): ResumoPedido {
  const subtotais = itens.map(subtotalCentavos)
  const soma = subtotais.reduce((acumulado, subtotal) => acumulado + subtotal, 0n)
  const total = totalCentavos(subtotais, descontoPedidoPercentual)

  return {
    subtotais: subtotais.map(paraReais),
    subtotalItens: paraReais(soma),
    valorDesconto: paraReais(soma - total),
    total: paraReais(total),
    excedeLimite: total > LIMITE_TOTAL_CENTAVOS,
  }
}

/** Produtos em UN e CX só vendem quantidade inteira; KG, L e M aceitam decimais (regra R8). */
export function quantidadeValidaParaUnidade(unidade: string, quantidade: number): boolean {
  return (unidade !== 'UN' && unidade !== 'CX') || Number.isInteger(quantidade)
}

/** Centavos como texto com 2 casas ("630.00"), sem passar por número decimal. Usado nos testes de paridade. */
export function centavosParaTexto(centavos: bigint): string {
  const inteiro = centavos / 100n
  const resto = (centavos % 100n).toString().padStart(2, '0')
  return `${inteiro}.${resto}`
}

/**
 * Valor de um item devolvido em CENTAVOS, com a regra do servidor (DevolucaoCalculo.ValorItem):
 * arredonda2(quantidade × preço × (1 − desc. item/100) × (1 − desc. pedido/100)), com UM arredondamento só.
 */
export function valorDevolucaoCentavos(item: ItemCalculo, descontoPedidoPercentual: number): bigint {
  const milesimos = paraInteiro(item.quantidade, 1000)
  const centavos = paraInteiro(item.precoUnitario, 100)
  const fatorItem = CEM_POR_CENTO - descontoEmCentesimos(item.descontoPercentual)
  const fatorPedido = CEM_POR_CENTO - descontoEmCentesimos(descontoPedidoPercentual)
  const divisor = 1000n * CEM_POR_CENTO * CEM_POR_CENTO

  return (milesimos * centavos * fatorItem * fatorPedido + divisor / 2n) / divisor
}
