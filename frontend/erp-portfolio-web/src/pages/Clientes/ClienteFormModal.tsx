/**
 * =====================================================================
 * Arquivo....: ClienteFormModal.tsx
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Modal com o formulário de inclusão/edição de cliente
 *              (React Hook Form + Zod). Erros de validação (400) e de
 *              documento duplicado (409) da API são exibidos nos campos.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/clientes e PUT /api/clientes/{id}
 *              (via useSalvarCliente)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { zodResolver } from '@hookform/resolvers/zod'
import { App, Col, Form, Input, Modal, Row, Select, Switch } from 'antd'
import { useEffect, type ReactNode } from 'react'
import { Controller, useForm, type FieldError } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { useSalvarCliente } from '../../hooks/useClientes'
import {
  clienteSchema,
  paraPayload,
  valoresIniciaisCliente,
  type ClienteFormValores,
} from '../../schemas/clienteSchema'
import type { Cliente } from '../../types/cliente'
import { formatarDocumento } from '../../utils/documento'
import { UFS } from '../../utils/ufs'

const ID_FORMULARIO = 'formulario-cliente'

const opcoesUf = UFS.map((uf) => ({ value: uf, label: uf }))

interface ClienteFormModalProps {
  aberto: boolean
  /** Cliente em edição; null para inclusão. */
  cliente: Cliente | null
  aoFechar: () => void
}

interface ItemFormularioProps {
  rotulo: string
  erro?: FieldError
  obrigatorio?: boolean
  children: ReactNode
}

function ItemFormulario({ rotulo, erro, obrigatorio, children }: ItemFormularioProps) {
  return (
    <Form.Item
      label={rotulo}
      required={obrigatorio}
      validateStatus={erro ? 'error' : undefined}
      help={erro?.message}
    >
      {children}
    </Form.Item>
  )
}

export function ClienteFormModal({ aberto, cliente, aoFechar }: ClienteFormModalProps) {
  const { message } = App.useApp()
  const salvarCliente = useSalvarCliente()
  const emEdicao = cliente !== null

  const {
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<ClienteFormValores>({
    resolver: zodResolver(clienteSchema),
    defaultValues: valoresIniciaisCliente,
  })

  useEffect(() => {
    if (!aberto) return

    reset(
      cliente
        ? {
            nome: cliente.nome,
            documento: formatarDocumento(cliente.documento),
            email: cliente.email ?? '',
            telefone: cliente.telefone ?? '',
            cidade: cliente.cidade,
            uf: cliente.uf,
            ativo: cliente.ativo,
          }
        : valoresIniciaisCliente,
    )
  }, [aberto, cliente, reset])

  async function salvar(valores: ClienteFormValores) {
    try {
      await salvarCliente.mutateAsync({ id: cliente?.id, dados: paraPayload(valores) })
      message.success(emEdicao ? 'Cliente atualizado com sucesso.' : 'Cliente cadastrado com sucesso.')
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)

      if (erroApi.status === 409) {
        setError('documento', { message: erroApi.mensagem })
        return
      }

      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(([campo]) => campo in valores)
      camposComErro.forEach(([campo, mensagem]) =>
        setError(campo as keyof ClienteFormValores, { message: mensagem }),
      )

      if (camposComErro.length === 0) {
        message.error(erroApi.mensagem)
      }
    }
  }

  return (
    <Modal
      open={aberto}
      title={emEdicao ? 'Editar cliente' : 'Novo cliente'}
      okText="Salvar"
      cancelText="Cancelar"
      okButtonProps={{ htmlType: 'submit', form: ID_FORMULARIO }}
      confirmLoading={salvarCliente.isPending}
      onCancel={aoFechar}
      destroyOnHidden
      width={640}
    >
      {/* noValidate: a validação nativa do navegador (type="email") impediria o Zod de exibir as mensagens */}
      <Form id={ID_FORMULARIO} layout="vertical" noValidate onFinish={() => handleSubmit(salvar)()}>
        <ItemFormulario rotulo="Nome" erro={errors.nome} obrigatorio>
          <Controller
            name="nome"
            control={control}
            render={({ field }) => <Input {...field} maxLength={150} autoFocus />}
          />
        </ItemFormulario>

        <Row gutter={16}>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="CPF/CNPJ" erro={errors.documento} obrigatorio>
              <Controller
                name="documento"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    maxLength={18}
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
            <ItemFormulario rotulo="Telefone" erro={errors.telefone}>
              <Controller
                name="telefone"
                control={control}
                render={({ field }) => <Input {...field} maxLength={20} placeholder="(11) 98765-4321" />}
              />
            </ItemFormulario>
          </Col>
        </Row>

        <ItemFormulario rotulo="E-mail" erro={errors.email}>
          <Controller
            name="email"
            control={control}
            render={({ field }) => <Input {...field} type="email" maxLength={150} />}
          />
        </ItemFormulario>

        <Row gutter={16}>
          <Col xs={24} sm={16}>
            <ItemFormulario rotulo="Cidade" erro={errors.cidade} obrigatorio>
              <Controller
                name="cidade"
                control={control}
                render={({ field }) => <Input {...field} maxLength={100} />}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={8}>
            <ItemFormulario rotulo="UF" erro={errors.uf} obrigatorio>
              <Controller
                name="uf"
                control={control}
                render={({ field }) => (
                  <Select
                    value={field.value || undefined}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    options={opcoesUf}
                    placeholder="Selecione"
                    showSearch
                  />
                )}
              />
            </ItemFormulario>
          </Col>
        </Row>

        {emEdicao && (
          <ItemFormulario rotulo="Ativo" erro={errors.ativo}>
            <Controller
              name="ativo"
              control={control}
              render={({ field }) => <Switch checked={field.value} onChange={field.onChange} />}
            />
          </ItemFormulario>
        )}
      </Form>
    </Modal>
  )
}
