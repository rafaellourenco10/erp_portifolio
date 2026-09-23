/**
 * =====================================================================
 * Arquivo....: VendedorFormDrawer.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Painel lateral (Drawer) com o formulário de inclusão/edição
 *              de vendedor (React Hook Form + Zod): nome, CPF, contato e %
 *              de comissão padrão. Espelho de FornecedorFormDrawer.tsx.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/vendedores e PUT /api/vendedores/{id}
 *              (via useSalvarVendedor)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { App, Button, Col, Drawer, Flex, Form, Grid, Input, InputNumber, Row, Switch } from 'antd'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { TagStatus } from '../../components/TagStatus'
import { useSalvarVendedor } from '../../hooks/useVendedores'
import {
  paraPayload,
  valoresIniciaisVendedor,
  vendedorSchema,
  type VendedorFormValores,
} from '../../schemas/vendedorSchema'
import type { Vendedor } from '../../types/vendedor'
import { formatarDocumento } from '../../utils/documento'
// O painel reaproveita as classes .drawer-cliente*, .caixa-status* etc. do módulo de Clientes.
import '../Clientes/clientes.css'

const ID_FORMULARIO = 'formulario-vendedor'

interface VendedorFormDrawerProps {
  aberto: boolean
  /** Vendedor em edição; null para inclusão. */
  vendedor: Vendedor | null
  aoFechar: () => void
}

export function VendedorFormDrawer({ aberto, vendedor, aoFechar }: VendedorFormDrawerProps) {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const salvarVendedor = useSalvarVendedor()
  const emEdicao = vendedor !== null

  const {
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<VendedorFormValores>({
    resolver: zodResolver(vendedorSchema),
    defaultValues: valoresIniciaisVendedor,
  })

  useEffect(() => {
    if (!aberto) return

    reset(
      vendedor
        ? {
            nome: vendedor.nome,
            cpf: formatarDocumento(vendedor.cpf),
            email: vendedor.email ?? '',
            telefone: vendedor.telefone ?? '',
            percentualComissao: vendedor.percentualComissao,
            ativo: vendedor.ativo,
          }
        : valoresIniciaisVendedor,
    )
  }, [aberto, vendedor, reset])

  async function salvar(valores: VendedorFormValores) {
    try {
      await salvarVendedor.mutateAsync({ id: vendedor?.id, dados: paraPayload(valores) })
      message.success(emEdicao ? 'Vendedor atualizado com sucesso.' : 'Vendedor cadastrado com sucesso.')
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)

      if (erroApi.status === 409) {
        setError('cpf', { message: erroApi.mensagem })
        return
      }

      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(([campo]) => campo in valores)
      camposComErro.forEach(([campo, mensagem]) =>
        setError(campo as keyof VendedorFormValores, { message: mensagem }),
      )

      if (camposComErro.length === 0) {
        message.error(erroApi.mensagem)
      }
    }
  }

  return (
    <Drawer
      open={aberto}
      onClose={aoFechar}
      size={telas.sm === false ? '100%' : 640}
      closable={{ placement: 'end' }}
      destroyOnHidden
      className="drawer-cliente"
      title={
        <div>
          <div className="drawer-cliente-titulo">
            <span className="ponto-destaque" />
            {emEdicao ? 'Editar vendedor' : 'Novo vendedor'}
          </div>
          <div className="drawer-cliente-subtitulo">Dados do vendedor e a comissão padrão das vendas dele</div>
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
            loading={salvarVendedor.isPending}
          >
            Salvar vendedor
          </Button>
        </Flex>
      }
    >
      {/* noValidate: a validação nativa do navegador (type="email") impediria o Zod de exibir as mensagens */}
      <Form
        id={ID_FORMULARIO}
        layout="vertical"
        size="large"
        requiredMark={false}
        noValidate
        onFinish={() => handleSubmit(salvar)()}
      >
        <Row gutter={20}>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Nome completo" erro={errors.nome} obrigatorio>
              <Controller
                name="nome"
                control={control}
                render={({ field }) => <Input {...field} maxLength={150} autoFocus />}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="CPF" erro={errors.cpf} obrigatorio>
              <Controller
                name="cpf"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    className="numeros-tabulares"
                    maxLength={14}
                    placeholder="Somente números ou com máscara"
                    onBlur={() => {
                      field.onChange(formatarDocumento(field.value))
                      field.onBlur()
                    }}
                  />
                )}
              />
            </ItemFormulario>
          </Col>

          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="E-mail" erro={errors.email}>
              <Controller
                name="email"
                control={control}
                render={({ field }) => (
                  <Input {...field} type="email" maxLength={150} placeholder="vendedor@empresa.com.br" />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Telefone / WhatsApp" erro={errors.telefone}>
              <Controller
                name="telefone"
                control={control}
                render={({ field }) => (
                  <Input {...field} className="numeros-tabulares" maxLength={20} placeholder="(11) 98765-4321" />
                )}
              />
            </ItemFormulario>
          </Col>

          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Comissão padrão" erro={errors.percentualComissao} obrigatorio>
              <Controller
                name="percentualComissao"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    aria-label="Comissão padrão"
                    className="campo-cheio numeros-tabulares"
                    min={0}
                    max={100}
                    precision={2}
                    decimalSeparator=","
                    controls={false}
                    suffix="%"
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
        </Row>

        {emEdicao && (
          <Controller
            name="ativo"
            control={control}
            render={({ field }) => (
              <div className="caixa-status">
                <div>
                  <div className="caixa-status-titulo">Status do cadastro</div>
                  <div className="caixa-status-descricao">Inativos continuam no cadastro, sem exclusão.</div>
                </div>
                <Flex align="center" gap={12}>
                  <TagStatus ativo={field.value} rotuloAtivo="Vendedor Ativo" rotuloInativo="Vendedor Inativo" />
                  <Switch checked={field.value} onChange={field.onChange} aria-label="Vendedor ativo" />
                </Flex>
              </div>
            )}
          />
        )}
      </Form>
    </Drawer>
  )
}
