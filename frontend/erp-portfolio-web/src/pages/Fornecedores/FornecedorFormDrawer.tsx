/**
 * =====================================================================
 * Arquivo....: FornecedorFormDrawer.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Painel lateral (Drawer) com o formulário de inclusão/edição
 *              de fornecedor (React Hook Form + Zod). Espelho de
 *              ClienteFormDrawer.tsx.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/fornecedores e PUT /api/fornecedores/{id}
 *              (via useSalvarFornecedor)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
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
import { useSalvarFornecedor } from '../../hooks/useFornecedores'
import {
  fornecedorSchema,
  paraPayload,
  valoresIniciaisFornecedor,
  type FornecedorFormValores,
} from '../../schemas/fornecedorSchema'
import type { Fornecedor } from '../../types/fornecedor'
import { formatarDocumento } from '../../utils/documento'
import { UFS, compararRelevanciaUf, ufCorrespondeBusca } from '../../utils/ufs'
// O painel reaproveita as classes .drawer-cliente*, .caixa-status* etc. do módulo de Clientes.
import '../Clientes/clientes.css'

const ID_FORMULARIO = 'formulario-fornecedor'

const opcoesUf = UFS.map((uf) => ({ value: uf.sigla, label: `${uf.nome} (${uf.sigla})`, uf }))

interface FornecedorFormDrawerProps {
  aberto: boolean
  /** Fornecedor em edição; null para inclusão. */
  fornecedor: Fornecedor | null
  aoFechar: () => void
}

export function FornecedorFormDrawer({ aberto, fornecedor, aoFechar }: FornecedorFormDrawerProps) {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const salvarFornecedor = useSalvarFornecedor()
  const emEdicao = fornecedor !== null

  const {
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<FornecedorFormValores>({
    resolver: zodResolver(fornecedorSchema),
    defaultValues: valoresIniciaisFornecedor,
  })

  useEffect(() => {
    if (!aberto) return

    reset(
      fornecedor
        ? {
            nome: fornecedor.nome,
            documento: formatarDocumento(fornecedor.documento),
            email: fornecedor.email ?? '',
            telefone: fornecedor.telefone ?? '',
            cidade: fornecedor.cidade,
            uf: fornecedor.uf,
            ativo: fornecedor.ativo,
          }
        : valoresIniciaisFornecedor,
    )
  }, [aberto, fornecedor, reset])

  async function salvar(valores: FornecedorFormValores) {
    try {
      await salvarFornecedor.mutateAsync({ id: fornecedor?.id, dados: paraPayload(valores) })
      message.success(emEdicao ? 'Fornecedor atualizado com sucesso.' : 'Fornecedor cadastrado com sucesso.')
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)

      if (erroApi.status === 409) {
        setError('documento', { message: erroApi.mensagem })
        return
      }

      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(([campo]) => campo in valores)
      camposComErro.forEach(([campo, mensagem]) =>
        setError(campo as keyof FornecedorFormValores, { message: mensagem }),
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
            {emEdicao ? 'Editar fornecedor' : 'Novo fornecedor'}
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
            loading={salvarFornecedor.isPending}
          >
            Salvar fornecedor
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
                  <Input {...field} type="email" maxLength={150} placeholder="contato@fornecedor.com.br" />
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
                  <TagStatus ativo={field.value} rotuloAtivo="Fornecedor Ativo" rotuloInativo="Fornecedor Inativo" />
                  <Switch checked={field.value} onChange={field.onChange} aria-label="Fornecedor ativo" />
                </Flex>
              </div>
            )}
          />
        )}
      </Form>
    </Drawer>
  )
}
