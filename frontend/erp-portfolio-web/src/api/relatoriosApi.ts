/**
 * =====================================================================
 * Arquivo....: relatoriosApi.ts
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Chamadas HTTP do módulo de Relatórios: os dados (JSON) para a
 *              tela e o download do arquivo (.xlsx/.pdf) gerado pelo servidor
 *              com os mesmos filtros. O nome do arquivo vem do cabeçalho
 *              Content-Disposition (exposto pelo CORS da API).
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (RelatoriosController)
 *                GET /relatorios/vendas?dataInicio=&dataFim=&status=&clienteId=&formato=
 *                GET /relatorios/compras?dataInicio=&dataFim=&status=&fornecedorId=&formato=
 *                GET /relatorios/estoque?categoriaId=&somenteAbaixoMinimo=&formato=
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 24/09/2026 - baixarArquivo exportado (usado também pela tela de Comissões).
 * =====================================================================
 */

import axios from 'axios'
import type {
  FormatoArquivo,
  RelatorioEstoque,
  RelatorioEstoqueFiltro,
  RelatorioPedidos,
  RelatorioPedidosFiltro,
} from '../types/relatorio'
import { axiosClient } from './axiosClient'

export type TipoRelatorio = 'vendas' | 'compras' | 'estoque'

export const relatoriosApi = {
  async pedidos(tipo: 'vendas' | 'compras', filtro: RelatorioPedidosFiltro): Promise<RelatorioPedidos> {
    const resposta = await axiosClient.get<RelatorioPedidos>(`/relatorios/${tipo}`, { params: filtro })
    return resposta.data
  },

  async estoque(filtro: RelatorioEstoqueFiltro): Promise<RelatorioEstoque> {
    const resposta = await axiosClient.get<RelatorioEstoque>('/relatorios/estoque', { params: filtro })
    return resposta.data
  },

  /** Baixa o relatório no formato pedido e dispara o download no navegador. */
  baixar(tipo: TipoRelatorio, filtro: RelatorioPedidosFiltro | RelatorioEstoqueFiltro, formato: FormatoArquivo) {
    return baixarArquivo(`/relatorios/${tipo}`, { ...filtro, formato }, `relatorio-${tipo}.${formato}`)
  },
}

/** GET de um arquivo gerado pela API e download no navegador, com o nome do Content-Disposition. */
export async function baixarArquivo(url: string, params: object, nomePadrao: string) {
  try {
    const resposta = await axiosClient.get<Blob>(url, { params, responseType: 'blob' })
    const nome = /filename="?([^";]+)"?/.exec(resposta.headers['content-disposition'] ?? '')?.[1] ?? nomePadrao

    const link = document.createElement('a')
    link.href = URL.createObjectURL(resposta.data)
    link.download = nome
    link.click()
    URL.revokeObjectURL(link.href)
  } catch (erro) {
    // Com responseType blob, o ProblemDetails de erro também chega como Blob: converte para JSON
    // para o lerErroApi mostrar a mensagem certa.
    if (axios.isAxiosError(erro) && erro.response?.data instanceof Blob) {
      try {
        erro.response.data = JSON.parse(await erro.response.data.text())
      } catch {
        // Corpo não é JSON: fica a mensagem genérica.
      }
    }
    throw erro
  }
}
