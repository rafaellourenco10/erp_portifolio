/**
 * =====================================================================
 * Arquivo....: PedidoCompraPage.tsx
 * Versão.....: 1.1.0
 * Data.......: 23/09/2026
 * Descrição..: Página do pedido de compra (rotas /pedidos-compra/novo e
 *              /pedidos-compra/:id). Espelho de PedidoPage.tsx, sem forma de pagamento.
 *              Confirmar abre o modal de parcelas (número e intervalo em dias, contas a
 *              pagar), salva o que estiver pendente e confirma. Confirmado dá entrada no
 *              estoque, atualiza o custo dos produtos e gera as parcelas a pagar (regra
 *              do servidor); a tela só reflete o resultado.
 *              Pedido salvo (/pedidos-compra/:id): o rascunho é editável e tem Salvar
 *              rascunho, Confirmar pedido e Cancelar pedido. Confirmado abre somente
 *              leitura e ainda pode ser cancelado (se o saldo ainda estiver disponível);
 *              Cancelado é só consulta. Erros da API aparecem no campo certo.
 * ---------------------------------------------------------------------
 * Fontes.....: GET /api/pedidos-compra/{id}, POST /api/pedidos-compra,
 *              PUT /api/pedidos-compra/{id}, PATCH /api/pedidos-compra/{id}/confirmar
 *              e /cancelar (via usePedidoCompra, useSalvarPedidoCompra,
 *              useConfirmarPedidoCompra e useCancelarPedidoCompra)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 23/09/2026 - Criação do arquivo.
 *   1.1.0 - 23/09/2026 - Confirmar abre o modal de parcelas (contas a pagar, etapa 8).
 * =====================================================================
 */

import { ArrowLeftOutlined, CheckCircleOutlined, CheckOutlined, StopOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, App, Button, Col, Flex, Form, InputNumber, Row, Spin } from 'antd'
import { useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Controller, useFieldArray, useForm, useWatch } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { ModalParcelas } from '../../components/ModalParcelas'
import { SelecaoFornecedor } from '../../components/SelecaoFornecedor'
import { SelecaoProduto } from '../../components/SelecaoProduto'
import { TagStatusPedido } from '../../components/TagStatusPedido'
import {
  useCancelarPedidoCompra,
  useConfirmarPedidoCompra,
  usePedidoCompra,
  useSalvarPedidoCompra,
} from '../../hooks/usePedidosCompra'
import {
  MAXIMO_ITENS,
  itemDoProduto,
  paraFormulario,
  paraPayload,
  pedidoCompraSchema,
  valoresIniciaisPedidoCompra,
  type PedidoCompraFormEntrada,
  type PedidoCompraFormValores,
} from '../../schemas/pedidoCompraSchema'
import type { PedidoConfirmarEntrada } from '../../types/pedido'
import type { PedidoCompra } from '../../types/pedidoCompra'
import { calcularPedido } from '../../utils/calculoPedido'
import { formatarReal } from '../../utils/moeda'
import '../Clientes/clientes.css'
import '../Pedidos/pedido.css'
import { ItensPedidoCompraTabela } from './ItensPedidoCompraTabela'

const ID_FORMULARIO = 'formulario-pedido-compra'

export function PedidoCompraPage() {
  const { id } = useParams()
  const navegar = useNavigate()
  const numero = id === undefined ? undefined : Number(id)
  const numeroInvalido = numero !== undefined && !Number.isInteger(numero)
  const { data: pedido, isLoading, isError, error } = usePedidoCompra(numeroInvalido ? undefined : numero)

  const naoEncontrado = numeroInvalido || (isError && isAxiosError(error) && error.response?.status === 404)

  let conteudo
  if (naoEncontrado) {
    conteudo = <Alert type="warning" showIcon title="Pedido de compra não encontrado." />
  } else if (isError) {
    conteudo = <Alert type="error" showIcon title={lerErroApi(error).mensagem} />
  } else if (numero !== undefined && (isLoading || !pedido)) {
    conteudo = (
      <Flex justify="center" style={{ padding: 48 }}>
        <Spin size="large" />
      </Flex>
    )
  } else {
    conteudo = <PedidoCompraFormulario pedido={pedido} />
  }

  return (
    <div className="pagina-pedidos-compra">
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <Flex align="center" gap={16} wrap>
            <h1 className="pagina-titulo">{id === undefined ? 'Novo pedido de compra' : `Pedido de compra nº ${id}`}</h1>
            {pedido && <TagStatusPedido status={pedido.status} />}
          </Flex>
          <p className="pagina-subtitulo">Fornecedor, itens, descontos e total do pedido.</p>
        </div>
        <Button size="large" icon={<ArrowLeftOutlined />} onClick={() => navegar('/pedidos-compra')}>
          Voltar para pedidos de compra
        </Button>
      </Flex>

      {conteudo}
    </div>
  )
}

/** Formulário do pedido de compra: vazio em /pedidos-compra/novo; com os dados do servidor em /pedidos-compra/:id. */
function PedidoCompraFormulario({ pedido }: { pedido?: PedidoCompra }) {
  const { message, modal } = App.useApp()
  const navegar = useNavigate()
  const salvarPedido = useSalvarPedidoCompra()
  const confirmarPedido = useConfirmarPedidoCompra()
  const cancelarPedido = useCancelarPedidoCompra()
  const queryClient = useQueryClient()
  const somenteLeitura = pedido !== undefined && pedido.status !== 'Rascunho'
  // Muda a cada produto adicionado: recria o seletor, que volta vazio e pronto para outra busca.
  const [chaveSeletor, setChaveSeletor] = useState(0)
  // Valores validados do formulário, guardados enquanto o modal de confirmar (parcelas) está aberto.
  const [confirmando, setConfirmando] = useState<PedidoCompraFormValores | null>(null)

  const {
    control,
    handleSubmit,
    reset,
    setError,
    clearErrors,
    getValues,
    formState: { errors },
  } = useForm<PedidoCompraFormEntrada, unknown, PedidoCompraFormValores>({
    resolver: zodResolver(pedidoCompraSchema),
    defaultValues: pedido ? paraFormulario(pedido) : valoresIniciaisPedidoCompra,
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
      // O mesmo produto não pode repetir no pedido (PC4): soma na linha que já existe.
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

  async function salvar(valores: PedidoCompraFormValores) {
    try {
      const salvo = await salvarPedido.mutateAsync({ id: pedido?.id, dados: paraPayload(valores) })

      if (pedido) {
        message.success('Rascunho atualizado com sucesso.')
        reset(paraFormulario(salvo))
      } else {
        message.success(`Rascunho nº ${salvo.id} salvo com sucesso.`)
        navegar(`/pedidos-compra/${salvo.id}`, { replace: true })
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
    if (erroApi.status === 409) queryClient.invalidateQueries({ queryKey: ['pedidos-compra'] })

    if (erroApi.errosPorCampo.fornecedorId) {
      setError('fornecedorId', { message: erroApi.errosPorCampo.fornecedorId })
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
  function pedirConfirmacao(valores: PedidoCompraFormValores) {
    if (!pedido) return
    setConfirmando(valores)
  }

  /** Fecha o modal de parcelas, salva o que estiver pendente e então confirma. */
  async function confirmar(parcelas: PedidoConfirmarEntrada) {
    if (!pedido || !confirmando) return
    try {
      await salvarPedido.mutateAsync({ id: pedido.id, dados: paraPayload(confirmando) })
      await confirmarPedido.mutateAsync({ id: pedido.id, dados: parcelas })
      message.success(`Pedido de compra nº ${pedido.id} confirmado com sucesso.`)
      setConfirmando(null)
    } catch (erro) {
      mostrarErroApi(erro)
    }
  }

  function pedirCancelamento() {
    if (!pedido) return
    modal.confirm({
      title: `Cancelar o pedido de compra nº ${pedido.id}?`,
      content:
        'O pedido continua na lista como Cancelado e não pode ser reaberto. Se o pedido já estava confirmado, a entrada no estoque é estornada — o que exige saldo suficiente em cada item (nada é vendido além do que a compra trouxe).',
      okText: 'Cancelar pedido',
      okButtonProps: { danger: true },
      cancelText: 'Voltar',
      onOk: async () => {
        try {
          await cancelarPedido.mutateAsync(pedido.id)
          message.success(`Pedido de compra nº ${pedido.id} cancelado.`)
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
            <ItemFormulario rotulo="Fornecedor" erro={errors.fornecedorId} obrigatorio>
              <Controller
                name="fornecedorId"
                control={control}
                render={({ field }) => (
                  <SelecaoFornecedor
                    id="pedido-compra-fornecedor"
                    aria-label="Fornecedor"
                    value={field.value}
                    onChange={(fornecedorId) => field.onChange(fornecedorId)}
                    fornecedorAtual={pedido ? { id: pedido.fornecedorId, nome: pedido.fornecedorNome } : undefined}
                    disabled={somenteLeitura}
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
            id="pedido-compra-adicionar-produto"
            aria-label="Adicionar produto"
            placeholder="Buscar produto por nome ou SKU para adicionar"
            campoPreco="custo"
            onChange={(_, produto) => adicionarProduto(produto)}
          />
        )}
        <ItensPedidoCompraTabela
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
                    id="pedido-compra-desconto"
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
      titulo={`Confirmar o pedido de compra nº ${pedido?.id}?`}
      descricao="Depois de confirmado, o pedido não pode mais ser editado: só cancelado. A entrada no estoque e o custo dos produtos são atualizados automaticamente. Escolha em quantas parcelas a compra será paga."
      carregando={salvarPedido.isPending || confirmarPedido.isPending}
      aoConfirmar={confirmar}
      aoFechar={() => setConfirmando(null)}
    />
    </>
  )
}
