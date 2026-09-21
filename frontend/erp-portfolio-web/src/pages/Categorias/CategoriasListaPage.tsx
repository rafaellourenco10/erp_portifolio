/**
 * =====================================================================
 * Arquivo....: CategoriasListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Tela do cadastro de categorias de produtos: busca por nome
 *              e filtro de status ao lado do botão de inclusão, tabela com
 *              paginação no servidor, edição e inativação.
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/categorias?busca=&ativo=&pagina=&tamanhoPagina=
 *              PATCH /api/categorias/{id}/inativar
 *              (via useListaCategorias / useInativarCategoria)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { EditOutlined, PlusOutlined, StopOutlined } from '@ant-design/icons'
import { Alert, App, Button, Flex, Grid, Input, Popconfirm, Segmented, Table, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatus } from '../../components/TagStatus'
import { useInativarCategoria, useListaCategorias } from '../../hooks/useCategorias'
import type { Categoria, CategoriaFiltro } from '../../types/categoria'
import { CategoriaFormDrawer } from './CategoriaFormDrawer'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'

type StatusFiltro = 'todos' | 'ativos' | 'inativos'

const opcoesStatus = [
  { label: 'Todas', value: 'todos' },
  { label: 'Ativas', value: 'ativos' },
  { label: 'Inativas', value: 'inativos' },
]

const formatoData = new Intl.DateTimeFormat('pt-BR')

export function CategoriasListaPage() {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<CategoriaFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [painelAberto, setPainelAberto] = useState(false)
  const [categoriaEmEdicao, setCategoriaEmEdicao] = useState<Categoria | null>(null)

  const { data, isFetching, isError, error } = useListaCategorias(filtro)
  const inativarCategoria = useInativarCategoria()

  const temFiltro = filtro.busca !== undefined || filtro.ativo !== undefined
  const statusAtual: StatusFiltro = filtro.ativo === undefined ? 'todos' : filtro.ativo ? 'ativos' : 'inativos'

  function abrirInclusao() {
    setCategoriaEmEdicao(null)
    setPainelAberto(true)
  }

  function abrirEdicao(categoria: Categoria) {
    setCategoriaEmEdicao(categoria)
    setPainelAberto(true)
  }

  async function inativar(categoria: Categoria) {
    try {
      await inativarCategoria.mutateAsync(categoria.id)
      message.success(`Categoria "${categoria.nome}" inativada.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const colunas: TableProps<Categoria>['columns'] = [
    {
      title: 'Categoria',
      dataIndex: 'nome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Status',
      dataIndex: 'ativo',
      width: 100,
      align: 'center',
      render: (ativo: boolean) => <TagStatus ativo={ativo} />,
    },
    {
      title: 'Cadastro',
      dataIndex: 'dataCadastro',
      width: 116,
      align: 'center',
      responsive: ['md'],
      render: (data: string) => <span className="numeros-tabulares">{formatoData.format(new Date(data))}</span>,
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 88,
      align: 'center',
      render: (_, categoria) => (
        <Flex gap={4} justify="center">
          <Tooltip title="Editar">
            <Button type="text" icon={<EditOutlined />} aria-label="Editar" onClick={() => abrirEdicao(categoria)} />
          </Tooltip>
          <Popconfirm
            title="Inativar categoria"
            description={`Deseja inativar "${categoria.nome}"? Os produtos nela continuam ligados a ela.`}
            okText="Inativar"
            cancelText="Cancelar"
            okButtonProps={{ danger: true }}
            placement="left"
            onConfirm={() => inativar(categoria)}
            disabled={!categoria.ativo}
          >
            <Tooltip title={categoria.ativo ? 'Inativar' : 'Categoria já inativa'}>
              <Button type="text" danger icon={<StopOutlined />} aria-label="Inativar" disabled={!categoria.ativo} />
            </Tooltip>
          </Popconfirm>
        </Flex>
      ),
    },
  ]

  return (
    <div className="pagina-categorias">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Categorias</h1>
          <p className="pagina-subtitulo">Cadastre as categorias que aparecem na seleção do cadastro de produtos.</p>
        </div>
        <Flex gap={12} wrap>
          <Input.Search
            size="large"
            placeholder="Buscar categoria"
            allowClear
            maxLength={60}
            aria-label="Buscar categoria"
            className="campo-busca-categoria"
            onSearch={(texto) => setFiltro((atual) => ({ ...atual, busca: texto.trim() || undefined, pagina: 1 }))}
          />
          <Segmented
            size="large"
            options={opcoesStatus}
            value={statusAtual}
            onChange={(status) =>
              setFiltro((atual) => ({
                ...atual,
                ativo: status === 'todos' ? undefined : status === 'ativos',
                pagina: 1,
              }))
            }
          />
          <Button type="primary" size="large" icon={<PlusOutlined />} onClick={abrirInclusao}>
            Nova categoria
          </Button>
        </Flex>
      </Flex>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar as categorias."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de categorias">
        <Table<Categoria>
          rowKey="id"
          columns={colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(categoria) => (categoria.ativo ? '' : 'linha-inativa')}
          locale={{
            emptyText: temFiltro ? 'Nenhuma categoria corresponde aos filtros.' : 'Nenhuma categoria cadastrada.',
          }}
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

      <CategoriaFormDrawer aberto={painelAberto} categoria={categoriaEmEdicao} aoFechar={() => setPainelAberto(false)} />
    </div>
  )
}
