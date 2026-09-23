/**
 * =====================================================================
 * Arquivo....: fornecedorSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Schema Zod do formulário de fornecedor (mesmas regras dos
 *              DTOs da API) e conversão dos valores do formulário para o
 *              payload. Espelho de clienteSchema.ts.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { z } from 'zod'
import type { FornecedorAtualizacao } from '../types/fornecedor'
import { documentoValido } from '../utils/documento'

export const fornecedorSchema = z.object({
  nome: z
    .string()
    .trim()
    .min(3, 'O nome deve ter no mínimo 3 caracteres.')
    .max(150, 'O nome deve ter no máximo 150 caracteres.'),
  documento: z
    .string()
    .trim()
    .min(1, 'Informe o CPF ou CNPJ.')
    .refine(documentoValido, 'CPF/CNPJ inválido.'),
  // trim antes da validação: a API também faz trim, então "a@b.com " não pode falhar só no front.
  email: z
    .string()
    .trim()
    .pipe(
      z.union([
        z.literal(''),
        z.email('E-mail inválido.').max(150, 'O e-mail deve ter no máximo 150 caracteres.'),
      ]),
    ),
  telefone: z
    .string()
    .trim()
    .regex(/^([0-9()+\-\s]{8,20})?$/, 'Telefone inválido.'),
  cidade: z
    .string()
    .trim()
    .min(2, 'Informe a cidade.')
    .max(100, 'A cidade deve ter no máximo 100 caracteres.'),
  uf: z.string().min(1, 'Selecione a UF.'),
  ativo: z.boolean(),
})

export type FornecedorFormValores = z.infer<typeof fornecedorSchema>

export const valoresIniciaisFornecedor: FornecedorFormValores = {
  nome: '',
  documento: '',
  email: '',
  telefone: '',
  cidade: '',
  uf: '',
  ativo: true,
}

/** Campos opcionais vazios são enviados como null, como a API os armazena. */
export function paraPayload(valores: FornecedorFormValores): FornecedorAtualizacao {
  return {
    ...valores,
    email: valores.email || null,
    telefone: valores.telefone || null,
  }
}
