/**
 * =====================================================================
 * Arquivo....: SelecaoVendedor.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Seleção de vendedor com BUSCA NO SERVIDOR (só vendedores ativos, 20
 *              por consulta, com espera de 300 ms após digitar). Mostra o nome e o
 *              CPF formatado em cada opção. Usado no formulário do pedido de venda
 *              (campo controlado do React Hook Form). Espelho de SelecaoFornecedor.tsx.
 *              Para um pedido salvo, passe `vendedorAtual` (pode estar inativo).
 * ---------------------------------------------------------------------
 * Fontes.....: useBuscaVendedores (GET /api/vendedores?nome=&ativo=true&tamanhoPagina=20)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { Flex, Select } from 'antd'
import { useMemo, useState } from 'react'
import { useBuscaVendedores } from '../hooks/useBuscaCadastros'
import type { Vendedor } from '../types/vendedor'
import { formatarDocumento } from '../utils/documento'

interface SelecaoVendedorProps {
  /** Id do vendedor escolhido; vazio quando nada foi escolhido. */
  value?: number | null
  onChange?: (vendedorId: number, vendedor: Vendedor) => void
  /** Vendedor que já está no pedido salvo (pode estar inativo e por isso não vir na busca). */
  vendedorAtual?: { id: number; nome: string }
  disabled?: boolean
  /** Se informado, mostra o botão de limpar (volta para nenhum vendedor). */
  aoLimpar?: () => void
  placeholder?: string
  id?: string
  'aria-label'?: string
}

interface OpcaoVendedor {
  value: number
  label: string
  vendedor?: Vendedor
}

export function SelecaoVendedor({
  value,
  onChange,
  vendedorAtual,
  disabled,
  aoLimpar,
  placeholder = 'Buscar vendedor por nome',
  id,
  'aria-label': rotuloAcessivel,
}: SelecaoVendedorProps) {
  const [texto, setTexto] = useState('')
  const [ultimo, setUltimo] = useState<{ id: number; nome: string }>()
  const { itens, buscando, erro } = useBuscaVendedores(texto)

  const opcoes = useMemo<OpcaoVendedor[]>(
    () => itens.map((vendedor) => ({ value: vendedor.id, label: vendedor.nome, vendedor })),
    [itens],
  )

  // labelInValue: o campo guarda o NOME do escolhido, sem precisar que ele esteja na lista da busca atual.
  const nomeEscolhido = [vendedorAtual, ultimo].find((f) => f !== undefined && f.id === value)?.nome
  const valorDoCampo = value == null ? undefined : { value, label: nomeEscolhido ?? String(value) }

  function aoEscolher(escolhido: { value: number }) {
    const vendedor = itens.find((f) => f.id === escolhido.value)
    if (!vendedor) return
    setUltimo({ id: vendedor.id, nome: vendedor.nome })
    onChange?.(vendedor.id, vendedor)
  }

  return (
    <Select<{ value: number; label: string }, OpcaoVendedor>
      id={id}
      aria-label={rotuloAcessivel}
      labelInValue
      showSearch={{ filterOption: false, onSearch: setTexto }}
      value={valorDoCampo}
      onChange={aoEscolher}
      options={opcoes}
      loading={buscando}
      disabled={disabled}
      allowClear={aoLimpar !== undefined}
      onClear={aoLimpar}
      placeholder={placeholder}
      className="campo-cheio"
      notFoundContent={
        buscando
          ? 'Buscando...'
          : erro
            ? 'Não foi possível buscar os vendedores.'
            : 'Nenhum vendedor ativo encontrado.'
      }
      optionRender={(opcao) =>
        opcao.data.vendedor ? (
          <Flex justify="space-between" gap={12}>
            <span>{opcao.data.vendedor.nome}</span>
            <span className="texto-discreto numeros-tabulares">
              {formatarDocumento(opcao.data.vendedor.cpf)}
            </span>
          </Flex>
        ) : (
          opcao.data.label
        )
      }
    />
  )
}
