/**
 * =====================================================================
 * Arquivo....: CardIndicador.tsx
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Card de indicador do Dashboard: título + conteúdo, com loading e
 *              erro próprios (cada card é independente — SPEC.md, D7). O
 *              conteúdo é livre (children), para caber tanto um número simples
 *              quanto vários números lado a lado (ex.: pedidos por status).
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { Card, Skeleton, Typography } from 'antd'
import type { ReactNode } from 'react'

interface CardIndicadorProps {
  titulo: string
  loading: boolean
  erro?: boolean
  children: ReactNode
}

export function CardIndicador({ titulo, loading, erro, children }: CardIndicadorProps) {
  return (
    <Card className="painel" title={<span className="rotulo-filtro">{titulo}</span>}>
      {loading ? (
        <Skeleton active title={false} paragraph={{ rows: 1, width: '60%' }} />
      ) : erro ? (
        <Typography.Text type="danger">Não foi possível carregar.</Typography.Text>
      ) : (
        children
      )}
    </Card>
  )
}
