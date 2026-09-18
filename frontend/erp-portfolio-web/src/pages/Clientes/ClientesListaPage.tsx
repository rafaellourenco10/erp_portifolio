/**
 * =====================================================================
 * Arquivo....: ClientesListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Tela de listagem de clientes: tabela com paginação no
 *              servidor, busca por nome, inclusão, edição e inativação.
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/clientes?nome=&pagina=&tamanhoPagina=
 *              PATCH /api/clientes/{id}/inativar
 *              (via useListaClientes / useInativarCliente)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { EditOutlined, PlusOutlined, StopOutlined } from '@ant-design/icons'
import { Alert, App, Button, Card, Flex, Input, Popconfirm, Table, Tag, Tooltip, Typography } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { useInativarCliente, useListaClientes } from '../../hooks/useClientes'
import type { Cliente, ClienteFiltro } from '../../types/cliente'
import { formatarDocumento } from '../../utils/documento'
import { ClienteFormModal } from './ClienteFormModal'

const formatoData = new Intl.DateTimeFormat('pt-BR')

export function ClientesListaPage() {
  const { message } = App.useApp()

  const [filtro, setFiltro] = useState<ClienteFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [modalAberto, setModalAberto] = useState(false)
  const [clienteEmEdicao, setClienteEmEdicao] = useState<Cliente | null>(null)

  const { data, isFetching, isError, error } = useListaClientes(filtro)
  const inativarCliente = useInativarCliente()

  function abrirInclusao() {
    setClienteEmEdicao(null)
    setModalAberto(true)
  }

  function abrirEdicao(cliente: Cliente) {
    setClienteEmEdicao(cliente)
    setModalAberto(true)
  }

  async function inativar(cliente: Cliente) {
    try {
      await inativarCliente.mutateAsync(cliente.id)
      message.success(`Cliente "${cliente.nome}" inativado.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const colunas: TableProps<Cliente>['columns'] = [
    { title: 'Nome', dataIndex: 'nome', ellipsis: true },
    {
      title: 'CPF/CNPJ',
      dataIndex: 'documento',
      width: 170,
      render: (documento: string) => formatarDocumento(documento),
    },
    { title: 'E-mail', dataIndex: 'email', ellipsis: true, responsive: ['lg'] },
    { title: 'Telefone', dataIndex: 'telefone', width: 140, responsive: ['md'] },
    {
      title: 'Cidade/UF',
      key: 'cidadeUf',
      width: 170,
      ellipsis: true,
      render: (_, cliente) => `${cliente.cidade}/${cliente.uf}`,
    },
    {
      title: 'Status',
      dataIndex: 'ativo',
      width: 90,
      render: (ativo: boolean) => (ativo ? <Tag color="green">Ativo</Tag> : <Tag>Inativo</Tag>),
    },
    {
      title: 'Cadastro',
      dataIndex: 'dataCadastro',
      width: 105,
      responsive: ['xl'],
      render: (data: string) => formatoData.format(new Date(data)),
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 96,
      align: 'center',
      render: (_, cliente) => (
        <Flex gap={4} justify="center">
          <Tooltip title="Editar">
            <Button type="text" icon={<EditOutlined />} onClick={() => abrirEdicao(cliente)} />
          </Tooltip>
          <Popconfirm
            title="Inativar cliente"
            description={`Deseja inativar "${cliente.nome}"?`}
            okText="Inativar"
            cancelText="Cancelar"
            okButtonProps={{ danger: true }}
            onConfirm={() => inativar(cliente)}
            disabled={!cliente.ativo}
          >
            <Tooltip title={cliente.ativo ? 'Inativar' : 'Cliente já inativo'}>
              <Button type="text" danger icon={<StopOutlined />} disabled={!cliente.ativo} />
            </Tooltip>
          </Popconfirm>
        </Flex>
      ),
    },
  ]

  return (
    <Card>
      <Flex justify="space-between" align="center" wrap gap={16} style={{ marginBottom: 16 }}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          Clientes
        </Typography.Title>

        <Flex gap={8} wrap>
          <Input.Search
            placeholder="Buscar por nome"
            allowClear
            enterButton
            maxLength={150}
            style={{ width: 300 }}
            onSearch={(nome) => setFiltro((atual) => ({ ...atual, nome: nome.trim() || undefined, pagina: 1 }))}
          />
          <Button type="primary" icon={<PlusOutlined />} onClick={abrirInclusao}>
            Novo cliente
          </Button>
        </Flex>
      </Flex>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar os clientes."
          description={lerErroApi(error).mensagem}
          style={{ marginBottom: 16 }}
        />
      )}

      <Table<Cliente>
        rowKey="id"
        columns={colunas}
        dataSource={data?.itens}
        loading={isFetching}
        locale={{ emptyText: 'Nenhum cliente encontrado.' }}
        scroll={{ x: 900 }}
        pagination={{
          current: filtro.pagina,
          pageSize: filtro.tamanhoPagina,
          total: data?.totalItens ?? 0,
          showSizeChanger: true,
          pageSizeOptions: [10, 20, 50, 100],
          showTotal: (total) => `${total} cliente(s)`,
        }}
        onChange={(paginacao) =>
          setFiltro((atual) => ({
            ...atual,
            pagina: paginacao.current ?? 1,
            tamanhoPagina: paginacao.pageSize ?? atual.tamanhoPagina,
          }))
        }
      />

      <ClienteFormModal aberto={modalAberto} cliente={clienteEmEdicao} aoFechar={() => setModalAberto(false)} />
    </Card>
  )
}
