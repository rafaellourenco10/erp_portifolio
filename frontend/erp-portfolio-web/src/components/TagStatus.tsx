/**
 * =====================================================================
 * Arquivo....: TagStatus.tsx
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Tag de status Ativo/Inativo no padrão do tema (fundo com
 *              12% da cor, borda com 30% e ponto indicador).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import './TagStatus.css'

interface TagStatusProps {
  ativo: boolean
  rotuloAtivo?: string
  rotuloInativo?: string
}

export function TagStatus({ ativo, rotuloAtivo = 'Ativo', rotuloInativo = 'Inativo' }: TagStatusProps) {
  return (
    <span className={`tag-status ${ativo ? 'tag-status-ativo' : 'tag-status-inativo'}`}>
      <span className="tag-status-ponto" />
      {ativo ? rotuloAtivo : rotuloInativo}
    </span>
  )
}
