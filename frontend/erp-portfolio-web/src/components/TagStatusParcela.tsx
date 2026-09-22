/**
 * =====================================================================
 * Arquivo....: TagStatusParcela.tsx
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Tag de status de uma parcela a receber: Pendente (cinza), Atrasado
 *              (vermelho, calculado — não é um status gravado), Recebido (verde) e
 *              Cancelado (cinza riscado). Mesmo padrão visual da TagStatus/TagStatusPedido.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import type { StatusParcela } from '../types/contaReceber'
import './TagStatus.css'

interface TagStatusParcelaProps {
  status: StatusParcela
  atrasado: boolean
}

export function TagStatusParcela({ status, atrasado }: TagStatusParcelaProps) {
  if (status === 'Pendente' && atrasado) {
    return (
      <span className="tag-status tag-status-cancelado">
        <span className="tag-status-ponto" />
        Atrasado
      </span>
    )
  }

  if (status === 'Recebido') {
    return (
      <span className="tag-status tag-status-confirmado">
        <span className="tag-status-ponto" />
        Recebido
      </span>
    )
  }

  if (status === 'Cancelado') {
    return (
      <span className="tag-status tag-status-inativo tag-status-riscado">
        <span className="tag-status-ponto" />
        Cancelado
      </span>
    )
  }

  return (
    <span className="tag-status tag-status-inativo">
      <span className="tag-status-ponto" />
      Pendente
    </span>
  )
}
