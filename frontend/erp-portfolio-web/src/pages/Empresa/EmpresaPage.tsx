/**
 * =====================================================================
 * Arquivo....: EmpresaPage.tsx
 * Versão.....: 1.0.0
 * Data.......: 28/09/2026
 * Descrição..: Fiscal → Empresa (etapa 16): dados do emitente da NF-e
 *              (identificação, endereço e série). Formulário único; a API
 *              cria ou atualiza a linha da empresa.
 * ---------------------------------------------------------------------
 * Fontes.....: GET/PUT /api/empresa (useEmpresa, useSalvarEmpresa)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 28/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { CheckOutlined } from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, App, Button, Col, Flex, Form, Input, InputNumber, Row, Select, Skeleton } from 'antd'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { lerErroApi } from '../../api/axiosClient'
import { ItemFormulario } from '../../components/ItemFormulario'
import { useEmpresa, useSalvarEmpresa } from '../../hooks/useEmpresa'
import {
  empresaSchema,
  paraPayload,
  valoresIniciaisEmpresa,
  type EmpresaFormEntrada,
  type EmpresaFormValores,
} from '../../schemas/empresaSchema'
import { formatarCep } from '../../schemas/enderecoFiscal'
import { formatarDocumento } from '../../utils/documento'
import { UFS, compararRelevanciaUf, ufCorrespondeBusca } from '../../utils/ufs'
import '../Clientes/clientes.css'

const opcoesUf = UFS.map((uf) => ({ value: uf.sigla, label: `${uf.nome} (${uf.sigla})`, uf }))

type CampoTexto = Exclude<keyof EmpresaFormEntrada, 'serieNfe' | 'uf'>

export function EmpresaPage() {
  const { message } = App.useApp()
  const { data: empresa, isLoading, isError } = useEmpresa()
  const salvarEmpresa = useSalvarEmpresa()

  const {
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<EmpresaFormEntrada, unknown, EmpresaFormValores>({
    resolver: zodResolver(empresaSchema),
    defaultValues: valoresIniciaisEmpresa,
  })

  useEffect(() => {
    if (!empresa) return
    reset({
      ...empresa,
      nomeFantasia: empresa.nomeFantasia ?? '',
      complemento: empresa.complemento ?? '',
      telefone: empresa.telefone ?? '',
      cnpj: formatarDocumento(empresa.cnpj),
      cep: formatarCep(empresa.cep),
    })
  }, [empresa, reset])

  async function salvar(valores: EmpresaFormValores) {
    try {
      await salvarEmpresa.mutateAsync(paraPayload(valores))
      message.success('Dados da empresa salvos.')
    } catch (erro) {
      const erroApi = lerErroApi(erro)
      const camposComErro = Object.entries(erroApi.errosPorCampo).filter(([campo]) => campo in valores)
      camposComErro.forEach(([campo, mensagem]) => setError(campo as keyof EmpresaFormEntrada, { message: mensagem }))
      if (camposComErro.length === 0) message.error(erroApi.mensagem)
    }
  }

  const campo = (nome: CampoTexto, rotulo: string, maximo: number, sm: number, obrigatorio = false, placeholder?: string) => (
    <Col xs={24} sm={sm}>
      <ItemFormulario rotulo={rotulo} erro={errors[nome]} obrigatorio={obrigatorio}>
        <Controller
          name={nome}
          control={control}
          render={({ field }) => <Input {...field} maxLength={maximo} placeholder={placeholder} />}
        />
      </ItemFormulario>
    </Col>
  )

  return (
    <>
      <Flex justify="space-between" align="flex-end" wrap gap={16} className="pagina-cabecalho">
        <div>
          <h1 className="pagina-titulo">Empresa</h1>
          <p className="pagina-subtitulo">Dados do emitente que saem na NF-e e no DANFE.</p>
        </div>
      </Flex>

      {isError && <Alert type="error" showIcon className="alerta-erro" title="Não foi possível carregar os dados da empresa." />}
      {!isLoading && !isError && !empresa && (
        <Alert
          type="info"
          showIcon
          className="alerta-erro"
          title="Empresa ainda não cadastrada"
          description="Preencha e salve para poder emitir NF-e. As notas são simuladas (ambiente de homologação, sem valor fiscal)."
        />
      )}

      <div className="painel" style={{ padding: 24 }}>
        {isLoading ? (
          <Skeleton active paragraph={{ rows: 8 }} />
        ) : (
          <Form layout="vertical" size="large" requiredMark={false} noValidate onFinish={() => handleSubmit(salvar)()}>
            <div className="drawer-secao-titulo">Identificação</div>
            <Row gutter={20} style={{ marginTop: 12 }}>
              {campo('razaoSocial', 'Razão social', 60, 12, true)}
              {campo('nomeFantasia', 'Nome fantasia', 60, 12)}
              <Col xs={24} sm={8}>
                <ItemFormulario rotulo="CNPJ" erro={errors.cnpj} obrigatorio>
                  <Controller
                    name="cnpj"
                    control={control}
                    render={({ field }) => (
                      <Input
                        {...field}
                        className="numeros-tabulares"
                        maxLength={18}
                        onBlur={() => {
                          field.onChange(formatarDocumento(field.value))
                          field.onBlur()
                        }}
                      />
                    )}
                  />
                </ItemFormulario>
              </Col>
              {campo('inscricaoEstadual', 'Inscrição Estadual', 20, 6, true)}
              {campo('telefone', 'Telefone', 20, 6)}
              <Col xs={24} sm={4}>
                <ItemFormulario rotulo="Série da NF-e" erro={errors.serieNfe} obrigatorio>
                  <Controller
                    name="serieNfe"
                    control={control}
                    render={({ field }) => (
                      <InputNumber
                        className="campo-cheio numeros-tabulares"
                        min={0}
                        max={999}
                        precision={0}
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

            <div className="drawer-secao">
              <div className="drawer-secao-titulo">Endereço</div>
            </div>
            <Row gutter={20}>
              {campo('cep', 'CEP', 9, 6, true, '00000-000')}
              {campo('logradouro', 'Logradouro', 60, 12, true)}
              {campo('numero', 'Número', 10, 6, true)}
              {campo('complemento', 'Complemento', 60, 8)}
              {campo('bairro', 'Bairro', 60, 8, true)}
              {campo('municipio', 'Município', 60, 8, true)}
              {campo('codigoMunicipio', 'Código IBGE do município', 7, 8, true, '3550308')}
              <Col xs={24} sm={8}>
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

            <Flex justify="flex-end">
              <Button type="primary" size="large" icon={<CheckOutlined />} htmlType="submit" loading={salvarEmpresa.isPending}>
                Salvar empresa
              </Button>
            </Flex>
          </Form>
        )}
      </div>
    </>
  )
}
