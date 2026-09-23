/**
 * =====================================================================
 * Arquivo....: vendedorSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Schema Zod do formulário de vendedor (mesmas regras dos DTOs
 *              da API: só CPF, % de comissão de 0 a 100 com até 2 casas) e
 *              conversão para o payload. Espelho de fornecedorSchema.ts.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { z } from 'zod'
import type { VendedorAtualizacao } from '../types/vendedor'
import { documentoValido, normalizarDocumento } from '../utils/documento'

export const vendedorSchema = z.object({
  nome: z
    .string()
    .trim()
    .min(3, 'O nome deve ter no mínimo 3 caracteres.')
    .max(150, 'O nome deve ter no máximo 150 caracteres.'),
  // Vendedor é pessoa física: CNPJ é recusado, mesmo válido.
  cpf: z
    .string()
    .trim()
    .min(1, 'Informe o CPF.')
    .refine((cpf) => normalizarDocumento(cpf).length === 11 && documentoValido(cpf), 'CPF inválido.'),
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
  percentualComissao: z
    .number('Informe a comissão.')
    .min(0, 'A comissão deve estar entre 0 e 100%.')
    .max(100, 'A comissão deve estar entre 0 e 100%.')
    .refine((valor) => Math.round(valor * 100) / 100 === valor, 'Use no máximo 2 casas decimais.'),
  ativo: z.boolean(),
})

export type VendedorFormValores = z.infer<typeof vendedorSchema>

export const valoresIniciaisVendedor: VendedorFormValores = {
  nome: '',
  cpf: '',
  email: '',
  telefone: '',
  percentualComissao: 0,
  ativo: true,
}

/** Campos opcionais vazios são enviados como null, como a API os armazena. */
export function paraPayload(valores: VendedorFormValores): VendedorAtualizacao {
  return {
    ...valores,
    email: valores.email || null,
    telefone: valores.telefone || null,
  }
}
