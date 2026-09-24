/**
 * =====================================================================
 * Arquivo....: BotoesExportar.tsx
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Botões "Excel" e "PDF" das telas de lista (Comissões, Contas a
 *              Receber e a Pagar): carregando só no botão clicado e erro da
 *              API em mensagem. Quem usa passa a chamada que baixa o arquivo.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { FileExcelOutlined, FilePdfOutlined } from '@ant-design/icons'
import { useMutation } from '@tanstack/react-query'
import { App, Button } from 'antd'
import { lerErroApi } from '../api/axiosClient'
import type { FormatoArquivo } from '../types/relatorio'

interface BotoesExportarProps {
  /** Baixa o arquivo no formato pedido (ex.: comissoesApi.exportar com o filtro da tela). */
  baixar: (formato: FormatoArquivo) => Promise<void>
  desativado?: boolean
}

export function BotoesExportar({ baixar, desativado }: BotoesExportarProps) {
  const { message } = App.useApp()
  const exportar = useMutation({ mutationFn: baixar, onError: (erro) => message.error(lerErroApi(erro).mensagem) })
  const carregando = (formato: FormatoArquivo) => exportar.isPending && exportar.variables === formato

  return (
    <>
      <Button size="large" icon={<FileExcelOutlined />} disabled={desativado} loading={carregando('xlsx')} onClick={() => exportar.mutate('xlsx')}>
        Excel
      </Button>
      <Button size="large" icon={<FilePdfOutlined />} disabled={desativado} loading={carregando('pdf')} onClick={() => exportar.mutate('pdf')}>
        PDF
      </Button>
    </>
  )
}
