/**
 * =====================================================================
 * Arquivo....: TagStatusOrcamento.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Tag de status do orçamento no padrão da TagStatus: Aberto = azul,
 *              Vencido (aberto com validade passada) = laranja, Aprovado = verde,
 *              Perdido = cinza.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { StatusOrcamento } from '../types/orcamento'
import './TagStatus.css'

const CLASSE_POR_STATUS: Record<StatusOrcamento, string> = {
  Aberto: 'tag-status-rascunho',
  Aprovado: 'tag-status-confirmado',
  Perdido: 'tag-status-inativo',
}

export function TagStatusOrcamento({ status, vencido }: { status: StatusOrcamento; vencido: boolean }) {
  const mostraVencido = status === 'Aberto' && vencido
  return (
    <span className={`tag-status ${mostraVencido ? 'tag-status-vencido' : CLASSE_POR_STATUS[status]}`}>
      <span className="tag-status-ponto" />
      {mostraVencido ? 'Vencido' : status}
    </span>
  )
}
