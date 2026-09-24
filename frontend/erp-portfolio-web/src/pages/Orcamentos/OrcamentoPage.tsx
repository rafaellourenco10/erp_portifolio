/**
 * =====================================================================
 * Arquivo....: OrcamentoPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 23/09/2026
 * Descrição..: Página do orçamento (rotas /orcamentos/novo e /orcamentos/:id), no mesmo
 *              desenho da página do pedido: cliente, vendedor, forma de pagamento,
 *              validade (padrão hoje + 15 dias), itens (mesma tabela do pedido), desconto,
 *              observações e resumo recalculado a cada digitação. Só o orçamento Aberto
 *              (vencido ou não) é editável; salvar um vencido com validade nova o prorroga.
 *              Erros 400 da API aparecem nos campos.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/orcamentos/{id}, POST /api/orcamentos, PUT /api/orcamentos/{id}
 *              (via useOrcamento e useSalvarOrcamento)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { ArrowLeftOutlined, CheckOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, App, Button, Col, DatePicker, Flex, Form, Input, InputNumber, Row, Select, Spin } from 'antd'
import { useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import dayjs from 'dayjs'
import { useEffect, useMemo, useState } from 'react'
import { Controller, useFieldArray, useForm, useWatch, type Control, type FieldError } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { SelecaoCliente } from '../../components/SelecaoCliente'
import { SelecaoProduto } from '../../components/SelecaoProduto'
import { SelecaoVendedor } from '../../components/SelecaoVendedor'
import { TagStatusOrcamento } from '../../components/TagStatusOrcamento'
import { useOrcamento, useSalvarOrcamento } from '../../hooks/useOrcamentos'
import {
  orcamentoSchema,
  paraFormulario,
  paraPayload,
  valoresIniciaisOrcamento,
  type OrcamentoFormEntrada,
  type OrcamentoFormValores,
} from '../../schemas/orcamentoSchema'
import { MAXIMO_ITENS, itemDoProduto, type PedidoFormEntrada, type PedidoFormValores } from '../../schemas/pedidoSchema'
import type { Orcamento } from '../../types/orcamento'
import { OPCOES_FORMA_PAGAMENTO } from '../../types/pedido'
import { calcularPedido } from '../../utils/calculoPedido'
import { formatarReal } from '../../utils/moeda'
import '../Clientes/clientes.css'
import { ItensPedidoTabela } from '../Pedidos/ItensPedidoTabela'
import '../Pedidos/pedido.css'

const ID_FORMULARIO = 'formulario-orcamento'

export function OrcamentoPage() {
  const { id } = useParams()
  const navegar = useNavigate()
  const numero = id === undefined ? undefined : Number(id)
  const numeroInvalido = numero !== undefined && !Number.isInteger(numero)
  const { data: orcamento, isLoading, isError, error } = useOrcamento(numeroInvalido ? undefined : numero)

  const naoEncontrado = numeroInvalido || (isError && isAxiosError(error) && error.response?.status === 404)

  let conteudo
  if (naoEncontrado) {
    conteudo = <Alert type="warning" showIcon title="Orçamento não encontrado." />
  } else if (isError) {
    conteudo = <Alert type="error" showIcon title={lerErroApi(error).mensagem} />
  } else if (numero !== undefined && (isLoading || !orcamento)) {
    conteudo = (
      <Flex justify="center" style={{ padding: 48 }}>
        <Spin size="large" />
      </Flex>
    )
  } else {
    conteudo = <OrcamentoFormulario orcamento={orcamento} />
  }

  return (
    <div className="pagina-pedido">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <Flex align="center" gap={16} wrap>
            <h1 className="pagina-titulo">{id === undefined ? 'Novo orçamento' : `Orçamento nº ${id}`}</h1>
            {orcamento && <TagStatusOrcamento status={orcamento.status} vencido={orcamento.vencido} />}
          </Flex>
          <p className="pagina-subtitulo">Cliente, itens, descontos, validade e total da proposta.</p>
        </div>
        <Button size="large" icon={<ArrowLeftOutlined />} onClick={() => navegar('/orcamentos')}>
          Voltar para orçamentos
        </Button>
      </Flex>

      {conteudo}
    </div>
  )
}

/** Formulário do orçamento: vazio em /orcamentos/novo; com os dados do servidor em /orcamentos/:id. */
function OrcamentoFormulario({ orcamento }: { orcamento?: Orcamento }) {
  const { message } = App.useApp()
  const navegar = useNavigate()
  const queryClient = useQueryClient()
  const salvarOrcamento = useSalvarOrcamento()
  const somenteLeitura = orcamento !== undefined && orcamento.status !== 'Aberto'
  // Muda a cada produto adicionado: recria o seletor, que volta vazio e pronto para outra busca.
  const [chaveSeletor, setChaveSeletor] = useState(0)

  const {
    control,
    handleSubmit,
    reset,
    setError,
    clearErrors,
    getValues,
    formState: { errors },
  } = useForm<OrcamentoFormEntrada, unknown, OrcamentoFormValores>({
    resolver: zodResolver(orcamentoSchema),
    defaultValues: orcamento ? paraFormulario(orcamento) : valoresIniciaisOrcamento(),
  })
  const { fields, append, update, remove } = useFieldArray({ control, name: 'itens' })

  // Depois de salvar, o orçamento volta do servidor e o formulário mostra exatamente o que foi gravado.
  useEffect(() => {
    if (orcamento) reset(paraFormulario(orcamento))
  }, [orcamento, reset])

  const itens = useWatch({ control, name: 'itens' })
  const descontoOrcamento = useWatch({ control, name: 'descontoPercentual' })
  const resumo = useMemo(
    () =>
      calcularPedido(
        (itens ?? []).map((item) => ({
          quantidade: item.quantidade ?? 0,
          precoUnitario: item.precoUnitario,
          descontoPercentual: item.descontoPercentual ?? 0,
        })),
        descontoOrcamento ?? 0,
      ),
    [itens, descontoOrcamento],
  )

  function adicionarProduto(produto: Parameters<typeof itemDoProduto>[0]) {
    const atuais = getValues('itens')
    const indice = atuais.findIndex((item) => item.produtoId === produto.id)

    if (indice >= 0) {
      // O mesmo produto não pode repetir: soma na linha que já existe.
      update(indice, { ...atuais[indice], quantidade: (atuais[indice].quantidade ?? 0) + 1 })
      message.info(`${produto.nome}: quantidade somada ao item que já estava no orçamento.`)
    } else if (atuais.length >= MAXIMO_ITENS) {
      message.warning(`Um orçamento pode ter no máximo ${MAXIMO_ITENS} itens.`)
      return
    } else {
      append(itemDoProduto(produto))
    }
    clearErrors('itens')
    setChaveSeletor((chave) => chave + 1)
  }

  async function salvar(valores: OrcamentoFormValores) {
    try {
      const salvo = await salvarOrcamento.mutateAsync({ id: orcamento?.id, dados: paraPayload(valores) })

      if (orcamento) {
        message.success('Orçamento atualizado com sucesso.')
        reset(paraFormulario(salvo))
      } else {
        message.success(`Orçamento nº ${salvo.id} salvo com sucesso.`)
        navegar(`/orcamentos/${salvo.id}`, { replace: true })
      }
    } catch (erro) {
      mostrarErroApi(erro)
    }
  }

  /** Erros 400 da API aparecem no campo certo; qualquer outro (409, rede...) vira uma mensagem. */
  function mostrarErroApi(erro: unknown) {
    const erroApi = lerErroApi(erro)
    let mostrouNoCampo = false

    // 409: o orçamento mudou de situação por outro caminho (outra aba): recarrega para a tela mostrar a verdadeira.
    if (erroApi.status === 409) queryClient.invalidateQueries({ queryKey: ['orcamentos'] })

    for (const campo of ['clienteId', 'vendedorId', 'formaPagamento', 'validade', 'observacoes', 'itens'] as const) {
      const mensagem = erroApi.errosPorCampo[campo]
      if (mensagem) {
        setError(campo, { message: mensagem })
        mostrouNoCampo = true
      }
    }

    if (!mostrouNoCampo) {
      message.error(Object.values(erroApi.errosPorCampo)[0] ?? erroApi.mensagem)
    }
  }

  const erroItens = errors.itens?.root?.message ?? errors.itens?.message

  return (
    <Form
      id={ID_FORMULARIO}
      layout="vertical"
      size="large"
      requiredMark={false}
      noValidate
      onFinish={() => handleSubmit(salvar)()}
    >
      {somenteLeitura && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 24 }}
          title={
            orcamento.status === 'Aprovado'
              ? `Orçamento aprovado: virou o pedido de venda nº ${orcamento.pedidoId} e não pode mais ser editado.`
              : `Orçamento perdido: somente leitura.${orcamento.motivoPerda ? ` Motivo: ${orcamento.motivoPerda}` : ''}`
          }
        />
      )}
      {orcamento?.vencido && (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 24 }}
          title="Orçamento vencido: para gerar o pedido, informe uma nova validade e salve."
        />
      )}

      <section className="painel pedido-secao" aria-label="Dados do orçamento">
        <h2 className="pedido-secao-titulo">Dados do orçamento</h2>
        <Row gutter={[20, 16]}>
          <Col xs={24} md={14}>
            <ItemFormulario rotulo="Cliente" erro={errors.clienteId} obrigatorio>
              <Controller
                name="clienteId"
                control={control}
                render={({ field }) => (
                  <SelecaoCliente
                    id="orcamento-cliente"
                    aria-label="Cliente"
                    value={field.value}
                    onChange={(clienteId) => field.onChange(clienteId)}
                    clienteAtual={orcamento ? { id: orcamento.clienteId, nome: orcamento.clienteNome } : undefined}
                    disabled={somenteLeitura}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} md={10}>
            <ItemFormulario rotulo="Validade" erro={errors.validade as FieldError | undefined} obrigatorio>
              <Controller
                name="validade"
                control={control}
                render={({ field }) => (
                  <DatePicker
                    id="orcamento-validade"
                    className="campo-cheio"
                    aria-label="Validade"
                    format="DD/MM/YYYY"
                    allowClear={false}
                    disabledDate={(dia) => dia.isBefore(dayjs(), 'day')}
                    disabled={somenteLeitura}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} md={14}>
            <ItemFormulario rotulo="Vendedor" erro={errors.vendedorId}>
              <Controller
                name="vendedorId"
                control={control}
                render={({ field }) => (
                  <SelecaoVendedor
                    id="orcamento-vendedor"
                    aria-label="Vendedor"
                    value={field.value}
                    onChange={(vendedorId) => field.onChange(vendedorId)}
                    aoLimpar={() => field.onChange(null)}
                    vendedorAtual={
                      orcamento?.vendedorId != null && orcamento.vendedorNome
                        ? { id: orcamento.vendedorId, nome: orcamento.vendedorNome }
                        : undefined
                    }
                    disabled={somenteLeitura}
                    placeholder="Opcional"
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} md={10}>
            <ItemFormulario rotulo="Forma de pagamento" erro={errors.formaPagamento}>
              <Controller
                name="formaPagamento"
                control={control}
                render={({ field }) => (
                  <Select
                    id="orcamento-forma-pagamento"
                    aria-label="Forma de pagamento"
                    value={field.value ?? undefined}
                    onChange={(valor) => field.onChange(valor ?? null)}
                    onBlur={field.onBlur}
                    options={OPCOES_FORMA_PAGAMENTO}
                    allowClear
                    disabled={somenteLeitura}
                    placeholder="Opcional"
                  />
                )}
              />
            </ItemFormulario>
          </Col>
        </Row>
      </section>

      <section className="painel pedido-secao" aria-label="Itens do orçamento">
        <h2 className="pedido-secao-titulo">Itens do orçamento ({fields.length})</h2>
        {!somenteLeitura && (
          <SelecaoProduto
            key={chaveSeletor}
            id="orcamento-adicionar-produto"
            aria-label="Adicionar produto"
            placeholder="Buscar produto por nome ou SKU para adicionar"
            onChange={(_, produto) => adicionarProduto(produto)}
          />
        )}
        <ItensPedidoTabela
          // A tabela só lê/escreve `itens.*`, que tem a mesma forma no pedido e no orçamento.
          control={control as unknown as Control<PedidoFormEntrada, unknown, PedidoFormValores>}
          linhas={fields}
          subtotais={resumo.subtotais}
          aoRemover={(indice) => {
            remove(indice)
            clearErrors('itens')
          }}
          somenteLeitura={somenteLeitura}
        />
        {erroItens && (
          <div className="erro-secao" role="alert">
            {erroItens}
          </div>
        )}
      </section>

      <section className="painel pedido-secao" aria-label="Resumo do orçamento">
        <h2 className="pedido-secao-titulo">Resumo</h2>
        <Row gutter={[20, 16]}>
          <Col xs={24} md={8}>
            <ItemFormulario rotulo="Desconto no orçamento todo" erro={errors.descontoPercentual}>
              <Controller
                name="descontoPercentual"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    id="orcamento-desconto"
                    aria-label="Desconto no orçamento todo"
                    className="campo-cheio numeros-tabulares"
                    min={0}
                    max={100}
                    precision={2}
                    decimalSeparator=","
                    controls={false}
                    suffix="%"
                    disabled={somenteLeitura}
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} md={16}>
            <div className="resumo-pedido">
              <div className="resumo-linha">
                <span>Soma dos itens</span>
                <span className="numeros-tabulares">{formatarReal(resumo.subtotalItens)}</span>
              </div>
              <div className="resumo-linha">
                <span>Desconto do orçamento</span>
                <span className="numeros-tabulares">− {formatarReal(resumo.valorDesconto)}</span>
              </div>
              <div className="resumo-linha resumo-total">
                <span>Total</span>
                <span className="numeros-tabulares valor-total">{formatarReal(resumo.total)}</span>
              </div>
              {resumo.excedeLimite && (
                <Alert type="error" showIcon title="O total do orçamento não pode passar de R$ 9.999.999.999,99." />
              )}
              <span className="texto-discreto">
                {orcamento
                  ? `Gravado no servidor: ${formatarReal(orcamento.valorTotal)}.${somenteLeitura ? '' : ' Ao salvar, vale o valor calculado pelo servidor.'}`
                  : 'Valor de conferência: ao salvar, vale o total calculado pelo servidor.'}
              </span>
            </div>
          </Col>
          <Col xs={24}>
            <ItemFormulario rotulo="Observações (saem no PDF)" erro={errors.observacoes}>
              <Controller
                name="observacoes"
                control={control}
                render={({ field }) => (
                  <Input.TextArea
                    id="orcamento-observacoes"
                    aria-label="Observações"
                    rows={3}
                    maxLength={500}
                    showCount
                    placeholder="Prazo de entrega, condições, garantia..."
                    disabled={somenteLeitura}
                    {...field}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
        </Row>
      </section>

      {!somenteLeitura && (
        <div className="pedido-rodape">
          <Button
            type="primary"
            size="large"
            icon={<CheckOutlined />}
            htmlType="submit"
            form={ID_FORMULARIO}
            loading={salvarOrcamento.isPending}
          >
            Salvar orçamento
          </Button>
        </div>
      )}
    </Form>
  )
}
