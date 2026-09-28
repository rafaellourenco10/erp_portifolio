/**
 * =====================================================================
 * Arquivo....: NotaFiscalPedido.tsx
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Seção "Nota fiscal" do pedido confirmado (etapa 16): botão
 *              Emitir NF-e ou, já emitida, número, chave e XML/DANFE.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/pedidos/{id}/nfe (useEmitirNfeComAviso)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { FileProtectOutlined } from '@ant-design/icons'
import { Button, Flex } from 'antd'
import { Link } from 'react-router-dom'
import { BotoesArquivosNfe } from '../../components/NotaFiscalAcoes'
import { useEmitirNfeComAviso } from '../../hooks/useEmitirNfeComAviso'
import type { Pedido } from '../../types/pedido'
import { formatarChave } from '../../utils/nfe'
import '../../components/TagStatus.css'

export function NotaFiscalPedido({ pedido }: { pedido: Pedido }) {
  const { executar, emitindo } = useEmitirNfeComAviso()

  return (
    <section className="painel pedido-secao" aria-label="Nota fiscal do pedido">
      <h2 className="pedido-secao-titulo">Nota fiscal</h2>
      {pedido.notaFiscalId === null ? (
        <Flex justify="space-between" align="center" wrap gap={12}>
          <span className="texto-discreto">
            Pedido sem NF-e. A nota é simulada (homologação, sem valor fiscal) e, depois de emitida, o pedido só pode ser
            devolvido, não cancelado.
          </span>
          <Button
            type="primary"
            size="large"
            icon={<FileProtectOutlined />}
            loading={emitindo?.origem === 'pedido'}
            onClick={() => executar('pedido', pedido.id)}
          >
            Emitir NF-e
          </Button>
        </Flex>
      ) : (
        <Flex justify="space-between" align="center" wrap gap={12}>
          <div>
            <div>
              <Link to={`/notas-fiscais?nota=${pedido.notaFiscalId}`}>
                <strong>NF-e nº {pedido.numeroNfe}</strong>
              </Link>{' '}
              <span className="tag-status tag-status-confirmado">
                <span className="tag-status-ponto" />
                Autorizada
              </span>
            </div>
            <div className="texto-discreto numeros-tabulares" style={{ fontSize: 13 }}>
              Chave {formatarChave(pedido.chaveNfe ?? '')}
            </div>
          </div>
          <BotoesArquivosNfe notaId={pedido.notaFiscalId} />
        </Flex>
      )}
    </section>
  )
}
