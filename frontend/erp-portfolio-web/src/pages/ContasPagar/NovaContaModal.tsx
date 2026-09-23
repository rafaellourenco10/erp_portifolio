/**
 * =====================================================================
 * Arquivo....: NovaContaModal.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Modal de lançamento de conta a pagar avulsa (aluguel, luz...):
 *              descrição, favorecido, valor total, 1º vencimento, nº de
 *              parcelas e intervalo (React Hook Form + Zod). A divisão em
 *              parcelas é feita pelo servidor; erros da API vão para o campo.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/contas-pagar (via useCriarContaAvulsa)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { zodResolver } from '@hookform/resolvers/zod'
import { App, Col, DatePicker, Form, Input, InputNumber, Modal, Row } from 'antd'
import dayjs from 'dayjs'
import { Controller, useForm, type FieldError } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { useCriarContaAvulsa } from '../../hooks/useContasPagar'
import { contaAvulsaSchema, paraPayload, type ContaAvulsaFormValores } from '../../schemas/contaAvulsaSchema'

const ID_FORMULARIO = 'formulario-conta-avulsa'

interface NovaContaModalProps {
  aberto: boolean
  aoFechar: () => void
}

export function NovaContaModal({ aberto, aoFechar }: NovaContaModalProps) {
  const { message } = App.useApp()
  const criarConta = useCriarContaAvulsa()

  const {
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<ContaAvulsaFormValores>({
    resolver: zodResolver(contaAvulsaSchema),
    defaultValues: {
      descricao: '',
      favorecido: '',
      valorTotal: undefined,
      primeiroVencimento: dayjs(),
      numeroParcelas: 1,
      intervaloDias: 30,
    },
  })

  async function salvar(valores: ContaAvulsaFormValores) {
    try {
      const parcelas = await criarConta.mutateAsync(paraPayload(valores))
      message.success(parcelas.length === 1 ? 'Conta lançada.' : `Conta lançada em ${parcelas.length} parcelas.`)
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)
      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(([campo]) => campo in valores)
      camposComErro.forEach(([campo, mensagem]) => setError(campo as keyof ContaAvulsaFormValores, { message: mensagem }))
      if (camposComErro.length === 0) message.error(erroApi.mensagem)
    }
  }

  return (
    <Modal
      open={aberto}
      title="Nova conta a pagar"
      okText="Lançar conta"
      cancelText="Cancelar"
      onCancel={aoFechar}
      okButtonProps={{ htmlType: 'submit', form: ID_FORMULARIO }}
      confirmLoading={criarConta.isPending}
      // O formulário volta vazio (com vencimento hoje) na próxima abertura.
      afterClose={() => reset()}
      destroyOnHidden
    >
      <p className="texto-discreto">Despesas que não vêm de compra nem de comissão: aluguel, luz, salários...</p>
      <Form id={ID_FORMULARIO} layout="vertical" requiredMark={false} noValidate onFinish={() => handleSubmit(salvar)()}>
        <ItemFormulario rotulo="Descrição" erro={errors.descricao} obrigatorio>
          <Controller
            name="descricao"
            control={control}
            render={({ field }) => <Input {...field} maxLength={200} placeholder="Ex.: Aluguel outubro" autoFocus />}
          />
        </ItemFormulario>
        <ItemFormulario rotulo="Favorecido" erro={errors.favorecido}>
          <Controller
            name="favorecido"
            control={control}
            render={({ field }) => <Input {...field} maxLength={150} placeholder="Ex.: Imobiliária Central (opcional)" />}
          />
        </ItemFormulario>
        <Row gutter={16}>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Valor total" erro={errors.valorTotal} obrigatorio>
              <Controller
                name="valorTotal"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    className="campo-cheio numeros-tabulares"
                    aria-label="Valor total"
                    min={0}
                    precision={2}
                    decimalSeparator=","
                    prefix="R$"
                    controls={false}
                    value={field.value}
                    onChange={(valor) => field.onChange(valor ?? undefined)}
                    onBlur={field.onBlur}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="1º vencimento" erro={errors.primeiroVencimento as FieldError | undefined} obrigatorio>
              <Controller
                name="primeiroVencimento"
                control={control}
                render={({ field }) => (
                  <DatePicker
                    className="campo-cheio"
                    aria-label="Primeiro vencimento"
                    format="DD/MM/YYYY"
                    allowClear={false}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Número de parcelas" erro={errors.numeroParcelas} obrigatorio>
              <Controller
                name="numeroParcelas"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    className="campo-cheio numeros-tabulares"
                    aria-label="Número de parcelas"
                    min={1}
                    max={12}
                    precision={0}
                    controls={false}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Intervalo entre parcelas (dias)" erro={errors.intervaloDias} obrigatorio>
              <Controller
                name="intervaloDias"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    className="campo-cheio numeros-tabulares"
                    aria-label="Intervalo entre parcelas em dias"
                    min={1}
                    max={180}
                    precision={0}
                    controls={false}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
        </Row>
      </Form>
    </Modal>
  )
}
