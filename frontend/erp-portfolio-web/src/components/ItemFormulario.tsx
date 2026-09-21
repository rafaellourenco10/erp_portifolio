/**
 * =====================================================================
 * Arquivo....: ItemFormulario.tsx
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Item de formulário do Ant Design ligado ao React Hook
 *              Form: rótulo com marca de obrigatório e mensagem de erro
 *              do campo. Extraído do ClienteFormDrawer para ser usado
 *              também pelos formulários de outros módulos.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { Form } from 'antd'
import type { ReactNode } from 'react'
import type { FieldError } from 'react-hook-form'

interface ItemFormularioProps {
  rotulo: string
  erro?: FieldError
  obrigatorio?: boolean
  children: ReactNode
}

export function ItemFormulario({ rotulo, erro, obrigatorio, children }: ItemFormularioProps) {
  return (
    <Form.Item
      label={
        <>
          {rotulo}
          {obrigatorio && <span className="campo-obrigatorio">*</span>}
        </>
      }
      validateStatus={erro ? 'error' : undefined}
      help={erro?.message}
    >
      {children}
    </Form.Item>
  )
}
