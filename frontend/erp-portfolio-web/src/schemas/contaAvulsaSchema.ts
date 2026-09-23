/**
 * =====================================================================
 * Arquivo....: contaAvulsaSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Schema Zod do formulário de conta avulsa (mesmas regras do
 *              ContaAvulsaCriacaoDto da API: descrição 3-200, favorecido até
 *              150, valor > 0 com 2 casas, 1-12 parcelas, intervalo 1-180 dias).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { Dayjs } from 'dayjs'
import { z } from 'zod'
import type { ContaAvulsaEntrada } from '../types/contaPagar'

export const contaAvulsaSchema = z.object({
  descricao: z
    .string()
    .trim()
    .min(3, 'A descrição deve ter no mínimo 3 caracteres.')
    .max(200, 'A descrição deve ter no máximo 200 caracteres.'),
  favorecido: z.string().trim().max(150, 'O favorecido deve ter no máximo 150 caracteres.'),
  valorTotal: z
    .number('Informe o valor.')
    .positive('O valor deve ser maior que zero.')
    .refine((valor) => Math.round(valor * 100) / 100 === valor, 'Use no máximo 2 casas decimais.'),
  primeiroVencimento: z.custom<Dayjs>((valor) => valor != null, 'Informe o vencimento.'),
  numeroParcelas: z.number('Informe as parcelas.').int().min(1, 'De 1 a 12 parcelas.').max(12, 'De 1 a 12 parcelas.'),
  intervaloDias: z.number('Informe o intervalo.').int().min(1, 'De 1 a 180 dias.').max(180, 'De 1 a 180 dias.'),
})

export type ContaAvulsaFormValores = z.infer<typeof contaAvulsaSchema>

/** Favorecido vazio vai como null, como a API armazena. */
export function paraPayload(valores: ContaAvulsaFormValores): ContaAvulsaEntrada {
  return {
    ...valores,
    favorecido: valores.favorecido || null,
    primeiroVencimento: valores.primeiroVencimento.format('YYYY-MM-DD'),
  }
}
