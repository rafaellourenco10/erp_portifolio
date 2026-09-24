/**
 * =====================================================================
 * Arquivo....: DevolucaoModal.tsx
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Modal "Registrar devolução" do pedido confirmado (etapa 14): para cada
 *              item ainda não devolvido, a quantidade a devolver (até o disponível, inteira
 *              em UN/CX) e se volta ao estoque; motivo e vencimento do reembolso. Mostra
 *              a prévia do valor com a regra do servidor (na devolução que zera o pedido,
 *              vale o que falta). Vale sempre o valor que o servidor devolve.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/pedidos/{id}/devolucoes (via useRegistrarDevolucao)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { App, Alert, Checkbox, DatePicker, Flex, Input, InputNumber, Modal, Table, type TableColumnsType } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import { useMemo, useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { useRegistrarDevolucao } from '../../hooks/useDevolucoes'
import type { Pedido, PedidoItem } from '../../types/pedido'
import { valorDevolucaoCentavos } from '../../utils/calculoPedido'
import { formatarQuantidade, formatarReal } from '../../utils/moeda'

interface DevolucaoModalProps {
  pedido: Pedido
  aberto: boolean
  aoFechar: () => void
}

interface Escolha {
  quantidade: number
  voltaEstoque: boolean
}

const unidadeInteira = (unidade: string) => unidade === 'UN' || unidade === 'CX'
const disponivel = (item: PedidoItem) => item.quantidade - item.quantidadeDevolvida

export function DevolucaoModal({ pedido, aberto, aoFechar }: DevolucaoModalProps) {
  const { message } = App.useApp()
  const registrar = useRegistrarDevolucao()
  const [escolhas, setEscolhas] = useState<Record<number, Escolha>>({})
  const [motivo, setMotivo] = useState('')
  const [vencimento, setVencimento] = useState<Dayjs>(dayjs())

  const itens = pedido.itens.filter((item) => disponivel(item) > 0)
  const escolha = (item: PedidoItem): Escolha => escolhas[item.id] ?? { quantidade: 0, voltaEstoque: true }
  const alterar = (item: PedidoItem, mudanca: Partial<Escolha>) =>
    setEscolhas((atuais) => ({ ...atuais, [item.id]: { ...escolha(item), ...mudanca } }))

  // Prévia com a regra do servidor: soma dos itens; se zera o pedido, o que falta; nunca mais do que falta.
  const previa = useMemo(() => {
    const restante = Math.round((pedido.valorTotal - pedido.valorDevolvido) * 100)
    const soma = pedido.itens.reduce(
      (total, item) => total + Number(valorDevolucaoCentavos({ ...item, quantidade: escolhas[item.id]?.quantidade ?? 0 }, pedido.descontoPercentual)),
      0,
    )
    const devolveTudo = pedido.itens.every((item) => (escolhas[item.id]?.quantidade ?? 0) === disponivel(item))
    return (devolveTudo ? restante : Math.min(soma, restante)) / 100
  }, [escolhas, pedido])

  async function confirmar() {
    const selecionados = itens
      .map((item) => ({ pedidoItemId: item.id, ...escolha(item) }))
      .filter((item) => item.quantidade > 0)
    if (selecionados.length === 0) {
      message.warning('Informe a quantidade de pelo menos um item.')
      return
    }

    try {
      const devolucao = await registrar.mutateAsync({
        pedidoId: pedido.id,
        dados: { itens: selecionados, motivo: motivo.trim() || null, vencimentoReembolso: vencimento.format('YYYY-MM-DD') },
      })
      const partes = [`${formatarReal(devolucao.valorAbatido)} abatidos das parcelas`]
      if (devolucao.valorReembolso > 0) partes.push(`${formatarReal(devolucao.valorReembolso)} de reembolso em Contas a Pagar`)
      if (devolucao.estornoComissao < 0) partes.push(`estorno de comissão de ${formatarReal(-devolucao.estornoComissao)}`)
      message.success(`Devolução nº ${devolucao.id} registrada: ${partes.join('; ')}.`, 6)
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)
      message.error(Object.values(erroApi.errosPorCampo)[0] ?? erroApi.mensagem)
    }
  }

  const colunas: TableColumnsType<PedidoItem> = [
    { title: 'Produto', dataIndex: 'produtoNome', ellipsis: true },
    {
      title: 'Vendido',
      key: 'vendido',
      width: 100,
      align: 'right',
      render: (_, item) => (
        <span className="numeros-tabulares">
          {formatarQuantidade(item.quantidade)} {item.unidade}
          {item.quantidadeDevolvida > 0 && (
            <div className="texto-discreto">já devolvido {formatarQuantidade(item.quantidadeDevolvida)}</div>
          )}
        </span>
      ),
    },
    {
      title: 'Devolver',
      key: 'devolver',
      width: 150,
      render: (_, item) => (
        <InputNumber
          className="campo-cheio numeros-tabulares"
          aria-label={`Quantidade a devolver de ${item.produtoNome}`}
          min={0}
          max={disponivel(item)}
          precision={unidadeInteira(item.unidade) ? 0 : 3}
          decimalSeparator=","
          suffix={`/ ${formatarQuantidade(disponivel(item))}`}
          value={escolha(item).quantidade}
          onChange={(valor) => alterar(item, { quantidade: valor ?? 0 })}
        />
      ),
    },
    {
      title: 'Volta ao estoque',
      key: 'volta',
      width: 130,
      align: 'center',
      render: (_, item) => (
        <Checkbox
          aria-label={`${item.produtoNome} volta ao estoque`}
          checked={escolha(item).voltaEstoque}
          onChange={(e) => alterar(item, { voltaEstoque: e.target.checked })}
        />
      ),
    },
  ]

  return (
    <Modal
      open={aberto}
      title={`Registrar devolução do pedido nº ${pedido.id}`}
      okText="Registrar devolução"
      okButtonProps={{ danger: true }}
      cancelText="Voltar"
      width={760}
      confirmLoading={registrar.isPending}
      onOk={confirmar}
      onCancel={aoFechar}
      afterClose={() => {
        setEscolhas({})
        setMotivo('')
        setVencimento(dayjs())
      }}
    >
      <Alert
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
        title="O valor devolvido abate primeiro as parcelas ainda não pagas; o que o cliente já pagou vira um reembolso em Contas a Pagar. A devolução é definitiva."
      />
      <Table<PedidoItem>
        rowKey="id"
        size="small"
        columns={colunas}
        dataSource={itens}
        pagination={false}
        scroll={{ x: 560 }}
      />
      <Flex gap={16} wrap style={{ marginTop: 16 }}>
        <div style={{ flex: '2 1 280px' }}>
          <ItemFormulario rotulo="Motivo (opcional)">
            <Input.TextArea
              aria-label="Motivo da devolução"
              rows={2}
              maxLength={200}
              showCount
              placeholder="Ex.: produto com defeito, cliente desistiu"
              value={motivo}
              onChange={(e) => setMotivo(e.target.value)}
            />
          </ItemFormulario>
        </div>
        <div style={{ flex: '1 1 180px' }}>
          <ItemFormulario rotulo="Vencimento do reembolso">
            <DatePicker
              className="campo-cheio"
              aria-label="Vencimento do reembolso"
              format="DD/MM/YYYY"
              allowClear={false}
              disabledDate={(dia) => dia.isBefore(dayjs(), 'day')}
              value={vencimento}
              onChange={(valor) => valor && setVencimento(valor)}
            />
          </ItemFormulario>
        </div>
      </Flex>
      <Flex justify="space-between" align="center" className="resumo-linha resumo-total">
        <span>Valor da devolução</span>
        <span className="numeros-tabulares valor-total" data-testid="previa-devolucao">
          {formatarReal(previa)}
        </span>
      </Flex>
    </Modal>
  )
}
