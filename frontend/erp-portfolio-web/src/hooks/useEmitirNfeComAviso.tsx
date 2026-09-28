/**
 * =====================================================================
 * Arquivo....: useEmitirNfeComAviso.tsx
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Emissão da NF-e simulada (etapa 16) com o aviso na tela:
 *              sucesso com o número; faltando dados (400), uma janela com a
 *              lista do que falta e onde completar.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { App } from 'antd'
import { lerErroApi } from '../api/axiosClient'
import { lerPendenciasNfe } from '../api/notasFiscaisApi'
import { useEmitirNfe } from './useNotasFiscais'

export function useEmitirNfeComAviso() {
  const { message, modal } = App.useApp()
  const emitir = useEmitirNfe()

  async function executar(origem: 'pedido' | 'devolucao', id: number) {
    try {
      const nota = await emitir.mutateAsync({ origem, id })
      message.success(`NF-e nº ${nota.numero} emitida e autorizada (simulação, sem valor fiscal).`)
    } catch (erro) {
      const pendencias = lerPendenciasNfe(erro)
      if (pendencias.length === 0) {
        message.error(lerErroApi(erro).mensagem)
        return
      }
      modal.warning({
        title: 'Faltam dados para emitir a NF-e',
        width: 520,
        content: (
          <>
            <ul style={{ paddingLeft: 20, margin: '8px 0' }}>
              {pendencias.map((p) => (
                <li key={p}>{p}</li>
              ))}
            </ul>
            {/* Texto, não <Link>: o modal do antd é renderizado fora do Router. */}
            <span className="texto-discreto">
              Complete em Fiscal → Empresa, no cadastro do cliente e no dos produtos, e tente de novo.
            </span>
          </>
        ),
      })
    }
  }

  return { executar, emitindo: emitir.isPending ? emitir.variables : undefined }
}
