/**
 * =====================================================================
 * Arquivo....: SelecaoFornecedor.tsx
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Seleção de fornecedor com BUSCA NO SERVIDOR (só fornecedores ativos, 20
 *              por consulta, com espera de 300 ms após digitar). Mostra o nome e o
 *              CPF/CNPJ formatado em cada opção. Feito para o formulário do pedido de
 *              compra: serve como campo controlado do React Hook Form (value/onChange).
 *              Espelho de SelecaoCliente.tsx.
 *              O nome do fornecedor já escolhido continua na tela mesmo depois de a busca
 *              mudar; para um pedido salvo, passe `fornecedorAtual` (pode estar inativo).
 * ---------------------------------------------------------------------
 * Fontes.....: useBuscaFornecedores (GET /api/fornecedores?nome=&ativo=true&tamanhoPagina=20)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - Prop opcional aoLimpar (botão de limpar, usado nos relatórios).
 * =====================================================================
 */

import { Flex, Select } from 'antd'
import { useMemo, useState } from 'react'
import { useBuscaFornecedores } from '../hooks/useBuscaCadastros'
import type { Fornecedor } from '../types/fornecedor'
import { formatarDocumento } from '../utils/documento'

interface SelecaoFornecedorProps {
  /** Id do fornecedor escolhido; vazio quando nada foi escolhido. */
  value?: number | null
  onChange?: (fornecedorId: number, fornecedor: Fornecedor) => void
  /** Fornecedor que já está no pedido salvo (pode estar inativo e por isso não vir na busca). */
  fornecedorAtual?: { id: number; nome: string }
  disabled?: boolean
  /** Se informado, mostra o botão de limpar (volta para nenhum fornecedor). */
  aoLimpar?: () => void
  placeholder?: string
  id?: string
  'aria-label'?: string
}

interface OpcaoFornecedor {
  value: number
  label: string
  fornecedor?: Fornecedor
}

export function SelecaoFornecedor({
  value,
  onChange,
  fornecedorAtual,
  disabled,
  aoLimpar,
  placeholder = 'Buscar fornecedor por nome',
  id,
  'aria-label': rotuloAcessivel,
}: SelecaoFornecedorProps) {
  const [texto, setTexto] = useState('')
  const [ultimo, setUltimo] = useState<{ id: number; nome: string }>()
  const { itens, buscando, erro } = useBuscaFornecedores(texto)

  const opcoes = useMemo<OpcaoFornecedor[]>(
    () => itens.map((fornecedor) => ({ value: fornecedor.id, label: fornecedor.nome, fornecedor })),
    [itens],
  )

  // labelInValue: o campo guarda o NOME do escolhido, sem precisar que ele esteja na lista da busca atual.
  const nomeEscolhido = [fornecedorAtual, ultimo].find((f) => f !== undefined && f.id === value)?.nome
  const valorDoCampo = value == null ? undefined : { value, label: nomeEscolhido ?? String(value) }

  function aoEscolher(escolhido: { value: number }) {
    const fornecedor = itens.find((f) => f.id === escolhido.value)
    if (!fornecedor) return
    setUltimo({ id: fornecedor.id, nome: fornecedor.nome })
    onChange?.(fornecedor.id, fornecedor)
  }

  return (
    <Select<{ value: number; label: string }, OpcaoFornecedor>
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
            ? 'Não foi possível buscar os fornecedores.'
            : 'Nenhum fornecedor ativo encontrado.'
      }
      optionRender={(opcao) =>
        opcao.data.fornecedor ? (
          <Flex justify="space-between" gap={12}>
            <span>{opcao.data.fornecedor.nome}</span>
            <span className="texto-discreto numeros-tabulares">
              {formatarDocumento(opcao.data.fornecedor.documento)}
            </span>
          </Flex>
        ) : (
          opcao.data.label
        )
      }
    />
  )
}
