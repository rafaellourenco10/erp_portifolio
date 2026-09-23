/**
 * =====================================================================
 * Arquivo....: ModalParcelas.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Modal de confirmar pedido com número de parcelas (1-12) e
 *              intervalo em dias (1-180), padrão 1 parcela em 30 dias.
 *              Extraído do PedidoPage para ser usado também pelo pedido de
 *              compra (contas a receber e contas a pagar usam o mesmo corpo).
 *              Os campos voltam ao padrão toda vez que o modal fecha.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo (extraído do PedidoPage), etapa 8.
 * =====================================================================
 */

import { Flex, InputNumber, Modal } from 'antd'
import { useState } from 'react'
import type { PedidoConfirmarEntrada } from '../types/pedido'
import { ItemFormulario } from './ItemFormulario'

interface ModalParcelasProps {
  aberto: boolean
  titulo: string
  descricao: string
  carregando: boolean
  aoConfirmar: (dados: PedidoConfirmarEntrada) => void
  aoFechar: () => void
}

export function ModalParcelas({ aberto, titulo, descricao, carregando, aoConfirmar, aoFechar }: ModalParcelasProps) {
  const [numeroParcelas, setNumeroParcelas] = useState(1)
  const [intervaloDias, setIntervaloDias] = useState(30)

  return (
    <Modal
      open={aberto}
      title={titulo}
      okText="Confirmar pedido"
      cancelText="Voltar"
      onOk={() => aoConfirmar({ numeroParcelas, intervaloDias })}
      onCancel={aoFechar}
      afterClose={() => {
        setNumeroParcelas(1)
        setIntervaloDias(30)
      }}
      confirmLoading={carregando}
    >
      <p>{descricao}</p>
      <Flex gap={16} wrap>
        <ItemFormulario rotulo="Número de parcelas" obrigatorio>
          <InputNumber
            className="campo-cheio numeros-tabulares"
            aria-label="Número de parcelas"
            min={1}
            max={12}
            precision={0}
            controls={false}
            value={numeroParcelas}
            onChange={(valor) => setNumeroParcelas(valor ?? 1)}
          />
        </ItemFormulario>
        <ItemFormulario rotulo="Intervalo entre parcelas (dias)" obrigatorio>
          <InputNumber
            className="campo-cheio numeros-tabulares"
            aria-label="Intervalo entre parcelas em dias"
            min={1}
            max={180}
            precision={0}
            controls={false}
            value={intervaloDias}
            onChange={(valor) => setIntervaloDias(valor ?? 30)}
          />
        </ItemFormulario>
      </Flex>
    </Modal>
  )
}
