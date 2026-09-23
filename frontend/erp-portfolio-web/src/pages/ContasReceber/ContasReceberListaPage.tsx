/**
 * =====================================================================
 * Arquivo....: ContasReceberListaPage.tsx
 * Versão.....: 1.2.0
 * Data.......: 22/09/2026
 * Descrição..: Tela de contas a receber: busca por cliente ou número do pedido,
 *              filtro de status (incluindo "Atrasado", calculado), tabela com
 *              paginação no servidor e ação de marcar parcela como recebida. No
 *              celular, cliente/pedido/parcela/valor/vencimento/status ficam
 *              numa única coluna (como em Produtos e Estoque).
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/contas-receber?busca=&status=&pagina=&tamanhoPagina=
 *              PATCH /api/contas-receber/{id}/receber
 *              (via useListaContasReceber / useReceberParcela)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 22/09/2026 - Colunas compactas no celular (T9).
 *   1.2.0 - 22/09/2026 - Corrige Tooltip sobrepondo o botão do Popconfirm ao
 *                        marcar como recebido (Tooltip só aparece desabilitado).
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { App, Alert, Button, Flex, Grid, Input, Popconfirm, Segmented, Table, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { TagStatusParcela } from '../../components/TagStatusParcela'
import { useListaContasReceber, useReceberParcela } from '../../hooks/useContasReceber'
import type { FiltroStatusParcela, Parcela, ParcelaFiltro } from '../../types/contaReceber'
import { formatarReal } from '../../utils/moeda'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'

type StatusTela = 'Todas' | FiltroStatusParcela

const opcoesStatus: { label: string; value: StatusTela }[] = [
  { label: 'Todas', value: 'Todas' },
  { label: 'Pendentes', value: 'Pendente' },
  { label: 'Atrasadas', value: 'Atrasado' },
  { label: 'Recebidas', value: 'Recebido' },
  { label: 'Canceladas', value: 'Cancelado' },
]

/** "2026-10-22" -> "22/10/2026", sem passar por Date (evita erro de fuso horário numa data sem hora). */
function formatarData(iso: string): string {
  const [ano, mes, dia] = iso.split('-')
  return `${dia}/${mes}/${ano}`
}

export function ContasReceberListaPage() {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<ParcelaFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const { data, isFetching, isError, error } = useListaContasReceber(filtro)
  const receberParcela = useReceberParcela()

  const statusAtual: StatusTela = filtro.status ?? 'Todas'

  async function receber(parcela: Parcela) {
    try {
      await receberParcela.mutateAsync(parcela.id)
      message.success(`Parcela ${parcela.numeroParcela}/${parcela.totalParcelas} do pedido nº ${parcela.pedidoId} marcada como recebida.`)
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const colunaAcoes: NonNullable<TableProps<Parcela>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 72,
    align: 'center',
    render: (_, parcela) => {
      const botao = (
        <Button
          type="text"
          icon={<CheckOutlined />}
          aria-label={`Marcar como recebido a parcela ${parcela.numeroParcela} do pedido ${parcela.pedidoId}`}
          disabled={parcela.status !== 'Pendente'}
        />
      )

      // Tooltip e Popconfirm no mesmo botão abririam os dois balões ao mesmo tempo (um por cima do outro);
      // o Popconfirm já explica a ação no título, então o Tooltip só entra quando o botão está desabilitado.
      return parcela.status === 'Pendente' ? (
        <Popconfirm
          title="Marcar como recebido"
          description={`Confirmar o recebimento da parcela ${parcela.numeroParcela}/${parcela.totalParcelas} (${formatarReal(parcela.valor)})?`}
          okText="Marcar recebido"
          cancelText="Cancelar"
          onConfirm={() => receber(parcela)}
        >
          {botao}
        </Popconfirm>
      ) : (
        <Tooltip title="Só parcelas pendentes podem ser recebidas">{botao}</Tooltip>
      )
    },
  }

  // No celular, cliente/pedido/parcela/valor/vencimento/status ficam empilhados numa única coluna.
  const colunasCelular: TableProps<Parcela>['columns'] = [
    {
      title: 'Parcela',
      key: 'resumo',
      render: (_, parcela) => (
        <div className="celula-compacta">
          <span className="celula-nome">{parcela.clienteNome}</span>
          <span className="texto-discreto numeros-tabulares">
            Pedido #{parcela.pedidoId} · Parcela {parcela.numeroParcela}/{parcela.totalParcelas}
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

  const colunas: TableProps<Parcela>['columns'] = [
    {
      title: 'Cliente',
      dataIndex: 'clienteNome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Pedido',
      dataIndex: 'pedidoId',
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
    <div className="pagina-contas-receber">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Contas a Receber</h1>
          <p className="pagina-subtitulo">Parcelas geradas ao confirmar pedidos, com vencimento e status de recebimento.</p>
        </div>
        <Flex gap={12} wrap>
          <Input.Search
            size="large"
            placeholder="Buscar por cliente ou nº do pedido"
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
                status: status === 'Todas' ? undefined : (status as FiltroStatusParcela),
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
          title="Não foi possível carregar as contas a receber."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de contas a receber">
        <Table<Parcela>
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
