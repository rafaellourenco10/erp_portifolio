/**
 * =====================================================================
 * Arquivo....: vite-env.d.ts
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Tipagem das variáveis de ambiente expostas pelo Vite.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

interface ImportMetaEnv {
  readonly VITE_API_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
