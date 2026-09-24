/**
 * =====================================================================
 * Arquivo....: OrcamentosListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de listagem de orçamentos (etapa 13), no mesmo desenho da lista de
 *              pedidos: filtros (busca por número ou cliente e status, incluindo
 *              "Vencido") no painel "Filtrar", tabela com paginação no servidor, validade
 *              e o pedido gerado (link) no aprovado. No celular, cada orçamento vira um cartão.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/orcamentos?busca=&status=&pagina=&tamanhoPagina=
 *              (via useListaOrcamentos)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { ArrowRightOutlined, CheckOutlined, FilterOutlined, PlusOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Badge, Button, Flex, Grid, Input, Popover, Select, Table, Tag, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatusOrcamento } from '../../components/TagStatusOrcamento'
import { useListaOrcamentos } from '../../hooks/useOrcamentos'
import {
  OPCOES_STATUS_ORCAMENTO,
  type FiltroStatusOrcamento,
  type OrcamentoFiltro,
  type OrcamentoResumo,
} from '../../types/orcamento'
import { formatarReal } from '../../utils/moeda'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'

type StatusTela = FiltroStatusOrcamento | 'todos'

interface FiltrosTela {
  busca: string
  status: StatusTela
}

const FILTROS_VAZIOS: FiltrosTela = { busca: '', status: 'todos' }

const opcoesStatus = [{ label: 'Todos', value: 'todos' }, ...OPCOES_STATUS_ORCAMENTO]

const formatoData = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short' })

/** Data sem hora (AAAA-MM-DD) → dd/mm/aaaa, sem passar por fuso horário. */
const formatarValidade = (data: string) => data.split('-').reverse().join('/')

function textoFiltrosAplicados(quantidade: number): string {
  if (quantidade === 0) return 'Nenhum filtro aplicado'
  return quantidade === 1 ? '1 filtro aplicado' : `${quantidade} filtros aplicados`
}

export function OrcamentosListaPage() {
  const navegar = useNavigate()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<OrcamentoFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [filtrosTela, setFiltrosTela] = useState<FiltrosTela>(FILTROS_VAZIOS)
  const [filtroAberto, setFiltroAberto] = useState(false)

  const { data, isFetching, isError, error } = useListaOrcamentos(filtro)

  const quantidadeFiltros = (filtro.busca ? 1 : 0) + (filtro.status !== undefined ? 1 : 0)
  const quantidadeFiltrosNaTela = (filtrosTela.busca.trim() ? 1 : 0) + (filtrosTela.status !== 'todos' ? 1 : 0)

  function aplicarFiltros(valores: FiltrosTela) {
    setFiltro((atual) => ({
      ...atual,
      pagina: 1,
      busca: valores.busca.trim() || undefined,
      status: valores.status === 'todos' ? undefined : valores.status,
    }))
    setFiltroAberto(false)
  }

  function limparFiltros() {
    setFiltrosTela(FILTROS_VAZIOS)
    aplicarFiltros(FILTROS_VAZIOS)
  }

  // Ao abrir o painel, o rascunho volta a refletir o que está aplicado (descarta edições não aplicadas de antes).
  function aoAbrirFiltro(aberto: boolean) {
    if (aberto) {
      setFiltrosTela({ busca: filtro.busca ?? '', status: filtro.status ?? 'todos' })
    }
    setFiltroAberto(aberto)
  }

  const abrirOrcamento = (orcamento: OrcamentoResumo) => navegar(`/orcamentos/${orcamento.id}`)

  const colunaAcoes: NonNullable<TableProps<OrcamentoResumo>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 88,
    align: 'center',
    render: (_, orcamento) => (
      <Tooltip title="Abrir orçamento">
        <Button
          type="text"
          icon={<ArrowRightOutlined />}
          aria-label={`Abrir orçamento ${orcamento.id}`}
          onClick={() => abrirOrcamento(orcamento)}
        />
      </Tooltip>
    ),
  }

  // Tag de status e, no aprovado, o link para o pedido gerado.
  const celulaStatus = (orcamento: OrcamentoResumo) => (
    <Flex vertical align="center" gap={2}>
      <TagStatusOrcamento status={orcamento.status} vencido={orcamento.vencido} />
      {orcamento.pedidoId !== null && (
        <Link to={`/pedidos/${orcamento.pedidoId}`} className="numeros-tabulares">
          Pedido #{orcamento.pedidoId}
        </Link>
      )}
    </Flex>
  )

  // No celular, os dados de cada orçamento ficam empilhados numa única coluna.
  const colunasCelular: TableProps<OrcamentoResumo>['columns'] = [
    {
      title: 'Orçamento',
      key: 'orcamento',
      render: (_, orcamento) => (
        <div className="celula-compacta">
          <Flex align="center" gap={8} wrap>
            <span className="pilula-documento numeros-tabulares">#{orcamento.id}</span>
            {celulaStatus(orcamento)}
          </Flex>
          <span className="celula-nome">{orcamento.clienteNome}</span>
          <span className="texto-discreto numeros-tabulares">
            {formatoData.format(new Date(orcamento.dataOrcamento))} · válido até {formatarValidade(orcamento.validade)}
          </span>
          <span className="numeros-tabulares">
            {formatarReal(orcamento.valorTotal)} ·{' '}
            {orcamento.quantidadeItens === 1 ? '1 item' : `${orcamento.quantidadeItens} itens`}
          </span>
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<OrcamentoResumo>['columns'] = [
    {
      title: 'Nº',
      dataIndex: 'id',
      width: 96,
      render: (id: number) => <span className="pilula-documento numeros-tabulares">#{id}</span>,
    },
    {
      title: 'Cliente',
      dataIndex: 'clienteNome',
      ellipsis: true,
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Data',
      dataIndex: 'dataOrcamento',
      width: 110,
      responsive: ['lg'],
      render: (data: string) => <span className="numeros-tabulares">{formatoData.format(new Date(data))}</span>,
    },
    {
      title: 'Validade',
      dataIndex: 'validade',
      width: 110,
      render: (validade: string) => <span className="numeros-tabulares">{formatarValidade(validade)}</span>,
    },
    {
      title: 'Itens',
      dataIndex: 'quantidadeItens',
      width: 76,
      align: 'center',
      responsive: ['xl'],
    },
    {
      title: 'Total',
      dataIndex: 'valorTotal',
      width: 150,
      align: 'right',
      render: (valor: number) => <span className="numeros-tabulares">{formatarReal(valor)}</span>,
    },
    {
      title: 'Status',
      key: 'status',
      width: 130,
      align: 'center',
      render: (_, orcamento) => celulaStatus(orcamento),
    },
    colunaAcoes,
  ]

  const conteudoFiltro = (
    <div className="popover-filtros">
      <div>
        <label className="rotulo-filtro" htmlFor="filtro-busca-orcamento">
          <SearchOutlined /> Buscar orçamento
        </label>
        <Input
          id="filtro-busca-orcamento"
          prefix={<SearchOutlined className="icone-discreto" />}
          placeholder="Número do orçamento ou nome do cliente"
          allowClear
          maxLength={150}
          value={filtrosTela.busca}
          onChange={(e) => setFiltrosTela((atual) => ({ ...atual, busca: e.target.value }))}
          onPressEnter={() => aplicarFiltros(filtrosTela)}
        />
      </div>

      <div>
        <label className="rotulo-filtro" htmlFor="filtro-status-orcamento">
          Status
        </label>
        <Select
          id="filtro-status-orcamento"
          className="campo-cheio"
          options={opcoesStatus}
          value={filtrosTela.status}
          onChange={(status: StatusTela) => setFiltrosTela((atual) => ({ ...atual, status }))}
        />
      </div>

      <Flex justify="space-between" align="center" gap={12} className="popover-filtros-rodape">
        <span className="texto-discreto">{textoFiltrosAplicados(quantidadeFiltrosNaTela)}</span>
        <Flex gap={8}>
          <Button size="small" onClick={limparFiltros}>
            Limpar
          </Button>
          <Button size="small" type="primary" icon={<CheckOutlined />} onClick={() => aplicarFiltros(filtrosTela)}>
            Aplicar
          </Button>
        </Flex>
      </Flex>
    </div>
  )

  return (
    <div className="pagina-orcamentos">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Orçamentos</h1>
          <p className="pagina-subtitulo">Propostas enviadas aos clientes: acompanhe a validade e transforme em pedido quando aprovadas.</p>
        </div>
        <Flex gap={12}>
          <Popover
            trigger="click"
            placement="bottomRight"
            open={filtroAberto}
            onOpenChange={aoAbrirFiltro}
            content={conteudoFiltro}
          >
            <Badge count={quantidadeFiltros} size="small" offset={[-6, 4]}>
              <Button size="large" icon={<FilterOutlined />}>
                Filtrar
              </Button>
            </Badge>
          </Popover>
          <Button type="primary" size="large" icon={<PlusOutlined />} onClick={() => navegar('/orcamentos/novo')}>
            Novo orçamento
          </Button>
        </Flex>
      </Flex>

      {quantidadeFiltros > 0 && (
        <Flex wrap align="center" gap={8} className="linha-filtros-ativos">
          <FilterOutlined className="icone-discreto" />
          {filtro.busca && (
            <Tag closable onClose={() => setFiltro((atual) => ({ ...atual, busca: undefined, pagina: 1 }))}>
              Busca: {filtro.busca}
            </Tag>
          )}
          {filtro.status !== undefined && (
            <Tag closable onClose={() => setFiltro((atual) => ({ ...atual, status: undefined, pagina: 1 }))}>
              Status: {filtro.status}
            </Tag>
          )}
          <Button type="link" size="small" onClick={limparFiltros}>
            Limpar tudo
          </Button>
        </Flex>
      )}

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar os orçamentos."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de orçamentos">
        <Table<OrcamentoResumo>
          rowKey="id"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(orcamento) => (orcamento.status === 'Perdido' ? 'linha-inativa' : '')}
          locale={{
            emptyText: quantidadeFiltros > 0 ? 'Nenhum orçamento corresponde aos filtros.' : 'Nenhum orçamento cadastrado.',
          }}
          scroll={ehCelular ? undefined : { x: 640 }}
          pagination={{
            current: filtro.pagina,
            pageSize: filtro.tamanhoPagina,
            total: data?.totalItens ?? 0,
            size: ehCelular ? 'small' : undefined,
            showSizeChanger: !ehCelular,
            pageSizeOptions: [10, 20, 50, 100],
            showTotal: (total, [inicio, fim]) =>
              total === 0 ? (
                'Nenhum registro'
              ) : ehCelular ? (
                `${inicio}–${fim} de ${total}`
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
    </div>
  )
}
