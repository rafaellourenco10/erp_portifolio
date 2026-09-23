/**
 * =====================================================================
 * Arquivo....: RelatorioPedidosPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Relatório de Vendas ou de Compras (mesma tela, muda só o rótulo
 *              e o seletor de cliente/fornecedor). Filtros: período (padrão: mês
 *              atual até hoje), status (padrão Confirmado) e cliente/fornecedor.
 *              "Gerar" consulta a API; os cards e a tabela mostram o que o
 *              servidor calculou. Exportar baixa .xlsx/.pdf com os filtros do
 *              último relatório gerado (mesmos números da tela, SPEC.md R3/R4).
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/relatorios/vendas | compras (?formato=xlsx|pdf)
 *              (via useRelatorioPedidos / useBaixarRelatorio)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { FileExcelOutlined, FilePdfOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, App, Button, Col, DatePicker, Flex, Row, Select, Table, Typography } from 'antd'
import type { TableProps } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { SelecaoCliente } from '../../components/SelecaoCliente'
import { SelecaoFornecedor } from '../../components/SelecaoFornecedor'
import { TagStatusPedido } from '../../components/TagStatusPedido'
import { useBaixarRelatorio, useRelatorioPedidos } from '../../hooks/useRelatorios'
import type { StatusPedido } from '../../types/pedido'
import type { FormatoArquivo, RelatorioPedidoLinha, RelatorioPedidosFiltro } from '../../types/relatorio'
import { formatarReal } from '../../utils/moeda'
import { CardIndicador } from '../Dashboard/CardIndicador'
// A tela reaproveita as classes .painel, .pagina-titulo etc. do módulo de Clientes.
import '../Clientes/clientes.css'

const formatoDataHora = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

const opcoesStatus: { label: string; value: StatusPedido | 'Todos' }[] = [
  { label: 'Confirmados', value: 'Confirmado' },
  { label: 'Rascunhos', value: 'Rascunho' },
  { label: 'Cancelados', value: 'Cancelado' },
  { label: 'Todos', value: 'Todos' },
]

interface RelatorioPedidosPageProps {
  tipo: 'vendas' | 'compras'
}

export function RelatorioPedidosPage({ tipo }: RelatorioPedidosPageProps) {
  const { message } = App.useApp()
  const ehVenda = tipo === 'vendas'
  const rotuloParceiro = ehVenda ? 'Cliente' : 'Fornecedor'

  // Campos da tela; só viram consulta ao clicar em "Gerar".
  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>([dayjs().startOf('month'), dayjs()])
  const [status, setStatus] = useState<StatusPedido | 'Todos'>('Confirmado')
  const [parceiroId, setParceiroId] = useState<number | null>(null)

  const [filtro, setFiltro] = useState<RelatorioPedidosFiltro | null>(null)
  const { data, isFetching, isError, error } = useRelatorioPedidos(tipo, filtro)
  const baixar = useBaixarRelatorio()

  function gerar() {
    if (!periodo) {
      message.warning('Escolha o período.')
      return
    }
    setFiltro({
      dataInicio: periodo[0].format('YYYY-MM-DD'),
      dataFim: periodo[1].format('YYYY-MM-DD'),
      status: status === 'Todos' ? undefined : status,
      ...(parceiroId === null ? {} : ehVenda ? { clienteId: parceiroId } : { fornecedorId: parceiroId }),
    })
  }

  async function exportar(formato: FormatoArquivo) {
    if (!filtro) return
    try {
      await baixar.mutateAsync({ tipo, filtro, formato })
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const colunas: TableProps<RelatorioPedidoLinha>['columns'] = [
    {
      title: 'Nº',
      dataIndex: 'id',
      width: 80,
      render: (id: number) => <span className="numeros-tabulares">#{id}</span>,
    },
    {
      title: 'Data',
      dataIndex: 'dataPedido',
      width: 150,
      render: (data: string) => <span className="numeros-tabulares">{formatoDataHora.format(new Date(data))}</span>,
    },
    {
      title: rotuloParceiro,
      dataIndex: 'nome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Itens',
      dataIndex: 'quantidadeItens',
      width: 80,
      align: 'right',
      render: (itens: number) => <span className="numeros-tabulares">{itens}</span>,
    },
    {
      title: 'Total',
      dataIndex: 'valorTotal',
      width: 140,
      align: 'right',
      render: (valor: number) => <span className="numeros-tabulares">{formatarReal(valor)}</span>,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      width: 130,
      align: 'center',
      render: (valor: StatusPedido) => <TagStatusPedido status={valor} />,
    },
  ]

  const exportando = (formato: FormatoArquivo) => baixar.isPending && baixar.variables?.formato === formato

  return (
    <div className="pagina-relatorio">
      <div className="pagina-cabecalho">
        <h1 className="pagina-titulo">Relatório de {ehVenda ? 'Vendas' : 'Compras'}</h1>
        <p className="pagina-subtitulo">
          {ehVenda ? 'Pedidos de venda' : 'Pedidos de compra'} do período, com totais. Exporte para Excel ou PDF.
        </p>
      </div>

      <section className="painel" style={{ padding: 16, marginBottom: 16 }} aria-label="Filtros do relatório">
        <Flex gap={12} wrap align="flex-end">
          <Flex vertical gap={4}>
            <span className="rotulo-filtro">Período</span>
            <DatePicker.RangePicker
              size="large"
              format="DD/MM/YYYY"
              value={periodo}
              onChange={(datas) => setPeriodo(datas?.[0] && datas[1] ? [datas[0], datas[1]] : null)}
              allowClear={false}
              aria-label="Período"
            />
          </Flex>
          <Flex vertical gap={4} style={{ minWidth: 160 }}>
            <span className="rotulo-filtro">Status</span>
            <Select size="large" options={opcoesStatus} value={status} onChange={setStatus} aria-label="Status" />
          </Flex>
          <Flex vertical gap={4} style={{ minWidth: 260, flex: 1 }}>
            <span className="rotulo-filtro">{rotuloParceiro}</span>
            {ehVenda ? (
              <SelecaoCliente
                value={parceiroId}
                onChange={(id) => setParceiroId(id)}
                aoLimpar={() => setParceiroId(null)}
                placeholder="Todos os clientes"
                aria-label="Cliente"
              />
            ) : (
              <SelecaoFornecedor
                value={parceiroId}
                onChange={(id) => setParceiroId(id)}
                aoLimpar={() => setParceiroId(null)}
                placeholder="Todos os fornecedores"
                aria-label="Fornecedor"
              />
            )}
          </Flex>
          <Button type="primary" size="large" icon={<SearchOutlined />} onClick={gerar} loading={isFetching}>
            Gerar
          </Button>
          <Button
            size="large"
            icon={<FileExcelOutlined />}
            disabled={!data}
            loading={exportando('xlsx')}
            onClick={() => exportar('xlsx')}
          >
            Excel
          </Button>
          <Button
            size="large"
            icon={<FilePdfOutlined />}
            disabled={!data}
            loading={exportando('pdf')}
            onClick={() => exportar('pdf')}
          >
            PDF
          </Button>
        </Flex>
      </section>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível gerar o relatório."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      {filtro === null ? (
        <Typography.Text type="secondary">Escolha os filtros e clique em Gerar.</Typography.Text>
      ) : (
        <>
          <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
            <Col xs={24} sm={8}>
              <CardIndicador titulo="Pedidos" loading={isFetching && !data} erro={isError}>
                <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
                  {data?.quantidadePedidos}
                </Typography.Title>
              </CardIndicador>
            </Col>
            <Col xs={24} sm={8}>
              <CardIndicador titulo="Valor total" loading={isFetching && !data} erro={isError}>
                <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
                  {data && formatarReal(data.valorTotal)}
                </Typography.Title>
              </CardIndicador>
            </Col>
            <Col xs={24} sm={8}>
              <CardIndicador titulo="Ticket médio" loading={isFetching && !data} erro={isError}>
                <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
                  {data && formatarReal(data.ticketMedio)}
                </Typography.Title>
              </CardIndicador>
            </Col>
          </Row>

          <section className="painel painel-tabela" aria-label="Pedidos do relatório">
            <Table<RelatorioPedidoLinha>
              rowKey="id"
              columns={colunas}
              dataSource={data?.linhas}
              loading={isFetching}
              locale={{ emptyText: 'Nenhum pedido para os filtros informados.' }}
              scroll={{ x: 700 }}
              pagination={{ defaultPageSize: 20, showSizeChanger: true, pageSizeOptions: [20, 50, 100], hideOnSinglePage: true }}
            />
          </section>
        </>
      )}
    </div>
  )
}
