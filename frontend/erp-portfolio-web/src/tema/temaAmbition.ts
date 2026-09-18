/**
 * =====================================================================
 * Arquivo....: temaAmbition.ts
 * Versão.....: 1.1.0
 * Data.......: 18/09/2026
 * Descrição..: Tema visual "Ambition ERP" (modo escuro). Fonte única das
 *              cores: alimenta os tokens do Ant Design e as variáveis CSS
 *              (--cor-*) usadas nos arquivos .css do projeto.
 * ---------------------------------------------------------------------
 * Fontes.....: docs/tema/stitch_erp_comercial_web_dark/DESIGN.md
 *              (tons de fundo clareados a pedido; verde e status mantidos)
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Fundo mais claro e mais contraste entre fundo,
 *                        cards e bordas.
 * =====================================================================
 */

import { theme, type ThemeConfig } from 'antd'

export const cores = {
  fundo: '#1B1E21',
  superficie: '#272B30',
  'superficie-elevada': '#30353A',
  popover: '#2E3237',
  campo: '#1F2225',
  'cabecalho-tabela': '#22262A',
  borda: '#3B4046',
  texto: '#E6E8EA',
  'texto-secundario': '#A3A9AF',
  primaria: '#22C55E',
  'primaria-hover': '#16A34A',
  'primaria-ativa': '#15803D',
  alerta: '#F59E0B',
  erro: '#EF4444',
  info: '#38BDF8',
} as const

const fonte = "'Inter Variable', Inter, system-ui, -apple-system, 'Segoe UI', sans-serif"
const anelFoco = '0 0 0 2px rgba(34, 197, 94, 0.2)'

/** Publica as cores como variáveis CSS (--cor-fundo, --cor-primaria, ...) no :root. */
export function aplicarVariaveisCss() {
  for (const [nome, valor] of Object.entries(cores)) {
    document.documentElement.style.setProperty(`--cor-${nome}`, valor)
  }
}

export const temaAmbition: ThemeConfig = {
  algorithm: theme.darkAlgorithm,
  token: {
    colorPrimary: cores.primaria,
    colorPrimaryHover: cores['primaria-hover'],
    colorPrimaryActive: cores['primaria-ativa'],
    colorSuccess: cores.primaria,
    colorWarning: cores.alerta,
    colorError: cores.erro,
    colorInfo: cores.info,
    colorLink: cores.primaria,
    colorBgBase: cores.fundo,
    colorBgLayout: cores.fundo,
    colorBgContainer: cores.superficie,
    colorBgElevated: cores.popover,
    colorBgMask: 'rgba(0, 0, 0, 0.6)',
    colorBorder: cores.borda,
    colorBorderSecondary: cores.borda,
    colorSplit: cores.borda,
    colorText: cores.texto,
    colorTextHeading: cores.texto,
    colorTextSecondary: cores['texto-secundario'],
    colorTextTertiary: cores['texto-secundario'],
    colorTextPlaceholder: cores['texto-secundario'],
    fontFamily: fonte,
    fontSize: 14,
    borderRadius: 8,
    borderRadiusSM: 4,
    borderRadiusLG: 12,
    controlHeight: 36,
    controlHeightLG: 40,
    boxShadowSecondary: '0 4px 12px rgba(0, 0, 0, 0.65)',
  },
  components: {
    Layout: {
      bodyBg: cores.fundo,
      headerBg: cores.superficie,
      siderBg: cores.superficie,
      headerHeight: 64,
      headerPadding: '0 24px',
    },
    Menu: {
      itemBg: 'transparent',
      itemColor: cores.texto,
      itemHoverBg: cores['superficie-elevada'],
      itemHoverColor: cores.texto,
      itemActiveBg: cores['superficie-elevada'],
      itemSelectedBg: cores.primaria,
      itemSelectedColor: cores.fundo,
      itemBorderRadius: 8,
      itemHeight: 44,
      itemMarginInline: 12,
      activeBarBorderWidth: 0,
    },
    Breadcrumb: {
      itemColor: cores['texto-secundario'],
      lastItemColor: cores.primaria,
      separatorColor: cores['texto-secundario'],
    },
    Button: {
      fontWeight: 600,
      primaryColor: cores.fundo,
      primaryShadow: 'none',
      defaultShadow: 'none',
      dangerShadow: 'none',
      defaultBg: cores.superficie,
      defaultColor: cores.texto,
      defaultBorderColor: cores.borda,
      defaultHoverBorderColor: cores.primaria,
      defaultHoverColor: cores.primaria,
    },
    Input: {
      colorBgContainer: cores.campo,
      hoverBg: cores.campo,
      activeBg: cores.campo,
      activeShadow: anelFoco,
      errorActiveShadow: '0 0 0 2px rgba(239, 68, 68, 0.2)',
    },
    Select: {
      colorBgContainer: cores.campo,
      selectorBg: cores.campo,
      activeOutlineColor: 'rgba(34, 197, 94, 0.2)',
      optionSelectedBg: 'rgba(34, 197, 94, 0.12)',
      optionSelectedColor: cores.texto,
      optionActiveBg: cores['superficie-elevada'],
      multipleItemBg: cores['superficie-elevada'],
      multipleItemBorderColor: cores.borda,
    },
    Segmented: {
      trackBg: cores.campo,
      itemColor: cores['texto-secundario'],
      itemHoverColor: cores.texto,
      itemHoverBg: 'transparent',
      itemSelectedBg: cores['superficie-elevada'],
      itemSelectedColor: cores.primaria,
    },
    Table: {
      colorBgContainer: cores.superficie,
      headerBg: cores['cabecalho-tabela'],
      headerColor: cores['texto-secundario'],
      headerSplitColor: 'transparent',
      headerBorderRadius: 0,
      rowHoverBg: cores['superficie-elevada'],
      borderColor: cores.borda,
      cellPaddingBlock: 12,
      cellPaddingInline: 12,
    },
    Pagination: {
      itemActiveBg: cores.primaria,
      itemActiveColor: cores.fundo,
    },
    Drawer: {
      colorBgElevated: cores.superficie,
    },
    Form: {
      labelColor: cores.texto,
      itemMarginBottom: 20,
    },
  },
}
