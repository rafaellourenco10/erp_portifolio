/**
 * =====================================================================
 * Arquivo....: GraficoFaturamento.tsx
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Gráfico de barras do faturamento diário do mês atual (uma série,
 *              ~30 dias). SVG desenhado à mão, sem biblioteca de gráfico: uma
 *              série pequena não justifica o peso de uma lib inteira (o bundle
 *              já tem aviso de chunk grande). Segue a skill de dataviz do
 *              projeto: barra com topo arredondado (4px) e base reta, gap de
 *              2px entre barras, cor única (é 1 série só, sem legenda), rótulos
 *              esparsos no eixo X, tooltip por barra (hover e foco de teclado).
 * ---------------------------------------------------------------------
 * Fontes.....: FaturamentoDia[] vindo de useResumoVendas (GET /api/dashboard/vendas).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { useState } from 'react'
import type { FaturamentoDia } from '../../types/dashboard'
import { formatarReal } from '../../utils/moeda'
import './dashboard.css'

const ALTURA = 180
const LARGURA_BARRA = 18
const GAP = 2

function formatarDia(iso: string): string {
  return iso.split('-')[2]
}

function formatarDataCompleta(iso: string): string {
  const [ano, mes, dia] = iso.split('-')
  return `${dia}/${mes}/${ano}`
}

export function GraficoFaturamento({ dados }: { dados: FaturamentoDia[] }) {
  const [indiceAtivo, setIndiceAtivo] = useState<number | null>(null)

  const maximo = Math.max(...dados.map((d) => d.valor), 1)
  const largura = dados.length * (LARGURA_BARRA + GAP)
  const ativo = indiceAtivo !== null ? dados[indiceAtivo] : null

  return (
    <div className="grafico-faturamento">
      <svg
        viewBox={`0 0 ${largura} ${ALTURA + 24}`}
        width="100%"
        height={ALTURA + 24}
        role="img"
        aria-label="Faturamento diário do mês atual"
        preserveAspectRatio="xMinYMin meet"
      >
        {/* Linha de base (eixo), hairline recessiva. */}
        <line x1={0} y1={ALTURA} x2={largura} y2={ALTURA} stroke="var(--cor-borda)" strokeWidth={1} />

        {dados.map((dia, indice) => {
          const alturaBarra = Math.round((dia.valor / maximo) * (ALTURA - 8))
          const x = indice * (LARGURA_BARRA + GAP)
          const y = ALTURA - alturaBarra
          const raio = Math.min(4, alturaBarra)
          const ehAtivo = indiceAtivo === indice
          const mostrarRotulo = indice % 5 === 0 || indice === dados.length - 1

          return (
            <g
              key={dia.dia}
              tabIndex={0}
              role="button"
              aria-label={`${formatarDataCompleta(dia.dia)}: ${formatarReal(dia.valor)}`}
              onMouseEnter={() => setIndiceAtivo(indice)}
              onMouseLeave={() => setIndiceAtivo(null)}
              onFocus={() => setIndiceAtivo(indice)}
              onBlur={() => setIndiceAtivo(null)}
              style={{ cursor: 'pointer', outline: 'none' }}
            >
              {/* Área de toque maior que a barra (interaction.md: hit target > mark). */}
              <rect x={x} y={0} width={LARGURA_BARRA + GAP} height={ALTURA} fill="transparent" />
              <rect
                x={x}
                y={alturaBarra === 0 ? ALTURA - 1 : y}
                width={LARGURA_BARRA}
                height={alturaBarra === 0 ? 1 : alturaBarra}
                rx={raio}
                fill={ehAtivo ? 'var(--cor-primaria-hover)' : 'var(--cor-primaria)'}
              />
              {mostrarRotulo && (
                <text
                  x={x + LARGURA_BARRA / 2}
                  y={ALTURA + 16}
                  textAnchor="middle"
                  fontSize={10}
                  fill="var(--cor-texto-secundario)"
                  className="numeros-tabulares"
                >
                  {formatarDia(dia.dia)}
                </text>
              )}
            </g>
          )
        })}
      </svg>

      <div className="grafico-tooltip" aria-hidden="true">
        {ativo ? (
          <>
            <strong className="numeros-tabulares">{formatarReal(ativo.valor)}</strong>
            <span className="texto-discreto"> · {formatarDataCompleta(ativo.dia)}</span>
          </>
        ) : (
          <span className="texto-discreto">Passe o mouse ou navegue pelas barras para ver o valor do dia.</span>
        )}
      </div>
    </div>
  )
}
