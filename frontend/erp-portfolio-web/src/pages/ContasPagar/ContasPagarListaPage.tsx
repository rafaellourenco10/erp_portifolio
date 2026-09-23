/**
 * =====================================================================
 * Arquivo....: ContasPagarListaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de contas a pagar: busca por fornecedor ou número do pedido de
 *              compra, filtro de status (incluindo "Atrasado", calculado), tabela
 *              com paginação no servidor e ação de marcar parcela como paga. Espelho
 *              da tela de Contas a Receber (mesmo layout, inclusive no celular).
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/contas-pagar?busca=&status=&pagina=&tamanhoPagina=
 *              PATCH /api/contas-pagar/{id}/pagar
 *              (via useListaContasPagar / usePagarParcela)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { App, Alert, Button, Flex, Grid, Input, Popconfirm, Segmented, Table, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatusParcela } from '../../components/TagStatusParcela'
import { useListaContasPagar, usePagarParcela } from '../../hooks/useContasPagar'
import type { FiltroStatusParcelaPagar, ParcelaPagar, ParcelaPagarFiltro } from '../../types/contaPagar'
import { formatarReal } from '../../utils/moeda'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'

type StatusTela = 'Todas' | FiltroStatusParcelaPagar

const opcoesStatus: { label: string; value: StatusTela }[] = [
  { label: 'Todas', value: 'Todas' },
  { label: 'Pendentes', value: 'Pendente' },
  { label: 'Atrasadas', value: 'Atrasado' },
  { label: 'Pagas', value: 'Pago' },
  { label: 'Canceladas', value: 'Cancelado' },
]

/** "2026-10-22" -> "22/10/2026", sem passar por Date (evita erro de fuso horário numa data sem hora). */
function formatarData(iso: string): string {
  const [ano, mes, dia] = iso.split('-')
  return `${dia}/${mes}/${ano}`
}

export function ContasPagarListaPage() {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<ParcelaPagarFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const { data, isFetching, isError, error } = useListaContasPagar(filtro)
  const pagarParcela = usePagarParcela()

  const statusAtual: StatusTela = filtro.status ?? 'Todas'

  async function pagar(parcela: ParcelaPagar) {
    try {
      await pagarParcela.mutateAsync(parcela.id)
      message.success(`Parcela ${parcela.numeroParcela}/${parcela.totalParcelas} do pedido de compra nº ${parcela.pedidoCompraId} marcada como paga.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const colunaAcoes: NonNullable<TableProps<ParcelaPagar>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 72,
    align: 'center',
    render: (_, parcela) => {
      const botao = (
        <Button
          type="text"
          icon={<CheckOutlined />}
          aria-label={`Marcar como paga a parcela ${parcela.numeroParcela} do pedido de compra ${parcela.pedidoCompraId}`}
          disabled={parcela.status !== 'Pendente'}
        />
      )

      // Tooltip e Popconfirm no mesmo botão abririam os dois balões ao mesmo tempo (um por cima do outro);
      // o Popconfirm já explica a ação no título, então o Tooltip só entra quando o botão está desabilitado.
      return parcela.status === 'Pendente' ? (
        <Popconfirm
          title="Marcar como paga"
          description={`Confirmar o pagamento da parcela ${parcela.numeroParcela}/${parcela.totalParcelas} (${formatarReal(parcela.valor)})?`}
          okText="Marcar paga"
          cancelText="Cancelar"
          onConfirm={() => pagar(parcela)}
        >
          {botao}
        </Popconfirm>
      ) : (
        <Tooltip title="Só parcelas pendentes podem ser pagas">{botao}</Tooltip>
      )
    },
  }

  // No celular, fornecedor/compra/parcela/valor/vencimento/status ficam empilhados numa única coluna.
  const colunasCelular: TableProps<ParcelaPagar>['columns'] = [
    {
      title: 'Parcela',
      key: 'resumo',
      render: (_, parcela) => (
        <div className="celula-compacta">
          <span className="celula-nome">{parcela.fornecedorNome}</span>
          <span className="texto-discreto numeros-tabulares">
            Compra #{parcela.pedidoCompraId} · Parcela {parcela.numeroParcela}/{parcela.totalParcelas}
          </span>
          <Flex align="center" gap={8} wrap>
            <span className="numeros-tabulares">
              {formatarReal(parcela.valor)} · vence {formatarData(parcela.vencimento)}
            </span>
            <TagStatusParcela status={parcela.status} atrasado={parcela.atrasado} />
          </Flex>
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<ParcelaPagar>['columns'] = [
    {
      title: 'Fornecedor',
      dataIndex: 'fornecedorNome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Compra',
      dataIndex: 'pedidoCompraId',
      width: 90,
      align: 'center',
      responsive: ['sm'],
      render: (id: number) => <span className="numeros-tabulares">#{id}</span>,
    },
    {
      title: 'Parcela',
      key: 'parcela',
      width: 90,
      align: 'center',
      render: (_, parcela) => (
        <span className="numeros-tabulares">
          {parcela.numeroParcela}/{parcela.totalParcelas}
        </span>
      ),
    },
    {
      title: 'Valor',
      dataIndex: 'valor',
      width: 130,
      align: 'right',
      render: (valor: number) => <span className="numeros-tabulares">{formatarReal(valor)}</span>,
    },
    {
      title: 'Vencimento',
      dataIndex: 'vencimento',
      width: 120,
      align: 'right',
      responsive: ['sm'],
      render: (vencimento: string) => <span className="numeros-tabulares">{formatarData(vencimento)}</span>,
    },
    {
      title: 'Status',
      key: 'status',
      width: 130,
      align: 'center',
      render: (_, parcela) => <TagStatusParcela status={parcela.status} atrasado={parcela.atrasado} />,
    },
    colunaAcoes,
  ]

  return (
    <div className="pagina-contas-pagar">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Contas a Pagar</h1>
          <p className="pagina-subtitulo">Parcelas geradas ao confirmar pedidos de compra, com vencimento e status de pagamento.</p>
        </div>
        <Flex gap={12} wrap>
          <Input.Search
            size="large"
            placeholder="Buscar por fornecedor ou nº da compra"
            allowClear
            maxLength={150}
            aria-label="Buscar"
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
                status: status === 'Todas' ? undefined : (status as FiltroStatusParcelaPagar),
                pagina: 1,
              }))
            }
          />
        </Flex>
      </Flex>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar as contas a pagar."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de contas a pagar">
        <Table<ParcelaPagar>
          rowKey="id"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          rowClassName={(parcela) => (parcela.status === 'Cancelado' ? 'linha-inativa' : '')}
          locale={{
            emptyText: filtro.busca || filtro.status ? 'Nenhuma parcela corresponde aos filtros.' : 'Nenhuma parcela gerada ainda.',
          }}
          scroll={ehCelular ? undefined : { x: 700 }}
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
