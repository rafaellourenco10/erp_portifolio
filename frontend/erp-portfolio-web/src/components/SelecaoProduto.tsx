/**
 * =====================================================================
 * Arquivo....: SelecaoProduto.tsx
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Seleção de produto com BUSCA NO SERVIDOR (por nome ou SKU; só produtos
 *              ativos, 20 por consulta, com espera de 300 ms após digitar). Cada opção
 *              mostra o SKU, o preço e a unidade; o onChange devolve o produto inteiro
 *              (preço e unidade alimentam o cálculo do pedido). `desabilitados` marca
 *              produtos que já estão no pedido (o mesmo produto não pode repetir).
 *              Para um item já salvo, passe `produtoAtual` (pode estar inativo).
 *              `campoPreco` escolhe qual preço mostrar na opção: preço de venda (pedido
 *              de venda, padrão) ou custo (pedido de compra).
 * ---------------------------------------------------------------------
 * Fontes.....: useBuscaProdutos (GET /api/produtos?busca=&ativo=true&tamanhoPagina=20)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - campoPreco, para o Pedido de Compra mostrar o Custo em vez do
 *                        preço de venda (etapa 7).
 * =====================================================================
 */

import { Flex, Select } from 'antd'
import { useMemo, useState } from 'react'
import { useBuscaProdutos } from '../hooks/useBuscaCadastros'
import type { Produto } from '../types/produto'
import { formatarReal } from '../utils/moeda'

interface SelecaoProdutoProps {
  /** Id do produto escolhido; vazio quando nada foi escolhido. */
  value?: number | null
  onChange?: (produtoId: number, produto: Produto) => void
  /** Produto que já está no item salvo (pode estar inativo e por isso não vir na busca). */
  produtoAtual?: { id: number; nome: string }
  /** Ids de produtos que já estão no pedido: aparecem, mas não podem ser escolhidos de novo. */
  desabilitados?: number[]
  /** Qual preço mostrar na opção. Padrão: preço de venda. */
  campoPreco?: 'precoVenda' | 'custo'
  disabled?: boolean
  placeholder?: string
  id?: string
  'aria-label'?: string
}

interface OpcaoProduto {
  value: number
  label: string
  disabled?: boolean
  produto?: Produto
}

export function SelecaoProduto({
  value,
  onChange,
  produtoAtual,
  desabilitados = [],
  campoPreco = 'precoVenda',
  disabled,
  placeholder = 'Buscar produto por nome ou SKU',
  id,
  'aria-label': rotuloAcessivel,
}: SelecaoProdutoProps) {
  const [texto, setTexto] = useState('')
  const [ultimo, setUltimo] = useState<{ id: number; nome: string }>()
  const { itens, buscando, erro } = useBuscaProdutos(texto)

  // O produto do próprio campo não conta como "já no pedido" (senão sua opção ficaria travada ao reabrir).
  const opcoes = useMemo<OpcaoProduto[]>(
    () =>
      itens.map((produto) => ({
        value: produto.id,
        label: produto.nome,
        disabled: desabilitados.includes(produto.id) && produto.id !== value,
        produto,
      })),
    [itens, value, desabilitados],
  )

  // labelInValue: o campo guarda o NOME do escolhido, sem precisar que ele esteja na lista da busca atual.
  const nomeEscolhido = [produtoAtual, ultimo].find((p) => p !== undefined && p.id === value)?.nome
  const valorDoCampo = value == null ? undefined : { value, label: nomeEscolhido ?? String(value) }

  function aoEscolher(escolhido: { value: number }) {
    const produto = itens.find((p) => p.id === escolhido.value)
    if (!produto) return
    setUltimo({ id: produto.id, nome: produto.nome })
    onChange?.(produto.id, produto)
  }

  return (
    <Select<{ value: number; label: string }, OpcaoProduto>
      id={id}
      aria-label={rotuloAcessivel}
      labelInValue
      showSearch={{ filterOption: false, onSearch: setTexto }}
      value={valorDoCampo}
      onChange={aoEscolher}
      options={opcoes}
      loading={buscando}
      disabled={disabled}
      placeholder={placeholder}
      className="campo-cheio"
      notFoundContent={
        buscando ? 'Buscando...' : erro ? 'Não foi possível buscar os produtos.' : 'Nenhum produto ativo encontrado.'
      }
      optionRender={(opcao) =>
        opcao.data.produto ? (
          <Flex justify="space-between" gap={12}>
            <span>
              {opcao.data.produto.nome}
              {opcao.data.disabled && <span className="texto-discreto"> (já no pedido)</span>}
            </span>
            <span className="texto-discreto numeros-tabulares">
              {opcao.data.produto.sku} · {formatarReal(opcao.data.produto[campoPreco])} / {opcao.data.produto.unidade}
            </span>
          </Flex>
        ) : (
          opcao.data.label
        )
      }
    />
  )
}
