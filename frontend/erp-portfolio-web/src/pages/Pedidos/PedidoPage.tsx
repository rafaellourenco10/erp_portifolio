/**
 * =====================================================================
 * Arquivo....: PedidoPage.tsx
 * Versão.....: 2.0.0
 * Data.......: 21/09/2026
 * Descrição..: Página do pedido (rotas /pedidos/novo e /pedidos/:id). Formulário com
 *              cliente (busca no servidor), forma de pagamento, itens (tabela editável;
 *              adicionar um produto que já está no pedido SOMA a quantidade), desconto do
 *              pedido e resumo com subtotal, desconto e total recalculados a cada digitação.
 *              O cálculo em tela é só pré-visualização: ao salvar o rascunho, o pedido é
 *              recarregado do servidor e passa a valer o total dele. Erros 400 da API
 *              aparecem nos campos.
 *              Pedido salvo (/pedidos/:id): rascunho é editável; Confirmado e Cancelado
 *              abrem somente leitura. Os botões Confirmar e Cancelar pedido entram na T14.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/pedidos/{id}, POST /api/pedidos, PUT /api/pedidos/{id}
 *              (via usePedido e useSalvarPedido)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo (provisória).
 *   1.1.0 - 21/09/2026 - Área de teste dos seletores de cliente e produto (T12).
 *   2.0.0 - 21/09/2026 - Formulário do pedido: novo, itens, total ao vivo e salvar rascunho (T13).
 * =====================================================================
 */

import { ArrowLeftOutlined, CheckOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, App, Button, Col, Flex, Form, InputNumber, Row, Select, Spin } from 'antd'
import { isAxiosError } from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Controller, useFieldArray, useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { SelecaoCliente } from '../../components/SelecaoCliente'
import { SelecaoProduto } from '../../components/SelecaoProduto'
import { TagStatusPedido } from '../../components/TagStatusPedido'
import { usePedido, useSalvarPedido } from '../../hooks/usePedidos'
import {
  MAXIMO_ITENS,
  itemDoProduto,
  paraFormulario,
  paraPayload,
  pedidoSchema,
  valoresIniciaisPedido,
  type PedidoFormEntrada,
  type PedidoFormValores,
} from '../../schemas/pedidoSchema'
import { OPCOES_FORMA_PAGAMENTO, type Pedido } from '../../types/pedido'
import { calcularPedido } from '../../utils/calculoPedido'
import { formatarReal } from '../../utils/moeda'
import '../Clientes/clientes.css'
import { ItensPedidoTabela } from './ItensPedidoTabela'
import './pedido.css'

const ID_FORMULARIO = 'formulario-pedido'

export function PedidoPage() {
  const { id } = useParams()
  const navegar = useNavigate()
  const numero = id === undefined ? undefined : Number(id)
  const numeroInvalido = numero !== undefined && !Number.isInteger(numero)
  const { data: pedido, isLoading, isError, error } = usePedido(numeroInvalido ? undefined : numero)

  const naoEncontrado = numeroInvalido || (isError && isAxiosError(error) && error.response?.status === 404)

  let conteudo
  if (naoEncontrado) {
    conteudo = <Alert type="warning" showIcon title="Pedido não encontrado." />
  } else if (isError) {
    conteudo = <Alert type="error" showIcon title={lerErroApi(error).mensagem} />
  } else if (numero !== undefined && (isLoading || !pedido)) {
    conteudo = (
      <Flex justify="center" style={{ padding: 48 }}>
        <Spin size="large" />
      </Flex>
    )
  } else {
    conteudo = <PedidoFormulario pedido={pedido} />
  }

  return (
    <div className="pagina-pedido">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <Flex align="center" gap={16} wrap>
            <h1 className="pagina-titulo">{id === undefined ? 'Novo pedido' : `Pedido nº ${id}`}</h1>
            {pedido && <TagStatusPedido status={pedido.status} />}
          </Flex>
          <p className="pagina-subtitulo">Cliente, itens, descontos e total do pedido.</p>
        </div>
        <Button size="large" icon={<ArrowLeftOutlined />} onClick={() => navegar('/pedidos')}>
          Voltar para pedidos
        </Button>
      </Flex>

      {conteudo}
    </div>
  )
}

/** Formulário do pedido: vazio em /pedidos/novo; com os dados do servidor em /pedidos/:id. */
function PedidoFormulario({ pedido }: { pedido?: Pedido }) {
  const { message } = App.useApp()
  const navegar = useNavigate()
  const salvarPedido = useSalvarPedido()
  const somenteLeitura = pedido !== undefined && pedido.status !== 'Rascunho'
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
  } = useForm<PedidoFormEntrada, unknown, PedidoFormValores>({
    resolver: zodResolver(pedidoSchema),
    defaultValues: pedido ? paraFormulario(pedido) : valoresIniciaisPedido,
  })
  const { fields, append, update, remove } = useFieldArray({ control, name: 'itens' })

  // Depois de salvar, o pedido volta do servidor e o formulário passa a mostrar exatamente o que foi gravado.
  useEffect(() => {
    if (pedido) reset(paraFormulario(pedido))
  }, [pedido, reset])

  const itens = useWatch({ control, name: 'itens' })
  const descontoPedido = useWatch({ control, name: 'descontoPercentual' })
  const resumo = useMemo(
    () =>
      calcularPedido(
        (itens ?? []).map((item) => ({
          quantidade: item.quantidade ?? 0,
          precoUnitario: item.precoUnitario,
          descontoPercentual: item.descontoPercentual ?? 0,
        })),
        descontoPedido ?? 0,
      ),
    [itens, descontoPedido],
  )

  function adicionarProduto(produto: Parameters<typeof itemDoProduto>[0]) {
    const atuais = getValues('itens')
    const indice = atuais.findIndex((item) => item.produtoId === produto.id)

    if (indice >= 0) {
      // O mesmo produto não pode repetir no pedido (R4): soma na linha que já existe.
      update(indice, { ...atuais[indice], quantidade: (atuais[indice].quantidade ?? 0) + 1 })
      message.info(`${produto.nome}: quantidade somada ao item que já estava no pedido.`)
    } else if (atuais.length >= MAXIMO_ITENS) {
      message.warning(`Um pedido pode ter no máximo ${MAXIMO_ITENS} itens.`)
      return
    } else {
      append(itemDoProduto(produto))
    }
    clearErrors('itens')
    setChaveSeletor((chave) => chave + 1)
  }

  async function salvar(valores: PedidoFormValores) {
    try {
      const salvo = await salvarPedido.mutateAsync({ id: pedido?.id, dados: paraPayload(valores) })

      if (pedido) {
        message.success('Rascunho atualizado com sucesso.')
        reset(paraFormulario(salvo))
      } else {
        message.success(`Rascunho nº ${salvo.id} salvo com sucesso.`)
        navegar(`/pedidos/${salvo.id}`, { replace: true })
      }
    } catch (erro) {
      const erroApi = lerErroApi(erro)
      let mostrouNoCampo = false

      if (erroApi.errosPorCampo.clienteId) {
        setError('clienteId', { message: erroApi.errosPorCampo.clienteId })
        mostrouNoCampo = true
      }
      if (erroApi.errosPorCampo.formaPagamento) {
        setError('formaPagamento', { message: erroApi.errosPorCampo.formaPagamento })
        mostrouNoCampo = true
      }
      if (erroApi.errosPorCampo.itens) {
        setError('itens', { message: erroApi.errosPorCampo.itens })
        mostrouNoCampo = true
      }

      if (!mostrouNoCampo) {
        message.error(Object.values(erroApi.errosPorCampo)[0] ?? erroApi.mensagem)
      }
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
          title={`Este pedido está ${pedido.status.toLowerCase()} e não pode ser editado.`}
        />
      )}

      <section className="painel pedido-secao" aria-label="Dados do pedido">
        <h2 className="pedido-secao-titulo">Dados do pedido</h2>
        <Row gutter={20}>
          <Col xs={24} md={14}>
            <ItemFormulario rotulo="Cliente" erro={errors.clienteId} obrigatorio>
              <Controller
                name="clienteId"
                control={control}
                render={({ field }) => (
                  <SelecaoCliente
                    id="pedido-cliente"
                    aria-label="Cliente"
                    value={field.value}
                    onChange={(clienteId) => field.onChange(clienteId)}
                    clienteAtual={pedido ? { id: pedido.clienteId, nome: pedido.clienteNome } : undefined}
                    disabled={somenteLeitura}
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
                    id="pedido-forma-pagamento"
                    aria-label="Forma de pagamento"
                    value={field.value ?? undefined}
                    onChange={(valor) => field.onChange(valor ?? null)}
                    onBlur={field.onBlur}
                    options={OPCOES_FORMA_PAGAMENTO}
                    allowClear
                    disabled={somenteLeitura}
                    placeholder="Obrigatória só para confirmar"
                  />
                )}
              />
            </ItemFormulario>
          </Col>
        </Row>
      </section>

      <section className="painel pedido-secao" aria-label="Itens do pedido">
        <h2 className="pedido-secao-titulo">Itens do pedido ({fields.length})</h2>
        {!somenteLeitura && (
          <SelecaoProduto
            key={chaveSeletor}
            id="pedido-adicionar-produto"
            aria-label="Adicionar produto"
            placeholder="Buscar produto por nome ou SKU para adicionar"
            onChange={(_, produto) => adicionarProduto(produto)}
          />
        )}
        <ItensPedidoTabela
          control={control}
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

      <section className="painel pedido-secao" aria-label="Resumo do pedido">
        <h2 className="pedido-secao-titulo">Resumo</h2>
        <Row gutter={20}>
          <Col xs={24} md={8}>
            <ItemFormulario rotulo="Desconto no pedido todo" erro={errors.descontoPercentual}>
              <Controller
                name="descontoPercentual"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    id="pedido-desconto"
                    aria-label="Desconto no pedido todo"
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
                <span className="numeros-tabulares" data-testid="resumo-soma">
                  {formatarReal(resumo.subtotalItens)}
                </span>
              </div>
              <div className="resumo-linha">
                <span>Desconto do pedido</span>
                <span className="numeros-tabulares" data-testid="resumo-desconto">
                  − {formatarReal(resumo.valorDesconto)}
                </span>
              </div>
              <div className="resumo-linha resumo-total">
                <span>Total</span>
                <span className="numeros-tabulares valor-total" data-testid="resumo-total">
                  {formatarReal(resumo.total)}
                </span>
              </div>
              {resumo.excedeLimite && (
                <Alert type="error" showIcon title="O total do pedido não pode passar de R$ 9.999.999.999,99." />
              )}
              <span className="texto-discreto">
                {pedido
                  ? `Gravado no servidor: ${formatarReal(pedido.valorTotal)}. Ao salvar, vale o valor calculado pelo servidor.`
                  : 'Valor de conferência: ao salvar, vale o total calculado pelo servidor.'}
              </span>
            </div>
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
            loading={salvarPedido.isPending}
          >
            Salvar rascunho
          </Button>
        </div>
      )}
    </Form>
  )
}
