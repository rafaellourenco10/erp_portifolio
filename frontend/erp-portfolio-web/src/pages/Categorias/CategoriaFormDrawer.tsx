/**
 * =====================================================================
 * Arquivo....: CategoriaFormDrawer.tsx
 * Versão.....: 1.1.0
 * Data.......: 22/09/2026
 * Descrição..: Painel lateral (Drawer) com o formulário de inclusão/edição
 *              de categoria (React Hook Form + Zod). Erros de validação
 *              (400) e de nome duplicado (409) da API aparecem no campo.
 *              Ao incluir, avisa o chamador (prop aoCriar) com a categoria
 *              nova — usado pelo atalho "Nova categoria" do seletor de Produtos.
 * ---------------------------------------------------------------------
 * Fontes.....: POST /api/categorias e PUT /api/categorias/{id}
 *              (via useSalvarCategoria)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 22/09/2026 - Prop aoCriar, para o atalho "Nova categoria" em Produtos.
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { App, Button, Drawer, Flex, Form, Grid, Input, Switch } from 'antd'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { TagStatus } from '../../components/TagStatus'
import { useSalvarCategoria } from '../../hooks/useCategorias'
import { categoriaSchema, valoresIniciaisCategoria, type CategoriaFormValores } from '../../schemas/categoriaSchema'
import type { Categoria } from '../../types/categoria'
// O painel reaproveita as classes .drawer-cliente*, .caixa-status* etc. do módulo de Clientes.
import '../Clientes/clientes.css'

const ID_FORMULARIO = 'formulario-categoria'

interface CategoriaFormDrawerProps {
  aberto: boolean
  /** Categoria em edição; null para inclusão. */
  categoria: Categoria | null
  aoFechar: () => void
  /** Chamado só na inclusão, com a categoria recém-criada (ex.: para selecioná-la em outro formulário). */
  aoCriar?: (categoria: Categoria) => void
}

export function CategoriaFormDrawer({ aberto, categoria, aoFechar, aoCriar }: CategoriaFormDrawerProps) {
  const { message } = App.useApp()
  const telas = Grid.useBreakpoint()
  const salvarCategoria = useSalvarCategoria()
  const emEdicao = categoria !== null

  const {
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<CategoriaFormValores>({
    resolver: zodResolver(categoriaSchema),
    defaultValues: valoresIniciaisCategoria,
  })

  useEffect(() => {
    if (!aberto) return
    reset(categoria ? { nome: categoria.nome, ativo: categoria.ativo } : valoresIniciaisCategoria)
  }, [aberto, categoria, reset])

  async function salvar(valores: CategoriaFormValores) {
    try {
      const salva = await salvarCategoria.mutateAsync({ id: categoria?.id, dados: valores })
      message.success(emEdicao ? 'Categoria atualizada com sucesso.' : 'Categoria cadastrada com sucesso.')
      if (!emEdicao) aoCriar?.(salva)
      aoFechar()
    } catch (erro) {
      const erroApi = lerErroApi(erro)

      if (erroApi.status === 409) {
        setError('nome', { message: erroApi.mensagem })
        return
      }

      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(([campo]) => campo in valores)
      camposComErro.forEach(([campo, mensagem]) =>
        setError(campo as keyof CategoriaFormValores, { message: mensagem }),
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
      size={telas.sm === false ? '100%' : 480}
      closable={{ placement: 'end' }}
      destroyOnHidden
      className="drawer-cliente"
      title={
        <div>
          <div className="drawer-cliente-titulo">
            <span className="ponto-destaque" />
            {emEdicao ? 'Editar categoria' : 'Nova categoria'}
          </div>
          <div className="drawer-cliente-subtitulo">Informe o nome da categoria de produtos</div>
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
            loading={salvarCategoria.isPending}
          >
            Salvar categoria
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
        <ItemFormulario rotulo="Nome da categoria" erro={errors.nome} obrigatorio>
          <Controller
            name="nome"
            control={control}
            render={({ field }) => (
              <Input
                {...field}
                maxLength={60}
                placeholder="Ex.: PERIFÉRICOS"
                autoFocus
                onChange={(e) => field.onChange(e.target.value.toUpperCase())}
              />
            )}
          />
        </ItemFormulario>

        {emEdicao && (
          <Controller
            name="ativo"
            control={control}
            render={({ field }) => (
              <div className="caixa-status">
                <div>
                  <div className="caixa-status-titulo">Status do cadastro</div>
                  <div className="caixa-status-descricao">
                    Inativa deixa de ser oferecida em novos produtos; os produtos atuais continuam nela.
                  </div>
                </div>
                <Flex align="center" gap={12}>
                  <TagStatus ativo={field.value} rotuloAtivo="Categoria Ativa" rotuloInativo="Categoria Inativa" />
                  <Switch checked={field.value} onChange={field.onChange} aria-label="Categoria ativa" />
                </Flex>
              </div>
            )}
          />
        )}
      </Form>
    </Drawer>
  )
}
