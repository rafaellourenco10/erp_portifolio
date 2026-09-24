/**
 * =====================================================================
 * Arquivo....: orcamentoSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Schema Zod do formulário de orçamento: o do pedido (itens, descontos,
 *              limite do total) + validade (obrigatória, não antes de hoje) e observações
 *              (até 500). Conversões entre formulário e API reaproveitam as do pedido.
 * ---------------------------------------------------------------------
 * Fontes.....: SPEC.md etapa 13 (OR1, OR3); schemas/pedidoSchema.ts
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import dayjs, { type Dayjs } from 'dayjs'
import { z } from 'zod'
import type { Orcamento, OrcamentoEntrada } from '../types/orcamento'
import { paraFormulario as pedidoParaFormulario, paraPayload as pedidoParaPayload, pedidoSchema, valoresIniciaisPedido } from './pedidoSchema'

/** Validade sugerida para um orçamento novo (OR3). */
export const DIAS_VALIDADE_PADRAO = 15

// safeExtend mantém a checagem do limite do total que o schema do pedido faz no superRefine.
export const orcamentoSchema = pedidoSchema.safeExtend({
  validade: z
    .custom<Dayjs | null>()
    .refine((valor) => valor != null, 'Informe a validade.')
    .refine((valor) => valor == null || !valor.isBefore(dayjs(), 'day'), 'A validade não pode ser anterior a hoje.'),
  observacoes: z.string().max(500, 'As observações devem ter no máximo 500 caracteres.'),
})

export type OrcamentoFormEntrada = z.input<typeof orcamentoSchema>
export type OrcamentoFormValores = z.output<typeof orcamentoSchema>

export function valoresIniciaisOrcamento(): OrcamentoFormEntrada {
  return { ...valoresIniciaisPedido, validade: dayjs().add(DIAS_VALIDADE_PADRAO, 'day'), observacoes: '' }
}

export function paraFormulario(orcamento: Orcamento): OrcamentoFormEntrada {
  return { ...pedidoParaFormulario(orcamento), validade: dayjs(orcamento.validade), observacoes: orcamento.observacoes ?? '' }
}

export function paraPayload(valores: OrcamentoFormValores): OrcamentoEntrada {
  return {
    ...pedidoParaPayload(valores),
    validade: valores.validade!.format('YYYY-MM-DD'),
    observacoes: valores.observacoes.trim() || null,
  }
}
