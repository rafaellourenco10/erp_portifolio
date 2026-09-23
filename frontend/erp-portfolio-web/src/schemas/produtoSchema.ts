/**
 * =====================================================================
 * Arquivo....: produtoSchema.ts
 * Versão.....: 1.3.0
 * Data.......: 22/09/2026
 * Descrição..: Schema Zod do formulário de produto (mesmas regras dos DTOs
 *              da API) e conversão dos valores do formulário para o payload.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 21/09/2026 - SKU restrito a dígitos (número de série).
 *   1.2.0 - 21/09/2026 - categoria (texto) trocada por categoriaId (seleção).
 *   1.3.0 - 22/09/2026 - estoqueMinimo obrigatório (padrão 0 = sem mínimo).
 * =====================================================================
 */

import { z } from 'zod'
import type { ProdutoAtualizacao } from '../types/produto'

export const UNIDADES = [
  { value: 'UN', label: 'UN — Unidade' },
  { value: 'KG', label: 'KG — Quilograma' },
  { value: 'L', label: 'L — Litro' },
  { value: 'M', label: 'M — Metro' },
  { value: 'CX', label: 'CX — Caixa' },
] as const

const VALOR_MAXIMO = 9_999_999_999.99
const QUANTIDADE_MAXIMA = 999_999.999

// O campo numérico vazio chega como null; a mensagem "Informe..." cobre esse caso.
const dinheiro = (rotulo: string) =>
  z
    .number()
    .nullable()
    .refine((valor) => valor !== null, `Informe ${rotulo}.`)
    .refine((valor) => valor === null || (valor >= 0 && valor <= VALOR_MAXIMO), 'O valor deve estar entre 0 e 9.999.999.999,99.')

// Estoque mínimo: 0 = sem mínimo definido (nunca conta como "saldo baixo"); obrigatório, mesmo se 0.
const estoqueMinimo = z
  .number()
  .nullable()
  .refine((valor) => valor !== null, 'Informe o estoque mínimo.')
  .refine(
    (valor) => valor === null || (valor >= 0 && valor <= QUANTIDADE_MAXIMA),
    'O estoque mínimo deve estar entre 0 e 999.999,999.',
  )

export const produtoSchema = z.object({
  nome: z
    .string()
    .trim()
    .min(3, 'O nome deve ter no mínimo 3 caracteres.')
    .max(150, 'O nome deve ter no máximo 150 caracteres.'),
  // O SKU é o número de série do produto: somente dígitos (mesma regra da API).
  sku: z
    .string()
    .trim()
    .min(1, 'Informe o SKU.')
    .regex(/^[0-9]{2,30}$/, 'O SKU deve ter de 2 a 30 dígitos (somente números).'),
  // Id de uma categoria cadastrada (seleção); null = sem categoria.
  categoriaId: z.number().nullable(),
  unidade: z.string().refine((valor) => UNIDADES.some((u) => u.value === valor), 'Selecione a unidade.'),
  precoVenda: dinheiro('o preço de venda'),
  custo: dinheiro('o custo'),
  estoqueMinimo,
  ativo: z.boolean(),
})

/** O que o formulário guarda enquanto o usuário digita (preço e custo podem estar vazios). */
export type ProdutoFormEntrada = z.input<typeof produtoSchema>
/** O que sai do schema depois de validado (preço e custo sempre preenchidos). */
export type ProdutoFormValores = z.output<typeof produtoSchema>

export const valoresIniciaisProduto: ProdutoFormEntrada = {
  nome: '',
  sku: '',
  categoriaId: null,
  unidade: '',
  precoVenda: null,
  custo: null,
  estoqueMinimo: 0,
  ativo: true,
}

export function paraPayload(valores: ProdutoFormValores): ProdutoAtualizacao {
  return { ...valores }
}
