---
name: Vértice Gestão
colors:
  surface: '#111416'
  surface-dim: '#111416'
  surface-bright: '#373a3c'
  surface-container-lowest: '#0c0f10'
  surface-container-low: '#191c1e'
  surface-container: '#1d2022'
  surface-container-high: '#272a2c'
  surface-container-highest: '#323537'
  on-surface: '#e1e2e5'
  on-surface-variant: '#bccbb9'
  inverse-surface: '#e1e2e5'
  inverse-on-surface: '#2e3133'
  outline: '#869585'
  outline-variant: '#3d4a3d'
  surface-tint: '#4ae176'
  primary: '#4be277'
  on-primary: '#003915'
  primary-container: '#22c55e'
  on-primary-container: '#004b1e'
  inverse-primary: '#006e2f'
  secondary: '#62df7d'
  on-secondary: '#003914'
  secondary-container: '#1ca64d'
  on-secondary-container: '#003111'
  tertiary: '#7cd0ff'
  on-tertiary: '#00354a'
  tertiary-container: '#2eb7f2'
  on-tertiary-container: '#00455f'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#6bff8f'
  primary-fixed-dim: '#4ae176'
  on-primary-fixed: '#002109'
  on-primary-fixed-variant: '#005321'
  secondary-fixed: '#7ffc97'
  secondary-fixed-dim: '#62df7d'
  on-secondary-fixed: '#002109'
  on-secondary-fixed-variant: '#005320'
  tertiary-fixed: '#c4e7ff'
  tertiary-fixed-dim: '#7bd0ff'
  on-tertiary-fixed: '#001e2c'
  on-tertiary-fixed-variant: '#004c69'
  background: '#111416'
  on-background: '#e1e2e5'
  surface-variant: '#323537'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 30px
    fontWeight: '700'
    lineHeight: 38px
    letterSpacing: -0.02em
  headline-xl:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.015em
  headline-lg:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '600'
    lineHeight: 24px
    letterSpacing: -0.005em
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.01em
  data-mono:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '500'
    lineHeight: 18px
    letterSpacing: '0'
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1rem
  margin: 1.5rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2rem
---

## Brand & Style

Este sistema de design foi concebido para plataformas ERP focadas em pequenas e médias empresas que necessitam de agilidade operacional, precisão financeira e controle absoluto sobre suas rotinas comerciais. A estética equilibra o rigor estruturado do padrão enterprise com a fluidez e a leveza de ferramentas SaaS contemporâneas.

O design adota uma estética **Corporate / Modern** no modo escuro profundo, eliminando distrações visuais e reduzindo a fadiga ocular em jornadas longas de trabalho. A experiência inspira confiança, eficiência, autoridade e clareza analítica, transmitindo a sensação de um centro de comando financeiro em tempo real.

## Colors

A paleta cromática foi desenvolvida para oferecer máximo contraste e legibilidade em interfaces analíticas densas, seguindo a semântica operacional e financeira:

- **Fundo Principal (Canvas):** `#0F1112` — Base sólida em preto grafite neutro que ancora toda a interface sem reflexos incômodos.
- **Superfícies & Contêineres:** `#1A1D1F` — Tom cinza escuro refinado utilizado para cards, tabelas, modais e barra lateral, criando separação hierárquica clara contra o canvas.
- **Superfícies Elevadas / Hover:** `#222629` — Estado de foco ou elevação intermediária de cartões e linhas selecionadas.
- **Bordas & Delimitadores:** `#2A2E31` — Linhas funcionais para tabelas, inputs e painéis modulares sem sobrecarregar a visão periférica.
- **Tipografia Principal:** `#E6E8EA` — Branco gelo de alto contraste, otimizado para números, títulos e dados críticos de vendas.
- **Tipografia Secundária (Muted):** `#9AA0A6` — Cinza balanceado para rótulos secundários, placeholders, metadados e legendas.
- **Acento Primário (Brand/Accent):** `#22C55E` — Verde vibrante simbolizando liquidez, crescimento financeiro e confirmações assertivas.
- **Acento Primário (Hover/Pressed):** `#16A34A` — Verde equilibrado para feedback tátil em estados ativos e interativos.
- **Semântica de Status:**
  - **Sucesso / Ativo / Faturamento:** `#22C55E`
  - **Inativo / Neutro / Rascunho:** `#9AA0A6`
  - **Alerta / Pendente / Estoque Baixo:** `#F59E0B`
  - **Erro / Perigo / Cancelamento / Dívida:** `#EF4444`
  - **Informativo / Processamento:** `#38BDF8`

## Typography

A tipografia utiliza a fonte **Inter** em toda a extensão do sistema, priorizando alta densidade de dados e legibilidade em pequenas escalas.

- Para valores numéricos e tabelas financeiras (R$, SKUs, CPF/CNPJ), recomenda-se a aplicação da propriedade OpenType `font-feature-settings: 'tnum' 1, 'cv05' 1`, mantendo algarismos alinhados verticalmente coluna a coluna.
- Títulos e subtítulos utilizam espaçamento de caracteres levemente condensado (`-0.01em` a `-0.02em`) para evitar quebras prematuras de linha em cabeçalhos de relatórios e dashboards.
- O texto corrido mantém o peso `400` com entrelinha estritamente proporcional para relatórios de emissão fiscal e descrições de produtos.

## Layout & Spacing

A estruturação adota um grid fluido de 12 colunas com base em múltiplos de 4px e 8px, inspirado em painéis modulares do ecossistema Ant Design:

- **Estrutura Base:** Sidebar fixa à esquerda (com largura padrão de 256px recolhível para 72px em modo compacto), topo de navegação global com altura fixa de 64px e área de trabalho com rolagem independente.
- **Breakpoints Responsivos:**
  - **Desktop Amplo (>= 1440px):** Layout em 12 colunas, margem lateral de `2rem`, gutters de `1.5rem`. Visualização simultânea de listagem analítica e drawer de detalhes.
  - **Desktop / Laptop (1024px – 1439px):** Layout em 12 colunas, margens de `1.5rem`, gutters de `1rem`. Painéis e formulários em 2 a 3 colunas de inputs.
  - **Tablet (768px – 1023px):** Barra lateral recolhida automaticamente para ícones, grid reorganizado para 6 colunas, formulários em coluna única ou dupla.
  - **Mobile (< 768px):** Sidebar convertida em menu drawer suspenso, margem de tela reduzida para `1rem`, tabelas com rolagem horizontal livre ou convertidas para cartões empilhados.

## Elevation & Depth

No ambiente escuro corporativo, a hierarquia é produzida principalmente através de camadas tonais (`Tonal Layers`) em conjunto com sombras pretas puras e sutis bordas funcionais:

- **Nível 0 (Plano de Fundo):** `#0F1112` — Sem sombras.
- **Nível 1 (Superfícies e Cartões):** Fundo `#1A1D1F`, delimitado por borda perimetral de 1px sólida `#2A2E31`. Sombra de repouso suave: `0 1px 3px rgba(0, 0, 0, 0.45)`.
- **Nível 2 (Dropdowns, Menus de Contexto e Popovers):** Fundo `#1F2326`, borda 1px `#2A2E31`, sombra projetada: `0 4px 12px rgba(0, 0, 0, 0.65)`.
- **Nível 3 (Modais e Drawers de Edição):** Fundo `#1A1D1F`, borda perimetral de 1px `#2A2E31`, sombra acentuada: `0 12px 28px rgba(0, 0, 0, 0.85)` sobre uma camada de backdrop em preto com 60% de opacidade (`rgba(0, 0, 0, 0.6)`).

## Shapes

O sistema utiliza predominantemente o raio de **8px (rounded-lg)**, estabelecendo uma apresentação técnica, equilibrada e uniforme típica de sistemas de gestão empresariais modernos:

- **Botoes, Campos de Entrada e Dropdowns:** Raio uniforme de `8px`.
- **Containers, Modais, Tabelas e Painéis de KPIs:** Raio de contorno externo de `8px` a `12px`, com as células internas mantendo cantos retos para precisão geométrica.
- **Tags, Badges e Indicadores de Status:** Raio reduzido de `4px` para rótulos compactos ou estilo pílula total (`9999px`) exclusivamente para contadores e chips numéricos.

## Components

### Botões
- **Primário:** Fundo `#22C55E`, texto `#0F1112` (ou preto profundo para contraste imediato de segurança), peso `600`, raio `8px`. No estado *hover*, fundo `#16A34A`.
- **Secundário / Padrão:** Fundo transparente ou `#1A1D1F`, borda de 1px `#2A2E31`, texto `#E6E8EA`. No *hover*, borda `#22C55E` e texto `#22C55E`.
- **Perigo / Destrutivo:** Fundo transparente, borda de 1px `#EF4444`, texto `#EF4444`. No *hover*, fundo `#EF4444` e texto `#FFFFFF`.

### Campos de Entrada (Inputs & Selects)
- **Base:** Fundo `#0F1112` ou `#141618`, borda de 1px sólida `#2A2E31`, altura de 36px (padrão) a 40px (confortável), raio `8px`, texto `#E6E8EA`. Placeholder com cor `#9AA0A6`.
- **Foco:** Borda `#22C55E` com anel perimetral tênue (`box-shadow: 0 0 0 2px rgba(34, 197, 94, 0.2)`).
- **Erro:** Borda `#EF4444` com mensagem explicativa em corpo `12px` na mesma cor.

### Tabelas de Dados (Data Tables)
- **Cabeçalho:** Fundo `#151719`, borda inferior `#2A2E31`, rótulos em `label-sm` com texto `#9AA0A6` em maiúsculas suaves.
- **Linhas e Células:** Altura compacta (44px a 48px), separadores horizontais em 1px `#2A2E31`. Hover da linha com fundo `#222629`.
- **Alinhamento:** Textos à esquerda, datas e códigos centralizados, valores monetários à direita com alinhamento tabular ativado.

### Tags & Chips de Status
- **Estrutura:** Altura de 24px, padding horizontal de 8px, borda de 1px com 20% de opacidade da respectiva cor de status.
- **Sucesso (Faturado / Concluído):** Fundo `rgba(34, 197, 94, 0.12)`, texto `#22C55E`, borda `rgba(34, 197, 94, 0.3)`.
- **Alerta (Aguardando Pagamento):** Fundo `rgba(245, 158, 11, 0.12)`, texto `#F59E0B`, borda `rgba(245, 158, 11, 0.3)`.
- **Perigo (Vencido / Cancelado):** Fundo `rgba(239, 68, 68, 0.12)`, texto `#EF4444`, borda `rgba(239, 68, 68, 0.3)`.
- **Neutro (Rascunho):** Fundo `rgba(154, 160, 166, 0.1)`, texto `#9AA0A6`, borda `rgba(154, 160, 166, 0.25)`.

### Caixas de Seleção & Rádios (Checkboxes / Radios)
- Caixa de 16x16px, borda de 1px `#2A2E31`, raio de 4px (checkbox) ou circular (radio). Quando selecionado, preenchimento total em `#22C55E` e ícone interno em `#0F1112`.

### Cards & Painéis de Indicadores (KPIs)
- Fundo `#1A1D1F`, borda de 1px `#2A2E31`, raio de 8px, padding interno de 16px a 20px. 
- Contém o rótulo da métrica em `#9AA0A6`, valor numérico de destaque em `#E6E8EA` e indicador contextual de variação percentual com seta positiva (`#22C55E`) ou negativa (`#EF4444`).