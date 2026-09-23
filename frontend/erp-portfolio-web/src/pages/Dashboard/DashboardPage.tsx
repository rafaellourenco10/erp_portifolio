/**
 * =====================================================================
 * Arquivo....: DashboardPage.tsx
 * Versão.....: 1.1.0
 * Data.......: 22/09/2026
 * Descrição..: Página inicial do Ambition ERP: cards de indicador do mês atual
 *              (faturamento/ticket médio, pedidos por status, contas a receber
 *              pendente/atrasado, produtos com saldo baixo de estoque) e o
 *              gráfico de faturamento diário. Cada card busca seu próprio
 *              indicador e mostra loading/erro independente (SPEC.md, D7).
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/dashboard/vendas, /contas-receber, /estoque
 *              (via useResumoVendas / useResumoContasReceber / useResumoEstoque)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo (cards de número).
 *   1.1.0 - 22/09/2026 - Gráfico de faturamento diário (T6).
 * =====================================================================
 */

import { Col, Flex, Row, Typography } from 'antd'
import { useResumoContasReceber, useResumoEstoque, useResumoVendas } from '../../hooks/useDashboard'
import { formatarReal } from '../../utils/moeda'
import { CardIndicador } from './CardIndicador'
import { GraficoFaturamento } from './GraficoFaturamento'
// A tela reaproveita as classes .painel, .pagina-titulo etc. do módulo de Clientes.
import '../Clientes/clientes.css'

export function DashboardPage() {
  const vendas = useResumoVendas()
  const contasReceber = useResumoContasReceber()
  const estoque = useResumoEstoque()

  return (
    <div className="pagina-dashboard">
      <div className="pagina-cabecalho">
        <h1 className="pagina-titulo">Painel</h1>
        <p className="pagina-subtitulo">Indicadores do mês atual.</p>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <CardIndicador titulo="Faturamento do mês" loading={vendas.isLoading} erro={vendas.isError}>
            {vendas.data && (
              <>
                <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
                  {formatarReal(vendas.data.faturamento)}
                </Typography.Title>
                <span className="texto-discreto numeros-tabulares">
                  Ticket médio {formatarReal(vendas.data.ticketMedio)} · {vendas.data.quantidadeConfirmados} confirmado
                  {vendas.data.quantidadeConfirmados === 1 ? '' : 's'}
                </span>
              </>
            )}
          </CardIndicador>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <CardIndicador titulo="Pedidos por status (mês)" loading={vendas.isLoading} erro={vendas.isError}>
            {vendas.data && (
              <Flex justify="space-between">
                <Flex vertical align="center">
                  <span className="numeros-tabulares" style={{ fontSize: 24, fontWeight: 600 }}>
                    {vendas.data.porStatus.rascunho}
                  </span>
                  <span className="texto-discreto">Rascunho</span>
                </Flex>
                <Flex vertical align="center">
                  <span className="numeros-tabulares" style={{ fontSize: 24, fontWeight: 600, color: 'var(--cor-primaria)' }}>
                    {vendas.data.porStatus.confirmado}
                  </span>
                  <span className="texto-discreto">Confirmado</span>
                </Flex>
                <Flex vertical align="center">
                  <span className="numeros-tabulares" style={{ fontSize: 24, fontWeight: 600, color: 'var(--cor-erro)' }}>
                    {vendas.data.porStatus.cancelado}
                  </span>
                  <span className="texto-discreto">Cancelado</span>
                </Flex>
              </Flex>
            )}
          </CardIndicador>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <CardIndicador titulo="Contas a receber" loading={contasReceber.isLoading} erro={contasReceber.isError}>
            {contasReceber.data && (
              <>
                <Typography.Title level={3} className="numeros-tabulares" style={{ margin: 0 }}>
                  {formatarReal(contasReceber.data.totalPendente)}
                </Typography.Title>
                <span className="texto-discreto numeros-tabulares">
                  {contasReceber.data.quantidadePendente} pendente{contasReceber.data.quantidadePendente === 1 ? '' : 's'}
                  {contasReceber.data.quantidadeAtrasado > 0 && (
                    <Typography.Text type="danger">
                      {' '}
                      · {formatarReal(contasReceber.data.totalAtrasado)} atrasado ({contasReceber.data.quantidadeAtrasado})
                    </Typography.Text>
                  )}
                </span>
              </>
            )}
          </CardIndicador>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <CardIndicador titulo="Saldo baixo de estoque" loading={estoque.isLoading} erro={estoque.isError}>
            {estoque.data && (
              <>
                <Typography.Title
                  level={3}
                  className="numeros-tabulares"
                  type={estoque.data.quantidadeSaldoBaixo > 0 ? 'warning' : undefined}
                  style={{ margin: 0 }}
                >
                  {estoque.data.quantidadeSaldoBaixo}
                </Typography.Title>
                <span className="texto-discreto">produto{estoque.data.quantidadeSaldoBaixo === 1 ? '' : 's'} com saldo ≤ {estoque.data.limiteSaldoBaixo}</span>
              </>
            )}
          </CardIndicador>
        </Col>
      </Row>

      <div style={{ marginTop: 16 }}>
        <CardIndicador titulo="Faturamento diário" loading={vendas.isLoading} erro={vendas.isError}>
          {vendas.data && <GraficoFaturamento dados={vendas.data.faturamentoPorDia} />}
        </CardIndicador>
      </div>
    </div>
  )
}
