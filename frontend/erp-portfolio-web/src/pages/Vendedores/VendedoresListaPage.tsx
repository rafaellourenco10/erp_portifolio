/**
 * =====================================================================
 * Arquivo....: VendedoresListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de listagem de vendedores: filtros (nome e status) num
 *              painel que abre a partir do botão "Filtrar", tabela com
 *              paginação no servidor (com a % de comissão), inclusão,
 *              edição e inativação. Espelho de FornecedoresListaPage.tsx.
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/vendedores?nome=&ativo=&pagina=&tamanhoPagina=
 *              PATCH /api/vendedores/{id}/inativar
 *              (via useListaVendedores / useInativarVendedor)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import {
  CheckOutlined,
  EditOutlined,
  FilterOutlined,
  MailOutlined,
  PhoneOutlined,
  PlusOutlined,
  SearchOutlined,
  StopOutlined,
} from '@ant-design/icons'
import {
  Alert,
  App,
  Badge,
  Button,
  Flex,
  Grid,
  Input,
  Popconfirm,
  Popover,
  Segmented,
  Table,
  Tag,
  Tooltip,
} from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatus } from '../../components/TagStatus'
import { useInativarVendedor, useListaVendedores } from '../../hooks/useVendedores'
import type { Vendedor, VendedorFiltro } from '../../types/vendedor'
import { formatarDocumento } from '../../utils/documento'
import { VendedorFormDrawer } from './VendedorFormDrawer'
// A tela reaproveita as classes .painel-tabela, .pilula-documento etc. do módulo de Clientes.
import '../Clientes/clientes.css'

type StatusFiltro = 'todos' | 'ativos' | 'inativos'

interface FiltrosTela {
  nome: string
  status: StatusFiltro
}

const FILTROS_VAZIOS: FiltrosTela = { nome: '', status: 'todos' }

const opcoesStatus = [
  { label: 'Todos', value: 'todos' },
  { label: 'Ativos', value: 'ativos' },
  { label: 'Inativos', value: 'inativos' },
]

const formatoData = new Intl.DateTimeFormat('pt-BR')
const formatoPercentual = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 })

function textoFiltrosAplicados(quantidade: number): string {
  if (quantidade === 0) return 'Nenhum filtro aplicado'
  return quantidade === 1 ? '1 filtro aplicado' : `${quantidade} filtros aplicados`
}

export function VendedoresListaPage() {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<VendedorFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [filtrosTela, setFiltrosTela] = useState<FiltrosTela>(FILTROS_VAZIOS)
  const [filtroAberto, setFiltroAberto] = useState(false)
  const [painelAberto, setPainelAberto] = useState(false)
  const [vendedorEmEdicao, setVendedorEmEdicao] = useState<Vendedor | null>(null)

  const { data, isFetching, isError, error } = useListaVendedores(filtro)
  const inativarVendedor = useInativarVendedor()

  const quantidadeFiltros =
    (filtro.nome ? 1 : 0) + (filtro.ativo !== undefined ? 1 : 0)
  const quantidadeFiltrosNaTela =
    (filtrosTela.nome.trim() ? 1 : 0) + (filtrosTela.status !== 'todos' ? 1 : 0)

  function aplicarFiltros(valores: FiltrosTela) {
    setFiltro((atual) => ({
      ...atual,
      pagina: 1,
      nome: valores.nome.trim() || undefined,
      ativo: valores.status === 'todos' ? undefined : valores.status === 'ativos',
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
        nome: filtro.nome ?? '',
        status: filtro.ativo === undefined ? 'todos' : filtro.ativo ? 'ativos' : 'inativos',
      })
    }
    setFiltroAberto(aberto)
  }

  function removerFiltroNome() {
    setFiltro((atual) => ({ ...atual, nome: undefined, pagina: 1 }))
  }

  function removerFiltroStatus() {
    setFiltro((atual) => ({ ...atual, ativo: undefined, pagina: 1 }))
  }

  function abrirInclusao() {
    setVendedorEmEdicao(null)
    setPainelAberto(true)
  }

  function abrirEdicao(vendedor: Vendedor) {
    setVendedorEmEdicao(vendedor)
    setPainelAberto(true)
  }

  async function inativar(vendedor: Vendedor) {
    try {
      await inativarVendedor.mutateAsync(vendedor.id)
      message.success(`Vendedor "${vendedor.nome}" inativado.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const renderizarAcoes = (vendedor: Vendedor) => (
    <Flex gap={4} justify="center">
      <Tooltip title="Editar">
        <Button type="text" icon={<EditOutlined />} aria-label="Editar" onClick={() => abrirEdicao(vendedor)} />
      </Tooltip>
      <Popconfirm
        title="Inativar vendedor"
        description={`Deseja inativar "${vendedor.nome}"?`}
        okText="Inativar"
        cancelText="Cancelar"
        okButtonProps={{ danger: true }}
        placement="left"
        onConfirm={() => inativar(vendedor)}
        disabled={!vendedor.ativo}
      >
        <Tooltip title={vendedor.ativo ? 'Inativar' : 'Vendedor já inativo'}>
          <Button type="text" danger icon={<StopOutlined />} aria-label="Inativar" disabled={!vendedor.ativo} />
        </Tooltip>
      </Popconfirm>
    </Flex>
  )

  const colunaAcoes: NonNullable<TableProps<Vendedor>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 88,
    align: 'center',
    render: (_, vendedor) => renderizarAcoes(vendedor),
  }

  // No celular, os dados de cada vendedor ficam empilhados numa única coluna.
  const colunasCelular: TableProps<Vendedor>['columns'] = [
    {
      title: 'Vendedor',
      key: 'vendedor',
      render: (_, vendedor) => (
        <div className="celula-compacta">
          <span className="celula-nome">{vendedor.nome}</span>
          <span className="pilula-documento numeros-tabulares">{formatarDocumento(vendedor.cpf)}</span>
          <Flex align="center" gap={8} wrap>
            <span className="texto-discreto numeros-tabulares">
              Comissão {formatoPercentual.format(vendedor.percentualComissao)}%
            </span>
            <TagStatus ativo={vendedor.ativo} />
          </Flex>
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<Vendedor>['columns'] = [
    {
      title: 'Nome',
      dataIndex: 'nome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'CPF',
      dataIndex: 'cpf',
      width: 160,
      render: (cpf: string) => (
        <span className="pilula-documento numeros-tabulares">{formatarDocumento(cpf)}</span>
      ),
    },
    {
      title: 'E-mail',
      dataIndex: 'email',
      width: 200,
      ellipsis: true,
      responsive: ['xl'],
      render: (email: string | null) =>
        email ? (
          <span className="celula-com-icone">
            <MailOutlined /> {email}
          </span>
        ) : (
          <span className="celula-vazia">—</span>
        ),
    },
    {
      title: 'Telefone / WhatsApp',
      dataIndex: 'telefone',
      width: 170,
      responsive: ['xl'],
      render: (telefone: string | null) =>
        telefone ? (
          <span className="celula-com-icone numeros-tabulares">
            <PhoneOutlined /> {telefone}
          </span>
        ) : (
          <span className="celula-vazia">—</span>
        ),
    },
    {
      title: 'Comissão',
      dataIndex: 'percentualComissao',
      width: 110,
      align: 'right',
      render: (percentual: number) => <span className="numeros-tabulares">{formatoPercentual.format(percentual)}%</span>,
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
      responsive: ['xxl'],
      render: (data: string) => <span className="numeros-tabulares">{formatoData.format(new Date(data))}</span>,
    },
    colunaAcoes,
  ]

  const conteudoFiltro = (
    <div className="popover-filtros">
      <div>
        <label className="rotulo-filtro" htmlFor="filtro-nome">
          <SearchOutlined /> Buscar cadastro
        </label>
        <Input
          id="filtro-nome"
          prefix={<SearchOutlined className="icone-discreto" />}
          placeholder="Buscar por nome"
          allowClear
          maxLength={150}
          value={filtrosTela.nome}
          onChange={(e) => setFiltrosTela((atual) => ({ ...atual, nome: e.target.value }))}
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
    <div className="pagina-vendedores">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Vendedores</h1>
          <p className="pagina-subtitulo">Gerencie os vendedores e a comissão padrão de cada um.</p>
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
            Novo vendedor
          </Button>
        </Flex>
      </Flex>

      {quantidadeFiltros > 0 && (
        <Flex wrap align="center" gap={8} className="linha-filtros-ativos">
          <FilterOutlined className="icone-discreto" />
          {filtro.nome && (
            <Tag closable onClose={removerFiltroNome}>
              Nome: {filtro.nome}
            </Tag>
          )}
          {filtro.ativo !== undefined && (
            <Tag closable onClose={removerFiltroStatus}>
              {filtro.ativo ? 'Ativos' : 'Inativos'}
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
          title="Não foi possível carregar os vendedores."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de vendedores">
        <Table<Vendedor>
          rowKey="id"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(vendedor) => (vendedor.ativo ? '' : 'linha-inativa')}
          locale={{
            emptyText:
              quantidadeFiltros > 0 ? 'Nenhum vendedor corresponde aos filtros.' : 'Nenhum vendedor cadastrado.',
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

      <VendedorFormDrawer
        aberto={painelAberto}
        vendedor={vendedorEmEdicao}
        aoFechar={() => setPainelAberto(false)}
      />
    </div>
  )
}
