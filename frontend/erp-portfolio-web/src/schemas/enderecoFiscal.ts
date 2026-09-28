/**
 * =====================================================================
 * Arquivo....: enderecoFiscal.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Regras Zod dos campos de endereço usados na NF-e (etapa 16),
 *              compartilhadas pelo cliente e pela empresa. Mesmas regras da
 *              API (Validacoes/EnderecoFiscal.cs); o código IBGE × UF só a API
 *              confere, e o erro dela volta no próprio campo.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { z } from 'zod'

/** Tira pontos, traços, barras e espaços (letras ficam para a validação recusar). */
export const semMascara = (valor: string) => valor.replace(/[.\-/\s]/g, '').toUpperCase()

const texto = (maximo: number, rotulo: string) =>
  z.string().trim().max(maximo, `${rotulo} deve ter no máximo ${maximo} caracteres.`)

/** Todos opcionais: no cliente só são exigidos na hora de emitir a NF-e. */
export const camposEnderecoFiscal = {
  cep: z.string().trim().refine((v) => v === '' || /^[0-9]{8}$/.test(semMascara(v)), 'O CEP deve ter 8 dígitos.'),
  logradouro: texto(60, 'O logradouro'),
  numero: texto(10, 'O número'),
  complemento: texto(60, 'O complemento'),
  bairro: texto(60, 'O bairro'),
  codigoMunicipio: z
    .string()
    .trim()
    .refine((v) => v === '' || /^[0-9]{7}$/.test(v), 'O código IBGE do município deve ter 7 dígitos.'),
}

/** Vazio vira null, como a API grava. */
export const ouNull = (valor: string) => (valor.trim() === '' ? null : valor.trim())

/** CEP gravado sem máscara, exibido como 00000-000. */
export const formatarCep = (cep: string | null) => (cep && cep.length === 8 ? `${cep.slice(0, 5)}-${cep.slice(5)}` : (cep ?? ''))
