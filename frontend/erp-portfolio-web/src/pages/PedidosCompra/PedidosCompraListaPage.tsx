/**
 * =====================================================================
 * Arquivo....: PedidosCompraListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de listagem de pedidos de compra: filtros (busca por número ou nome
 *              do fornecedor e status) num painel que abre a partir do botão "Filtrar",
 *              tabela com paginação no servidor (mais recentes primeiro) e ações para
 *              abrir um pedido ou criar um novo. No celular, cada pedido vira um cartão.
 *              Espelho de PedidosListaPage.tsx.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/pedidos-compra?busca=&status=&pagina=&tamanhoPagina=
 *              (via useListaPedidosCompra)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { ArrowRightOutlined, CheckOutlined, FilterOutlined, PlusOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, Badge, Button, Flex, Grid, Input, Popover, Select, Table, Tag, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatusPedido } from '../../components/TagStatusPedido'
import { useListaPedidosCompra } from '../../hooks/usePedidosCompra'
import { OPCOES_STATUS_PEDIDO, type StatusPedido } from '../../types/pedido'
import type { PedidoCompraFiltro, PedidoCompraResumo } from '../../types/pedidoCompra'
import { formatarReal } from '../../utils/moeda'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'

type StatusTela = StatusPedido | 'todos'

interface FiltrosTela {
  busca: string
  status: StatusTela
}

const FILTROS_VAZIOS: FiltrosTela = { busca: '', status: 'todos' }

const opcoesStatus = [{ label: 'Todos', value: 'todos' }, ...OPCOES_STATUS_PEDIDO]

const formatoDataHora = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

function textoFiltrosAplicados(quantidade: number): string {
  if (quantidade === 0) return 'Nenhum filtro aplicado'
  return quantidade === 1 ? '1 filtro aplicado' : `${quantidade} filtros aplicados`
}

export function PedidosCompraListaPage() {
  const navegar = useNavigate()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<PedidoCompraFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [filtrosTela, setFiltrosTela] = useState<FiltrosTela>(FILTROS_VAZIOS)
  const [filtroAberto, setFiltroAberto] = useState(false)

  const { data, isFetching, isError, error } = useListaPedidosCompra(filtro)

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

  const abrirPedido = (pedido: PedidoCompraResumo) => navegar(`/pedidos-compra/${pedido.id}`)

  const colunaAcoes: NonNullable<TableProps<PedidoCompraResumo>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 88,
    align: 'center',
    render: (_, pedido) => (
      <Tooltip title="Abrir pedido">
        <Button
          type="text"
          icon={<ArrowRightOutlined />}
          aria-label={`Abrir pedido de compra ${pedido.id}`}
          onClick={() => abrirPedido(pedido)}
        />
      </Tooltip>
    ),
  }

  // No celular, os dados de cada pedido ficam empilhados numa única coluna.
  const colunasCelular: TableProps<PedidoCompraResumo>['columns'] = [
    {
      title: 'Pedido',
      key: 'pedido',
      render: (_, pedido) => (
        <div className="celula-compacta">
          <Flex align="center" gap={8} wrap>
            <span className="pilula-documento numeros-tabulares">#{pedido.id}</span>
            <TagStatusPedido status={pedido.status} />
          </Flex>
          <span className="celula-nome">{pedido.fornecedorNome}</span>
          <span className="texto-discreto numeros-tabulares">{formatoDataHora.format(new Date(pedido.dataPedido))}</span>
          <span className="numeros-tabulares">
            {formatarReal(pedido.valorTotal)} · {pedido.quantidadeItens === 1 ? '1 item' : `${pedido.quantidadeItens} itens`}
          </span>
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<PedidoCompraResumo>['columns'] = [
    {
      title: 'Nº',
      dataIndex: 'id',
      width: 96,
      render: (id: number) => <span className="pilula-documento numeros-tabulares">#{id}</span>,
    },
    {
      title: 'Fornecedor',
      dataIndex: 'fornecedorNome',
      ellipsis: true,
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Data',
      dataIndex: 'dataPedido',
      width: 150,
      responsive: ['lg'],
      render: (data: string) => <span className="numeros-tabulares">{formatoDataHora.format(new Date(data))}</span>,
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
      dataIndex: 'status',
      width: 130,
      align: 'center',
      render: (status: StatusPedido) => <TagStatusPedido status={status} />,
    },
    colunaAcoes,
  ]

  const conteudoFiltro = (
    <div className="popover-filtros">
      <div>
        <label className="rotulo-filtro" htmlFor="filtro-busca-pedido-compra">
          <SearchOutlined /> Buscar pedido
        </label>
        <Input
          id="filtro-busca-pedido-compra"
          prefix={<SearchOutlined className="icone-discreto" />}
          placeholder="Número do pedido ou nome do fornecedor"
          allowClear
          maxLength={150}
          value={filtrosTela.busca}
          onChange={(e) => setFiltrosTela((atual) => ({ ...atual, busca: e.target.value }))}
          onPressEnter={() => aplicarFiltros(filtrosTela)}
        />
      </div>

      <div>
        <label className="rotulo-filtro" htmlFor="filtro-status-pedido-compra">
          Status
        </label>
        <Select
          id="filtro-status-pedido-compra"
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
    <div className="pagina-pedidos-compra">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Pedidos de Compra</h1>
          <p className="pagina-subtitulo">Compras de mercadoria: monte rascunhos, confirme ou cancele pedidos.</p>
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
          <Button type="primary" size="large" icon={<PlusOutlined />} onClick={() => navegar('/pedidos-compra/novo')}>
            Novo pedido de compra
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
          title="Não foi possível carregar os pedidos de compra."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de pedidos de compra">
        <Table<PedidoCompraResumo>
          rowKey="id"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(pedido) => (pedido.status === 'Cancelado' ? 'linha-inativa' : '')}
          locale={{
            emptyText:
              quantidadeFiltros > 0 ? 'Nenhum pedido corresponde aos filtros.' : 'Nenhum pedido de compra cadastrado.',
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
