/**
 * =====================================================================
 * Arquivo....: documento.ts
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Validação e formatação de CPF/CNPJ (mesma regra da API em
 *              DocumentoValidador.cs), incluindo o CNPJ alfanumérico.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

const PESOS_CNPJ_PRIMEIRO_DV = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
const PESOS_CNPJ_SEGUNDO_DV = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]

/** Remove a máscara e converte para maiúsculas. */
export function normalizarDocumento(documento: string): string {
  return documento.replace(/[^0-9A-Za-z]/g, '').toUpperCase()
}

// Valor de cada caractere = código ASCII - 48 (dígitos 0-9, letras A=17 ... Z=42).
function valorCaractere(caractere: string): number {
  return caractere.charCodeAt(0) - 48
}

function calcularDv(valores: number[], pesos: number[]): number {
  const soma = pesos.reduce((acumulado, peso, i) => acumulado + valores[i] * peso, 0)
  const resto = soma % 11
  return resto < 2 ? 0 : 11 - resto
}

function todosIguais(texto: string): boolean {
  return new Set(texto).size === 1
}

function cpfValido(cpf: string): boolean {
  if (!/^\d{11}$/.test(cpf) || todosIguais(cpf)) return false

  const digitos = [...cpf].map(Number)
  const primeiroDv = calcularDv(digitos, [10, 9, 8, 7, 6, 5, 4, 3, 2])
  const segundoDv = calcularDv(digitos, [11, 10, 9, 8, 7, 6, 5, 4, 3, 2])

  return digitos[9] === primeiroDv && digitos[10] === segundoDv
}

function cnpjValido(cnpj: string): boolean {
  if (!/^[0-9A-Z]{12}\d{2}$/.test(cnpj) || todosIguais(cnpj)) return false

  const valores = [...cnpj].map(valorCaractere)
  const primeiroDv = calcularDv(valores, PESOS_CNPJ_PRIMEIRO_DV)
  const segundoDv = calcularDv(valores, PESOS_CNPJ_SEGUNDO_DV)

  return valores[12] === primeiroDv && valores[13] === segundoDv
}

export function documentoValido(documento: string): boolean {
  const normalizado = normalizarDocumento(documento)
  if (normalizado.length === 11) return cpfValido(normalizado)
  if (normalizado.length === 14) return cnpjValido(normalizado)
  return false
}

/** Aplica a máscara de CPF (000.000.000-00) ou CNPJ (00.000.000/0000-00). */
export function formatarDocumento(documento: string): string {
  const normalizado = normalizarDocumento(documento)

  if (normalizado.length === 11) {
    return normalizado.replace(/^(.{3})(.{3})(.{3})(.{2})$/, '$1.$2.$3-$4')
  }
  if (normalizado.length === 14) {
    return normalizado.replace(/^(.{2})(.{3})(.{3})(.{4})(.{2})$/, '$1.$2.$3/$4-$5')
  }
  return documento
}
