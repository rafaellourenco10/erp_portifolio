/**
 * =====================================================================
 * Arquivo....: axiosClient.ts
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Instância do Axios apontando para a API (VITE_API_URL) e
 *              leitura dos erros no formato ProblemDetails do ASP.NET Core.
 * ---------------------------------------------------------------------
 * Fontes.....: .env.development -> VITE_API_URL
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import axios from 'axios'

export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
})

interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

export interface ErroApi {
  status?: number
  mensagem: string
  /** Erros de validação por campo, com a chave em camelCase (ex.: "documento"). */
  errosPorCampo: Record<string, string>
}

export function lerErroApi(erro: unknown): ErroApi {
  if (!axios.isAxiosError<ProblemDetails>(erro)) {
    return { mensagem: 'Erro inesperado.', errosPorCampo: {} }
  }

  if (!erro.response) {
    return { mensagem: 'Não foi possível conectar à API. Verifique se ela está em execução.', errosPorCampo: {} }
  }

  const problema = erro.response.data
  const errosPorCampo: Record<string, string> = {}

  // O ASP.NET retorna as chaves com o nome da propriedade C# (ex.: "Documento" ou "$.documento").
  for (const [chave, mensagens] of Object.entries(problema?.errors ?? {})) {
    const nomeCampo = chave.split('.').pop() ?? chave
    const campo = nomeCampo.charAt(0).toLowerCase() + nomeCampo.slice(1)
    errosPorCampo[campo] = mensagens[0]
  }

  return {
    status: erro.response.status,
    mensagem: problema?.detail ?? problema?.title ?? `Erro ${erro.response.status} ao chamar a API.`,
    errosPorCampo,
  }
}
