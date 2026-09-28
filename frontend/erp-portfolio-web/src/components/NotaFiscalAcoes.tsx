/**
 * =====================================================================
 * Arquivo....: NotaFiscalAcoes.tsx
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Botões de baixar o XML e o DANFE de uma NF-e simulada
 *              (etapa 16), usados no pedido, nas devoluções e na lista de notas.
 * ---------------------------------------------------------------------
 * Fontes.....: notasFiscaisApi.baixarXml / baixarDanfe
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { CodeOutlined, FilePdfOutlined } from '@ant-design/icons'
import { useMutation } from '@tanstack/react-query'
import { App, Button, Flex } from 'antd'
import { lerErroApi } from '../api/axiosClient'
import { notasFiscaisApi } from '../api/notasFiscaisApi'

export function BotoesArquivosNfe({ notaId, pequeno = false }: { notaId: number; pequeno?: boolean }) {
  const { message } = App.useApp()
  const baixar = useMutation({
    mutationFn: (tipo: 'xml' | 'danfe') => (tipo === 'xml' ? notasFiscaisApi.baixarXml(notaId) : notasFiscaisApi.baixarDanfe(notaId)),
    onError: (erro) => message.error(lerErroApi(erro).mensagem),
  })
  const carregando = (tipo: 'xml' | 'danfe') => baixar.isPending && baixar.variables === tipo
  const tamanho = pequeno ? 'small' : 'middle'

  return (
    <Flex gap={8} wrap>
      <Button size={tamanho} icon={<CodeOutlined />} loading={carregando('xml')} onClick={() => baixar.mutate('xml')}>
        XML
      </Button>
      <Button size={tamanho} icon={<FilePdfOutlined />} loading={carregando('danfe')} onClick={() => baixar.mutate('danfe')}>
        DANFE
      </Button>
    </Flex>
  )
}
