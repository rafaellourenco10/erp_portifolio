/**
 * =====================================================================
 * Arquivo....: vite.config.ts
 * Versão.....: 1.1.0
 * Data.......: 18/09/2026
 * Descrição..: Configuração do Vite. A porta 5180 é fixa porque é a
 *              origem liberada no CORS da API (appsettings.Development.json).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 21/10/2026 - Porta 5173 -> 5180 (a 5173 é usada por outro projeto).
 * =====================================================================
 */

import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5180,
    strictPort: true,
  },
})
