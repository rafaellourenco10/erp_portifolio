/**
 * =====================================================================
 * Arquivo....: ClienteFormDrawer.tsx
 * Versão.....: 1.1.0
 * Data.......: 21/09/2026
 * Descrição..: Painel lateral (Drawer) com o formulário de inclusão/edição
 *              de cliente (React Hook Form + Zod), no layout do tema
 *              Ambition ERP. Erros de validação (400) e de documento
 *              duplicado (409) da API são exibidos nos campos.
 *              Substitui o antigo ClienteFormModal.tsx.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/clientes e PUT /api/clientes/{id}
 *              (via useSalvarCliente)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo (formulário migrado do modal
 *                        para painel lateral).
 *   1.1.0 - 21/09/2026 - ItemFormulario extraído para components/.
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { App, Button, Col, Drawer, Flex, Form, Grid, Input, Row, Select, Switch } from 'antd'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { TagStatus } from '../../components/TagStatus'
import { useSalvarCliente } from '../../hooks/useClientes'
import {
  clienteSchema,
  paraPayload,
  valoresIniciaisCliente,
  type ClienteFormValores,
} from '../../schemas/clienteSchema'
import type { Cliente } from '../../types/cliente'
import { formatarDocumento } from '../../utils/documento'
import { UFS, compararRelevanciaUf, ufCorrespondeBusca } from '../../utils/ufs'
import './clientes.css'

const ID_FORMULARIO = 'formulario-cliente'

const opcoesUf = UFS.map((uf) => ({ value: uf.sigla, label: `${uf.nome} (${uf.sigla})`, uf }))

interface ClienteFormDrawerProps {
  aberto: boolean
  /** Cliente em edição; null para inclusão. */
  cliente: Cliente | null
  aoFechar: () => void
}

export function ClienteFormDrawer({ aberto, cliente, aoFechar }: ClienteFormDrawerProps) {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
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
            {emEdicao ? 'Editar cliente' : 'Novo cliente'}
          </div>
          <div className="drawer-cliente-subtitulo">Preencha os dados cadastrais da empresa ou pessoa física</div>
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
            loading={salvarCliente.isPending}
          >
            Salvar cliente
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
            <ItemFormulario rotulo="Razão Social / Nome completo" erro={errors.nome} obrigatorio>
              <Controller
                name="nome"
                control={control}
                render={({ field }) => <Input {...field} maxLength={150} autoFocus />}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="CNPJ / CPF" erro={errors.documento} obrigatorio>
              <Controller
                name="documento"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    className="numeros-tabulares"
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
            <ItemFormulario rotulo="E-mail principal" erro={errors.email}>
              <Controller
                name="email"
                control={control}
                render={({ field }) => (
                  <Input {...field} type="email" maxLength={150} placeholder="contato@empresa.com.br" />
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
            <ItemFormulario rotulo="Cidade" erro={errors.cidade} obrigatorio>
              <Controller
                name="cidade"
                control={control}
                render={({ field }) => <Input {...field} maxLength={100} />}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Estado (UF)" erro={errors.uf} obrigatorio>
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
                    showSearch={{
                      filterOption: (busca, opcao) => !!opcao && ufCorrespondeBusca(busca, opcao.uf),
                      filterSort: (a, b, { searchValue }) => compararRelevanciaUf(a.uf, b.uf, searchValue),
                    }}
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
                  <TagStatus ativo={field.value} rotuloAtivo="Cliente Ativo" rotuloInativo="Cliente Inativo" />
                  <Switch checked={field.value} onChange={field.onChange} aria-label="Cliente ativo" />
                </Flex>
              </div>
            )}
          />
        )}
      </Form>
    </Drawer>
  )
}
