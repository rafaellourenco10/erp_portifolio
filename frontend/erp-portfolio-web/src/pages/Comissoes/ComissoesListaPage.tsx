/**
 * =====================================================================
 * Arquivo....: ComissoesListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de comissões dos vendedores (Financeiro): filtros por
 *              vendedor, status e período (data do recebimento da parcela),
 *              cards Gerado / A pagar / Pago (somas do filtro, calculadas no
 *              servidor), tabela paginada no servidor e "marcar como paga"
 *              por linha ou em lote (linhas pendentes selecionadas).
 * ---------------------------------------------------------------------
 * Fontes.....: GET  /api/comissoes?vendedorId=&status=&dataInicio=&dataFim=&pagina=&tamanhoPagina=
 *              POST /api/comissoes/pagar
 *              (via useListaComissoes / usePagarComissoes)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { Alert, App, Button, Col, DatePicker, Flex, Popconfirm, Row, Segmented, Table, Tooltip, Typography } from 'antd'
import type { TableProps } from 'antd'
import type { Dayjs } from 'dayjs'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { SelecaoVendedor } from '../../components/SelecaoVendedor'
import { useListaComissoes, usePagarComissoes } from '../../hooks/useComissoes'
import type { Comissao, ComissaoFiltro, StatusComissao } from '../../types/comissao'
import { formatarReal } from '../../utils/moeda'
import { CardIndicador } from '../Dashboard/CardIndicador'
// A tela reaproveita as classes .painel, .pagina-titulo etc. do módulo de Clientes e as .tag-status do TagStatus.css.
import '../Clientes/clientes.css'
import '../../components/TagStatus.css'

type StatusTela = 'Todas' | StatusComissao

const opcoesStatus: { label: string; value: StatusTela }[] = [
  { label: 'Todas', value: 'Todas' },
  { label: 'A pagar', value: 'Pendente' },
  { label: 'Pagas', value: 'Paga' },
]

const formatoData = new Intl.DateTimeFormat('pt-BR')
const formatoPercentual = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 })

function TagStatusComissao({ status }: { status: StatusComissao }) {
  return (
    <span className={`tag-status ${status === 'Paga' ? 'tag-status-confirmado' : 'tag-status-inativo'}`}>
      <span className="tag-status-ponto" />
      {status === 'Paga' ? 'Paga' : 'A pagar'}
    </span>
  )
}

export function ComissoesListaPage() {
  const { message } = App.useApp()
  const [filtro, setFiltro] = useState<ComissaoFiltro>({ pagina: 1, tamanhoPagina: 20 })
  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>(null)
  const [selecionadas, setSelecionadas] = useState<number[]>([])

  const { data, isFetching, isError, error } = useListaComissoes(filtro)
  const pagarComissoes = usePagarComissoes()

  // Mudou o filtro: volta para a página 1 e limpa a seleção (as linhas selecionadas podem sumir da tela).
  function filtrar(mudanca: Partial<ComissaoFiltro>) {
    setSelecionadas([])
    setFiltro((atual) => ({ ...atual, ...mudanca, pagina: 1 }))
  }

  async function pagar(ids: number[]) {
    try {
      await pagarComissoes.mutateAsync(ids)
      message.success(ids.length === 1 ? 'Comissão marcada como paga.' : `${ids.length} comissões marcadas como pagas.`)
      setSelecionadas([])
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const itens = data?.resultado.itens ?? []
  const totalSelecionado = itens.filter((c) => selecionadas.includes(c.id)).reduce((soma, c) => soma + c.valor, 0)

  const colunas: TableProps<Comissao>['columns'] = [
    {
      title: 'Vendedor',
      dataIndex: 'vendedorNome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Pedido',
      key: 'pedido',
      width: 110,
      render: (_, c) => (
        <span className="numeros-tabulares">
          #{c.pedidoId} · {c.numeroParcela}/{c.totalParcelas}
        </span>
      ),
    },
    { title: 'Cliente', dataIndex: 'clienteNome', ellipsis: true, responsive: ['lg'] },
    {
      title: 'Recebido',
      dataIndex: 'valorBase',
      width: 120,
      align: 'right',
      render: (valor: number) => <span className="numeros-tabulares">{formatarReal(valor)}</span>,
    },
    {
      title: '%',
      dataIndex: 'percentual',
      width: 70,
      align: 'right',
      render: (valor: number) => <span className="numeros-tabulares">{formatoPercentual.format(valor)}%</span>,
    },
    {
      title: 'Comissão',
      dataIndex: 'valor',
      width: 120,
      align: 'right',
      render: (valor: number) => <strong className="numeros-tabulares">{formatarReal(valor)}</strong>,
    },
    {
      title: 'Recebido em',
      dataIndex: 'dataGeracao',
      width: 115,
      align: 'center',
      render: (data: string) => <span className="numeros-tabulares">{formatoData.format(new Date(data))}</span>,
    },
    {
      title: 'Status',
      key: 'status',
      width: 150,
      align: 'center',
      render: (_, c) => (
        <Flex vertical align="center" gap={2}>
          <TagStatusComissao status={c.status} />
          {c.dataPagamento && (
            <span className="texto-discreto numeros-tabulares" style={{ fontSize: 12 }}>
              em {formatoData.format(new Date(c.dataPagamento))}
            </span>
          )}
        </Flex>
      ),
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 72,
      align: 'center',
      render: (_, c) =>
        c.status === 'Pendente' ? (
          <Popconfirm
            title="Marcar como paga"
            description={`Confirmar o pagamento de ${formatarReal(c.valor)} a ${c.vendedorNome}?`}
            okText="Marcar paga"
            cancelText="Cancelar"
            onConfirm={() => pagar([c.id])}
          >
            <Button type="text" icon={<CheckOutlined />} aria-label={`Marcar como paga a comissão do pedido ${c.pedidoId}`} />
          </Popconfirm>
        ) : (
          // Tooltip só no botão desabilitado: com o Popconfirm, os dois balões abririam juntos.
          <Tooltip title="Comissão já paga">
            <Button type="text" icon={<CheckOutlined />} disabled aria-label="Comissão já paga" />
          </Tooltip>
        ),
    },
  ]

  return (
    <div className="pagina-comissoes">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Comissões</h1>
          <p className="pagina-subtitulo">
            Geradas quando o cliente paga a parcela, com a % congelada no pedido. Marque como paga ao repassar ao vendedor.
          </p>
        </div>
        <Popconfirm
          title="Marcar como pagas"
          description={`Confirmar o pagamento de ${selecionadas.length} comissão(ões), total ${formatarReal(totalSelecionado)}?`}
          okText="Marcar pagas"
          cancelText="Cancelar"
          onConfirm={() => pagar(selecionadas)}
          disabled={selecionadas.length === 0}
        >
          <Button
            type="primary"
            size="large"
            icon={<CheckOutlined />}
            disabled={selecionadas.length === 0}
            loading={pagarComissoes.isPending}
          >
            Marcar como pagas{selecionadas.length > 0 ? ` (${selecionadas.length})` : ''}
          </Button>
        </Popconfirm>
      </Flex>

      <section className="painel" style={{ padding: 16, marginBottom: 16 }} aria-label="Filtros das comissões">
        <Flex gap={12} wrap align="flex-end">
          <Flex vertical gap={4} style={{ minWidth: 240, flex: 1 }}>
            <span className="rotulo-filtro">Vendedor</span>
            <SelecaoVendedor
              value={filtro.vendedorId ?? null}
              onChange={(vendedorId) => filtrar({ vendedorId })}
              aoLimpar={() => filtrar({ vendedorId: undefined })}
              placeholder="Todos os vendedores"
              aria-label="Vendedor"
            />
          </Flex>
          <Flex vertical gap={4}>
            <span className="rotulo-filtro">Recebido entre</span>
            <DatePicker.RangePicker
              format="DD/MM/YYYY"
              value={periodo}
              aria-label="Período do recebimento"
              onChange={(datas) => {
                const novo = datas?.[0] && datas[1] ? ([datas[0], datas[1]] as [Dayjs, Dayjs]) : null
                setPeriodo(novo)
                filtrar({
                  dataInicio: novo?.[0].format('YYYY-MM-DD'),
                  dataFim: novo?.[1].format('YYYY-MM-DD'),
                })
              }}
            />
          </Flex>
          <Flex vertical gap={4}>
            <span className="rotulo-filtro">Status</span>
            <Segmented
              options={opcoesStatus}
              value={filtro.status ?? 'Todas'}
              onChange={(status) => filtrar({ status: status === 'Todas' ? undefined : (status as StatusComissao) })}
            />
          </Flex>
        </Flex>
      </section>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar as comissões."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={8}>
          <CardIndicador titulo="Gerado" loading={!data && isFetching} erro={isError}>
            <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
              {data && formatarReal(data.totais.totalGerado)}
            </Typography.Title>
          </CardIndicador>
        </Col>
        <Col xs={24} sm={8}>
          <CardIndicador titulo="A pagar" loading={!data && isFetching} erro={isError}>
            <Typography.Title level={3} type="warning" className="numeros-tabulares" style={{ margin: 0 }}>
              {data && formatarReal(data.totais.totalPendente)}
            </Typography.Title>
          </CardIndicador>
        </Col>
        <Col xs={24} sm={8}>
          <CardIndicador titulo="Pago" loading={!data && isFetching} erro={isError}>
            <Typography.Title level={3} type="success" className="numeros-tabulares" style={{ margin: 0 }}>
              {data && formatarReal(data.totais.totalPago)}
            </Typography.Title>
          </CardIndicador>
        </Col>
      </Row>

      <section className="painel painel-tabela" aria-label="Lista de comissões">
        <Table<Comissao>
          rowKey="id"
          columns={colunas}
          dataSource={itens}
          loading={isFetching}
          rowSelection={{
            selectedRowKeys: selecionadas,
            onChange: (chaves) => setSelecionadas(chaves.map(Number)),
            // Só as pendentes podem ser selecionadas para pagar.
            getCheckboxProps: (c) => ({ disabled: c.status !== 'Pendente' }),
          }}
          locale={{
            emptyText:
              filtro.vendedorId || filtro.status || filtro.dataInicio
                ? 'Nenhuma comissão para os filtros.'
                : 'Nenhuma comissão ainda: elas aparecem quando o cliente paga uma parcela de pedido com vendedor.',
          }}
          scroll={{ x: 1000 }}
          pagination={{
            current: filtro.pagina,
            pageSize: filtro.tamanhoPagina,
            total: data?.resultado.totalItens ?? 0,
            showSizeChanger: true,
            pageSizeOptions: [20, 50, 100],
            showTotal: (total, [inicio, fim]) =>
              total === 0 ? (
                'Nenhum registro'
              ) : (
                <span>
                  Mostrando <strong>{inicio} a {fim}</strong> de <strong>{total}</strong> registros
                </span>
              ),
          }}
          onChange={(paginacao) => {
            setSelecionadas([])
            setFiltro((atual) => ({
              ...atual,
              pagina: paginacao.current ?? 1,
              tamanhoPagina: paginacao.pageSize ?? atual.tamanhoPagina,
            }))
          }}
        />
      </section>
    </div>
  )
}
