/**
 * =====================================================================
 * Arquivo....: categoriaSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Schema Zod do formulário de categoria (mesmas regras dos
 *              DTOs da API).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { z } from 'zod'

export const categoriaSchema = z.object({
  nome: z
    .string()
    .trim()
    .min(2, 'O nome deve ter no mínimo 2 caracteres.')
    .max(60, 'O nome deve ter no máximo 60 caracteres.')
    .transform((valor) => valor.toUpperCase()),
  ativo: z.boolean(),
})

export type CategoriaFormValores = z.infer<typeof categoriaSchema>

export const valoresIniciaisCategoria: CategoriaFormValores = { nome: '', ativo: true }
