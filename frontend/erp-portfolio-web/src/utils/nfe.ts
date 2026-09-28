/**
 * =====================================================================
 * Arquivo....: nfe.ts
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Formatação da chave de acesso da NF-e (etapa 16).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

/** Chave de 44 dígitos em grupos de 4, como no DANFE. */
export const formatarChave = (chave: string) => chave.replace(/(.{4})(?=.)/g, '$1 ')
