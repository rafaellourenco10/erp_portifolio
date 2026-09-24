/**
 * =====================================================================
 * Arquivo....: DevolucoesPedido.tsx
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Seção "Devoluções" da página do pedido (etapa 14): histórico com data,
 *              itens (e se voltaram ao estoque), motivo, valor, quanto foi abatido das
 *              parcelas, reembolso e estorno de comissão. Só aparece se houver devolução.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/pedidos/{id}/devolucoes (via useDevolucoes)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { Table, type TableColumnsType } from 'antd'
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

export function DevolucoesPedido({ pedidoId }: { pedidoId: number }) {
  const { data: devolucoes } = useDevolucoes(pedidoId)
  if (!devolucoes || devolucoes.length === 0) return null

  return (
    <section className="painel pedido-secao" aria-label="Devoluções do pedido">
      <h2 className="pedido-secao-titulo">Devoluções ({devolucoes.length})</h2>
      <Table<Devolucao> rowKey="id" size="small" columns={colunas} dataSource={devolucoes} pagination={false} />
    </section>
  )
}
