/**
 * =====================================================================
 * Arquivo....: pedidoCompraSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Schema Zod do formulário de pedido de compra (mesmas regras dos DTOs da
 *              API e da SPEC: 1 a 100 itens, quantidade de 0,001 a 999.999,999, UN/CX só
 *              inteiro, descontos de 0 a 100, total até R$ 9.999.999.999,99) e conversões
 *              entre o formulário e a API. Espelho de pedidoSchema.ts, sem forma de
 *              pagamento; o preço de cada item nasce do Custo do produto (não do preço de
 *              venda).
 * ---------------------------------------------------------------------
 * Fontes.....: SPEC.md (regras PC2 e PC4); utils/calculoPedido.ts (reaproveitado do
 *              Pedido de Venda)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { z } from 'zod'
import type { Produto } from '../types/produto'
import type { PedidoCompra, PedidoCompraEntrada } from '../types/pedidoCompra'
import { calcularPedido, quantidadeValidaParaUnidade } from '../utils/calculoPedido'

export const MAXIMO_ITENS = 100
export const LIMITE_TOTAL = 9_999_999_999.99

// O campo numérico vazio chega como null; a mensagem "Informe..." cobre esse caso.
const itemSchema = z
  .object({
    produtoId: z.number(),
    produtoNome: z.string(),
    sku: z.string(),
    unidade: z.string(),
    precoUnitario: z.number(),
    quantidade: z
      .number()
      .nullable()
      .refine((valor) => valor !== null, 'Informe a quantidade.')
      .refine(
        (valor) => valor === null || (valor >= 0.001 && valor <= 999_999.999),
        'A quantidade deve estar entre 0,001 e 999.999,999.',
      ),
    // Desconto vazio vale 0.
    descontoPercentual: z
      .number()
      .nullable()
      .transform((valor) => valor ?? 0)
      .refine((valor) => valor >= 0 && valor <= 100, 'O desconto deve estar entre 0 e 100.'),
  })
  .superRefine((item, contexto) => {
    if (item.quantidade !== null && !quantidadeValidaParaUnidade(item.unidade, item.quantidade)) {
      contexto.addIssue({
        code: 'custom',
        path: ['quantidade'],
        message: `Produtos em ${item.unidade} só aceitam quantidade inteira.`,
      })
    }
  })

export const pedidoCompraSchema = z
  .object({
    fornecedorId: z
      .number()
      .nullable()
      .refine((valor) => valor !== null, 'Selecione o fornecedor.'),
    descontoPercentual: z
      .number()
      .nullable()
      .transform((valor) => valor ?? 0)
      .refine((valor) => valor >= 0 && valor <= 100, 'O desconto deve estar entre 0 e 100.'),
    itens: z
      .array(itemSchema)
      .min(1, 'Adicione ao menos um item.')
      .max(MAXIMO_ITENS, `Um pedido pode ter no máximo ${MAXIMO_ITENS} itens.`),
  })
  .superRefine((pedido, contexto) => {
    const resumo = calcularPedido(
      pedido.itens.map((item) => ({
        quantidade: item.quantidade ?? 0,
        precoUnitario: item.precoUnitario,
        descontoPercentual: item.descontoPercentual,
      })),
      pedido.descontoPercentual,
    )
    if (resumo.excedeLimite) {
      contexto.addIssue({
        code: 'custom',
        path: ['itens'],
        message: 'O total do pedido não pode passar de R$ 9.999.999.999,99.',
      })
    }
  })

/** O que o formulário guarda enquanto o usuário digita (campos numéricos podem estar vazios). */
export type PedidoCompraFormEntrada = z.input<typeof pedidoCompraSchema>
export type PedidoCompraItemFormEntrada = PedidoCompraFormEntrada['itens'][number]
/** O que sai do schema depois de validado. */
export type PedidoCompraFormValores = z.output<typeof pedidoCompraSchema>

export const valoresIniciaisPedidoCompra: PedidoCompraFormEntrada = {
  fornecedorId: null,
  descontoPercentual: 0,
  itens: [],
}

/** Nova linha de item, com o Custo atual do produto (o servidor congela o preço ao salvar). */
export function itemDoProduto(produto: Produto): PedidoCompraItemFormEntrada {
  return {
    produtoId: produto.id,
    produtoNome: produto.nome,
    sku: produto.sku,
    unidade: produto.unidade,
    precoUnitario: produto.custo,
    quantidade: 1,
    descontoPercentual: 0,
  }
}

/** Pedido de compra vindo da API → valores do formulário (os itens mostram o preço congelado). */
export function paraFormulario(pedido: PedidoCompra): PedidoCompraFormEntrada {
  return {
    fornecedorId: pedido.fornecedorId,
    descontoPercentual: pedido.descontoPercentual,
    itens: pedido.itens.map((item) => ({
      produtoId: item.produtoId,
      produtoNome: item.produtoNome,
      sku: item.sku,
      unidade: item.unidade,
      precoUnitario: item.precoUnitario,
      quantidade: item.quantidade,
      descontoPercentual: item.descontoPercentual,
    })),
  }
}

/** Valores validados → corpo do POST/PUT (sem preço: o servidor copia do Custo do produto). */
export function paraPayload(valores: PedidoCompraFormValores): PedidoCompraEntrada {
  return {
    fornecedorId: valores.fornecedorId as number,
    descontoPercentual: valores.descontoPercentual,
    itens: valores.itens.map((item) => ({
      produtoId: item.produtoId,
      quantidade: item.quantidade as number,
      descontoPercentual: item.descontoPercentual,
    })),
  }
}
