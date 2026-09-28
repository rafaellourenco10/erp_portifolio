/**
 * =====================================================================
 * Arquivo....: NotasFiscaisListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Fiscal → Notas Fiscais (etapa 16): NF-e simuladas emitidas
 *              (saída = venda, entrada = devolução) com filtros por período,
 *              cliente, tipo e número/chave; detalhe com itens e impostos,
 *              XML, DANFE e exportação Excel/PDF. ?nota=ID abre o detalhe.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/notas-fiscais, /api/notas-fiscais/{id} (+ /xml, /danfe)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { EyeOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Button, DatePicker, Descriptions, Drawer, Flex, Grid, Input, Segmented, Skeleton, Table, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import type { Dayjs } from 'dayjs'
import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { lerErroApi } from '../../api/axiosClient'
import { notasFiscaisApi } from '../../api/notasFiscaisApi'
import { BotoesExportar } from '../../components/BotoesExportar'
import { BotoesArquivosNfe } from '../../components/NotaFiscalAcoes'
import { SelecaoCliente } from '../../components/SelecaoCliente'
import { useListaNotasFiscais, useNotaFiscal } from '../../hooks/useNotasFiscais'
import type { NotaFiscalFiltro, NotaFiscalItem, NotaFiscalResumo, TipoNotaFiscal } from '../../types/notaFiscal'
import { formatarDocumento } from '../../utils/documento'
import { formatarChave } from '../../utils/nfe'
import { formatarQuantidade, formatarReal } from '../../utils/moeda'
import '../Clientes/clientes.css'
import '../../components/TagStatus.css'

const formatoDataHora = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
const valor = (numero: number) => <span className="numeros-tabulares">{formatarReal(numero)}</span>

type TipoTela = 'Todas' | TipoNotaFiscal

function TagTipo({ tipo }: { tipo: TipoNotaFiscal }) {
  return (
    <span className={`tag-status ${tipo === 'Saida' ? 'tag-status-confirmado' : 'tag-status-rascunho'}`}>
      <span className="tag-status-ponto" />
      {tipo === 'Saida' ? 'Saída' : 'Entrada (devolução)'}
    </span>
  )
}

export function NotasFiscaisListaPage() {
  const telas = Grid.useBreakpoint()
  const [parametros, setParametros] = useSearchParams()
  const [filtro, setFiltro] = useState<NotaFiscalFiltro>({ pagina: 1, tamanhoPagina: 20 })
  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>(null)
  const [busca, setBusca] = useState('')
  const notaAberta = Number(parametros.get('nota')) || null

  const { data, isFetching, isError, error } = useListaNotasFiscais(filtro)

  function filtrar(mudanca: Partial<NotaFiscalFiltro>) {
    setFiltro((atual) => ({ ...atual, ...mudanca, pagina: 1 }))
  }

  const abrir = (id: number | null) => setParametros(id === null ? {} : { nota: String(id) })

  const colunas: TableProps<NotaFiscalResumo>['columns'] = [
    {
      title: 'Número',
      key: 'numero',
      width: 110,
      render: (_, n) => (
        <span className="pilula-documento numeros-tabulares">
          {n.numero} <span className="texto-discreto">/ {n.serie}</span>
        </span>
      ),
    },
    { title: 'Tipo', dataIndex: 'tipo', width: 170, render: (tipo: TipoNotaFiscal) => <TagTipo tipo={tipo} /> },
    {
      title: 'Emissão',
      dataIndex: 'dataEmissao',
      width: 140,
      responsive: ['md'],
      render: (data: string) => <span className="numeros-tabulares">{formatoDataHora.format(new Date(data))}</span>,
    },
    {
      title: 'Destinatário',
      key: 'destinatario',
      ellipsis: true,
      render: (_, n) => (
        <div className="celula-compacta">
          <span className="celula-nome">{n.destinatarioNome}</span>
          <span className="texto-discreto numeros-tabulares">
            {formatarDocumento(n.destinatarioDocumento)} · {n.destinatarioUf}
          </span>
        </div>
      ),
    },
    {
      title: 'Pedido',
      dataIndex: 'pedidoId',
      width: 90,
      responsive: ['lg'],
      render: (id: number) => <Link to={`/pedidos/${id}`}>#{id}</Link>,
    },
    { title: 'Total', dataIndex: 'valorTotal', width: 130, align: 'right', render: valor },
    {
      title: 'Chave de acesso',
      dataIndex: 'chave',
      width: 200,
      responsive: ['xl'],
      render: (chave: string) => (
        <Tooltip title={formatarChave(chave)}>
          <span className="texto-discreto numeros-tabulares">…{chave.slice(-12)}</span>
        </Tooltip>
      ),
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 90,
      align: 'center',
      render: (_, n) => (
        <Button icon={<EyeOutlined />} aria-label={`Ver NF-e nº ${n.numero}`} onClick={() => abrir(n.id)}>
          Ver
        </Button>
      ),
    },
  ]

  return (
    <div>
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Notas Fiscais</h1>
          <p className="pagina-subtitulo">
            NF-e simuladas (ambiente de homologação, sem valor fiscal): saída na venda, entrada na devolução. Emita pelo
            pedido confirmado ou pela devolução.
          </p>
        </div>
        <Flex gap={8} wrap>
          <BotoesExportar
            baixar={(formato) => notasFiscaisApi.exportar(filtro, formato)}
            desativado={!data || data.totalItens === 0}
          />
        </Flex>
      </Flex>

      <section className="painel" style={{ padding: 16, marginBottom: 16 }} aria-label="Filtros das notas">
        <Flex gap={12} wrap align="flex-end">
          <Flex vertical gap={4} style={{ minWidth: 240, flex: 1 }}>
            <span className="rotulo-filtro">Cliente</span>
            <SelecaoCliente
              value={filtro.clienteId ?? null}
              onChange={(clienteId) => filtrar({ clienteId })}
              aoLimpar={() => filtrar({ clienteId: undefined })}
              placeholder="Todos os clientes"
              aria-label="Cliente"
            />
          </Flex>
          <Flex vertical gap={4}>
            <span className="rotulo-filtro">Emitidas entre</span>
            <DatePicker.RangePicker
              format="DD/MM/YYYY"
              value={periodo}
              aria-label="Período de emissão"
              onChange={(datas) => {
                const novo = datas?.[0] && datas[1] ? ([datas[0], datas[1]] as [Dayjs, Dayjs]) : null
                setPeriodo(novo)
                filtrar({ dataInicio: novo?.[0].format('YYYY-MM-DD'), dataFim: novo?.[1].format('YYYY-MM-DD') })
              }}
            />
          </Flex>
          <Flex vertical gap={4}>
            <span className="rotulo-filtro">Tipo</span>
            <Segmented<TipoTela>
              options={[
                { label: 'Todas', value: 'Todas' },
                { label: 'Saída', value: 'Saida' },
                { label: 'Entrada', value: 'Entrada' },
              ]}
              value={filtro.tipo ?? 'Todas'}
              onChange={(tipo) => filtrar({ tipo: tipo === 'Todas' ? undefined : tipo })}
            />
          </Flex>
          <Flex vertical gap={4} style={{ minWidth: 220 }}>
            <span className="rotulo-filtro">Número ou chave</span>
            <Input
              allowClear
              prefix={<SearchOutlined className="icone-discreto" />}
              value={busca}
              aria-label="Número ou chave"
              placeholder="Ex.: 12 ou trecho da chave"
              onChange={(e) => {
                setBusca(e.target.value)
                if (e.target.value === '') filtrar({ busca: undefined })
              }}
              onPressEnter={() => filtrar({ busca: busca.trim() || undefined })}
            />
          </Flex>
        </Flex>
      </section>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar as notas fiscais."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de notas fiscais">
        <Table<NotaFiscalResumo>
          rowKey="id"
          columns={colunas}
          dataSource={data?.itens ?? []}
          loading={isFetching}
          scroll={{ x: 820 }}
          locale={{
            emptyText:
              filtro.clienteId || filtro.tipo || filtro.dataInicio || filtro.busca
                ? 'Nenhuma nota para os filtros.'
                : 'Nenhuma NF-e emitida ainda: emita pelo pedido de venda confirmado.',
          }}
          pagination={{
            current: filtro.pagina,
            pageSize: filtro.tamanhoPagina,
            total: data?.totalItens ?? 0,
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
          onChange={(paginacao) =>
            setFiltro((atual) => ({
              ...atual,
              pagina: paginacao.current ?? 1,
              tamanhoPagina: paginacao.pageSize ?? atual.tamanhoPagina,
            }))
          }
        />
      </section>

      <Drawer
        open={notaAberta !== null}
        onClose={() => abrir(null)}
        size={telas.md === false ? '100%' : 900}
        destroyOnHidden
        title="Nota fiscal eletrônica"
      >
        {notaAberta !== null && <DetalheNota id={notaAberta} aoAbrir={abrir} />}
      </Drawer>
    </div>
  )
}

function DetalheNota({ id, aoAbrir }: { id: number; aoAbrir: (id: number) => void }) {
  const { data: nota, isLoading, isError, error } = useNotaFiscal(id)

  if (isLoading) return <Skeleton active paragraph={{ rows: 10 }} />
  if (isError || !nota)
    return <Alert type="error" showIcon title="Não foi possível carregar a nota." description={lerErroApi(error).mensagem} />

  const colunasItens: TableProps<NotaFiscalItem>['columns'] = [
    {
      title: 'Produto',
      key: 'produto',
      width: 240,
      render: (_, i) => (
        <div className="celula-compacta">
          <span>{i.descricao}</span>
          <span className="texto-discreto numeros-tabulares">
            {i.codigo} · NCM {i.ncm} · CFOP {i.cfop}
          </span>
        </div>
      ),
    },
    {
      title: 'Qtd',
      key: 'qtd',
      width: 90,
      align: 'right',
      render: (_, i) => (
        <span className="numeros-tabulares">
          {formatarQuantidade(i.quantidade)} {i.unidade}
        </span>
      ),
    },
    { title: 'Bruto', dataIndex: 'valorBruto', width: 110, align: 'right', render: valor },
    { title: 'Desconto', dataIndex: 'valorDesconto', width: 100, align: 'right', render: valor },
    { title: 'Base ICMS', dataIndex: 'baseIcms', width: 110, align: 'right', render: valor },
    {
      title: 'ICMS',
      key: 'icms',
      width: 120,
      align: 'right',
      render: (_, i) => (
        <span className="numeros-tabulares">
          {formatarReal(i.valorIcms)} <span className="texto-discreto">({i.aliquotaIcms}%)</span>
        </span>
      ),
    },
    { title: 'PIS', dataIndex: 'valorPis', width: 90, align: 'right', render: valor },
    { title: 'COFINS', dataIndex: 'valorCofins', width: 100, align: 'right', render: valor },
  ]

  return (
    <Flex vertical gap={20}>
      <Flex justify="space-between" align="center" wrap gap={12}>
        <Flex align="center" gap={12} wrap>
          <strong style={{ fontSize: 18 }}>
            NF-e nº {nota.numero} / série {nota.serie}
          </strong>
          <TagTipo tipo={nota.tipo} />
        </Flex>
        <BotoesArquivosNfe notaId={nota.id} />
      </Flex>

      <Descriptions
        size="small"
        bordered
        column={{ xs: 1, sm: 2 }}
        items={[
          { key: 'chave', label: 'Chave de acesso', span: 'filled', children: <span className="numeros-tabulares">{formatarChave(nota.chave)}</span> },
          { key: 'emissao', label: 'Emissão', children: formatoDataHora.format(new Date(nota.dataEmissao)) },
          { key: 'protocolo', label: 'Protocolo (simulado)', children: <span className="numeros-tabulares">{nota.protocolo}</span> },
          {
            key: 'dest',
            label: 'Destinatário',
            span: 'filled',
            children: `${nota.destinatarioNome} · ${formatarDocumento(nota.destinatarioDocumento)} · ${nota.destinatarioUf}`,
          },
          {
            key: 'origem',
            label: 'Origem',
            span: 'filled',
            children: (
              <>
                <Link to={`/pedidos/${nota.pedidoId}`}>Pedido #{nota.pedidoId}</Link>
                {nota.devolucaoId !== null && ` · devolução #${nota.devolucaoId}`}
              </>
            ),
          },
          ...(nota.chaveReferenciada
            ? [
                {
                  key: 'ref',
                  label: 'NF-e devolvida',
                  span: 'filled' as const,
                  children: (
                    <Button type="link" style={{ padding: 0, height: 'auto' }} onClick={() => nota.notaReferenciadaId !== null && aoAbrir(nota.notaReferenciadaId)}>
                      <span className="numeros-tabulares">{formatarChave(nota.chaveReferenciada)}</span>
                    </Button>
                  ),
                },
              ]
            : []),
        ]}
      />

      <Table<NotaFiscalItem>
        rowKey="numeroItem"
        size="small"
        columns={colunasItens}
        dataSource={nota.itens}
        pagination={false}
        scroll={{ x: 1000 }}
      />

      <Descriptions
        size="small"
        bordered
        column={{ xs: 1, sm: 2, md: 4 }}
        items={[
          { key: 'prod', label: 'Produtos', children: valor(nota.valorProdutos) },
          { key: 'desc', label: 'Desconto', children: valor(nota.valorDesconto) },
          { key: 'bc', label: 'Base ICMS', children: valor(nota.baseIcms) },
          { key: 'icms', label: 'ICMS', children: valor(nota.valorIcms) },
          { key: 'pis', label: 'PIS', children: valor(nota.valorPis) },
          { key: 'cofins', label: 'COFINS', children: valor(nota.valorCofins) },
          { key: 'total', label: 'Total da nota', span: 'filled', children: <strong>{valor(nota.valorTotal)}</strong> },
        ]}
      />
    </Flex>
  )
}
