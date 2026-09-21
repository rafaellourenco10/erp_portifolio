/**
 * =====================================================================
 * Arquivo....: SelecaoCliente.tsx
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Seleção de cliente com BUSCA NO SERVIDOR (só clientes ativos, 20 por
 *              consulta, com espera de 300 ms após digitar). Mostra o nome e o CPF/CNPJ
 *              formatado em cada opção. Feito para o formulário do pedido: serve como
 *              campo controlado do React Hook Form (value/onChange).
 *              O nome do cliente já escolhido continua na tela mesmo depois de a busca
 *              mudar; para um pedido salvo, passe `clienteAtual` (pode estar inativo).
 * ---------------------------------------------------------------------
 * Fontes.....: useBuscaClientes (GET /api/clientes?nome=&ativo=true&tamanhoPagina=20)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { Flex, Select } from 'antd'
import { useMemo, useState } from 'react'
import { useBuscaClientes } from '../hooks/useBuscaCadastros'
import type { Cliente } from '../types/cliente'
import { formatarDocumento } from '../utils/documento'

interface SelecaoClienteProps {
  /** Id do cliente escolhido; vazio quando nada foi escolhido. */
  value?: number | null
  onChange?: (clienteId: number, cliente: Cliente) => void
  /** Cliente que já está no pedido salvo (pode estar inativo e por isso não vir na busca). */
  clienteAtual?: { id: number; nome: string }
  disabled?: boolean
  placeholder?: string
  id?: string
  'aria-label'?: string
}

interface OpcaoCliente {
  value: number
  label: string
  cliente?: Cliente
}

export function SelecaoCliente({
  value,
  onChange,
  clienteAtual,
  disabled,
  placeholder = 'Buscar cliente por nome',
  id,
  'aria-label': rotuloAcessivel,
}: SelecaoClienteProps) {
  const [texto, setTexto] = useState('')
  const [ultimo, setUltimo] = useState<{ id: number; nome: string }>()
  const { itens, buscando, erro } = useBuscaClientes(texto)

  const opcoes = useMemo<OpcaoCliente[]>(
    () => itens.map((cliente) => ({ value: cliente.id, label: cliente.nome, cliente })),
    [itens],
  )

  // labelInValue: o campo guarda o NOME do escolhido, sem precisar que ele esteja na lista da busca atual.
  const nomeEscolhido = [clienteAtual, ultimo].find((c) => c !== undefined && c.id === value)?.nome
  const valorDoCampo = value == null ? undefined : { value, label: nomeEscolhido ?? String(value) }

  function aoEscolher(escolhido: { value: number }) {
    const cliente = itens.find((c) => c.id === escolhido.value)
    if (!cliente) return
    setUltimo({ id: cliente.id, nome: cliente.nome })
    onChange?.(cliente.id, cliente)
  }

  return (
    <Select<{ value: number; label: string }, OpcaoCliente>
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
        buscando ? 'Buscando...' : erro ? 'Não foi possível buscar os clientes.' : 'Nenhum cliente ativo encontrado.'
      }
      optionRender={(opcao) =>
        opcao.data.cliente ? (
          <Flex justify="space-between" gap={12}>
            <span>{opcao.data.cliente.nome}</span>
            <span className="texto-discreto numeros-tabulares">{formatarDocumento(opcao.data.cliente.documento)}</span>
          </Flex>
        ) : (
          opcao.data.label
        )
      }
    />
  )
}
