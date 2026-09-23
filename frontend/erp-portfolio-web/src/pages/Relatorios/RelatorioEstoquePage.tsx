/**
 * =====================================================================
 * Arquivo....: RelatorioEstoquePage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Relatório de Estoque: posição atual dos produtos ativos (saldo,
 *              estoque mínimo, custo, valor em estoque e marcação de abaixo do
 *              mínimo). Filtros: categoria e "só abaixo do mínimo". Mesmo
 *              desenho da tela de Vendas/Compras: "Gerar" consulta; exportar
 *              baixa .xlsx/.pdf com os filtros do último relatório gerado.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/relatorios/estoque (?formato=xlsx|pdf)
 *              (via useRelatorioEstoque / useBaixarRelatorio / useCategoriasAtivas)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { FileExcelOutlined, FilePdfOutlined, SearchOutlined } from '@ant-design/icons'
import { Alert, App, Button, Checkbox, Col, Flex, Row, Select, Table, Typography } from 'antd'
import type { TableProps } from 'antd'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { useCategoriasAtivas } from '../../hooks/useCategorias'
import { useBaixarRelatorio, useRelatorioEstoque } from '../../hooks/useRelatorios'
import type { FormatoArquivo, RelatorioEstoqueFiltro, RelatorioEstoqueLinha } from '../../types/relatorio'
import { formatarReal } from '../../utils/moeda'
import { CardIndicador } from '../Dashboard/CardIndicador'
// A tela reaproveita as classes .painel, .pagina-titulo etc. do módulo de Clientes.
import '../Clientes/clientes.css'

const formatoQuantidade = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 3 })

export function RelatorioEstoquePage() {
  const { message } = App.useApp()
  const { data: categorias, isLoading: carregandoCategorias } = useCategoriasAtivas()

  // Campos da tela; só viram consulta ao clicar em "Gerar".
  const [categoriaId, setCategoriaId] = useState<number | undefined>()
  const [somenteAbaixoMinimo, setSomenteAbaixoMinimo] = useState(false)

  const [filtro, setFiltro] = useState<RelatorioEstoqueFiltro | null>(null)
  const { data, isFetching, isError, error } = useRelatorioEstoque(filtro)
  const baixar = useBaixarRelatorio()

  async function exportar(formato: FormatoArquivo) {
    if (!filtro) return
    try {
      await baixar.mutateAsync({ tipo: 'estoque', filtro, formato })
    } catch (erro) {
      message.error(lerErroApi(erro).mensagem)
    }
  }

  const exportando = (formato: FormatoArquivo) => baixar.isPending && baixar.variables?.formato === formato
  const quantidade = (valor: number) => <span className="numeros-tabulares">{formatoQuantidade.format(valor)}</span>
  const moeda = (valor: number) => <span className="numeros-tabulares">{formatarReal(valor)}</span>

  const colunas: TableProps<RelatorioEstoqueLinha>['columns'] = [
    {
      title: 'Produto',
      dataIndex: 'nome',
      render: (nome: string) => <span className="celula-nome">{nome}</span>,
    },
    { title: 'SKU', dataIndex: 'sku', width: 110, render: (sku: string) => <span className="numeros-tabulares">{sku}</span> },
    { title: 'Categoria', dataIndex: 'categoriaNome', render: (nome: string | null) => nome ?? '—' },
    { title: 'Un.', dataIndex: 'unidade', width: 60 },
    { title: 'Saldo', dataIndex: 'saldo', width: 90, align: 'right', render: quantidade },
    { title: 'Mínimo', dataIndex: 'estoqueMinimo', width: 90, align: 'right', render: quantidade },
    { title: 'Custo', dataIndex: 'custo', width: 120, align: 'right', render: moeda },
    { title: 'Valor em estoque', dataIndex: 'valorEstoque', width: 150, align: 'right', render: moeda },
    {
      title: 'Situação',
      dataIndex: 'abaixoMinimo',
      width: 150,
      align: 'center',
      render: (abaixo: boolean) =>
        abaixo ? (
          <span className="tag-status tag-status-cancelado">
            <span className="tag-status-ponto" />
            Abaixo do mínimo
          </span>
        ) : (
          <span className="tag-status tag-status-confirmado">
            <span className="tag-status-ponto" />
            OK
          </span>
        ),
    },
  ]

  return (
    <div className="pagina-relatorio">
      <div className="pagina-cabecalho">
        <h1 className="pagina-titulo">Relatório de Estoque</h1>
        <p className="pagina-subtitulo">Posição atual dos produtos ativos, com valor em estoque. Exporte para Excel ou PDF.</p>
      </div>

      <section className="painel" style={{ padding: 16, marginBottom: 16 }} aria-label="Filtros do relatório">
        <Flex gap={12} wrap align="flex-end">
          <Flex vertical gap={4} style={{ minWidth: 240 }}>
            <span className="rotulo-filtro">Categoria</span>
            <Select
              size="large"
              allowClear
              placeholder="Todas as categorias"
              loading={carregandoCategorias}
              options={categorias?.itens.map((c) => ({ value: c.id, label: c.nome }))}
              value={categoriaId}
              onChange={setCategoriaId}
              aria-label="Categoria"
            />
          </Flex>
          <Checkbox checked={somenteAbaixoMinimo} onChange={(e) => setSomenteAbaixoMinimo(e.target.checked)} style={{ paddingBottom: 10 }}>
            Só abaixo do mínimo
          </Checkbox>
          <Button
            type="primary"
            size="large"
            icon={<SearchOutlined />}
            onClick={() => setFiltro({ categoriaId, somenteAbaixoMinimo })}
            loading={isFetching}
          >
            Gerar
          </Button>
          <Button size="large" icon={<FileExcelOutlined />} disabled={!data} loading={exportando('xlsx')} onClick={() => exportar('xlsx')}>
            Excel
          </Button>
          <Button size="large" icon={<FilePdfOutlined />} disabled={!data} loading={exportando('pdf')} onClick={() => exportar('pdf')}>
            PDF
          </Button>
        </Flex>
      </section>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível gerar o relatório."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      {filtro === null ? (
        <Typography.Text type="secondary">Escolha os filtros e clique em Gerar.</Typography.Text>
      ) : (
        <>
          <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
            <Col xs={24} sm={8}>
              <CardIndicador titulo="Produtos" loading={isFetching && !data} erro={isError}>
                <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
                  {data?.quantidadeProdutos}
                </Typography.Title>
              </CardIndicador>
            </Col>
            <Col xs={24} sm={8}>
              <CardIndicador titulo="Valor total em estoque" loading={isFetching && !data} erro={isError}>
                <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
                  {data && formatarReal(data.valorTotalEstoque)}
                </Typography.Title>
              </CardIndicador>
            </Col>
            <Col xs={24} sm={8}>
              <CardIndicador titulo="Abaixo do mínimo" loading={isFetching && !data} erro={isError}>
                <Typography.Title
                  level={3}
                  className="numeros-tabulares"
                  type={data && data.quantidadeAbaixoMinimo > 0 ? 'warning' : undefined}
                  style={{ margin: 0 }}
                >
                  {data?.quantidadeAbaixoMinimo}
                </Typography.Title>
              </CardIndicador>
            </Col>
          </Row>

          <section className="painel painel-tabela" aria-label="Produtos do relatório">
            <Table<RelatorioEstoqueLinha>
              rowKey="produtoId"
              columns={colunas}
              dataSource={data?.linhas}
              loading={isFetching}
              locale={{ emptyText: 'Nenhum produto para os filtros informados.' }}
              scroll={{ x: 1000 }}
              pagination={{ defaultPageSize: 20, showSizeChanger: true, pageSizeOptions: [20, 50, 100], hideOnSinglePage: true }}
            />
          </section>
        </>
      )}
    </div>
  )
}
