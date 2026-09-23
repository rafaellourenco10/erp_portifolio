/**
 * =====================================================================
 * Arquivo....: FornecedoresListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de listagem de fornecedores: filtros (nome, UFs em
 *              multi-select e status) num painel que abre a partir do
 *              botão "Filtrar", tabela com paginação no servidor,
 *              inclusão, edição e inativação. Espelho de
 *              ClientesListaPage.tsx.
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/fornecedores?nome=&ufs=&ativo=&pagina=&tamanhoPagina=
 *              PATCH /api/fornecedores/{id}/inativar
 *              (via useListaFornecedores / useInativarFornecedor)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
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
  Select,
  Table,
  Tag,
  Tooltip,
} from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatus } from '../../components/TagStatus'
import { useInativarFornecedor, useListaFornecedores } from '../../hooks/useFornecedores'
import type { Fornecedor, FornecedorFiltro } from '../../types/fornecedor'
import { formatarDocumento } from '../../utils/documento'
import { UFS, compararRelevanciaUf, ufCorrespondeBusca } from '../../utils/ufs'
import { FornecedorFormDrawer } from './FornecedorFormDrawer'
// A tela reaproveita as classes .painel-tabela, .pilula-documento etc. do módulo de Clientes.
import '../Clientes/clientes.css'

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

export function FornecedoresListaPage() {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<FornecedorFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [filtrosTela, setFiltrosTela] = useState<FiltrosTela>(FILTROS_VAZIOS)
  const [filtroAberto, setFiltroAberto] = useState(false)
  const [painelAberto, setPainelAberto] = useState(false)
  const [fornecedorEmEdicao, setFornecedorEmEdicao] = useState<Fornecedor | null>(null)

  const { data, isFetching, isError, error } = useListaFornecedores(filtro)
  const inativarFornecedor = useInativarFornecedor()

  const quantidadeFiltros =
    (filtro.nome ? 1 : 0) + (filtro.ufs?.length ? 1 : 0) + (filtro.ativo !== undefined ? 1 : 0)
  const quantidadeFiltrosNaTela =
    (filtrosTela.nome.trim() ? 1 : 0) + (filtrosTela.ufs.length > 0 ? 1 : 0) + (filtrosTela.status !== 'todos' ? 1 : 0)

  function aplicarFiltros(valores: FiltrosTela) {
    setFiltro((atual) => ({
      ...atual,
      pagina: 1,
      nome: valores.nome.trim() || undefined,
      ufs: valores.ufs.length > 0 ? valores.ufs : undefined,
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
        ufs: filtro.ufs ?? [],
        status: filtro.ativo === undefined ? 'todos' : filtro.ativo ? 'ativos' : 'inativos',
      })
    }
    setFiltroAberto(aberto)
  }

  function removerFiltroNome() {
    setFiltro((atual) => ({ ...atual, nome: undefined, pagina: 1 }))
  }

  function removerFiltroUf(sigla: string) {
    setFiltro((atual) => {
      const restantes = atual.ufs?.filter((uf) => uf !== sigla)
      return { ...atual, ufs: restantes?.length ? restantes : undefined, pagina: 1 }
    })
  }

  function removerFiltroStatus() {
    setFiltro((atual) => ({ ...atual, ativo: undefined, pagina: 1 }))
  }

  function abrirInclusao() {
    setFornecedorEmEdicao(null)
    setPainelAberto(true)
  }

  function abrirEdicao(fornecedor: Fornecedor) {
    setFornecedorEmEdicao(fornecedor)
    setPainelAberto(true)
  }

  async function inativar(fornecedor: Fornecedor) {
    try {
      await inativarFornecedor.mutateAsync(fornecedor.id)
      message.success(`Fornecedor "${fornecedor.nome}" inativado.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const renderizarAcoes = (fornecedor: Fornecedor) => (
    <Flex gap={4} justify="center">
      <Tooltip title="Editar">
        <Button type="text" icon={<EditOutlined />} aria-label="Editar" onClick={() => abrirEdicao(fornecedor)} />
      </Tooltip>
      <Popconfirm
        title="Inativar fornecedor"
        description={`Deseja inativar "${fornecedor.nome}"?`}
        okText="Inativar"
        cancelText="Cancelar"
        okButtonProps={{ danger: true }}
        placement="left"
        onConfirm={() => inativar(fornecedor)}
        disabled={!fornecedor.ativo}
      >
        <Tooltip title={fornecedor.ativo ? 'Inativar' : 'Fornecedor já inativo'}>
          <Button type="text" danger icon={<StopOutlined />} aria-label="Inativar" disabled={!fornecedor.ativo} />
        </Tooltip>
      </Popconfirm>
    </Flex>
  )

  const colunaAcoes: NonNullable<TableProps<Fornecedor>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 88,
    align: 'center',
    render: (_, fornecedor) => renderizarAcoes(fornecedor),
  }

  // No celular, os dados de cada fornecedor ficam empilhados numa única coluna.
  const colunasCelular: TableProps<Fornecedor>['columns'] = [
    {
      title: 'Fornecedor',
      key: 'fornecedor',
      render: (_, fornecedor) => (
        <div className="celula-compacta">
          <span className="celula-nome">{fornecedor.nome}</span>
          <span className="pilula-documento numeros-tabulares">{formatarDocumento(fornecedor.documento)}</span>
          <Flex align="center" gap={8} wrap>
            <span className="celula-com-icone texto-discreto">
              <EnvironmentOutlined /> {fornecedor.cidade} - {fornecedor.uf}
            </span>
            <TagStatus ativo={fornecedor.ativo} />
          </Flex>
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<Fornecedor>['columns'] = [
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
      render: (_, fornecedor) => (
        <span className="celula-com-icone">
          <EnvironmentOutlined /> {fornecedor.cidade} - {fornecedor.uf}
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

  const conteudoFiltro = (
    <div className="popover-filtros">
      <div>
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
      </div>

      <div>
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
    <div className="pagina-fornecedores">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Fornecedores</h1>
          <p className="pagina-subtitulo">Gerencie seus fornecedores e seus dados cadastrais.</p>
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
            Novo fornecedor
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
          {filtro.ufs?.map((uf) => (
            <Tag key={uf} closable onClose={() => removerFiltroUf(uf)}>
              {uf}
            </Tag>
          ))}
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
          title="Não foi possível carregar os fornecedores."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de fornecedores">
        <Table<Fornecedor>
          rowKey="id"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(fornecedor) => (fornecedor.ativo ? '' : 'linha-inativa')}
          locale={{
            emptyText:
              quantidadeFiltros > 0 ? 'Nenhum fornecedor corresponde aos filtros.' : 'Nenhum fornecedor cadastrado.',
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

      <FornecedorFormDrawer
        aberto={painelAberto}
        fornecedor={fornecedorEmEdicao}
        aoFechar={() => setPainelAberto(false)}
      />
    </div>
  )
}
