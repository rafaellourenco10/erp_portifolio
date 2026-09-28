/**
 * =====================================================================
 * Arquivo....: clienteSchema.ts
 * Versão.....: 1.2.0
 * Data.......: 21/09/2026
 * Descrição..: Schema Zod do formulário de cliente (mesmas regras dos DTOs
 *              da API) e conversão dos valores do formulário para o payload.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 21/09/2026 - E-mail com trim antes de validar (igual à API).
 *   1.2.0 - 28/09/2026 - Dados fiscais opcionais (NF-e, etapa 16).
 * =====================================================================
 */

import { z } from 'zod'
import type { ClienteAtualizacao } from '../types/cliente'
import { documentoValido } from '../utils/documento'
import { camposEnderecoFiscal, ouNull, semMascara } from './enderecoFiscal'

export const clienteSchema = z.object({
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
  ...camposEnderecoFiscal,
  inscricaoEstadual: z
    .string()
    .trim()
    .refine((v) => v === '' || /^([0-9]{2,14}|ISENTO)$/.test(semMascara(v)), 'Use de 2 a 14 dígitos ou ISENTO.'),
  ativo: z.boolean(),
})

export type ClienteFormValores = z.infer<typeof clienteSchema>

export const valoresIniciaisCliente: ClienteFormValores = {
  nome: '',
  documento: '',
  email: '',
  telefone: '',
  cidade: '',
  uf: '',
  logradouro: '',
  numero: '',
  complemento: '',
  bairro: '',
  cep: '',
  codigoMunicipio: '',
  inscricaoEstadual: '',
  ativo: true,
}

/** Campos opcionais vazios são enviados como null, como a API os armazena. */
export function paraPayload(valores: ClienteFormValores): ClienteAtualizacao {
  return {
    ...valores,
    email: valores.email || null,
    telefone: valores.telefone || null,
    logradouro: ouNull(valores.logradouro),
    numero: ouNull(valores.numero),
    complemento: ouNull(valores.complemento),
    bairro: ouNull(valores.bairro),
    cep: ouNull(valores.cep),
    codigoMunicipio: ouNull(valores.codigoMunicipio),
    inscricaoEstadual: ouNull(valores.inscricaoEstadual),
  }
}
