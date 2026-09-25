/**
 * =====================================================================
 * Arquivo....: fluxoCaixaApi.ts
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Chamadas HTTP do Fluxo de Caixa: os dados (JSON) para a tela e
 *              o arquivo (.xlsx/.pdf) gerado pelo servidor com o mesmo filtro.
 * ---------------------------------------------------------------------
 * Fontes.....: API Ambition ERP (FluxoCaixaController)
 *                GET /fluxo-caixa?dataInicio=&dataFim=&agrupamento=Dia|Mes&formato=
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { FluxoCaixa, FluxoCaixaFiltro } from '../types/fluxoCaixa'
import type { FormatoArquivo } from '../types/relatorio'
import { axiosClient } from './axiosClient'
import { baixarArquivo } from './relatoriosApi'

export const fluxoCaixaApi = {
  async obter(filtro: FluxoCaixaFiltro): Promise<FluxoCaixa> {
    const resposta = await axiosClient.get<FluxoCaixa>('/fluxo-caixa', { params: filtro })
    return resposta.data
  },

  exportar(filtro: FluxoCaixaFiltro, formato: FormatoArquivo) {
    return baixarArquivo('/fluxo-caixa', { ...filtro, formato }, `fluxo-caixa.${formato}`)
  },
}
