/**
 * =====================================================================
 * Arquivo....: TagStatusParcela.tsx
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Tag de status de uma parcela a receber ou a pagar: Pendente (cinza),
 *              Atrasado (vermelho, calculado — não é um status gravado), Recebido/Pago (verde) e
 *              Cancelado (cinza riscado). Mesmo padrão visual da TagStatus/TagStatusPedido.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - Aceita também o status da parcela a pagar ("Pago"), etapa 8.
 * =====================================================================
 */

import type { StatusParcelaPagar } from '../types/contaPagar'
import type { StatusParcela } from '../types/contaReceber'
import './TagStatus.css'

interface TagStatusParcelaProps {
  status: StatusParcela | StatusParcelaPagar
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

  if (status === 'Recebido' || status === 'Pago') {
    return (
      <span className="tag-status tag-status-confirmado">
        <span className="tag-status-ponto" />
        {status}
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
