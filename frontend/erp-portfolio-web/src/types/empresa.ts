/**
 * =====================================================================
 * Arquivo....: empresa.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Dados da empresa emitente da NF-e (EmpresaDto da API, etapa 16).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

export interface Empresa {
  razaoSocial: string
  nomeFantasia: string | null
  /** Sem máscara. */
  cnpj: string
  inscricaoEstadual: string
  logradouro: string
  numero: string
  complemento: string | null
  bairro: string
  /** 8 dígitos. */
  cep: string
  municipio: string
  /** Código IBGE (7 dígitos). */
  codigoMunicipio: string
  uf: string
  telefone: string | null
  /** 0 a 999. */
  serieNfe: number
}
