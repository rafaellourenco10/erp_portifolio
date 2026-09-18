/**
 * =====================================================================
 * Arquivo....: LogoAmbition.tsx
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Logo "Ambition ERP": ícone em SVG e texto em HTML (o texto
 *              dentro do SVG não se ajusta à largura real da fonte Inter).
 *              No modo compacto (menu recolhido) exibe apenas o ícone.
 * ---------------------------------------------------------------------
 * Fontes.....: docs/tema/stitch_erp_comercial_web_dark/code.html
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { cores } from '../tema/temaAmbition'
import './LogoAmbition.css'

interface LogoAmbitionProps {
  compacto?: boolean
  /** Lado do ícone em pixels. */
  tamanho?: number
}

export function LogoAmbition({ compacto = false, tamanho = 32 }: LogoAmbitionProps) {
  return (
    <span className="logo-ambition" role="img" aria-label="Ambition ERP">
      <svg viewBox="0 0 32 32" width={tamanho} height={tamanho} aria-hidden="true">
        <rect
          x="1"
          y="1"
          width="30"
          height="30"
          rx="8"
          fill={cores.primaria}
          fillOpacity="0.15"
          stroke={cores.primaria}
          strokeWidth="2"
        />
        <path
          d="M9.5 23L16 9.5L22.5 23M12.2 18H19.8"
          stroke={cores.primaria}
          strokeWidth="2.5"
          strokeLinecap="round"
          strokeLinejoin="round"
          fill="none"
        />
      </svg>
      {!compacto && (
        <span className="logo-ambition-texto">
          Ambition<span className="logo-ambition-erp">ERP</span>
        </span>
      )}
    </span>
  )
}
