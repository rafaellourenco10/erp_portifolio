/**
 * =====================================================================
 * Arquivo....: main.tsx
 * Versão.....: 1.1.0
 * Data.......: 18/09/2026
 * Descrição..: Ponto de entrada do front-end. Registra os providers do
 *              TanStack Query e do Ant Design (idioma pt-BR e tema
 *              Ambition ERP) e carrega a fonte Inter.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Tema escuro Ambition ERP e fonte Inter.
 * =====================================================================
 */

import '@fontsource-variable/inter'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { App as AntApp, ConfigProvider } from 'antd'
import ptBR from 'antd/locale/pt_BR'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { aplicarVariaveisCss, temaAmbition } from './tema/temaAmbition'

aplicarVariaveisCss()

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ConfigProvider locale={ptBR} theme={temaAmbition}>
        <AntApp>
          <App />
        </AntApp>
      </ConfigProvider>
    </QueryClientProvider>
  </StrictMode>,
)
