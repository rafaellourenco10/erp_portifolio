/**
 * =====================================================================
 * Arquivo....: ComissoesListaPage.tsx
 * Versão.....: 2.1.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de comissões dos vendedores (Financeiro): filtros por
 *              vendedor, status e período (data do recebimento da parcela),
 *              cards Gerado / A pagar / Em pagamento / Pago (somas do filtro,
 *              calculadas no servidor), tabela paginada no servidor e "Gerar
 *              conta a pagar" com as pendentes selecionadas de um vendedor: o
 *              pagamento em si é feito em Contas a Pagar (etapa 12).
 * ---------------------------------------------------------------------
 * Fontes.....: GET  /api/comissoes?vendedorId=&status=&dataInicio=&dataFim=&pagina=&tamanhoPagina=
 *              POST /api/comissoes/gerar-conta
 *              (via useListaComissoes / useGerarContaComissoes)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   2.1.0 - 24/09/2026 - Estorno de devolução (negativo, "Estorno · devolução #D") e aviso de que os
 *                        estornos pendentes são descontados ao gerar a conta (etapa 14).
 *   2.0.0 - 23/09/2026 - "Gerar conta a pagar" no lugar de "Marcar como pagas"; status e
 *                        card Em pagamento (etapa 12).
 * =====================================================================
 */

import { FileAddOutlined } from '@ant-design/icons'
import { Alert, App, Button, Col, DatePicker, Flex, Modal, Row, Segmented, Table, Tooltip, Typography } from 'antd'
import type { TableProps } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { SelecaoVendedor } from '../../components/SelecaoVendedor'
import { useGerarContaComissoes, useListaComissoes } from '../../hooks/useComissoes'
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
  { label: 'Em pagamento', value: 'EmPagamento' },
  { label: 'Pagas', value: 'Paga' },
]

const formatoData = new Intl.DateTimeFormat('pt-BR')
const formatoPercentual = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 })

const TAG_STATUS: Record<StatusComissao, { classe: string; rotulo: string }> = {
  Pendente: { classe: 'tag-status-inativo', rotulo: 'A pagar' },
  EmPagamento: { classe: 'tag-status-rascunho', rotulo: 'Em pagamento' },
  Paga: { classe: 'tag-status-confirmado', rotulo: 'Paga' },
}

function TagStatusComissao({ status }: { status: StatusComissao }) {
  return (
    <span className={`tag-status ${TAG_STATUS[status].classe}`}>
      <span className="tag-status-ponto" />
      {TAG_STATUS[status].rotulo}
    </span>
  )
}

export function ComissoesListaPage() {
  const { message } = App.useApp()
  const [filtro, setFiltro] = useState<ComissaoFiltro>({ pagina: 1, tamanhoPagina: 20 })
  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>(null)
  const [selecionadas, setSelecionadas] = useState<number[]>([])

  const { data, isFetching, isError, error } = useListaComissoes(filtro)
  const gerarConta = useGerarContaComissoes()
  // Aberto = modal de gerar conta visível; guarda o vencimento escolhido.
  const [vencimento, setVencimento] = useState<Dayjs | null>(null)

  // Mudou o filtro: volta para a página 1 e limpa a seleção (as linhas selecionadas podem sumir da tela).
  function filtrar(mudanca: Partial<ComissaoFiltro>) {
    setSelecionadas([])
    setFiltro((atual) => ({ ...atual, ...mudanca, pagina: 1 }))
  }

  const itens = data?.resultado.itens ?? []
  const escolhidas = itens.filter((c) => selecionadas.includes(c.id))
  const totalSelecionado = escolhidas.reduce((soma, c) => soma + c.valor, 0)
  const vendedoresSelecionados = [...new Set(escolhidas.map((c) => c.vendedorNome))]
  const umVendedorSo = vendedoresSelecionados.length === 1

  async function confirmarGerarConta() {
    if (!vencimento) return
    try {
      const conta = await gerarConta.mutateAsync({ ids: selecionadas, vencimento: vencimento.format('YYYY-MM-DD') })
      message.success(
        `Conta a pagar de ${formatarReal(conta.valor)} gerada (${conta.quantidadeComissoes} comissões). Pague em Financeiro → Contas a Pagar.`,
      )
      setSelecionadas([])
      setVencimento(null)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

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
          #{c.pedidoId} · {c.devolucaoId !== null ? `estorno dev. #${c.devolucaoId}` : `${c.numeroParcela}/${c.totalParcelas}`}
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
      render: (valor: number) => (
        <strong className="numeros-tabulares" style={valor < 0 ? { color: 'var(--cor-erro)' } : undefined}>
          {formatarReal(valor)}
        </strong>
      ),
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
  ]

  return (
    <div className="pagina-comissoes">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Comissões</h1>
          <p className="pagina-subtitulo">
            Geradas quando o cliente paga a parcela, com a % congelada no pedido. Para pagar o vendedor, gere a conta a pagar
            e pague-a em Contas a Pagar.
          </p>
        </div>
        <Tooltip title={selecionadas.length > 0 && !umVendedorSo ? 'Selecione comissões de um único vendedor' : undefined}>
          <Button
            type="primary"
            size="large"
            icon={<FileAddOutlined />}
            disabled={selecionadas.length === 0 || !umVendedorSo}
            onClick={() => setVencimento(dayjs())}
          >
            Gerar conta a pagar{selecionadas.length > 0 ? ` (${selecionadas.length})` : ''}
          </Button>
        </Tooltip>
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
        <Col xs={24} sm={12} lg={6}>
          <CardIndicador titulo="Gerado" loading={!data && isFetching} erro={isError}>
            <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
              {data && formatarReal(data.totais.totalGerado)}
            </Typography.Title>
          </CardIndicador>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <CardIndicador titulo="A pagar" loading={!data && isFetching} erro={isError}>
            <Typography.Title level={3} type="warning" className="numeros-tabulares" style={{ margin: 0 }}>
              {data && formatarReal(data.totais.totalPendente)}
            </Typography.Title>
          </CardIndicador>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <CardIndicador titulo="Em pagamento" loading={!data && isFetching} erro={isError}>
            <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
              {data && formatarReal(data.totais.totalEmPagamento)}
            </Typography.Title>
          </CardIndicador>
        </Col>
        <Col xs={24} sm={12} lg={6}>
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
            // Só as pendentes (A pagar) podem virar conta a pagar.
            getCheckboxProps: (c) => ({ disabled: c.status !== 'Pendente' }),
          }}
          locale={{
            emptyText:
              filtro.vendedorId || filtro.status || filtro.dataInicio
                ? 'Nenhuma comissão para os filtros.'
                : 'Nenhuma comissão ainda: elas aparecem quando o cliente paga uma parcela de pedido com vendedor.',
          }}
          scroll={{ x: 920 }}
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

      <Modal
        open={vencimento !== null}
        title="Gerar conta a pagar"
        okText="Gerar conta"
        cancelText="Voltar"
        onOk={confirmarGerarConta}
        onCancel={() => setVencimento(null)}
        confirmLoading={gerarConta.isPending}
      >
        <p>
          {selecionadas.length} comissão(ões) de <strong>{vendedoresSelecionados[0]}</strong>, total{' '}
          <strong className="numeros-tabulares">{formatarReal(totalSelecionado)}</strong>. Elas ficam "Em pagamento" e
          viram pagas quando você pagar a conta em Contas a Pagar.
        </p>
        <p className="texto-discreto">
          Estornos de devolução pendentes deste vendedor entram automaticamente e são descontados do total; o valor
          final aparece na mensagem ao gerar a conta.
        </p>
        <Flex vertical gap={4}>
          <span className="rotulo-filtro">Vencimento</span>
          <DatePicker
            format="DD/MM/YYYY"
            allowClear={false}
            aria-label="Vencimento da conta"
            value={vencimento}
            onChange={(data) => setVencimento(data)}
          />
        </Flex>
      </Modal>
    </div>
  )
}
