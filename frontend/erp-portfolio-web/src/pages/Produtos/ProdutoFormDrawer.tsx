/**
 * =====================================================================
 * Arquivo....: ProdutoFormDrawer.tsx
 * Versão.....: 1.4.0
 * Data.......: 22/09/2026
 * Descrição..: Painel lateral (Drawer) com o formulário de inclusão/edição
 *              de produto (React Hook Form + Zod), no mesmo layout do
 *              painel de clientes. Erros de validação (400) e de SKU
 *              duplicado (409) da API são exibidos nos campos. O seletor de
 *              categoria tem um atalho "Nova categoria" que abre o
 *              CategoriaFormDrawer por cima e já seleciona a categoria criada.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/produtos e PUT /api/produtos/{id}
 *              (via useSalvarProduto)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 21/09/2026 - Campo SKU só aceita dígitos.
 *   1.2.0 - 21/09/2026 - Categoria vira uma seleção das categorias ativas.
 *   1.3.0 - 22/09/2026 - Atalho "Nova categoria" no seletor.
 *   1.4.0 - 22/09/2026 - Campo Estoque mínimo.
 * =====================================================================
 */

import { CheckOutlined, PlusOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { App, Button, Col, Divider, Drawer, Flex, Form, Grid, Input, InputNumber, Row, Select, Switch } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { TagStatus } from '../../components/TagStatus'
import { useCategoriasAtivas } from '../../hooks/useCategorias'
import { useSalvarProduto } from '../../hooks/useProdutos'
import {
  UNIDADES,
  paraPayload,
  produtoSchema,
  valoresIniciaisProduto,
  type ProdutoFormEntrada,
  type ProdutoFormValores,
} from '../../schemas/produtoSchema'
import type { Categoria } from '../../types/categoria'
import type { Produto } from '../../types/produto'
import { CategoriaFormDrawer } from '../Categorias/CategoriaFormDrawer'
// O painel reaproveita as classes .drawer-cliente*, .caixa-status* etc. do módulo de Clientes.
import '../Clientes/clientes.css'

const ID_FORMULARIO = 'formulario-produto'

interface ProdutoFormDrawerProps {
  aberto: boolean
  /** Produto em edição; null para inclusão. */
  produto: Produto | null
  aoFechar: () => void
}

export function ProdutoFormDrawer({ aberto, produto, aoFechar }: ProdutoFormDrawerProps) {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const salvarProduto = useSalvarProduto()
  const { data: categorias, isLoading: carregandoCategorias } = useCategoriasAtivas()
  const emEdicao = produto !== null
  const [novaCategoriaAberta, setNovaCategoriaAberta] = useState(false)
  // Guardada localmente: a busca de "categorias ativas" pode levar um instante para refletir a inclusão.
  const [categoriaRecemCriada, setCategoriaRecemCriada] = useState<Categoria | null>(null)

  // A categoria atual do produto entra na lista mesmo se foi inativada depois: senão sumiria da edição.
  const opcoesCategoria = useMemo(() => {
    const opcoes = (categorias?.itens ?? []).map((categoria) => ({ value: categoria.id, label: categoria.nome }))
    if (produto?.categoriaId != null && !opcoes.some((opcao) => opcao.value === produto.categoriaId)) {
      opcoes.unshift({ value: produto.categoriaId, label: `${produto.categoriaNome} (inativa)` })
    }
    if (categoriaRecemCriada && !opcoes.some((opcao) => opcao.value === categoriaRecemCriada.id)) {
      opcoes.unshift({ value: categoriaRecemCriada.id, label: categoriaRecemCriada.nome })
    }
    return opcoes
  }, [categorias, produto, categoriaRecemCriada])

  const {
    control,
    handleSubmit,
    reset,
    setError,
    setValue,
    formState: { errors },
  } = useForm<ProdutoFormEntrada, unknown, ProdutoFormValores>({
    resolver: zodResolver(produtoSchema),
    defaultValues: valoresIniciaisProduto,
  })

  function aoCriarCategoria(categoria: Categoria) {
    setCategoriaRecemCriada(categoria)
    setValue('categoriaId', categoria.id)
  }

  useEffect(() => {
    if (!aberto) return

    reset(
      produto
        ? {
            nome: produto.nome,
            sku: produto.sku,
            categoriaId: produto.categoriaId,
            unidade: produto.unidade,
            precoVenda: produto.precoVenda,
            custo: produto.custo,
            estoqueMinimo: produto.estoqueMinimo,
            ativo: produto.ativo,
          }
        : valoresIniciaisProduto,
    )
  }, [aberto, produto, reset])

  async function salvar(valores: ProdutoFormValores) {
    try {
      await salvarProduto.mutateAsync({ id: produto?.id, dados: paraPayload(valores) })
      message.success(emEdicao ? 'Produto atualizado com sucesso.' : 'Produto cadastrado com sucesso.')
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)

      if (erroApi.status === 409) {
        setError('sku', { message: erroApi.mensagem })
        return
      }

      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(([campo]) => campo in valores)
      camposComErro.forEach(([campo, mensagem]) =>
        setError(campo as keyof ProdutoFormEntrada, { message: mensagem }),
      )

      if (camposComErro.length === 0) {
        message.error(erroApi.mensagem)
      }
    }
  }

  return (
    <>
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
            {emEdicao ? 'Editar produto' : 'Novo produto'}
          </div>
          <div className="drawer-cliente-subtitulo">Preencha os dados do produto</div>
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
            loading={salvarProduto.isPending}
          >
            Salvar produto
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
        <Row gutter={20}>
          <Col xs={24}>
            <ItemFormulario rotulo="Nome do produto" erro={errors.nome} obrigatorio>
              <Controller
                name="nome"
                control={control}
                render={({ field }) => <Input {...field} maxLength={150} autoFocus />}
              />
            </ItemFormulario>
          </Col>

          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="SKU" erro={errors.sku} obrigatorio>
              <Controller
                name="sku"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    className="numeros-tabulares"
                    inputMode="numeric"
                    maxLength={30}
                    placeholder="Ex.: 10012345"
                    onChange={(e) => field.onChange(e.target.value.replace(/\D/g, ''))}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Unidade" erro={errors.unidade} obrigatorio>
              <Controller
                name="unidade"
                control={control}
                render={({ field }) => (
                  <Select
                    value={field.value || undefined}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    options={[...UNIDADES]}
                    placeholder="Selecione"
                  />
                )}
              />
            </ItemFormulario>
          </Col>

          <Col xs={24}>
            <ItemFormulario rotulo="Categoria" erro={errors.categoriaId}>
              <Controller
                name="categoriaId"
                control={control}
                render={({ field }) => (
                  <Select
                    value={field.value ?? undefined}
                    onChange={(valor: number | undefined) => field.onChange(valor ?? null)}
                    onBlur={field.onBlur}
                    options={opcoesCategoria}
                    loading={carregandoCategorias}
                    allowClear
                    showSearch={{ optionFilterProp: 'label' }}
                    placeholder="Sem categoria"
                    notFoundContent="Nenhuma categoria ativa."
                    popupRender={(menu) => (
                      <>
                        {menu}
                        <Divider style={{ margin: '8px 0' }} />
                        <Button type="text" icon={<PlusOutlined />} block onClick={() => setNovaCategoriaAberta(true)}>
                          Nova categoria
                        </Button>
                      </>
                    )}
                  />
                )}
              />
            </ItemFormulario>
          </Col>

          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Preço de venda" erro={errors.precoVenda} obrigatorio>
              <Controller
                name="precoVenda"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    className="campo-cheio numeros-tabulares"
                    prefix="R$"
                    min={0}
                    max={9_999_999_999.99}
                    precision={2}
                    decimalSeparator=","
                    controls={false}
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Custo" erro={errors.custo} obrigatorio>
              <Controller
                name="custo"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    className="campo-cheio numeros-tabulares"
                    prefix="R$"
                    min={0}
                    max={9_999_999_999.99}
                    precision={2}
                    decimalSeparator=","
                    controls={false}
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                  />
                )}
              />
            </ItemFormulario>
          </Col>
          <Col xs={24} sm={12}>
            <ItemFormulario rotulo="Estoque mínimo" erro={errors.estoqueMinimo} obrigatorio>
              <Controller
                name="estoqueMinimo"
                control={control}
                render={({ field }) => (
                  <InputNumber
                    className="campo-cheio numeros-tabulares"
                    min={0}
                    max={999_999.999}
                    precision={3}
                    decimalSeparator=","
                    controls={false}
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
                  <TagStatus ativo={field.value} rotuloAtivo="Produto Ativo" rotuloInativo="Produto Inativo" />
                  <Switch checked={field.value} onChange={field.onChange} aria-label="Produto ativo" />
                </Flex>
              </div>
            )}
          />
        )}
      </Form>
    </Drawer>

    <CategoriaFormDrawer
      aberto={novaCategoriaAberta}
      categoria={null}
      aoFechar={() => setNovaCategoriaAberta(false)}
      aoCriar={aoCriarCategoria}
    />
    </>
  )
}
