/**
 * =====================================================================
 * Arquivo....: estoqueEntradaSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Schema Zod do formulário de entrada manual de estoque
 *              (mesmas regras do EstoqueEntradaDto da API).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { z } from 'zod'

export const estoqueEntradaSchema = z.object({
  produtoId: z
    .number()
    .nullable()
    .refine((valor) => valor !== null, 'Selecione um produto.'),
  produtoNome: z.string(),
  quantidade: z
    .number()
    .nullable()
    .refine((valor) => valor !== null, 'Informe a quantidade.')
    .refine(
      (valor) => valor === null || (valor >= 0.001 && valor <= 999_999.999),
      'A quantidade deve estar entre 0,001 e 999.999,999.',
    ),
  motivo: z.string().max(200, 'O motivo deve ter no máximo 200 caracteres.'),
})

export type EstoqueEntradaFormEntrada = z.input<typeof estoqueEntradaSchema>
export type EstoqueEntradaFormValores = z.output<typeof estoqueEntradaSchema>

export const valoresIniciaisEntrada: EstoqueEntradaFormEntrada = {
  produtoId: null,
  produtoNome: '',
  quantidade: null,
  motivo: '',
}
