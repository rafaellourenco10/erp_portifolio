/**
 * =====================================================================
 * Arquivo....: empresaSchema.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Schema Zod do formulário da empresa emitente (etapa 16), com
 *              as mesmas regras do EmpresaDto da API.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { z } from 'zod'
import type { Empresa } from '../types/empresa'
import { documentoValido } from '../utils/documento'
import { camposEnderecoFiscal, ouNull, semMascara } from './enderecoFiscal'

const obrigatorio = (schema: z.ZodString, mensagem: string) => schema.refine((v) => v.trim() !== '', mensagem)

export const empresaSchema = z.object({
  razaoSocial: z
    .string()
    .trim()
    .min(3, 'A razão social deve ter entre 3 e 60 caracteres.')
    .max(60, 'A razão social deve ter entre 3 e 60 caracteres.'),
  nomeFantasia: z.string().trim().max(60, 'O nome fantasia deve ter no máximo 60 caracteres.'),
  cnpj: z
    .string()
    .trim()
    .refine((v) => semMascara(v).length === 14 && documentoValido(v), 'CNPJ inválido.'),
  inscricaoEstadual: z
    .string()
    .trim()
    .refine((v) => /^[0-9]{2,14}$/.test(semMascara(v)), 'A Inscrição Estadual deve ter de 2 a 14 dígitos.'),
  logradouro: obrigatorio(camposEnderecoFiscal.logradouro, 'Informe o logradouro.'),
  numero: obrigatorio(camposEnderecoFiscal.numero, 'Informe o número.'),
  complemento: camposEnderecoFiscal.complemento,
  bairro: obrigatorio(camposEnderecoFiscal.bairro, 'Informe o bairro.'),
  cep: z.string().trim().refine((v) => /^[0-9]{8}$/.test(semMascara(v)), 'O CEP deve ter 8 dígitos.'),
  municipio: z.string().trim().min(2, 'Informe o município.').max(60, 'O município deve ter no máximo 60 caracteres.'),
  codigoMunicipio: z.string().trim().regex(/^[0-9]{7}$/, 'O código IBGE do município deve ter 7 dígitos.'),
  uf: z.string().min(1, 'Selecione a UF.'),
  telefone: z
    .string()
    .trim()
    .regex(/^([0-9()+\-\s]{8,20})?$/, 'Telefone inválido.'),
  serieNfe: z
    .number()
    .nullable()
    .refine((v) => v !== null && Number.isInteger(v) && v >= 0 && v <= 999, 'A série deve estar entre 0 e 999.'),
})

export type EmpresaFormEntrada = z.input<typeof empresaSchema>
export type EmpresaFormValores = z.output<typeof empresaSchema>

export const valoresIniciaisEmpresa: EmpresaFormEntrada = {
  razaoSocial: '',
  nomeFantasia: '',
  cnpj: '',
  inscricaoEstadual: '',
  logradouro: '',
  numero: '',
  complemento: '',
  bairro: '',
  cep: '',
  municipio: '',
  codigoMunicipio: '',
  uf: '',
  telefone: '',
  serieNfe: 1,
}

export function paraPayload(valores: EmpresaFormValores): Empresa {
  return {
    ...valores,
    nomeFantasia: ouNull(valores.nomeFantasia),
    complemento: ouNull(valores.complemento),
    telefone: ouNull(valores.telefone),
    serieNfe: valores.serieNfe ?? 1,
  }
}
