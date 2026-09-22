/**
 * =====================================================================
 * Arquivo....: EntradaEstoqueDrawer.tsx
 * Versão.....: 1.0.0
 * Data.......: 22/09/2026
 * Descrição..: Painel lateral (Drawer) para lançar uma entrada manual de
 *              estoque: produto (seleção com busca no servidor), quantidade
 *              e motivo opcional (texto livre). Sem fornecedor por enquanto.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/estoque/entradas (via useRegistrarEntrada)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 22/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { App, Button, Drawer, Flex, Form, Grid, Input, InputNumber } from 'antd'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { SelecaoProduto } from '../../components/SelecaoProduto'
import { useRegistrarEntrada } from '../../hooks/useEstoque'
import {
  estoqueEntradaSchema,
  valoresIniciaisEntrada,
  type EstoqueEntradaFormEntrada,
  type EstoqueEntradaFormValores,
} from '../../schemas/estoqueEntradaSchema'
// O painel reaproveita as classes .drawer-cliente*, .caixa-status* etc. do módulo de Clientes.
import '../Clientes/clientes.css'

const ID_FORMULARIO = 'formulario-entrada-estoque'

interface EntradaEstoqueDrawerProps {
  aberto: boolean
  aoFechar: () => void
}

export function EntradaEstoqueDrawer({ aberto, aoFechar }: EntradaEstoqueDrawerProps) {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const registrarEntrada = useRegistrarEntrada()

  const {
    control,
    handleSubmit,
    reset,
    setValue,
    setError,
    formState: { errors },
  } = useForm<EstoqueEntradaFormEntrada, unknown, EstoqueEntradaFormValores>({
    resolver: zodResolver(estoqueEntradaSchema),
    defaultValues: valoresIniciaisEntrada,
  })

  useEffect(() => {
    if (aberto) reset(valoresIniciaisEntrada)
  }, [aberto, reset])

  async function salvar(valores: EstoqueEntradaFormValores) {
    try {
      await registrarEntrada.mutateAsync({
        produtoId: valores.produtoId!,
        quantidade: valores.quantidade!,
        motivo: valores.motivo.trim() || undefined,
      })
      message.success(`Entrada de ${valores.quantidade} lançada para "${valores.produtoNome}".`)
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)

      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(
        ([campo]) => campo === 'produtoId' || campo === 'quantidade' || campo === 'motivo',
      )
      camposComErro.forEach(([campo, mensagem]) => setError(campo as keyof EstoqueEntradaFormValores, { message: mensagem }))

      if (camposComErro.length === 0) {
        message.error(erroApi.mensagem)
      }
    }
  }

  return (
    <Drawer
      open={aberto}
      onClose={aoFechar}
      size={telas.sm === false ? '100%' : 480}
      closable={{ placement: 'end' }}
      destroyOnHidden
      className="drawer-cliente"
      title={
        <div>
          <div className="drawer-cliente-titulo">
            <span className="ponto-destaque" />
            Nova entrada de estoque
          </div>
          <div className="drawer-cliente-subtitulo">Lance uma compra ou ajuste no saldo do produto.</div>
        </div>
      }
      footer={
        <Flex justify="flex-end" gap={12}>
          <Button size="large" onClick={aoFechar}>
            Cancelar
          </Button>
          <Button
            type="primary"
            size="large"
            icon={<CheckOutlined />}
            htmlType="submit"
            form={ID_FORMULARIO}
            loading={registrarEntrada.isPending}
          >
            Lançar entrada
          </Button>
        </Flex>
      }
    >
      <Form
        id={ID_FORMULARIO}
        layout="vertical"
        size="large"
        requiredMark={false}
        noValidate
        onFinish={() => handleSubmit(salvar)()}
      >
        <ItemFormulario rotulo="Produto" erro={errors.produtoId} obrigatorio>
          <Controller
            name="produtoId"
            control={control}
            render={({ field }) => (
              <SelecaoProduto
                id="entrada-produto"
                aria-label="Produto"
                value={field.value}
                onChange={(produtoId, produto) => {
                  field.onChange(produtoId)
                  setValue('produtoNome', produto.nome)
                }}
              />
            )}
          />
        </ItemFormulario>

        <ItemFormulario rotulo="Quantidade" erro={errors.quantidade} obrigatorio>
          <Controller
            name="quantidade"
            control={control}
            render={({ field }) => (
              <InputNumber
                className="campo-cheio numeros-tabulares"
                aria-label="Quantidade"
                min={0}
                max={999_999.999}
                precision={3}
                decimalSeparator=","
                controls={false}
                autoFocus
                value={field.value}
                onChange={field.onChange}
                onBlur={field.onBlur}
              />
            )}
          />
        </ItemFormulario>

        <ItemFormulario rotulo="Motivo" erro={errors.motivo}>
          <Controller
            name="motivo"
            control={control}
            render={({ field }) => <Input {...field} maxLength={200} placeholder="Ex.: Compra NF 1234" />}
          />
        </ItemFormulario>
      </Form>
    </Drawer>
  )
}
