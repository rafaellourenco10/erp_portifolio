/**
 * =====================================================================
 * Arquivo....: main.tsx
 * Versão.....: 1.2.0
 * Data.......: 21/09/2026
 * Descrição..: Ponto de entrada do front-end. Registra os providers do
 *              TanStack Query, do Ant Design (idioma pt-BR e tema
 *              Ambition ERP) e do React Router, e carrega a fonte Inter.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Tema escuro Ambition ERP e fonte Inter.
 *   1.2.0 - 21/09/2026 - BrowserRouter (navegação por rotas).
 * =====================================================================
 */

import '@fontsource-variable/inter'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { App as AntApp, ConfigProvider } from 'antd'
import ptBR from 'antd/locale/pt_BR'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
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
          <BrowserRouter>
            <App />
          </BrowserRouter>
        </AntApp>
      </ConfigProvider>
    </QueryClientProvider>
  </StrictMode>,
)
