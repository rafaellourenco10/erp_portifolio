/**
 * =====================================================================
 * Arquivo....: GraficoFluxoCaixa.tsx
 * Versão.....: 1.0.0
 * Data.......: 24/09/2026
 * Descrição..: Gráfico do Fluxo de Caixa em dois painéis com o mesmo eixo X
 *              (small multiples — nunca dois eixos Y no mesmo plot):
 *              em cima a linha do saldo acumulado; embaixo as colunas de
 *              entradas (para cima, azul) e saídas (para baixo, laranja), com o
 *              previsto mais claro empilhado depois do realizado. SVG à mão,
 *              como o do Dashboard. Cores validadas com a skill de dataviz no
 *              fundo escuro (azul #3987E5 × laranja #D95926: CVD ΔE 26,8);
 *              verde × vermelho reprova para daltônicos. Marca de hoje,
 *              crosshair e tooltip por período (mouse e teclado); a tabela da
 *              tela é a versão acessível com todos os valores.
 * ---------------------------------------------------------------------
 * Fontes.....: FluxoCaixa.periodos (GET /api/fluxo-caixa via useFluxoCaixa).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 24/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { useState } from 'react'
import type { PeriodoFluxoCaixa } from '../../types/fluxoCaixa'
import { formatarReal } from '../../utils/moeda'

const COR_ENTRADA = '#3987E5'
const COR_SAIDA = '#D95926'
const OPACIDADE_PREVISTO = 0.45

const LARGURA = 900
const MARGEM_ESQUERDA = 72
const ALTURA_SALDO = 110
const ALTURA_BARRAS = 150
const ESPACO = 28 // entre os painéis (rótulo "Entradas e saídas")
const TOPO_BARRAS = ALTURA_SALDO + ESPACO
const ALTURA_TOTAL = TOPO_BARRAS + ALTURA_BARRAS + 22 // + faixa dos rótulos do eixo X

const compacto = new Intl.NumberFormat('pt-BR', { notation: 'compact', style: 'currency', currency: 'BRL', maximumFractionDigits: 1 })

interface GraficoFluxoCaixaProps {
  periodos: PeriodoFluxoCaixa[]
  /** Chave (AAAA-MM-DD) do período de hoje, ou null se hoje está fora. */
  chaveHoje: string | null
  rotulo: (inicio: string) => string
}

/** Retângulo com cantos de 4px só na ponta de dados (a base fica reta). */
function coluna(x: number, largura: number, base: number, altura: number, paraCima: boolean) {
  if (altura <= 0) return ''
  const r = Math.min(4, altura, largura / 2)
  const ponta = paraCima ? base - altura : base + altura
  const s = paraCima ? 1 : -1 // direção do arredondamento
  return [
    `M${x},${base}`,
    `V${ponta + s * r}`,
    `Q${x},${ponta} ${x + r},${ponta}`,
    `H${x + largura - r}`,
    `Q${x + largura},${ponta} ${x + largura},${ponta + s * r}`,
    `V${base}`,
    'Z',
  ].join(' ')
}

export function GraficoFluxoCaixa({ periodos, chaveHoje, rotulo }: GraficoFluxoCaixaProps) {
  const [ativo, setAtivo] = useState<number | null>(null)
  if (periodos.length === 0) return null

  const n = periodos.length
  const passo = (LARGURA - MARGEM_ESQUERDA) / n
  const larguraColuna = Math.max(2, Math.min(24, passo - 2))
  const centro = (i: number) => MARGEM_ESQUERDA + i * passo + passo / 2

  // Painel do saldo: escala sempre inclui o zero, para o negativo aparecer como negativo.
  const saldos = periodos.map((p) => p.saldo)
  const [minSaldo, maxSaldo] = [Math.min(0, ...saldos), Math.max(0, ...saldos)]
  const faixaSaldo = maxSaldo - minSaldo || 1
  const ySaldo = (v: number) => 8 + (1 - (v - minSaldo) / faixaSaldo) * (ALTURA_SALDO - 16)
  const linhaSaldo = periodos.map((p, i) => `${i === 0 ? 'M' : 'L'}${centro(i)},${ySaldo(p.saldo)}`).join(' ')

  // Painel das colunas: eixo no meio proporcional (entradas para cima, saídas para baixo).
  const maxEntrada = Math.max(0, ...periodos.map((p) => p.entradasRealizadas + p.entradasPrevistas))
  const maxSaida = Math.max(0, ...periodos.map((p) => p.saidasRealizadas + p.saidasPrevistas))
  const escala = (ALTURA_BARRAS - 8) / (maxEntrada + maxSaida || 1)
  const baseBarras = TOPO_BARRAS + 4 + maxEntrada * escala

  const indiceHoje = chaveHoje ? periodos.findIndex((p) => p.inicio === chaveHoje) : -1
  const cadaRotulo = Math.ceil(n / 8)
  const periodoAtivo = ativo !== null ? periodos[ativo] : null

  return (
    <div className="grafico-fluxo-caixa">
      <div className="grafico-legenda" aria-hidden="true">
        <span><i className="chave-linha" /> Saldo</span>
        <span><i className="chave-cor" style={{ background: COR_ENTRADA }} /> Entradas</span>
        <span><i className="chave-cor" style={{ background: COR_SAIDA }} /> Saídas</span>
        <span className="texto-discreto">cor clara = previsto</span>
      </div>

      {/* No celular o SVG não encolhe além do legível: rola de lado dentro do quadro, como a tabela. */}
      <div className="grafico-rolagem">
      <svg
        viewBox={`0 0 ${LARGURA} ${ALTURA_TOTAL}`}
        width="100%"
        role="img"
        aria-label="Saldo acumulado e entradas e saídas por período; os valores estão na tabela abaixo"
        onMouseLeave={() => setAtivo(null)}
      >
        {/* Painel do saldo: eixo zero e rótulos do máximo/mínimo. */}
        {maxSaldo > 0 && (
          <text x={MARGEM_ESQUERDA - 8} y={ySaldo(maxSaldo) + 4} textAnchor="end" className="eixo-rotulo">{compacto.format(maxSaldo)}</text>
        )}
        {minSaldo < 0 && (
          <text x={MARGEM_ESQUERDA - 8} y={ySaldo(minSaldo) + 4} textAnchor="end" className="eixo-rotulo">{compacto.format(minSaldo)}</text>
        )}
        <line x1={MARGEM_ESQUERDA} x2={LARGURA} y1={ySaldo(0)} y2={ySaldo(0)} stroke="var(--cor-borda)" strokeWidth={1} />
        <text x={MARGEM_ESQUERDA - 8} y={ySaldo(0) + 4} textAnchor="end" className="eixo-rotulo">R$ 0</text>
        <path d={linhaSaldo} fill="none" stroke="var(--cor-primaria)" strokeWidth={2} strokeLinejoin="round" strokeLinecap="round" />

        {/* Painel das colunas. */}
        <text x={MARGEM_ESQUERDA} y={TOPO_BARRAS - 10} className="eixo-rotulo">Entradas e saídas</text>
        <line x1={MARGEM_ESQUERDA} x2={LARGURA} y1={baseBarras} y2={baseBarras} stroke="var(--cor-borda)" strokeWidth={1} />

        {/* Hoje: hairline nos dois painéis. */}
        {indiceHoje >= 0 && (
          <g>
            <line x1={centro(indiceHoje)} x2={centro(indiceHoje)} y1={0} y2={TOPO_BARRAS + ALTURA_BARRAS} stroke="var(--cor-texto-secundario)" strokeWidth={1} />
            <text x={centro(indiceHoje) + 4} y={TOPO_BARRAS - 10} className="eixo-rotulo">hoje</text>
          </g>
        )}

        {periodos.map((p, i) => {
          const x = centro(i) - larguraColuna / 2
          const [er, ep, sr, sp] = [p.entradasRealizadas, p.entradasPrevistas, p.saidasRealizadas, p.saidasPrevistas].map((v) => v * escala)
          // Previsto empilhado depois do realizado, com 2px de fundo entre eles.
          const gapE = er > 0 && ep > 0 ? 2 : 0
          const gapS = sr > 0 && sp > 0 ? 2 : 0
          const texto = `${rotulo(p.inicio)}: entradas ${formatarReal(p.entradasRealizadas + p.entradasPrevistas)}, saídas ${formatarReal(p.saidasRealizadas + p.saidasPrevistas)}, saldo ${formatarReal(p.saldo)}`

          return (
            <g
              key={p.inicio}
              tabIndex={0}
              role="button"
              aria-label={texto}
              onMouseEnter={() => setAtivo(i)}
              onFocus={() => setAtivo(i)}
              onBlur={() => setAtivo(null)}
              style={{ cursor: 'pointer', outline: 'none' }}
            >
              {/* Área de toque: a coluna inteira do período, nos dois painéis. */}
              <rect x={centro(i) - passo / 2} y={0} width={passo} height={TOPO_BARRAS + ALTURA_BARRAS} fill="transparent" />
              {ativo === i && (
                <line x1={centro(i)} x2={centro(i)} y1={0} y2={TOPO_BARRAS + ALTURA_BARRAS} stroke="var(--cor-texto)" strokeOpacity={0.35} strokeWidth={1} />
              )}
              {sr > 0 && <path d={coluna(x, larguraColuna, baseBarras, sr, false)} fill={COR_SAIDA} />}
              {sp > 0 && <path d={coluna(x, larguraColuna, baseBarras + sr + gapS, sp - gapS, false)} fill={COR_SAIDA} fillOpacity={OPACIDADE_PREVISTO} />}
              {er > 0 && <path d={coluna(x, larguraColuna, baseBarras, er, true)} fill={COR_ENTRADA} />}
              {ep > 0 && <path d={coluna(x, larguraColuna, baseBarras - er - gapE, ep - gapE, true)} fill={COR_ENTRADA} fillOpacity={OPACIDADE_PREVISTO} />}
              {ativo === i && (
                <circle cx={centro(i)} cy={ySaldo(p.saldo)} r={4} fill="var(--cor-primaria)" stroke="var(--cor-superficie)" strokeWidth={2} />
              )}
              {i % cadaRotulo === 0 && (
                <text x={centro(i)} y={ALTURA_TOTAL - 6} textAnchor="middle" className="eixo-rotulo">
                  {rotulo(p.inicio)}
                </text>
              )}
            </g>
          )
        })}
      </svg>
      </div>

      <div className="grafico-tooltip" aria-hidden="true">
        {periodoAtivo ? (
          <>
            <strong>{rotulo(periodoAtivo.inicio)}</strong>
            <span className="texto-discreto"> · entradas </span>
            <span className="numeros-tabulares">{formatarReal(periodoAtivo.entradasRealizadas + periodoAtivo.entradasPrevistas)}</span>
            <span className="texto-discreto"> · saídas </span>
            <span className="numeros-tabulares">{formatarReal(periodoAtivo.saidasRealizadas + periodoAtivo.saidasPrevistas)}</span>
            <span className="texto-discreto"> · saldo </span>
            <strong className="numeros-tabulares" style={periodoAtivo.saldo < 0 ? { color: 'var(--cor-erro)' } : undefined}>
              {formatarReal(periodoAtivo.saldo)}
            </strong>
          </>
        ) : (
          <span className="texto-discreto">Passe o mouse ou navegue pelo gráfico para ver o período.</span>
        )}
      </div>
    </div>
  )
}
