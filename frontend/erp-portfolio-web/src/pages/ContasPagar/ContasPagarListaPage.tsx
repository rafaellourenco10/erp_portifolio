/**
 * =====================================================================
 * Arquivo....: ContasPagarListaPage.tsx
 * Versão.....: 2.2.0
 * Data.......: 23/09/2026
 * Descrição..: Tela de contas a pagar de três origens (compra, comissão e avulsa):
 *              busca por nº da compra ou favorecido/descrição, filtros de origem e de
 *              status (incluindo "Atrasado", calculado), tabela com paginação no
 *              servidor, marcar como paga, cancelar (avulsa/comissão) e "Nova conta"
 *              (avulsa). Mesmo layout da tela de Contas a Receber, inclusive no celular.
 * ---------------------------------------------------------------------
 * Fontes.....: GET   /api/contas-pagar?busca=&status=&pagina=&tamanhoPagina=
 *              GET   /api/contas-pagar/exportar?busca=&status=&origem=&formato=xlsx|pdf
 *              PATCH /api/contas-pagar/{id}/pagar e /cancelar, POST /api/contas-pagar
 *              (via useListaContasPagar / usePagarParcela / useCancelarParcela / NovaContaModal)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   2.1.0 - 24/09/2026 - Origem Devolução (reembolso) no filtro e na tag; sem Cancelar (etapa 14).
 *   2.0.0 - 23/09/2026 - Origem, favorecido e descrição; filtro de origem; Nova conta
 *                        (avulsa) e Cancelar (etapa 12).
 *   2.2.0 - 24/09/2026 - Exportar Excel/PDF com todas as parcelas do filtro.
 * =====================================================================
 */

import { CheckOutlined, PlusOutlined, StopOutlined } from '@ant-design/icons'
import { App, Alert, Button, Flex, Grid, Input, Popconfirm, Segmented, Select, Table, Tag, Tooltip } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { contasPagarApi } from '../../api/contasPagarApi'
import { BotoesExportar } from '../../components/BotoesExportar'
import { TagStatusParcela } from '../../components/TagStatusParcela'
import { useCancelarParcela, useListaContasPagar, usePagarParcela } from '../../hooks/useContasPagar'
import type { FiltroStatusParcelaPagar, OrigemContaPagar, ParcelaPagar, ParcelaPagarFiltro } from '../../types/contaPagar'
import { formatarReal } from '../../utils/moeda'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'
import { NovaContaModal } from './NovaContaModal'

type StatusTela = 'Todas' | FiltroStatusParcelaPagar

const opcoesStatus: { label: string; value: StatusTela }[] = [
  { label: 'Todas', value: 'Todas' },
  { label: 'Pendentes', value: 'Pendente' },
  { label: 'Atrasadas', value: 'Atrasado' },
  { label: 'Pagas', value: 'Pago' },
  { label: 'Canceladas', value: 'Cancelado' },
]

const opcoesOrigem: { label: string; value: OrigemContaPagar }[] = [
  { label: 'Compras', value: 'Compra' },
  { label: 'Comissões', value: 'Comissao' },
  { label: 'Avulsas', value: 'Avulsa' },
  { label: 'Reembolsos de devolução', value: 'Devolucao' },
]

const ROTULO_ORIGEM: Record<OrigemContaPagar, string> = { Compra: 'Compra', Comissao: 'Comissão', Avulsa: 'Avulsa', Devolucao: 'Devolução' }

/** O que a conta é: "Compra #N" nas de compra; a descrição nas outras. */
function descricaoDa(parcela: ParcelaPagar): string {
  return parcela.origem === 'Compra' ? `Compra #${parcela.pedidoCompraId}` : (parcela.descricao ?? '')
}

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
  const cancelarParcela = useCancelarParcela()
  const [novaContaAberta, setNovaContaAberta] = useState(false)

  const statusAtual: StatusTela = filtro.status ?? 'Todas'

  async function pagar(parcela: ParcelaPagar) {
    try {
      await pagarParcela.mutateAsync(parcela.id)
      message.success(
        parcela.origem === 'Comissao'
          ? 'Conta de comissão paga: as comissões ligadas ficaram pagas.'
          : `Parcela ${parcela.numeroParcela}/${parcela.totalParcelas} (${descricaoDa(parcela)}) marcada como paga.`,
      )
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  async function cancelar(parcela: ParcelaPagar) {
    try {
      await cancelarParcela.mutateAsync(parcela.id)
      message.success(
        parcela.origem === 'Comissao'
          ? 'Conta de comissão cancelada: as comissões voltaram para "A pagar".'
          : `Parcela ${parcela.numeroParcela}/${parcela.totalParcelas} (${descricaoDa(parcela)}) cancelada.`,
      )
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const colunaAcoes: NonNullable<TableProps<ParcelaPagar>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 96,
    align: 'center',
    render: (_, parcela) => {
      const botao = (
        <Button
          type="text"
          icon={<CheckOutlined />}
          aria-label={`Marcar como paga a parcela ${parcela.numeroParcela} de ${descricaoDa(parcela)}`}
          disabled={parcela.status !== 'Pendente'}
        />
      )

      // Tooltip e Popconfirm no mesmo botão abririam os dois balões ao mesmo tempo (um por cima do outro);
      // o Popconfirm já explica a ação no título, então o Tooltip só entra quando o botão está desabilitado.
      const acaoPagar =
        parcela.status === 'Pendente' ? (
          <Popconfirm
            title="Marcar como paga"
            description={`Confirmar o pagamento da parcela ${parcela.numeroParcela}/${parcela.totalParcelas} (${formatarReal(parcela.valor)})?`}
            okText="Marcar paga"
            cancelText="Voltar"
            onConfirm={() => pagar(parcela)}
          >
            {botao}
          </Popconfirm>
        ) : (
          <Tooltip title="Só parcelas pendentes podem ser pagas">{botao}</Tooltip>
        )

      // Cancelar só avulsa/comissão pendente; a de compra é cancelada pelo pedido de compra e o reembolso de devolução não cancela.
      const podeCancelar = parcela.status === 'Pendente' && parcela.origem !== 'Compra' && parcela.origem !== 'Devolucao'
      const botaoCancelar = (
        <Button type="text" danger icon={<StopOutlined />} aria-label={`Cancelar ${descricaoDa(parcela)}`} disabled={!podeCancelar} />
      )
      const acaoCancelar = podeCancelar ? (
        <Popconfirm
          title="Cancelar conta"
          description={
            parcela.origem === 'Comissao'
              ? 'As comissões desta conta voltam para "A pagar".'
              : `Cancelar a parcela ${parcela.numeroParcela}/${parcela.totalParcelas} (${formatarReal(parcela.valor)})?`
          }
          okText="Cancelar conta"
          okButtonProps={{ danger: true }}
          cancelText="Voltar"
          onConfirm={() => cancelar(parcela)}
        >
          {botaoCancelar}
        </Popconfirm>
      ) : (
        <Tooltip
          title={
            parcela.origem === 'Compra'
              ? 'Cancele pelo pedido de compra'
              : parcela.origem === 'Devolucao'
                ? 'Reembolso de devolução não pode ser cancelado'
                : 'Só parcelas pendentes podem ser canceladas'
          }
        >
          {botaoCancelar}
        </Tooltip>
      )

      return (
        <Flex gap={4} justify="center">
          {acaoPagar}
          {acaoCancelar}
        </Flex>
      )
    },
  }

  // No celular, favorecido/descrição/parcela/valor/vencimento/status ficam empilhados numa única coluna.
  const colunasCelular: TableProps<ParcelaPagar>['columns'] = [
    {
      title: 'Parcela',
      key: 'resumo',
      render: (_, parcela) => (
        <div className="celula-compacta">
          <span className="celula-nome">{parcela.favorecido ?? '—'}</span>
          <span className="texto-discreto numeros-tabulares">
            {descricaoDa(parcela)} · Parcela {parcela.numeroParcela}/{parcela.totalParcelas}
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
      title: 'Favorecido',
      dataIndex: 'favorecido',
      render: (nome: string | null) => (nome ? <span className="celula-nome">{nome}</span> : <span className="celula-vazia">—</span>),
    },
    {
      title: 'Descrição',
      key: 'descricao',
      ellipsis: true,
      responsive: ['sm'],
      render: (_, parcela) => (
        <Flex align="center" gap={8}>
          <Tag variant="filled">{ROTULO_ORIGEM[parcela.origem]}</Tag>
          <span className="numeros-tabulares">{descricaoDa(parcela)}</span>
        </Flex>
      ),
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
          <p className="pagina-subtitulo">Compras, comissões dos vendedores e contas avulsas (aluguel, luz...), com vencimento e status de pagamento.</p>
        </div>
        <Flex gap={12} wrap>
          <Button type="primary" size="large" icon={<PlusOutlined />} onClick={() => setNovaContaAberta(true)}>
            Nova conta
          </Button>
          <Select<OrigemContaPagar>
            size="large"
            allowClear
            placeholder="Todas as origens"
            aria-label="Origem"
            style={{ minWidth: 170 }}
            options={opcoesOrigem}
            value={filtro.origem}
            onChange={(origem) => setFiltro((atual) => ({ ...atual, origem, pagina: 1 }))}
          />
          <Input.Search
            size="large"
            placeholder="Buscar por favorecido, descrição ou nº da compra"
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
          <BotoesExportar baixar={(formato) => contasPagarApi.exportar(filtro, formato)} desativado={!data || data.totalItens === 0} />
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
            emptyText: filtro.busca || filtro.status || filtro.origem ? 'Nenhuma parcela corresponde aos filtros.' : 'Nenhuma parcela gerada ainda.',
          }}
          scroll={ehCelular ? undefined : { x: 820 }}
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

      <NovaContaModal aberto={novaContaAberta} aoFechar={() => setNovaContaAberta(false)} />
    </div>
  )
}
