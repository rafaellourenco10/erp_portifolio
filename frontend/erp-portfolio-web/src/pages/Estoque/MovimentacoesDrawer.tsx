/**
 * =====================================================================
 * Arquivo....: MovimentacoesDrawer.tsx
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Painel lateral (Drawer) somente leitura com o extrato de
 *              movimentações de um produto (mais recente primeiro):
 *              tipo, quantidade, motivo/origem e data.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/estoque/{produtoId}/movimentacoes?pagina=&tamanhoPagina=
 *              (via useMovimentacoesProduto)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { ArrowDownOutlined, ArrowUpOutlined } from '@ant-design/icons'
import { Drawer, Grid, Table, Tag } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { useMovimentacoesProduto } from '../../hooks/useEstoque'
import type { EstoqueFiltro, EstoqueResumo, Movimentacao } from '../../types/estoque'
import { formatarQuantidade } from '../../utils/moeda'
// O painel reaproveita as classes .drawer-cliente* do módulo de Clientes.
import '../Clientes/clientes.css'

const formatoDataHora = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
const FILTRO_INICIAL: EstoqueFiltro = { pagina: 1, tamanhoPagina: 10 }

interface MovimentacoesDrawerProps {
  produto: EstoqueResumo | null
  aoFechar: () => void
}

export function MovimentacoesDrawer({ produto, aoFechar }: MovimentacoesDrawerProps) {
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.sm === false

  const [filtro, setFiltro] = useState<EstoqueFiltro>(FILTRO_INICIAL)
  const [produtoIdAnterior, setProdutoIdAnterior] = useState<number | null>(null)

  // Volta pra página 1 ao trocar de produto (inclui fechar: produto vira null), ajustando o
  // estado durante a renderização em vez de um efeito (evita um segundo commit desnecessário).
  const produtoIdAtual = produto?.produtoId ?? null
  if (produtoIdAtual !== produtoIdAnterior) {
    setProdutoIdAnterior(produtoIdAtual)
    setFiltro(FILTRO_INICIAL)
  }

  const { data, isFetching } = useMovimentacoesProduto(produtoIdAtual, filtro)

  const colunas: TableProps<Movimentacao>['columns'] = [
    {
      title: 'Tipo',
      dataIndex: 'tipo',
      width: 110,
      render: (tipo: Movimentacao['tipo']) =>
        tipo === 'Entrada' ? (
          <Tag color="success" icon={<ArrowUpOutlined />}>
            Entrada
          </Tag>
        ) : (
          <Tag color="error" icon={<ArrowDownOutlined />}>
            Saída
          </Tag>
        ),
    },
    {
      title: 'Quantidade',
      dataIndex: 'quantidade',
      width: 110,
      align: 'right',
      render: (quantidade: number) => <span className="numeros-tabulares">{formatarQuantidade(quantidade)}</span>,
    },
    {
      title: 'Motivo',
      dataIndex: 'motivo',
      ellipsis: true,
      render: (motivo: string | null) => motivo ?? <span className="celula-vazia">—</span>,
    },
    {
      title: 'Data',
      dataIndex: 'dataMovimentacao',
      width: 140,
      align: 'right',
      render: (data: string) => <span className="numeros-tabulares">{formatoDataHora.format(new Date(data))}</span>,
    },
  ]

  return (
    <Drawer
      open={produto !== null}
      onClose={aoFechar}
      size={telas.sm === false ? '100%' : 640}
      closable={{ placement: 'end' }}
      destroyOnHidden
      className="drawer-cliente"
      title={
        <div>
          <div className="drawer-cliente-titulo">
            <span className="ponto-destaque" />
            Movimentações — {produto?.produtoNome}
          </div>
          <div className="drawer-cliente-subtitulo">
            SKU {produto?.sku} · Saldo atual: {produto ? formatarQuantidade(produto.saldo) : ''}
          </div>
        </div>
      }
    >
      <Table<Movimentacao>
        rowKey={(linha) => `${linha.tipo}-${linha.dataMovimentacao}-${linha.pedidoId ?? 'manual'}`}
        columns={colunas}
        dataSource={data?.itens}
        loading={isFetching}
        locale={{ emptyText: 'Nenhuma movimentação para este produto.' }}
        scroll={ehCelular ? undefined : { x: 480 }}
        pagination={{
          current: filtro.pagina,
          pageSize: filtro.tamanhoPagina,
          total: data?.totalItens ?? 0,
          size: 'small',
          showSizeChanger: false,
        }}
        onChange={(paginacao) =>
          setFiltro((atual) => ({ ...atual, pagina: paginacao.current ?? 1 }))
        }
      />
    </Drawer>
  )
}
