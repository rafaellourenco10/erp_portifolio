/**
 * =====================================================================
 * Arquivo....: EstoqueListaPage.tsx
 * Versão.....: 1.1.0
 * Data.......: 22/09/2026
 * Descrição..: Tela de estoque: busca por nome ou SKU do produto, tabela
 *              com o saldo atual (paginação no servidor), botão para
 *              lançar uma entrada manual e ação para ver o extrato de
 *              movimentações de um produto. No celular, SKU/nome/saldo
 *              ficam numa única coluna (como em Produtos).
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/estoque?busca=&pagina=&tamanhoPagina=
 *              (via useListaEstoque)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 22/09/2026 - Colunas compactas no celular (T10).
 * =====================================================================
 */

import { HistoryOutlined, PlusOutlined } from '@ant-design/icons'
import { Alert, Button, Flex, Grid, Input, Table, Tooltip, Typography } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { useListaEstoque } from '../../hooks/useEstoque'
import type { EstoqueFiltro, EstoqueResumo } from '../../types/estoque'
import { formatarQuantidade } from '../../utils/moeda'
import { EntradaEstoqueDrawer } from './EntradaEstoqueDrawer'
import { MovimentacoesDrawer } from './MovimentacoesDrawer'
// A tela reaproveita as classes .painel, .pagina-titulo, .celula-nome etc. do módulo de Clientes.
import '../Clientes/clientes.css'

function ColunaSaldo({ saldo, unidade }: { saldo: number; unidade?: string }) {
  const texto = unidade ? `${formatarQuantidade(saldo)} ${unidade}` : formatarQuantidade(saldo)
  return <span className="numeros-tabulares">{saldo < 0 ? <Typography.Text type="danger">{texto}</Typography.Text> : texto}</span>
}

export function EstoqueListaPage() {
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [filtro, setFiltro] = useState<EstoqueFiltro>({ pagina: 1, tamanhoPagina: 10 })
  const [entradaAberta, setEntradaAberta] = useState(false)
  const [produtoDoExtrato, setProdutoDoExtrato] = useState<EstoqueResumo | null>(null)

  const { data, isFetching, isError, error } = useListaEstoque(filtro)

  const colunaAcoes: NonNullable<TableProps<EstoqueResumo>['columns']>[number] = {
    title: 'Ações',
    key: 'acoes',
    width: 72,
    align: 'center',
    render: (_, produto) => (
      <Tooltip title="Ver movimentações">
        <Button
          type="text"
          icon={<HistoryOutlined />}
          aria-label={`Ver movimentações de ${produto.produtoNome}`}
          onClick={() => setProdutoDoExtrato(produto)}
        />
      </Tooltip>
    ),
  }

  // No celular, SKU, nome, unidade e saldo ficam empilhados numa única coluna (como em Produtos).
  const colunasCelular: TableProps<EstoqueResumo>['columns'] = [
    {
      title: 'Produto',
      key: 'produto',
      render: (_, produto) => (
        <div className="celula-compacta">
          <span className="celula-nome">{produto.produtoNome}</span>
          <span className="pilula-documento numeros-tabulares">{produto.sku}</span>
          <ColunaSaldo saldo={produto.saldo} unidade={produto.unidade} />
        </div>
      ),
    },
    colunaAcoes,
  ]

  const colunas: TableProps<EstoqueResumo>['columns'] = [
    {
      title: 'SKU',
      dataIndex: 'sku',
      width: 170,
      render: (sku: string) => <span className="pilula-documento numeros-tabulares">{sku}</span>,
    },
    {
      title: 'Produto',
      dataIndex: 'produtoNome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    {
      title: 'Un.',
      dataIndex: 'unidade',
      width: 64,
      align: 'center',
      responsive: ['sm'],
    },
    {
      title: 'Saldo',
      key: 'saldo',
      width: 120,
      align: 'right',
      render: (_, produto) => <ColunaSaldo saldo={produto.saldo} />,
    },
    colunaAcoes,
  ]

  return (
    <div className="pagina-estoque">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Estoque</h1>
          <p className="pagina-subtitulo">Saldo de cada produto, com entradas manuais e baixa automática pelas vendas.</p>
        </div>
        <Flex gap={12} wrap>
          <Input.Search
            size="large"
            placeholder="Buscar por nome ou SKU"
            allowClear
            maxLength={150}
            aria-label="Buscar produto"
            className="campo-busca-categoria"
            onSearch={(texto) => setFiltro((atual) => ({ ...atual, busca: texto.trim() || undefined, pagina: 1 }))}
          />
          <Button type="primary" size="large" icon={<PlusOutlined />} onClick={() => setEntradaAberta(true)}>
            Nova entrada
          </Button>
        </Flex>
      </Flex>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível carregar o estoque."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      <section className="painel painel-tabela" aria-label="Lista de estoque">
        <Table<EstoqueResumo>
          rowKey="produtoId"
          columns={ehCelular ? colunasCelular : colunas}
          dataSource={data?.itens}
          loading={isFetching}
          locale={{
            emptyText: filtro.busca ? 'Nenhum produto corresponde à busca.' : 'Nenhum produto cadastrado.',
          }}
          scroll={ehCelular ? undefined : { x: 520 }}
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

      <EntradaEstoqueDrawer aberto={entradaAberta} aoFechar={() => setEntradaAberta(false)} />
      <MovimentacoesDrawer produto={produtoDoExtrato} aoFechar={() => setProdutoDoExtrato(null)} />
    </div>
  )
}
