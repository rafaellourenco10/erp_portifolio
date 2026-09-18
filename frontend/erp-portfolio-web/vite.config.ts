/**
 * =====================================================================
 * Arquivo....: vite.config.ts
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Configuração do Vite. A porta 5173 é fixa porque é a
 *              origem liberada no CORS da API (appsettings.Development.json).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
  },
})
