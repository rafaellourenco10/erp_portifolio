/**
 * =====================================================================
 * Arquivo....: App.tsx
 * Versão.....: 1.1.0
 * Data.......: 18/09/2026
 * Descrição..: Layout principal do Ambition ERP: menu lateral (256px,
 *              recolhível para 72px; vira gaveta no celular), cabeçalho
 *              com breadcrumb e área de conteúdo. Nesta etapa existe
 *              apenas o módulo Clientes.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Layout do tema Ambition ERP (logo, menu recolhível,
 *                        gaveta no celular, breadcrumb).
 * =====================================================================
 */

import { MenuFoldOutlined, MenuOutlined, MenuUnfoldOutlined, TeamOutlined } from '@ant-design/icons'
import { Breadcrumb, Button, Drawer, Grid, Layout, Menu, type MenuProps } from 'antd'
import { useState } from 'react'
import './App.css'
import { LogoAmbition } from './components/LogoAmbition'
import { ClientesListaPage } from './pages/Clientes/ClientesListaPage'

const itensMenu: MenuProps['items'] = [{ key: 'clientes', icon: <TeamOutlined />, label: 'Clientes' }]

const trilha = [{ title: 'Gestão Comercial' }, { title: 'Clientes' }]

export default function App() {
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const [recolhido, setRecolhido] = useState(false)
  const [menuCelularAberto, setMenuCelularAberto] = useState(false)

  return (
    <Layout className="app">
      {!ehCelular && (
        <Layout.Sider
          className="app-menu-lateral"
          width={256}
          collapsedWidth={72}
          collapsible
          collapsed={recolhido}
          trigger={null}
          breakpoint="lg"
          onBreakpoint={setRecolhido}
        >
          <div className="app-logo">
            <LogoAmbition compacto={recolhido} />
          </div>
          {!recolhido && <div className="app-secao-menu">Gestão comercial</div>}
          <Menu mode="inline" selectedKeys={['clientes']} items={itensMenu} />
        </Layout.Sider>
      )}

      <Layout>
        <Layout.Header className="app-cabecalho">
          {ehCelular ? (
            <>
              <Button
                type="text"
                icon={<MenuOutlined />}
                aria-label="Abrir menu"
                onClick={() => setMenuCelularAberto(true)}
              />
              <LogoAmbition tamanho={28} />
            </>
          ) : (
            <>
              <Button
                type="text"
                icon={recolhido ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                aria-label={recolhido ? 'Expandir menu' : 'Recolher menu'}
                onClick={() => setRecolhido((atual) => !atual)}
              />
              <Breadcrumb items={trilha} />
            </>
          )}
        </Layout.Header>

        <Layout.Content className="app-conteudo">
          <ClientesListaPage />
        </Layout.Content>
      </Layout>

      <Drawer
        placement="left"
        size={272}
        open={ehCelular && menuCelularAberto}
        onClose={() => setMenuCelularAberto(false)}
        title={<LogoAmbition tamanho={28} />}
        className="app-menu-celular"
      >
        <div className="app-secao-menu">Gestão comercial</div>
        <Menu
          mode="inline"
          selectedKeys={['clientes']}
          items={itensMenu}
          onClick={() => setMenuCelularAberto(false)}
        />
      </Drawer>
    </Layout>
  )
}
