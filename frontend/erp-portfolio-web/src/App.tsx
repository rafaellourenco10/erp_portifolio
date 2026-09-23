/**
 * =====================================================================
 * Arquivo....: App.tsx
 * Versão.....: 1.16.0
 * Data.......: 23/09/2026
 * Descrição..: Layout principal do Ambition ERP: menu lateral (256px,
 *              recolhível para 72px; vira gaveta no celular), cabeçalho
 *              com breadcrumb e área de conteúdo. As telas são trocadas
 *              por rota (/, /clientes, /fornecedores, /produtos, /categorias,
 *              /vendedores, /pedidos, /pedidos-compra, /estoque, /contas-receber, /contas-pagar, /comissoes, /relatorios/*) com o
 *              React Router. O menu é dividido por departamento (Cadastro,
 *              Ordem Vendas/Compras, Depósito, Financeiro); o Dashboard (/)
 *              fica fora das seções: resume vários módulos.
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
 *   1.5.0 - 22/09/2026 - Rota e item de menu de Estoque.
 *   1.6.0 - 22/09/2026 - Rota e item de menu de Contas a Receber.
 *   1.7.0 - 22/09/2026 - Painel (Dashboard) como rota inicial "/", fora de
 *                        "Gestão Comercial"; rota desconhecida cai em "/" (era /clientes).
 *   1.8.0 - 23/09/2026 - Rota e item de menu de Fornecedores (etapa 7).
 *   1.9.0 - 23/09/2026 - Item de menu e rotas de Pedidos de Compra (/pedidos-compra,
 *                        /pedidos-compra/novo, /pedidos-compra/:id), etapa 7.
 *   1.10.0 - 23/09/2026 - "Painel" vira "Dashboard"; "Gestão Comercial" dividida nas
 *                         seções Cadastro, Ordem Vendas/Compras, Depósito e Financeiro.
 *                         "Pedidos" vira "Pedidos de Venda" (menu, breadcrumb e títulos).
 *   1.11.0 - 23/09/2026 - Rota e item de menu de Contas a Pagar (Financeiro), etapa 8.
 *   1.12.0 - 23/09/2026 - Seção Relatórios: Vendas e Compras (etapa 9).
 *   1.13.0 - 23/09/2026 - Relatório de Estoque (etapa 9).
 *   1.14.0 - 23/09/2026 - Seções do menu abrem/fecham clicando no título; o estado
 *                         fica salvo no navegador (localStorage).
 *   1.15.0 - 23/09/2026 - Rota e item de menu de Vendedores (Cadastro), etapa 10.
 *   1.16.0 - 23/09/2026 - Rota e item de menu de Comissões (Financeiro), etapa 11.
 * =====================================================================
 */

import {
  AppstoreOutlined,
  BarChartOutlined,
  ContainerOutlined,
  DatabaseOutlined,
  DollarOutlined,
  DownOutlined,
  HomeOutlined,
  PercentageOutlined,
  LineChartOutlined,
  MenuFoldOutlined,
  MenuOutlined,
  MenuUnfoldOutlined,
  ShopOutlined,
  SolutionOutlined,
  ShoppingCartOutlined,
  ShoppingOutlined,
  TagsOutlined,
  TeamOutlined,
  WalletOutlined,
} from '@ant-design/icons'
import { Breadcrumb, Button, Drawer, Grid, Layout, Menu, type MenuProps } from 'antd'
import { useState } from 'react'
import { Link, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import './App.css'
import { LogoAmbition } from './components/LogoAmbition'
import { CategoriasListaPage } from './pages/Categorias/CategoriasListaPage'
import { ClientesListaPage } from './pages/Clientes/ClientesListaPage'
import { ComissoesListaPage } from './pages/Comissoes/ComissoesListaPage'
import { ContasPagarListaPage } from './pages/ContasPagar/ContasPagarListaPage'
import { ContasReceberListaPage } from './pages/ContasReceber/ContasReceberListaPage'
import { DashboardPage } from './pages/Dashboard/DashboardPage'
import { EstoqueListaPage } from './pages/Estoque/EstoqueListaPage'
import { FornecedoresListaPage } from './pages/Fornecedores/FornecedoresListaPage'
import { PedidoPage } from './pages/Pedidos/PedidoPage'
import { PedidosListaPage } from './pages/Pedidos/PedidosListaPage'
import { PedidoCompraPage } from './pages/PedidosCompra/PedidoCompraPage'
import { PedidosCompraListaPage } from './pages/PedidosCompra/PedidosCompraListaPage'
import { ProdutosListaPage } from './pages/Produtos/ProdutosListaPage'
import { RelatorioEstoquePage } from './pages/Relatorios/RelatorioEstoquePage'
import { RelatorioPedidosPage } from './pages/Relatorios/RelatorioPedidosPage'
import { VendedoresListaPage } from './pages/Vendedores/VendedoresListaPage'

// O Dashboard fica fora das seções: resume vários módulos, não pertence a um departamento.
const itensPainel = [{ key: '/', icon: <HomeOutlined />, label: 'Dashboard' }] satisfies MenuProps['items']

// Telas agrupadas por departamento. A chave de cada item é o caminho da rota.
const secoes = [
  {
    titulo: 'Cadastro',
    itens: [
      { key: '/clientes', icon: <TeamOutlined />, label: 'Clientes' },
      { key: '/fornecedores', icon: <ShopOutlined />, label: 'Fornecedores' },
      { key: '/vendedores', icon: <SolutionOutlined />, label: 'Vendedores' },
      { key: '/produtos', icon: <TagsOutlined />, label: 'Produtos' },
      { key: '/categorias', icon: <AppstoreOutlined />, label: 'Categorias' },
    ],
  },
  {
    titulo: 'Ordem Vendas/Compras',
    itens: [
      { key: '/pedidos', icon: <ShoppingCartOutlined />, label: 'Pedidos de Venda' },
      { key: '/pedidos-compra', icon: <ShoppingOutlined />, label: 'Pedidos de Compra' },
    ],
  },
  { titulo: 'Depósito', itens: [{ key: '/estoque', icon: <DatabaseOutlined />, label: 'Estoque' }] },
  {
    titulo: 'Financeiro',
    itens: [
      { key: '/contas-receber', icon: <DollarOutlined />, label: 'Contas a Receber' },
      { key: '/contas-pagar', icon: <WalletOutlined />, label: 'Contas a Pagar' },
      { key: '/comissoes', icon: <PercentageOutlined />, label: 'Comissões' },
    ],
  },
  {
    titulo: 'Relatórios',
    itens: [
      { key: '/relatorios/vendas', icon: <LineChartOutlined />, label: 'Vendas' },
      { key: '/relatorios/compras', icon: <BarChartOutlined />, label: 'Compras' },
      { key: '/relatorios/estoque', icon: <ContainerOutlined />, label: 'Estoque' },
    ],
  },
] satisfies { titulo: string; itens: MenuProps['items'] }[]

const CHAVE_SECOES_FECHADAS = 'ambition.menu.secoesFechadas'

/** Seções que o usuário deixou fechadas; storage indisponível (modo privado etc.) = todas abertas. */
function lerSecoesFechadas(): string[] {
  try {
    const salvo: unknown = JSON.parse(localStorage.getItem(CHAVE_SECOES_FECHADAS) ?? '[]')
    return Array.isArray(salvo) ? salvo.filter((item): item is string => typeof item === 'string') : []
  } catch {
    return []
  }
}

/** Título de uma seção do menu: clicar abre/fecha a lista de telas dela. */
function TituloSecao({ titulo, aberta, aoAlternar }: { titulo: string; aberta: boolean; aoAlternar: () => void }) {
  return (
    <button type="button" className="app-secao-menu" aria-expanded={aberta} onClick={aoAlternar}>
      <span>{titulo}</span>
      <DownOutlined className={aberta ? 'app-secao-seta' : 'app-secao-seta app-secao-seta-fechada'} />
    </button>
  )
}

/** true se a rota atual pertence a este item de menu (a raiz "/" só bate exata, nunca por prefixo). */
function ehRotaDoItem(chave: string, pathname: string): boolean {
  return chave === '/' ? pathname === '/' : pathname === chave || pathname.startsWith(`${chave}/`)
}

/** Título da subpágina de um módulo (a última parte do breadcrumb), ou undefined na página principal. */
function tituloDaSubpagina(pathname: string): string | undefined {
  if (pathname === '/pedidos/novo') return 'Novo pedido de venda'
  const numero = /^\/pedidos\/(\d+)$/.exec(pathname)?.[1]
  if (numero) return `Pedido de venda nº ${numero}`

  if (pathname === '/pedidos-compra/novo') return 'Novo pedido de compra'
  const numeroCompra = /^\/pedidos-compra\/(\d+)$/.exec(pathname)?.[1]
  return numeroCompra ? `Pedido de compra nº ${numeroCompra}` : undefined
}

export default function App() {
  const telas = Grid.useBreakpoint()
  const ehCelular = telas.md === false

  const { pathname } = useLocation()
  const navegar = useNavigate()
  // O item do menu vale também nas subpáginas (/pedidos/novo, /pedidos/12 continuam marcando "Pedidos").
  const secaoAtual = secoes.find((secao) => secao.itens.some((item) => ehRotaDoItem(item.key, pathname)))
  const itemAtual = [...itensPainel, ...secoes.flatMap((secao) => secao.itens)].find((item) =>
    ehRotaDoItem(String(item.key), pathname),
  )
  const chavesSelecionadas = itemAtual ? [String(itemAtual.key)] : []
  const subpagina = tituloDaSubpagina(pathname)
  const trilha =
    itemAtual?.key === '/'
      ? [{ title: 'Dashboard' }]
      : [
          { title: secaoAtual?.titulo },
          { title: subpagina && itemAtual ? <Link to={String(itemAtual.key)}>{itemAtual.label}</Link> : itemAtual?.label },
          ...(subpagina ? [{ title: subpagina }] : []),
        ]

  const [recolhido, setRecolhido] = useState(false)
  const [menuCelularAberto, setMenuCelularAberto] = useState(false)
  const [secoesFechadas, setSecoesFechadas] = useState(lerSecoesFechadas)

  function alternarSecao(titulo: string) {
    const novas = secoesFechadas.includes(titulo)
      ? secoesFechadas.filter((t) => t !== titulo)
      : [...secoesFechadas, titulo]
    setSecoesFechadas(novas)
    try {
      localStorage.setItem(CHAVE_SECOES_FECHADAS, JSON.stringify(novas))
    } catch {
      // Sem storage: vale só até recarregar a página.
    }
  }

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
          <Menu mode="inline" selectedKeys={chavesSelecionadas} items={itensPainel} onClick={({ key }) => navegar(key)} />
          {secoes.map((secao) => (
            <div key={secao.titulo}>
              {!recolhido && (
                <TituloSecao
                  titulo={secao.titulo}
                  aberta={!secoesFechadas.includes(secao.titulo)}
                  aoAlternar={() => alternarSecao(secao.titulo)}
                />
              )}
              {/* Recolhido não mostra títulos, então os ícones ficam sempre visíveis. */}
              {(recolhido || !secoesFechadas.includes(secao.titulo)) && (
                <Menu mode="inline" selectedKeys={chavesSelecionadas} items={secao.itens} onClick={({ key }) => navegar(key)} />
              )}
            </div>
          ))}
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
            <Route path="/" element={<DashboardPage />} />
            <Route path="/clientes" element={<ClientesListaPage />} />
            <Route path="/fornecedores" element={<FornecedoresListaPage />} />
            <Route path="/vendedores" element={<VendedoresListaPage />} />
            <Route path="/produtos" element={<ProdutosListaPage />} />
            <Route path="/categorias" element={<CategoriasListaPage />} />
            <Route path="/pedidos" element={<PedidosListaPage />} />
            <Route path="/pedidos/novo" element={<PedidoPage />} />
            <Route path="/pedidos/:id" element={<PedidoPage />} />
            <Route path="/pedidos-compra" element={<PedidosCompraListaPage />} />
            <Route path="/pedidos-compra/novo" element={<PedidoCompraPage />} />
            <Route path="/pedidos-compra/:id" element={<PedidoCompraPage />} />
            <Route path="/estoque" element={<EstoqueListaPage />} />
            <Route path="/contas-receber" element={<ContasReceberListaPage />} />
            <Route path="/contas-pagar" element={<ContasPagarListaPage />} />
            <Route path="/comissoes" element={<ComissoesListaPage />} />
            {/* key diferente: trocar entre vendas e compras recria a tela (não leva o cliente escolhido como fornecedor). */}
            <Route path="/relatorios/vendas" element={<RelatorioPedidosPage key="vendas" tipo="vendas" />} />
            <Route path="/relatorios/compras" element={<RelatorioPedidosPage key="compras" tipo="compras" />} />
            <Route path="/relatorios/estoque" element={<RelatorioEstoquePage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
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
        <Menu
          mode="inline"
          selectedKeys={chavesSelecionadas}
          items={itensPainel}
          onClick={({ key }) => {
            navegar(key)
            setMenuCelularAberto(false)
          }}
        />
        {secoes.map((secao) => (
          <div key={secao.titulo}>
            <TituloSecao
              titulo={secao.titulo}
              aberta={!secoesFechadas.includes(secao.titulo)}
              aoAlternar={() => alternarSecao(secao.titulo)}
            />
            {!secoesFechadas.includes(secao.titulo) && (
              <Menu
                mode="inline"
                selectedKeys={chavesSelecionadas}
                items={secao.itens}
                onClick={({ key }) => {
                  navegar(key)
                  setMenuCelularAberto(false)
                }}
              />
            )}
          </div>
        ))}
      </Drawer>
    </Layout>
  )
}
