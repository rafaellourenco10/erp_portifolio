/**
 * =====================================================================
 * Arquivo....: TagStatusPedido.tsx
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Tag de status do pedido (Rascunho, Confirmado, Cancelado) no mesmo
 *              padrão visual da TagStatus (fundo com 12% da cor, borda com 30% e
 *              ponto indicador): azul, verde e vermelho do tema Ambition.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { StatusPedido } from '../types/pedido'
import './TagStatus.css'

const CLASSE_POR_STATUS: Record<StatusPedido, string> = {
  Rascunho: 'tag-status-rascunho',
  Confirmado: 'tag-status-confirmado',
  Cancelado: 'tag-status-cancelado',
}

export function TagStatusPedido({ status }: { status: StatusPedido }) {
  return (
    <span className={`tag-status ${CLASSE_POR_STATUS[status]}`}>
      <span className="tag-status-ponto" />
      {status}
    </span>
  )
}
