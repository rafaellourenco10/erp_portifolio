/**
 * =====================================================================
 * Arquivo....: pedidoSchema.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Schema Zod do formulário de pedido (mesmas regras dos DTOs da API e da
 *              SPEC: 1 a 100 itens, quantidade de 0,001 a 999.999,999, UN/CX só inteiro,
 *              descontos de 0 a 100, total até R$ 9.999.999.999,99) e conversões entre o
 *              formulário e a API. Cada linha guarda também nome, SKU, unidade e preço do
 *              produto: são só para exibir e calcular; o servidor copia o preço sozinho.
 * ---------------------------------------------------------------------
 * Fontes.....: SPEC.md (regras R4, R8 e R10); utils/calculoPedido.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - vendedorId (opcional no rascunho), etapa 10.
 * =====================================================================
 */

import { z } from 'zod'
import type { FormaPagamento, Pedido, PedidoEntrada } from '../types/pedido'
import type { Produto } from '../types/produto'
import { calcularPedido, quantidadeValidaParaUnidade } from '../utils/calculoPedido'

export const MAXIMO_ITENS = 100
export const LIMITE_TOTAL = 9_999_999_999.99

const FORMAS_PAGAMENTO = ['Dinheiro', 'Pix', 'Boleto', 'Cartao'] as const

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

export const pedidoSchema = z
  .object({
    clienteId: z
      .number()
      .nullable()
      .refine((valor) => valor !== null, 'Selecione o cliente.'),
    formaPagamento: z.enum(FORMAS_PAGAMENTO).nullable(),
    // Opcional no rascunho; a API exige para confirmar (e devolve o erro no campo).
    vendedorId: z.number().nullable(),
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
export type PedidoFormEntrada = z.input<typeof pedidoSchema>
export type PedidoItemFormEntrada = PedidoFormEntrada['itens'][number]
/** O que sai do schema depois de validado. */
export type PedidoFormValores = z.output<typeof pedidoSchema>

export const valoresIniciaisPedido: PedidoFormEntrada = {
  clienteId: null,
  formaPagamento: null,
  vendedorId: null,
  descontoPercentual: 0,
  itens: [],
}

/** Nova linha de item, com o preço atual do produto (o servidor congela o preço ao salvar). */
export function itemDoProduto(produto: Produto): PedidoItemFormEntrada {
  return {
    produtoId: produto.id,
    produtoNome: produto.nome,
    sku: produto.sku,
    unidade: produto.unidade,
    precoUnitario: produto.precoVenda,
    quantidade: 1,
    descontoPercentual: 0,
  }
}

/** Pedido vindo da API → valores do formulário (os itens mostram o preço congelado). */
export function paraFormulario(pedido: Pedido): PedidoFormEntrada {
  return {
    clienteId: pedido.clienteId,
    formaPagamento: pedido.formaPagamento,
    vendedorId: pedido.vendedorId,
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

/** Valores validados → corpo do POST/PUT (sem preço: o servidor copia do produto). */
export function paraPayload(valores: PedidoFormValores): PedidoEntrada {
  return {
    clienteId: valores.clienteId as number,
    formaPagamento: valores.formaPagamento as FormaPagamento | null,
    vendedorId: valores.vendedorId,
    descontoPercentual: valores.descontoPercentual,
    itens: valores.itens.map((item) => ({
      produtoId: item.produtoId,
      quantidade: item.quantidade as number,
      descontoPercentual: item.descontoPercentual,
    })),
  }
}
