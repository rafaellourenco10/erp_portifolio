/**
 * =====================================================================
 * Arquivo....: ClientesListaPage.tsx
 * Versão.....: 1.1.0
 * Data.......: 18/09/2026
 * Descrição..: Tela de listagem de clientes: filtros (nome, UFs em
 *              multi-select e status), tabela com paginação no servidor,
 *              inclusão, edição e inativação.
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/clientes?nome=&ufs=&ativo=&pagina=&tamanhoPagina=
 *              PATCH /api/clientes/{id}/inativar
 *              (via useListaClientes / useInativarCliente)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Tema Ambition ERP; filtros por UFs (multi-select)
 *                        e status; formulário em painel lateral.
 * =====================================================================
 */

import {
  CheckOutlined,
  EditOutlined,
  EnvironmentOutlined,
  FilterOutlined,
  MailOutlined,
  PhoneOutlined,
  PlusOutlined,
  SearchOutlined,
  StopOutlined,
} from '@ant-design/icons'
import { Alert, App, Button, Col, Flex, Grid, Input, Popconfirm, Row, Segmented, Select, Table, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatus } from '../../components/TagStatus'
import { useInativarCliente, useListaClientes } from '../../hooks/useClientes'
import type { Cliente, ClienteFiltro } from '../../types/cliente'
import { formatarDocumento } from '../../utils/documento'
import { UFS, compararRelevanciaUf, ufCorrespondeBusca } from '../../utils/ufs'
import { ClienteFormDrawer } from './ClienteFormDrawer'
import './clientes.css'

type StatusFiltro = 'todos' | 'ativos' | 'inativos'

interface FiltrosTela {
  nome: string
  ufs: string[]
  status: StatusFiltro
}

const FILTROS_VAZIOS: FiltrosTela = { nome: '', ufs: [], status: 'todos' }

const opcoesUf = UFS.map((uf) => ({ value: uf.sigla, label: uf.sigla, uf }))

const opcoesStatus = [
  { label: 'Todos', value: 'todos' },
  { label: 'Ativos', value: 'ativos' },
  { label: 'Inativos', value: 'inativos' },
]

const formatoData = new Intl.DateTimeFormat('pt-BR')

function textoFiltrosAplicados(quantidade: number): string {
  if (quantidade === 0) return 'Nenhum filtro aplicado'
  return quantidade === 1 ? '1 filtro aplicado' : `${quantidade} filtros aplicados`
}

export function ClientesListaPage() {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<ClienteFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [filtrosTela, setFiltrosTela] = useState<FiltrosTela>(FILTROS_VAZIOS)
  const [painelAberto, setPainelAberto] = useState(false)
  const [clienteEmEdicao, setClienteEmEdicao] = useState<Cliente | null>(null)

  const { data, isFetching, isError, error } = useListaClientes(filtro)
  const inativarCliente = useInativarCliente()

  const quantidadeFiltros =
    (filtro.nome ? 1 : 0) + (filtro.ufs?.length ? 1 : 0) + (filtro.ativo !== undefined ? 1 : 0)

  function aplicarFiltros(valores: FiltrosTela) {
    setFiltro((atual) => ({
      ...atual,
      pagina: 1,
      nome: valores.nome.trim() || undefined,
      ufs: valores.ufs.length > 0 ? valores.ufs : undefined,
      ativo: valores.status === 'todos' ? undefined : valores.status === 'ativos',
    }))
  }

  function limparFiltros() {
    setFiltrosTela(FILTROS_VAZIOS)
    aplicarFiltros(FILTROS_VAZIOS)
  }

  function abrirInclusao() {
    setClienteEmEdicao(null)
    setPainelAberto(true)
  }

  function abrirEdicao(cliente: Cliente) {
    setClienteEmEdicao(cliente)
    setPainelAberto(true)
  }

  async function inativar(cliente: Cliente) {
    try {
      await inativarCliente.mutateAsync(cliente.id)
      message.success(`Cliente "${cliente.nome}" inativado.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const renderizarAcoes = (cliente: Cliente) => (
    <Flex gap={4} justify="center">
      <Tooltip title="Editar">
        <Button type="text" icon={<EditOutlined />} aria-label="Editar" onClick={() => abrirEdicao(cliente)} />
      </Tooltip>
      <Popconfirm
        title="Inativar cliente"
        description={`Deseja inativar "${cliente.nome}"?`}
        okText="Inativar"
        cancelText="Cancelar"
        okButtonProps={{ danger: true }}
        placement="left"
        onConfirm={() => inativar(cliente)}
        disabled={!cliente.ativo}
      >
        <Tooltip title={cliente.ativo ? 'Inativar' : 'Cliente já inativo'}>
          <Button type="text" danger icon={<StopOutlined />} aria-label="Inativar" disabled={!cliente.ativo} />
        </Tooltip>
      </Popconfirm>
    </Flex>
  )

  const colunaAcoes: NonNullable<TableProps<Cliente>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 88,
    align: 'center',
    render: (_, cliente) => renderizarAcoes(cliente),
  }

  // No celular, os dados de cada cliente ficam empilhados numa única coluna.
  const colunasCelular: TableProps<Cliente>['columns'] = [
    {
      title: 'Cliente',
      key: 'cliente',
      render: (_, cliente) => (
        <div className="celula-compacta">
          <span className="celula-nome">{cliente.nome}</span>
          <span className="pilula-documento numeros-tabulares">{formatarDocumento(cliente.documento)}</span>
          <Flex align="center" gap={8} wrap>
            <span className="celula-com-icone texto-discreto">
              <EnvironmentOutlined /> {cliente.cidade} - {cliente.uf}
            </span>
            <TagStatus ativo={cliente.ativo} />
          </Flex>
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<Cliente>['columns'] = [
    {
      title: 'Nome / Razão social',
      dataIndex: 'nome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'CPF / CNPJ',
      dataIndex: 'documento',
      width: 180,
      render: (documento: string) => (
        <span className="pilula-documento numeros-tabulares">{formatarDocumento(documento)}</span>
      ),
    },
    {
      title: 'E-mail principal',
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
      title: 'Cidade / UF',
      key: 'cidadeUf',
      width: 170,
      ellipsis: true,
      render: (_, cliente) => (
        <span className="celula-com-icone">
          <EnvironmentOutlined /> {cliente.cidade} - {cliente.uf}
        </span>
      ),
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

  return (
    <div className="pagina-clientes">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Clientes</h1>
          <p className="pagina-subtitulo">Gerencie sua carteira de clientes e seus dados cadastrais.</p>
        </div>
        <Button type="primary" size="large" icon={<PlusOutlined />} onClick={abrirInclusao}>
          Novo cliente
        </Button>
      </Flex>

      <section className="painel painel-filtros" aria-label="Filtros">
        <Row gutter={[16, 16]}>
          <Col xs={24} lg={10}>
            <label className="rotulo-filtro" htmlFor="filtro-nome">
              <SearchOutlined /> Buscar cadastro
            </label>
            <Input
              id="filtro-nome"
              prefix={<SearchOutlined className="icone-discreto" />}
              placeholder="Buscar por nome ou razão social"
              allowClear
              maxLength={150}
              value={filtrosTela.nome}
              onChange={(e) => setFiltrosTela((atual) => ({ ...atual, nome: e.target.value }))}
              onPressEnter={() => aplicarFiltros(filtrosTela)}
            />
          </Col>
          <Col xs={24} sm={14} lg={8}>
            <Flex justify="space-between" align="baseline">
              <span className="rotulo-filtro">Estado (UF)</span>
              {filtrosTela.ufs.length > 0 && (
                <span className="contador-selecao">
                  {filtrosTela.ufs.length === 1 ? '1 selecionada' : `${filtrosTela.ufs.length} selecionadas`}
                </span>
              )}
            </Flex>
            <Select
              mode="multiple"
              aria-label="Estado (UF)"
              className="campo-cheio"
              placeholder="Todas as UFs"
              allowClear
              maxTagCount="responsive"
              options={opcoesUf}
              optionRender={(opcao) => (
                <span>
                  {opcao.data.uf.nome} <span className="icone-discreto">({opcao.data.uf.sigla})</span>
                </span>
              )}
              showSearch={{
                filterOption: (busca, opcao) => !!opcao && ufCorrespondeBusca(busca, opcao.uf),
                filterSort: (a, b, { searchValue }) => compararRelevanciaUf(a.uf, b.uf, searchValue),
              }}
              value={filtrosTela.ufs}
              onChange={(ufs: string[]) => setFiltrosTela((atual) => ({ ...atual, ufs }))}
            />
          </Col>
          <Col xs={24} sm={10} lg={6}>
            <span className="rotulo-filtro">Status cadastral</span>
            <Segmented
              block
              options={opcoesStatus}
              value={filtrosTela.status}
              onChange={(status) => setFiltrosTela((atual) => ({ ...atual, status: status as StatusFiltro }))}
            />
          </Col>
        </Row>

        <Flex justify="space-between" align="center" wrap gap={12} className="painel-filtros-rodape">
          <span className="texto-discreto">
            <FilterOutlined /> {textoFiltrosAplicados(quantidadeFiltros)}
          </span>
          <Flex gap={8}>
            <Button onClick={limparFiltros}>Limpar filtros</Button>
            <Button type="primary" icon={<CheckOutlined />} onClick={() => aplicarFiltros(filtrosTela)}>
              Filtrar
            </Button>
          </Flex>
        </Flex>
      </section>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar os clientes."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de clientes">
        <Table<Cliente>
          rowKey="id"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(cliente) => (cliente.ativo ? '' : 'linha-inativa')}
          locale={{
            emptyText: quantidadeFiltros > 0 ? 'Nenhum cliente corresponde aos filtros.' : 'Nenhum cliente cadastrado.',
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

      <ClienteFormDrawer aberto={painelAberto} cliente={clienteEmEdicao} aoFechar={() => setPainelAberto(false)} />
    </div>
  )
}
