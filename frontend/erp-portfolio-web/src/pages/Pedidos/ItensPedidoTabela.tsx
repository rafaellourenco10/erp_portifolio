/**
 * =====================================================================
 * Arquivo....: ItensPedidoTabela.tsx
 * Versão.....: 1.1.0
 * Data.......: 21/09/2026
 * Descrição..: Tabela editável dos itens do pedido (produto, quantidade, preço só leitura,
 *              desconto %, subtotal e remover). A quantidade aceita decimais só para
 *              KG, L e M (UN e CX só inteiros). O subtotal vem do cálculo em tela
 *              (pré-visualização); o preço mostrado é o do produto ao adicionar o item, ou
 *              o preço congelado quando o pedido já foi salvo.
 *              Em tela larga é uma tabela; no celular (< 768 px) cada item vira um cartão
 *              empilhado, com todos os campos visíveis e sem rolagem horizontal.
 * ---------------------------------------------------------------------
 * Fontes.....: campos do formulário do pedido (React Hook Form, useFieldArray)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 *   1.1.0 - 21/09/2026 - Cartões por item no celular (T15).
 * =====================================================================
 */

import { DeleteOutlined } from '@ant-design/icons'
import { Button, Col, Flex, Grid, InputNumber, Row, Table, type TableColumnsType } from 'antd'
import { Controller, type Control } from 'react-hook-form'
import type { PedidoFormEntrada, PedidoFormValores, PedidoItemFormEntrada } from '../../schemas/pedidoSchema'
import { formatarReal } from '../../utils/moeda'
import './pedido.css'

type LinhaItem = PedidoItemFormEntrada & { id: string }

interface ItensPedidoTabelaProps {
  control: Control<PedidoFormEntrada, unknown, PedidoFormValores>
  /** Linhas do useFieldArray (cada uma com o `id` interno do React Hook Form). */
  linhas: LinhaItem[]
  /** Subtotal de cada linha, na mesma ordem (em reais). */
  subtotais: number[]
  aoRemover: (indice: number) => void
  somenteLeitura?: boolean
}

const unidadeInteira = (unidade: string) => unidade === 'UN' || unidade === 'CX'

interface CampoItemProps {
  control: ItensPedidoTabelaProps['control']
  indice: number
  linha: LinhaItem
  somenteLeitura?: boolean
}

function CampoQuantidade({ control, indice, linha, somenteLeitura }: CampoItemProps) {
  return (
    <Controller
      name={`itens.${indice}.quantidade`}
      control={control}
      render={({ field, fieldState }) => (
        <>
          <InputNumber
            className="campo-cheio numeros-tabulares"
            aria-label={`Quantidade de ${linha.produtoNome}`}
            min={0}
            max={999_999.999}
            precision={unidadeInteira(linha.unidade) ? 0 : 3}
            decimalSeparator=","
            controls={false}
            suffix={linha.unidade}
            status={fieldState.error ? 'error' : undefined}
            disabled={somenteLeitura}
            value={field.value}
            onChange={field.onChange}
            onBlur={field.onBlur}
          />
          {fieldState.error && <div className="erro-celula">{fieldState.error.message}</div>}
        </>
      )}
    />
  )
}

function CampoDesconto({ control, indice, linha, somenteLeitura }: CampoItemProps) {
  return (
    <Controller
      name={`itens.${indice}.descontoPercentual`}
      control={control}
      render={({ field, fieldState }) => (
        <>
          <InputNumber
            className="campo-cheio numeros-tabulares"
            aria-label={`Desconto de ${linha.produtoNome}`}
            min={0}
            max={100}
            precision={2}
            decimalSeparator=","
            controls={false}
            suffix="%"
            status={fieldState.error ? 'error' : undefined}
            disabled={somenteLeitura}
            value={field.value}
            onChange={field.onChange}
            onBlur={field.onBlur}
          />
          {fieldState.error && <div className="erro-celula">{fieldState.error.message}</div>}
        </>
      )}
    />
  )
}

export function ItensPedidoTabela({ control, linhas, subtotais, aoRemover, somenteLeitura }: ItensPedidoTabelaProps) {
  const colunas: TableColumnsType<LinhaItem> = [
    {
      title: 'Produto',
      key: 'produto',
      render: (_, linha) => (
        <div className="celula-compacta">
          <span className="celula-nome">{linha.produtoNome}</span>
          <span className="texto-discreto numeros-tabulares">SKU {linha.sku}</span>
        </div>
      ),
    },
    {
      title: 'Quantidade',
      key: 'quantidade',
      width: 170,
      render: (_, linha, indice) => <CampoQuantidade control={control} indice={indice} linha={linha} somenteLeitura={somenteLeitura} />,
    },
    {
      title: 'Preço',
      key: 'preco',
      width: 130,
      align: 'right',
      render: (_, linha) => <span className="numeros-tabulares">{formatarReal(linha.precoUnitario)}</span>,
    },
    {
      title: 'Desconto',
      key: 'desconto',
      width: 130,
      render: (_, linha, indice) => <CampoDesconto control={control} indice={indice} linha={linha} somenteLeitura={somenteLeitura} />,
    },
    {
      title: 'Subtotal',
      key: 'subtotal',
      width: 150,
      align: 'right',
      render: (_, __, indice) => (
        <strong className="numeros-tabulares" data-testid={`subtotal-${indice}`}>
          {formatarReal(subtotais[indice] ?? 0)}
        </strong>
      ),
    },
    ...(somenteLeitura
      ? []
      : [
          {
            title: '',
            key: 'remover',
            width: 56,
            render: (_: unknown, linha: LinhaItem, indice: number) => (
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                aria-label={`Remover ${linha.produtoNome}`}
                onClick={() => aoRemover(indice)}
              />
            ),
          },
        ]),
  ]

  const telas = Grid.useBreakpoint()

  if (telas.md === false) {
    return (
      <div className="tabela-itens-pedido itens-cartoes">
        {linhas.length === 0 && <div className="painel itens-vazio texto-discreto">Nenhum item. Busque um produto acima para adicionar.</div>}
        {linhas.map((linha, indice) => (
          <div key={linha.id} className="painel item-cartao">
            <Flex justify="space-between" align="flex-start" gap={12}>
              <div className="celula-compacta">
                <span className="celula-nome">{linha.produtoNome}</span>
                <span className="texto-discreto numeros-tabulares">SKU {linha.sku}</span>
              </div>
              {!somenteLeitura && (
                <Button
                  type="text"
                  danger
                  icon={<DeleteOutlined />}
                  aria-label={`Remover ${linha.produtoNome}`}
                  onClick={() => aoRemover(indice)}
                />
              )}
            </Flex>
            <Row gutter={[12, 12]}>
              <Col xs={12}>
                <div className="rotulo-filtro">Quantidade</div>
                <CampoQuantidade control={control} indice={indice} linha={linha} somenteLeitura={somenteLeitura} />
              </Col>
              <Col xs={12}>
                <div className="rotulo-filtro">Desconto</div>
                <CampoDesconto control={control} indice={indice} linha={linha} somenteLeitura={somenteLeitura} />
              </Col>
            </Row>
            <Flex justify="space-between" align="baseline" wrap gap={12} className="item-cartao-valores">
              <span className="texto-discreto">
                Preço <span className="numeros-tabulares">{formatarReal(linha.precoUnitario)}</span>
              </span>
              <span>
                Subtotal{' '}
                <strong className="numeros-tabulares" data-testid={`subtotal-${indice}`}>
                  {formatarReal(subtotais[indice] ?? 0)}
                </strong>
              </span>
            </Flex>
          </div>
        ))}
      </div>
    )
  }

  return (
    <div className="painel painel-tabela tabela-itens-pedido">
      <Table<LinhaItem>
        rowKey="id"
        columns={colunas}
        dataSource={linhas}
        pagination={false}
        scroll={{ x: 'max-content' }}
        locale={{ emptyText: 'Nenhum item. Busque um produto acima para adicionar.' }}
      />
    </div>
  )
}
