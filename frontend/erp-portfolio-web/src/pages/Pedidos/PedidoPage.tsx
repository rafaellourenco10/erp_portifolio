/**
 * =====================================================================
 * Arquivo....: PedidoPage.tsx
 * Versão.....: 2.5.0
 * Data.......: 23/09/2026
 * Descrição..: Página do pedido (rotas /pedidos/novo e /pedidos/:id). Formulário com
 *              cliente (busca no servidor), forma de pagamento, itens (tabela editável;
 *              adicionar um produto que já está no pedido SOMA a quantidade), desconto do
 *              pedido e resumo com subtotal, desconto e total recalculados a cada digitação.
 *              O cálculo em tela é só pré-visualização: ao salvar o rascunho, o pedido é
 *              recarregado do servidor e passa a valer o total dele. Erros 400 da API
 *              aparecem nos campos.
 *              Pedido salvo (/pedidos/:id): o rascunho é editável e tem Salvar rascunho,
 *              Confirmar pedido (valida, abre um modal para escolher número de parcelas e
 *              intervalo em dias, salva o que estiver pendente e confirma) e Cancelar
 *              pedido. Confirmado abre somente leitura e ainda pode ser cancelado;
 *              Cancelado é só consulta. Erros da API aparecem no campo certo.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/pedidos/{id}, POST /api/pedidos, PUT /api/pedidos/{id},
 *              PATCH /api/pedidos/{id}/confirmar e /cancelar
 *              (via usePedido, useSalvarPedido, useConfirmarPedido e useCancelarPedido)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo (provisória).
 *   1.1.0 - 21/09/2026 - Área de teste dos seletores de cliente e produto (T12).
 *   2.0.0 - 21/09/2026 - Formulário do pedido: novo, itens, total ao vivo e salvar rascunho (T13).
 *   2.1.0 - 21/09/2026 - Confirmar pedido e Cancelar pedido, com confirmação em janela (T14).
 *   2.2.0 - 21/09/2026 - Espaçamento das linhas de campos empilhadas no celular (T15).
 *   2.3.0 - 22/09/2026 - Confirmar abre um modal com número de parcelas e intervalo em
 *                        dias (contas a receber), no lugar do Modal.confirm simples.
 *   2.4.0 - 23/09/2026 - Modal de parcelas extraído para components/ModalParcelas (etapa 8).
 *   2.5.0 - 23/09/2026 - Campo Vendedor (obrigatório só para confirmar) e % de comissão congelada
 *                        no pedido confirmado (etapa 10).
 * =====================================================================
 */

import { ArrowLeftOutlined, CheckCircleOutlined, CheckOutlined, StopOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, App, Button, Col, Flex, Form, InputNumber, Row, Select, Spin } from 'antd'
import { useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Controller, useFieldArray, useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { ModalParcelas } from '../../components/ModalParcelas'
import { SelecaoCliente } from '../../components/SelecaoCliente'
import { SelecaoProduto } from '../../components/SelecaoProduto'
import { SelecaoVendedor } from '../../components/SelecaoVendedor'
import { TagStatusPedido } from '../../components/TagStatusPedido'
import { useCancelarPedido, useConfirmarPedido, usePedido, useSalvarPedido } from '../../hooks/usePedidos'
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
import { OPCOES_FORMA_PAGAMENTO, type Pedido, type PedidoConfirmarEntrada } from '../../types/pedido'
import { calcularPedido } from '../../utils/calculoPedido'
import { formatarReal } from '../../utils/moeda'
import '../Clientes/clientes.css'
import { ItensPedidoTabela } from './ItensPedidoTabela'
import './pedido.css'

const formatoPercentual = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 })

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
            <h1 className="pagina-titulo">{id === undefined ? 'Novo pedido de venda' : `Pedido de venda nº ${id}`}</h1>
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
  const { message, modal } = App.useApp()
  const navegar = useNavigate()
  const salvarPedido = useSalvarPedido()
  const confirmarPedido = useConfirmarPedido()
  const cancelarPedido = useCancelarPedido()
  const queryClient = useQueryClient()
  const somenteLeitura = pedido !== undefined && pedido.status !== 'Rascunho'
  // Muda a cada produto adicionado: recria o seletor, que volta vazio e pronto para outra busca.
  const [chaveSeletor, setChaveSeletor] = useState(0)

  // Valores validados do formulário, guardados enquanto o modal de confirmar (parcelas) está aberto.
  const [confirmando, setConfirmando] = useState<PedidoFormValores | null>(null)

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
      mostrarErroApi(erro)
    }
  }

  /** Erros 400 da API aparecem no campo certo; qualquer outro (409, rede...) vira uma mensagem. */
  function mostrarErroApi(erro: unknown) {
    const erroApi = lerErroApi(erro)
    let mostrouNoCampo = false

    // 409: o pedido mudou de situação por outro caminho (outra aba, outra pessoa): recarrega para a tela mostrar a verdadeira.
    if (erroApi.status === 409) queryClient.invalidateQueries({ queryKey: ['pedidos'] })

    if (erroApi.errosPorCampo.clienteId) {
      setError('clienteId', { message: erroApi.errosPorCampo.clienteId })
      mostrouNoCampo = true
    }
    if (erroApi.errosPorCampo.vendedorId) {
      setError('vendedorId', { message: erroApi.errosPorCampo.vendedorId })
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

  /** Confirmar: valida a tela e abre o modal de parcelas (número de parcelas e intervalo). */
  function pedirConfirmacao(valores: PedidoFormValores) {
    if (!pedido) return
    setConfirmando(valores)
  }

  /** Fecha o modal de parcelas, salva o que estiver pendente e então confirma. */
  async function confirmar(parcelas: PedidoConfirmarEntrada) {
    if (!pedido || !confirmando) return
    try {
      await salvarPedido.mutateAsync({ id: pedido.id, dados: paraPayload(confirmando) })
      await confirmarPedido.mutateAsync({ id: pedido.id, dados: parcelas })
      message.success(`Pedido de venda nº ${pedido.id} confirmado com sucesso.`)
      setConfirmando(null)
    } catch (erro) {
      mostrarErroApi(erro)
    }
  }

  function pedirCancelamento() {
    if (!pedido) return
    modal.confirm({
      title: `Cancelar o pedido nº ${pedido.id}?`,
      content: 'O pedido continua na lista como Cancelado e não pode ser reaberto.',
      okText: 'Cancelar pedido',
      okButtonProps: { danger: true },
      cancelText: 'Voltar',
      onOk: async () => {
        try {
          await cancelarPedido.mutateAsync(pedido.id)
          message.success(`Pedido de venda nº ${pedido.id} cancelado.`)
        } catch (erro) {
          mostrarErroApi(erro)
        }
      },
    })
  }

  const erroItens = errors.itens?.root?.message ?? errors.itens?.message

  return (
    <>
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
            pedido.status === 'Confirmado'
              ? 'Pedido confirmado: não pode ser editado, só cancelado.'
              : 'Pedido cancelado: somente leitura.'
          }
        />
      )}

      <section className="painel pedido-secao" aria-label="Dados do pedido">
        <h2 className="pedido-secao-titulo">Dados do pedido</h2>
        <Row gutter={[20, 16]}>
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
          <Col xs={24} md={14}>
            <ItemFormulario rotulo="Vendedor" erro={errors.vendedorId}>
              <Controller
                name="vendedorId"
                control={control}
                render={({ field }) => (
                  <SelecaoVendedor
                    id="pedido-vendedor"
                    aria-label="Vendedor"
                    value={field.value}
                    onChange={(vendedorId) => field.onChange(vendedorId)}
                    aoLimpar={() => field.onChange(null)}
                    vendedorAtual={
                      pedido?.vendedorId != null && pedido.vendedorNome
                        ? { id: pedido.vendedorId, nome: pedido.vendedorNome }
                        : undefined
                    }
                    disabled={somenteLeitura}
                    placeholder="Obrigatório só para confirmar"
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          {pedido?.percentualComissao != null && (
            <Col xs={24} md={10}>
              <ItemFormulario rotulo="Comissão do vendedor">
                <span className="numeros-tabulares" style={{ lineHeight: '40px' }}>
                  {formatoPercentual.format(pedido.percentualComissao)}% (congelada na confirmação)
                </span>
              </ItemFormulario>
            </Col>
          )}
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
        <Row gutter={[20, 16]}>
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
                  ? `Gravado no servidor: ${formatarReal(pedido.valorTotal)}.${somenteLeitura ? '' : ' Ao salvar, vale o valor calculado pelo servidor.'}`
                  : 'Valor de conferência: ao salvar, vale o total calculado pelo servidor.'}
              </span>
            </div>
          </Col>
        </Row>
      </section>

      {pedido?.status !== 'Cancelado' && (
        <div className="pedido-rodape">
          {pedido && (
            <Button
              danger
              size="large"
              icon={<StopOutlined />}
              className="pedido-rodape-cancelar"
              onClick={pedirCancelamento}
              loading={cancelarPedido.isPending}
            >
              Cancelar pedido
            </Button>
          )}
          {!somenteLeitura && (
            <>
              <Button
                size="large"
                icon={<CheckOutlined />}
                htmlType="submit"
                form={ID_FORMULARIO}
                loading={salvarPedido.isPending}
              >
                Salvar rascunho
              </Button>
              {pedido && (
                <Button
                  type="primary"
                  size="large"
                  icon={<CheckCircleOutlined />}
                  onClick={() => handleSubmit(pedirConfirmacao)()}
                  loading={confirmarPedido.isPending}
                >
                  Confirmar pedido
                </Button>
              )}
            </>
          )}
        </div>
      )}
    </Form>

    <ModalParcelas
      aberto={confirmando !== null}
      titulo={`Confirmar o pedido nº ${pedido?.id}?`}
      descricao="Depois de confirmado, o pedido não pode mais ser editado: só cancelado. Escolha em quantas parcelas a venda será recebida."
      carregando={salvarPedido.isPending || confirmarPedido.isPending}
      aoConfirmar={confirmar}
      aoFechar={() => setConfirmando(null)}
    />
    </>
  )
}
