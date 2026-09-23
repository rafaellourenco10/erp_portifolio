/**
 * =====================================================================
 * Arquivo....: QuadroVencimentos.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Quadro do Dashboard com as parcelas pendentes atrasadas ou que
 *              vencem nos próximos 7 dias (a pagar ou a receber), as mais
 *              urgentes primeiro: quem, de onde, valor e uma etiqueta
 *              "Atrasada há N dias" (vermelha) / "Vence hoje" / "Vence em N
 *              dias" (amarela). O servidor calcula os dias e os totais.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/dashboard/vencimentos-pagar | vencimentos-receber
 *              (via useVencimentosPagar / useVencimentosReceber)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { Flex, Tag, Typography } from 'antd'
import { Link } from 'react-router-dom'
import type { Vencimentos } from '../../types/dashboard'
import { formatarReal } from '../../utils/moeda'
import { CardIndicador } from './CardIndicador'

interface QuadroVencimentosProps {
  titulo: string
  /** Rota da tela completa (Contas a Pagar / a Receber). */
  rota: string
  dados: Vencimentos | undefined
  loading: boolean
  erro: boolean
}

/** "2026-10-22" -> "22/10", sem passar por Date (evita erro de fuso numa data sem hora). */
function diaMes(iso: string): string {
  const [, mes, dia] = iso.split('-')
  return `${dia}/${mes}`
}

function EtiquetaPrazo({ dias }: { dias: number }) {
  if (dias < 0) return <Tag color="red">Atrasada há {-dias} {dias === -1 ? 'dia' : 'dias'}</Tag>
  if (dias === 0) return <Tag color="gold">Vence hoje</Tag>
  return <Tag color="gold">Vence em {dias} {dias === 1 ? 'dia' : 'dias'}</Tag>
}

export function QuadroVencimentos({ titulo, rota, dados, loading, erro }: QuadroVencimentosProps) {
  return (
    <CardIndicador titulo={titulo} loading={loading} erro={erro}>
      {dados && (
        <Flex vertical gap={12}>
          <Flex justify="space-between" align="baseline" wrap gap={8}>
            <span className="numeros-tabulares">
              <strong>{formatarReal(dados.total)}</strong> em {dados.quantidade} {dados.quantidade === 1 ? 'parcela' : 'parcelas'}
              {dados.quantidadeAtrasadas > 0 && (
                <Typography.Text type="danger"> · {dados.quantidadeAtrasadas} atrasada{dados.quantidadeAtrasadas === 1 ? '' : 's'}</Typography.Text>
              )}
            </span>
            <Link to={rota}>Ver todas</Link>
          </Flex>

          {dados.itens.length === 0 ? (
            <span className="texto-discreto">Nada atrasado nem vencendo nos próximos 7 dias.</span>
          ) : (
            <Flex vertical gap={8}>
              {dados.itens.map((item) => (
                <Flex key={item.id} justify="space-between" align="center" gap={12}>
                  <Flex vertical style={{ minWidth: 0 }}>
                    <span className="celula-nome" style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {item.titulo}
                    </span>
                    <span className="texto-discreto" style={{ fontSize: 12, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {item.detalhe} · {diaMes(item.vencimento)}
                    </span>
                  </Flex>
                  <Flex vertical align="flex-end" gap={2} style={{ flexShrink: 0 }}>
                    <span className="numeros-tabulares">{formatarReal(item.valor)}</span>
                    <EtiquetaPrazo dias={item.dias} />
                  </Flex>
                </Flex>
              ))}
              {dados.quantidade > dados.itens.length && (
                <span className="texto-discreto">e mais {dados.quantidade - dados.itens.length}…</span>
              )}
            </Flex>
          )}
        </Flex>
      )}
    </CardIndicador>
  )
}
