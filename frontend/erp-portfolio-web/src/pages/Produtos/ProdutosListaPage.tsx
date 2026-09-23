/**
 * =====================================================================
 * Arquivo....: ProdutosListaPage.tsx
 * Versão.....: 1.2.0
 * Data.......: 22/09/2026
 * Descrição..: Tela de listagem de produtos: filtros (busca por nome ou
 *              SKU, status e categoria) num painel que abre a partir do
 *              botão "Filtrar", tabela com paginação no servidor, inclusão,
 *              edição e inativação. Mostra a margem (preço x custo).
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/produtos?busca=&ativo=&categoriaId=&pagina=&tamanhoPagina=
 *              PATCH /api/produtos/{id}/inativar
 *              (via useListaProdutos / useInativarProduto)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 21/09/2026 - Coluna Categoria mostra o nome da categoria cadastrada.
 *   1.2.0 - 22/09/2026 - Filtro por categoria no painel Filtrar.
 * =====================================================================
 */

import { CheckOutlined, EditOutlined, FilterOutlined, PlusOutlined, SearchOutlined, StopOutlined } from '@ant-design/icons'
import { Alert, App, Badge, Button, Flex, Grid, Input, Popconfirm, Popover, Segmented, Select, Table, Tag, Tooltip, Typography } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatus } from '../../components/TagStatus'
import { useCategoriasAtivas } from '../../hooks/useCategorias'
import { useInativarProduto, useListaProdutos } from '../../hooks/useProdutos'
import type { Produto, ProdutoFiltro } from '../../types/produto'
import { calcularMargem, formatarPercentual, formatarReal } from '../../utils/moeda'
import { ProdutoFormDrawer } from './ProdutoFormDrawer'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'

type StatusFiltro = 'todos' | 'ativos' | 'inativos'

interface FiltrosTela {
  busca: string
  status: StatusFiltro
  categoriaId: number | undefined
}

const FILTROS_VAZIOS: FiltrosTela = { busca: '', status: 'todos', categoriaId: undefined }

const opcoesStatus = [
  { label: 'Todos', value: 'todos' },
  { label: 'Ativos', value: 'ativos' },
  { label: 'Inativos', value: 'inativos' },
]

function textoFiltrosAplicados(quantidade: number): string {
  if (quantidade === 0) return 'Nenhum filtro aplicado'
  return quantidade === 1 ? '1 filtro aplicado' : `${quantidade} filtros aplicados`
}

function ColunaMargem({ produto }: { produto: Produto }) {
  const margem = calcularMargem(produto.precoVenda, produto.custo)
  if (margem === null) return <span className="celula-vazia">—</span>

  return (
    <span className="numeros-tabulares">
      {margem < 0 ? <Typography.Text type="danger">{formatarPercentual(margem)}</Typography.Text> : formatarPercentual(margem)}
    </span>
  )
}

export function ProdutosListaPage() {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<ProdutoFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [filtrosTela, setFiltrosTela] = useState<FiltrosTela>(FILTROS_VAZIOS)
  const [filtroAberto, setFiltroAberto] = useState(false)
  const [painelAberto, setPainelAberto] = useState(false)
  const [produtoEmEdicao, setProdutoEmEdicao] = useState<Produto | null>(null)

  const { data, isFetching, isError, error } = useListaProdutos(filtro)
  const inativarProduto = useInativarProduto()
  const { data: categorias } = useCategoriasAtivas()
  const opcoesCategoriaFiltro = (categorias?.itens ?? []).map((categoria) => ({ value: categoria.id, label: categoria.nome }))

  const quantidadeFiltros =
    (filtro.busca ? 1 : 0) + (filtro.ativo !== undefined ? 1 : 0) + (filtro.categoriaId !== undefined ? 1 : 0)
  const quantidadeFiltrosNaTela =
    (filtrosTela.busca.trim() ? 1 : 0) + (filtrosTela.status !== 'todos' ? 1 : 0) + (filtrosTela.categoriaId !== undefined ? 1 : 0)

  function aplicarFiltros(valores: FiltrosTela) {
    setFiltro((atual) => ({
      ...atual,
      pagina: 1,
      busca: valores.busca.trim() || undefined,
      ativo: valores.status === 'todos' ? undefined : valores.status === 'ativos',
      categoriaId: valores.categoriaId,
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
      setFiltrosTela({
        busca: filtro.busca ?? '',
        status: filtro.ativo === undefined ? 'todos' : filtro.ativo ? 'ativos' : 'inativos',
        categoriaId: filtro.categoriaId,
      })
    }
    setFiltroAberto(aberto)
  }

  function abrirInclusao() {
    setProdutoEmEdicao(null)
    setPainelAberto(true)
  }

  function abrirEdicao(produto: Produto) {
    setProdutoEmEdicao(produto)
    setPainelAberto(true)
  }

  async function inativar(produto: Produto) {
    try {
      await inativarProduto.mutateAsync(produto.id)
      message.success(`Produto "${produto.nome}" inativado.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const colunaAcoes: NonNullable<TableProps<Produto>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 88,
    align: 'center',
    render: (_, produto) => (
      <Flex gap={4} justify="center">
        <Tooltip title="Editar">
          <Button type="text" icon={<EditOutlined />} aria-label="Editar" onClick={() => abrirEdicao(produto)} />
        </Tooltip>
        <Popconfirm
          title="Inativar produto"
          description={`Deseja inativar "${produto.nome}"?`}
          okText="Inativar"
          cancelText="Cancelar"
          okButtonProps={{ danger: true }}
          placement="left"
          onConfirm={() => inativar(produto)}
          disabled={!produto.ativo}
        >
          <Tooltip title={produto.ativo ? 'Inativar' : 'Produto já inativo'}>
            <Button type="text" danger icon={<StopOutlined />} aria-label="Inativar" disabled={!produto.ativo} />
          </Tooltip>
        </Popconfirm>
      </Flex>
    ),
  }

  // No celular, os dados de cada produto ficam empilhados numa única coluna.
  const colunasCelular: TableProps<Produto>['columns'] = [
    {
      title: 'Produto',
      key: 'produto',
      render: (_, produto) => (
        <div className="celula-compacta">
          <span className="celula-nome">{produto.nome}</span>
          <span className="pilula-documento numeros-tabulares">{produto.sku}</span>
          <Flex align="center" gap={8} wrap>
            <span className="numeros-tabulares">
              {formatarReal(produto.precoVenda)} / {produto.unidade}
            </span>
            <TagStatus ativo={produto.ativo} />
          </Flex>
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<Produto>['columns'] = [
    {
      title: 'SKU',
      dataIndex: 'sku',
      width: 170,
      render: (sku: string) => <span className="pilula-documento numeros-tabulares">{sku}</span>,
    },
    {
      title: 'Produto',
      dataIndex: 'nome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Categoria',
      dataIndex: 'categoriaNome',
      width: 170,
      ellipsis: true,
      responsive: ['lg'],
      render: (categoria: string | null) => categoria ?? <span className="celula-vazia">—</span>,
    },
    {
      title: 'Un.',
      dataIndex: 'unidade',
      width: 64,
      align: 'center',
    },
    {
      title: 'Preço de venda',
      dataIndex: 'precoVenda',
      width: 140,
      align: 'right',
      render: (preco: number) => <span className="numeros-tabulares">{formatarReal(preco)}</span>,
    },
    {
      title: 'Custo',
      dataIndex: 'custo',
      width: 120,
      align: 'right',
      responsive: ['xl'],
      render: (custo: number) => <span className="numeros-tabulares">{formatarReal(custo)}</span>,
    },
    {
      title: 'Margem',
      key: 'margem',
      width: 100,
      align: 'right',
      responsive: ['xl'],
      render: (_, produto) => <ColunaMargem produto={produto} />,
    },
    {
      title: 'Status',
      dataIndex: 'ativo',
      width: 100,
      align: 'center',
      render: (ativo: boolean) => <TagStatus ativo={ativo} />,
    },
    colunaAcoes,
  ]

  const conteudoFiltro = (
    <div className="popover-filtros">
      <div>
        <label className="rotulo-filtro" htmlFor="filtro-busca">
          <SearchOutlined /> Buscar produto
        </label>
        <Input
          id="filtro-busca"
          prefix={<SearchOutlined className="icone-discreto" />}
          placeholder="Buscar por nome ou SKU"
          allowClear
          maxLength={150}
          value={filtrosTela.busca}
          onChange={(e) => setFiltrosTela((atual) => ({ ...atual, busca: e.target.value }))}
          onPressEnter={() => aplicarFiltros(filtrosTela)}
        />
      </div>

      <div>
        <span className="rotulo-filtro">Status cadastral</span>
        <Segmented
          block
          options={opcoesStatus}
          value={filtrosTela.status}
          onChange={(status) => setFiltrosTela((atual) => ({ ...atual, status: status as StatusFiltro }))}
        />
      </div>

      <div>
        <span className="rotulo-filtro">Categoria</span>
        <Select
          className="campo-cheio"
          value={filtrosTela.categoriaId}
          onChange={(categoriaId: number | undefined) => setFiltrosTela((atual) => ({ ...atual, categoriaId }))}
          options={opcoesCategoriaFiltro}
          allowClear
          showSearch={{ optionFilterProp: 'label' }}
          placeholder="Todas as categorias"
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
    <div className="pagina-produtos">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Produtos</h1>
          <p className="pagina-subtitulo">Gerencie o catálogo de produtos, preços e custos.</p>
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
          <Button type="primary" size="large" icon={<PlusOutlined />} onClick={abrirInclusao}>
            Novo produto
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
          {filtro.ativo !== undefined && (
            <Tag closable onClose={() => setFiltro((atual) => ({ ...atual, ativo: undefined, pagina: 1 }))}>
              {filtro.ativo ? 'Ativos' : 'Inativos'}
            </Tag>
          )}
          {filtro.categoriaId !== undefined && (
            <Tag closable onClose={() => setFiltro((atual) => ({ ...atual, categoriaId: undefined, pagina: 1 }))}>
              Categoria:{' '}
              {opcoesCategoriaFiltro.find((opcao) => opcao.value === filtro.categoriaId)?.label ?? filtro.categoriaId}
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
          title="Não foi possível carregar os produtos."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de produtos">
        <Table<Produto>
          rowKey="id"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(produto) => (produto.ativo ? '' : 'linha-inativa')}
          locale={{
            emptyText: quantidadeFiltros > 0 ? 'Nenhum produto corresponde aos filtros.' : 'Nenhum produto cadastrado.',
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

      <ProdutoFormDrawer aberto={painelAberto} produto={produtoEmEdicao} aoFechar={() => setPainelAberto(false)} />
    </div>
  )
}
