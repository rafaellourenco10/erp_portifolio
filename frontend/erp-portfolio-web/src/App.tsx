/**
 * =====================================================================
 * Arquivo....: App.tsx
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: Layout principal do ERP (cabeçalho, menu lateral e área
 *              de conteúdo). Nesta etapa existe apenas o módulo Clientes.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */

import { TeamOutlined } from '@ant-design/icons'
import { Layout, Menu, Typography } from 'antd'
import { ClientesListaPage } from './pages/Clientes/ClientesListaPage'

const itensMenu = [{ key: 'clientes', icon: <TeamOutlined />, label: 'Clientes' }]

export default function App() {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Header style={{ display: 'flex', alignItems: 'center' }}>
        <Typography.Title level={4} style={{ color: '#fff', margin: 0 }}>
          ERP Portfólio
        </Typography.Title>
      </Layout.Header>

      <Layout>
        <Layout.Sider breakpoint="lg" collapsedWidth={0} width={200} theme="light">
          <Menu mode="inline" selectedKeys={['clientes']} items={itensMenu} style={{ height: '100%' }} />
        </Layout.Sider>

        <Layout.Content style={{ padding: 24 }}>
          <ClientesListaPage />
        </Layout.Content>
      </Layout>
    </Layout>
  )
}
