/**
 * =====================================================================
 * Arquivo....: FluxoCaixaPage.tsx
 * Versão.....: 1.1.0
 * Data.......: 24/09/2026
 * Descrição..: Tela do Fluxo de Caixa (Financeiro, etapa 15): visão Diária
 *              (abre no mês atual) ou Mensal (ano atual), cards de saldo
 *              inicial, entradas, saídas, saldo final e menor saldo, avisos de
 *              atrasados e de saldo negativo, e a tabela por período com
 *              realizado × previsto e o saldo. Tudo calculado no servidor.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/fluxo-caixa?dataInicio=&dataFim=&agrupamento=&formato=
 *              (via useFluxoCaixa / BotoesExportar)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 *   1.1.0 - 24/09/2026 - Gráfico (GraficoFluxoCaixa) entre os cards e a tabela (T4).
 * =====================================================================
 */

import { SearchOutlined } from '@ant-design/icons'
import { Alert, Button, Col, DatePicker, Flex, Row, Segmented, Table, Typography } from 'antd'
import type { TableProps } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import { useState } from 'react'
import { lerErroApi } from '../../api/axiosClient'
import { fluxoCaixaApi } from '../../api/fluxoCaixaApi'
import { BotoesExportar } from '../../components/BotoesExportar'
import { useFluxoCaixa } from '../../hooks/useFluxoCaixa'
import type { AgrupamentoFluxoCaixa, FluxoCaixaFiltro, PeriodoFluxoCaixa } from '../../types/fluxoCaixa'
import { formatarReal } from '../../utils/moeda'
import { CardIndicador } from '../Dashboard/CardIndicador'
import { GraficoFluxoCaixa } from './GraficoFluxoCaixa'
// A tela reaproveita as classes .painel, .pagina-titulo etc. do módulo de Clientes.
import '../Clientes/clientes.css'
import './fluxoCaixa.css'

const opcoesVisao: { label: string; value: AgrupamentoFluxoCaixa }[] = [
  { label: 'Diário', value: 'Dia' },
  { label: 'Mensal', value: 'Mes' },
]

const formatoDia = new Intl.DateTimeFormat('pt-BR', { weekday: 'short', day: '2-digit', month: '2-digit' })
const formatoMes = new Intl.DateTimeFormat('pt-BR', { month: 'short', year: 'numeric' })
const formatoData = new Intl.DateTimeFormat('pt-BR')

/** "AAAA-MM-DD" como data local (sem o deslocamento de fuso do new Date(iso)). */
function paraData(iso: string) {
  const [ano, mes, dia] = iso.split('-').map(Number)
  return new Date(ano, mes - 1, dia)
}

function rotuloPeriodo(inicio: string, agrupamento: AgrupamentoFluxoCaixa) {
  return (agrupamento === 'Dia' ? formatoDia : formatoMes).format(paraData(inicio))
}

/** Período padrão de cada visão: mês atual (diário) ou ano atual (mensal). */
function periodoPadrao(agrupamento: AgrupamentoFluxoCaixa): [Dayjs, Dayjs] {
  const unidade = agrupamento === 'Dia' ? 'month' : 'year'
  return [dayjs().startOf(unidade), dayjs().endOf(unidade)]
}

function paraFiltro(agrupamento: AgrupamentoFluxoCaixa, [inicio, fim]: [Dayjs, Dayjs]): FluxoCaixaFiltro {
  // Na visão mensal o seletor escolhe meses: do 1º dia do primeiro ao último dia do último.
  const [de, ate] = agrupamento === 'Mes' ? [inicio.startOf('month'), fim.endOf('month')] : [inicio, fim]
  return { dataInicio: de.format('YYYY-MM-DD'), dataFim: ate.format('YYYY-MM-DD'), agrupamento }
}

const moeda = (valor: number) =>
  valor === 0 ? <span className="texto-discreto">—</span> : <span className="numeros-tabulares">{formatarReal(valor)}</span>

export function FluxoCaixaPage() {
  const [agrupamento, setAgrupamento] = useState<AgrupamentoFluxoCaixa>('Dia')
  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs]>(() => periodoPadrao('Dia'))
  // Abre já calculado no mês atual; depois só recalcula ao trocar a visão ou clicar em "Gerar".
  const [filtro, setFiltro] = useState<FluxoCaixaFiltro>(() => paraFiltro('Dia', periodoPadrao('Dia')))
  const { data, isFetching, isError, error } = useFluxoCaixa(filtro)

  function trocarVisao(nova: AgrupamentoFluxoCaixa) {
    const padrao = periodoPadrao(nova)
    setAgrupamento(nova)
    setPeriodo(padrao)
    setFiltro(paraFiltro(nova, padrao))
  }

  // "Hoje" do servidor: onde termina o realizado; na visão mensal, o mês que contém hoje.
  const chaveHoje = data ? (filtro.agrupamento === 'Dia' ? data.hoje : `${data.hoje.slice(0, 7)}-01`) : null
  const carregando = isFetching && !data
  const temAtrasados = !!data && (data.atrasadoReceber > 0 || data.atrasadoPagar > 0)

  const colunas: TableProps<PeriodoFluxoCaixa>['columns'] = [
    {
      title: filtro.agrupamento === 'Dia' ? 'Dia' : 'Mês',
      dataIndex: 'inicio',
      width: 130,
      fixed: 'left',
      render: (inicio: string) => (
        <span className="numeros-tabulares">
          {rotuloPeriodo(inicio, filtro.agrupamento)}
          {inicio === chaveHoje && <span className="marca-hoje">hoje</span>}
        </span>
      ),
    },
    {
      title: 'Entradas',
      children: [
        { title: 'Realizadas', dataIndex: 'entradasRealizadas', align: 'right', width: 130, render: moeda },
        { title: 'Previstas', dataIndex: 'entradasPrevistas', align: 'right', width: 130, render: moeda },
      ],
    },
    {
      title: 'Saídas',
      children: [
        { title: 'Realizadas', dataIndex: 'saidasRealizadas', align: 'right', width: 130, render: moeda },
        { title: 'Previstas', dataIndex: 'saidasPrevistas', align: 'right', width: 130, render: moeda },
      ],
    },
    {
      title: 'Resultado',
      key: 'resultado',
      align: 'right',
      width: 130,
      render: (_, p) => moeda(p.entradasRealizadas + p.entradasPrevistas - p.saidasRealizadas - p.saidasPrevistas),
    },
    {
      title: 'Saldo',
      dataIndex: 'saldo',
      align: 'right',
      width: 140,
      render: (saldo: number) => (
        <strong className="numeros-tabulares" style={saldo < 0 ? { color: 'var(--cor-erro)' } : undefined}>
          {formatarReal(saldo)}
        </strong>
      ),
    },
  ]

  return (
    <div className="pagina-fluxo-caixa">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Fluxo de Caixa</h1>
          <p className="pagina-subtitulo">
            O que entrou e saiu (realizado) e o que vai entrar e sair (previsto, pelo vencimento das parcelas pendentes), com
            o saldo acumulado.
          </p>
        </div>
      </Flex>

      <section className="painel" style={{ padding: 16, marginBottom: 16 }} aria-label="Filtros do fluxo de caixa">
        <Flex gap={12} wrap align="flex-end">
          <Flex vertical gap={4}>
            <span className="rotulo-filtro">Visão</span>
            <Segmented size="large" options={opcoesVisao} value={agrupamento} onChange={trocarVisao} />
          </Flex>
          <Flex vertical gap={4}>
            <span className="rotulo-filtro">Período</span>
            <DatePicker.RangePicker
              size="large"
              picker={agrupamento === 'Dia' ? 'date' : 'month'}
              format={agrupamento === 'Dia' ? 'DD/MM/YYYY' : 'MM/YYYY'}
              value={periodo}
              onChange={(datas) => datas?.[0] && datas[1] && setPeriodo([datas[0], datas[1]])}
              allowClear={false}
              aria-label="Período"
            />
          </Flex>
          <Button type="primary" size="large" icon={<SearchOutlined />} onClick={() => setFiltro(paraFiltro(agrupamento, periodo))} loading={isFetching}>
            Gerar
          </Button>
          <BotoesExportar baixar={(formato) => fluxoCaixaApi.exportar(filtro, formato)} desativado={!data || isError} />
        </Flex>
      </section>

      {isError && (
        <Alert
          type="error"
          showIcon
          title="Não foi possível calcular o fluxo de caixa."
          description={lerErroApi(error).mensagem}
          className="alerta-erro"
        />
      )}

      {data && data.menorSaldo < 0 && (
        <Alert
          type="error"
          showIcon
          className="alerta-erro"
          title={`O saldo fica negativo: ${formatarReal(data.menorSaldo)} em ${
            filtro.agrupamento === 'Dia' ? formatoData.format(paraData(data.dataMenorSaldo)) : rotuloPeriodo(data.dataMenorSaldo, 'Mes')
          }.`}
          description="Antecipe recebimentos ou adie pagamentos para cobrir esse período."
        />
      )}

      {temAtrasados && (
        <Alert
          type="warning"
          showIcon
          className="alerta-erro"
          title={`Em atraso: ${formatarReal(data.atrasadoReceber)} a receber e ${formatarReal(data.atrasadoPagar)} a pagar.`}
          description="Parcelas pendentes já vencidas contam como previstas para hoje."
        />
      )}

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        {[
          { titulo: 'Saldo inicial', valor: data?.saldoInicial },
          { titulo: 'Entradas', valor: data?.totalEntradas, tipo: 'success' as const },
          { titulo: 'Saídas', valor: data?.totalSaidas, tipo: 'danger' as const },
          { titulo: 'Saldo final', valor: data?.saldoFinal, negativo: (data?.saldoFinal ?? 0) < 0 },
        ].map((card) => (
          <Col key={card.titulo} flex="1 1 190px">
            <CardIndicador titulo={card.titulo} loading={carregando} erro={isError}>
              <Typography.Title
                level={3}
                type={card.negativo ? 'danger' : card.tipo}
                className="numeros-tabulares"
                style={{ margin: 0, whiteSpace: 'nowrap' }}
              >
                {card.valor !== undefined && formatarReal(card.valor)}
              </Typography.Title>
            </CardIndicador>
          </Col>
        ))}
        <Col flex="1 1 190px">
          <CardIndicador titulo="Menor saldo" loading={carregando} erro={isError}>
            {data && (
              <>
                <Typography.Title level={3} type={data.menorSaldo < 0 ? 'danger' : undefined} className="numeros-tabulares" style={{ margin: 0, whiteSpace: 'nowrap' }}>
                  {formatarReal(data.menorSaldo)}
                </Typography.Title>
                <span className="texto-discreto numeros-tabulares">
                  {filtro.agrupamento === 'Dia' ? formatoData.format(paraData(data.dataMenorSaldo)) : rotuloPeriodo(data.dataMenorSaldo, 'Mes')}
                </span>
              </>
            )}
          </CardIndicador>
        </Col>
      </Row>

      {data && data.periodos.length > 0 && (
        <section className="painel" style={{ padding: 16, marginBottom: 16, opacity: isFetching ? 0.6 : 1 }} aria-label="Gráfico do fluxo de caixa">
          <GraficoFluxoCaixa periodos={data.periodos} chaveHoje={chaveHoje} rotulo={(inicio) => rotuloPeriodo(inicio, filtro.agrupamento)} />
        </section>
      )}

      <section className="painel painel-tabela" aria-label="Fluxo de caixa por período">
        <Table<PeriodoFluxoCaixa>
          rowKey="inicio"
          size="small"
          bordered
          columns={colunas}
          dataSource={data?.periodos ?? []}
          loading={isFetching}
          pagination={false}
          scroll={{ x: 960 }}
          rowClassName={(p) => (p.inicio === chaveHoje ? 'linha-hoje' : p.inicio < (chaveHoje ?? '') ? '' : 'linha-prevista')}
        />
      </section>
    </div>
  )
}
