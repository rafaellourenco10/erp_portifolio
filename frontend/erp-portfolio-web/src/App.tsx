/**
 * =====================================================================
 * Arquivo....: App.tsx
 * Versão.....: 1.4.0
 * Data.......: 21/09/2026
 * Descrição..: Layout principal do Ambition ERP: menu lateral (256px,
 *              recolhível para 72px; vira gaveta no celular), cabeçalho
 *              com breadcrumb e área de conteúdo. As telas são trocadas
 *              por rota (/clientes, /produtos, /categorias, /pedidos) com o React Router.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Layout do tema Ambition ERP (logo, menu recolhível,
 *                        gaveta no celular, breadcrumb).
 *   1.2.0 - 21/09/2026 - Rotas com React Router; módulo Produtos no menu;
 *                        breadcrumb acompanha a rota.
 *   1.3.0 - 21/09/2026 - Rota e item de menu de Categorias.
 *   1.4.0 - 21/09/2026 - Item de menu e rotas de Pedidos (/pedidos, /pedidos/novo,
 *                        /pedidos/:id); o menu e o breadcrumb valem também nas subpáginas.
 * =====================================================================
 */

import {
  AppstoreOutlined,
  MenuFoldOutlined,
  MenuOutlined,
  MenuUnfoldOutlined,
  ShoppingCartOutlined,
  TagsOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import { Breadcrumb, Button, Drawer, Grid, Layout, Menu, type MenuProps } from 'antd'
import { useState } from 'react'
import { Link, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import './App.css'
import { LogoAmbition } from './components/LogoAmbition'
import { CategoriasListaPage } from './pages/Categorias/CategoriasListaPage'
import { ClientesListaPage } from './pages/Clientes/ClientesListaPage'
import { PedidoPage } from './pages/Pedidos/PedidoPage'
import { PedidosListaPage } from './pages/Pedidos/PedidosListaPage'
import { ProdutosListaPage } from './pages/Produtos/ProdutosListaPage'

// A chave de cada item é o caminho da rota.
const itensMenu = [
  { key: '/clientes', icon: <TeamOutlined />, label: 'Clientes' },
  { key: '/produtos', icon: <TagsOutlined />, label: 'Produtos' },
  { key: '/categorias', icon: <AppstoreOutlined />, label: 'Categorias' },
  { key: '/pedidos', icon: <ShoppingCartOutlined />, label: 'Pedidos' },
] satisfies MenuProps['items']

/** Título da subpágina de um módulo (a última parte do breadcrumb), ou undefined na página principal. */
function tituloDaSubpagina(pathname: string): string | undefined {
  if (pathname === '/pedidos/novo') return 'Novo pedido'
  const numero = /^\/pedidos\/(\d+)$/.exec(pathname)?.[1]
  return numero ? `Pedido nº ${numero}` : undefined
}

export default function App() {
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const { pathname } = useLocation()
  const navegar = useNavigate()
  // O item do menu vale também nas subpáginas (/pedidos/novo, /pedidos/12 continuam marcando "Pedidos").
  const itemAtual = itensMenu.find((item) => pathname === item.key || pathname.startsWith(`${item.key}/`))
  const chavesSelecionadas = itemAtual ? [itemAtual.key] : []
  const subpagina = tituloDaSubpagina(pathname)
  const trilha = [
    { title: 'Gestão Comercial' },
    { title: subpagina && itemAtual ? <Link to={itemAtual.key}>{itemAtual.label}</Link> : itemAtual?.label },
    ...(subpagina ? [{ title: subpagina }] : []),
  ]

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
          <Menu mode="inline" selectedKeys={chavesSelecionadas} items={itensMenu} onClick={({ key }) => navegar(key)} />
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
          <Routes>
            <Route path="/clientes" element={<ClientesListaPage />} />
            <Route path="/produtos" element={<ProdutosListaPage />} />
            <Route path="/categorias" element={<CategoriasListaPage />} />
            <Route path="/pedidos" element={<PedidosListaPage />} />
            <Route path="/pedidos/novo" element={<PedidoPage />} />
            <Route path="/pedidos/:id" element={<PedidoPage />} />
            <Route path="*" element={<Navigate to="/clientes" replace />} />
          </Routes>
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
          selectedKeys={chavesSelecionadas}
          items={itensMenu}
          onClick={({ key }) => {
            navegar(key)
            setMenuCelularAberto(false)
          }}
        />
      </Drawer>
    </Layout>
  )
}
