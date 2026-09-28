/**
 * =====================================================================
 * Arquivo....: DevolucoesPedido.tsx
 * Versão.....: 1.1.0
 * Data.......: 24/09/2026
 * Descrição..: Seção "Devoluções" da página do pedido (etapa 14): histórico com data,
 *              itens (e se voltaram ao estoque), motivo, valor, quanto foi abatido das
 *              parcelas, reembolso e estorno de comissão. Só aparece se houver devolução.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/pedidos/{id}/devolucoes (via useDevolucoes)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 *   1.1.0 - 28/09/2026 - Coluna NF-e: emitir a nota de devolução ou baixar XML/DANFE (etapa 16).
 * =====================================================================
 */

import { FileProtectOutlined } from '@ant-design/icons'
import { Button, Flex, Table, type TableColumnsType } from 'antd'
import { BotoesArquivosNfe } from '../../components/NotaFiscalAcoes'
import { useEmitirNfeComAviso } from '../../hooks/useEmitirNfeComAviso'
import { useDevolucoes } from '../../hooks/useDevolucoes'
import type { Devolucao } from '../../types/devolucao'
import { formatarQuantidade, formatarReal } from '../../utils/moeda'

const formatoData = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

const valor = (numero: number) => <span className="numeros-tabulares">{formatarReal(numero)}</span>

const colunas: TableColumnsType<Devolucao> = [
  { title: 'Nº', dataIndex: 'id', width: 70, render: (id: number) => <span className="pilula-documento numeros-tabulares">#{id}</span> },
  {
    title: 'Data',
    dataIndex: 'dataDevolucao',
    width: 130,
    responsive: ['md'],
    render: (data: string) => <span className="numeros-tabulares">{formatoData.format(new Date(data))}</span>,
  },
  {
    title: 'Itens',
    key: 'itens',
    render: (_, devolucao) => (
      <div className="celula-compacta">
        {devolucao.itens.map((item) => (
          <span key={item.id}>
            {formatarQuantidade(item.quantidade)} {item.unidade} · {item.produtoNome}
            {!item.voltaEstoque && <span className="texto-discreto"> (perda, não voltou ao estoque)</span>}
          </span>
        ))}
        {devolucao.motivo && <span className="texto-discreto">Motivo: {devolucao.motivo}</span>}
      </div>
    ),
  },
  { title: 'Valor', dataIndex: 'valorTotal', width: 120, align: 'right', render: valor },
  { title: 'Abatido', dataIndex: 'valorAbatido', width: 120, align: 'right', responsive: ['lg'], render: valor },
  { title: 'Reembolso', dataIndex: 'valorReembolso', width: 120, align: 'right', responsive: ['lg'], render: valor },
  {
    title: 'Estorno comissão',
    dataIndex: 'estornoComissao',
    width: 140,
    align: 'right',
    responsive: ['xl'],
    render: (estorno: number) => (estorno < 0 ? valor(estorno) : <span className="texto-discreto">—</span>),
  },
]

/** pedidoTemNfe: só dá para emitir a nota de devolução se a venda tem NF-e de saída para referenciar. */
export function DevolucoesPedido({ pedidoId, pedidoTemNfe }: { pedidoId: number; pedidoTemNfe: boolean }) {
  const { data: devolucoes } = useDevolucoes(pedidoId)
  const { executar, emitindo } = useEmitirNfeComAviso()
  if (!devolucoes || devolucoes.length === 0) return null

  const colunaNfe: TableColumnsType<Devolucao>[number] = {
    title: 'NF-e',
    key: 'nfe',
    width: 190,
    render: (_, devolucao) =>
      devolucao.notaFiscalId !== null ? (
        <Flex vertical gap={4}>
          <span className="numeros-tabulares">nº {devolucao.numeroNfe}</span>
          <BotoesArquivosNfe notaId={devolucao.notaFiscalId} pequeno />
        </Flex>
      ) : pedidoTemNfe ? (
        <Button
          size="small"
          icon={<FileProtectOutlined />}
          loading={emitindo?.origem === 'devolucao' && emitindo.id === devolucao.id}
          onClick={() => executar('devolucao', devolucao.id)}
        >
          Emitir NF-e
        </Button>
      ) : (
        <span className="texto-discreto">Venda sem NF-e</span>
      ),
  }

  return (
    <section className="painel pedido-secao" aria-label="Devoluções do pedido">
      <h2 className="pedido-secao-titulo">Devoluções ({devolucoes.length})</h2>
      <Table<Devolucao> rowKey="id" size="small" columns={[...colunas, colunaNfe]} dataSource={devolucoes} pagination={false} />
    </section>
  )
}
