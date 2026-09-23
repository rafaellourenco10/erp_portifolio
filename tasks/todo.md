# Tarefas: Módulo Dashboard

Contexto: [SPEC.md](../SPEC.md) · [plan.md](plan.md). Marque `[x]` ao concluir e verificar. Regras D1 a D7 e "critérios de sucesso" 1 a 8 estão na spec.

Comandos (raiz): `dotnet build ErpPortfolio.slnx -c Release` · `dotnet test backend/ErpPortfolio.Tests -c Release` · (front, em `frontend/erp-portfolio-web`) `npx tsc -b` · `npx oxlint src`.
"E2E temporário" = instância da API em porta separada (5099) verificada manualmente (`curl`), com dados de teste apagados no fim.

---

## Fase 1: Resumos por módulo (reaproveitando os services existentes)

- [ ] **T1: Resumo de vendas (`PedidoService`)** (M)
  - Descrição: `ObterResumoVendasAsync()` — faturamento do mês, ticket médio, pedidos por status, faturamento diário (D1-D4).
  - Aceite:
    - Faturamento = soma de `valorTotal` só de pedidos `Confirmado` com `dataPedido` no mês/ano atual (UTC).
    - Ticket médio = faturamento ÷ confirmados no mês; `0` sem erro quando não há nenhum.
    - Faturamento diário tem um ponto por **cada dia** do mês, dias sem venda com `0`.
  - Verificar: `dotnet test` (ticket médio com 0 pedidos, agrupamento por dia sem buraco); `dotnet build` 0 avisos.
  - Dependências: nenhuma
  - Arquivos: `DTOs/VendasResumoDto.cs`, `DTOs/FaturamentoDiaDto.cs`, `Services/IPedidoService.cs`, `Services/PedidoService.cs`, `backend/ErpPortfolio.Tests/DashboardResumoTests.cs`

- [ ] **T2: Resumo de contas a receber (`ContasReceberService`)** (S)
  - Descrição: `ObterResumoAsync()` — total e quantidade pendente/atrasado (D5), sem filtro de mês.
  - Aceite: soma e contagem batem com as parcelas `Pendente`; "atrasado" é o subconjunto com `vencimento` no passado, mesmo cálculo já usado na listagem de Contas a Receber.
  - Verificar: `dotnet build` 0 avisos; conferido junto com o E2E da T4.
  - Dependências: nenhuma
  - Arquivos: `DTOs/ContasReceberResumoDto.cs`, `Services/IContasReceberService.cs`, `Services/ContasReceberService.cs`

- [ ] **T3: Resumo de estoque (`EstoqueService`)** (S)
  - Descrição: `ObterResumoAsync()` — quantidade de produtos ativos com saldo ≤ `LimiteSaldoBaixo` (5) (D6).
  - Aceite: produto com saldo exatamente 5 conta; saldo 6 não conta; só produtos ativos entram.
  - Verificar: `dotnet test` (limite exato); `dotnet build` 0 avisos.
  - Dependências: nenhuma
  - Arquivos: `DTOs/EstoqueResumoDashboardDto.cs`, `Services/IEstoqueService.cs`, `Services/EstoqueService.cs`, `backend/ErpPortfolio.Tests/DashboardResumoTests.cs`

### Checkpoint 1: resumos prontos
- [ ] `dotnet test` verde e `dotnet build` sem avisos
- [ ] Revisão do Rafael antes de seguir

---

## Fase 2: API

- [ ] **T4: `DashboardController`** (S)
  - Descrição: três endpoints finos (`/dashboard/vendas`, `/dashboard/contas-receber`, `/dashboard/estoque`), cada um só delegando pro service correspondente (D7).
  - Aceite: os três endpoints respondem 200 com os números certos, testados contra dados de teste isolados (cliente/produto/pedidos `ZZT…`).
  - Verificar: E2E temporário (pedidos confirmados e rascunho no mês, parcela atrasada, produto com saldo baixo — os três endpoints conferidos com esses dados); `dotnet build` 0 avisos.
  - Dependências: T1, T2, T3
  - Arquivos: `Controllers/DashboardController.cs`

### Checkpoint 2 (CP1 do plano): API pronta
- [ ] Critérios 1 a 8 da spec verificados por E2E na API real
- [ ] `dotnet test` verde, `dotnet build` sem avisos
- [ ] Dados reais intactos; dados de teste apagados
- [ ] Revisão do Rafael antes de começar a tela

---

## Fase 3: Tela

- [ ] **T5: Base do front + cards de número** (M)
  - Descrição: espelho do contrato da API (`types/dashboard.ts`, `api/dashboardApi.ts`, `hooks/useDashboard.ts`), página `DashboardPage.tsx` com os cards (Faturamento, Ticket médio, Pedidos por status, Contas a receber, Saldo baixo).
  - Aceite: os três hooks buscam em paralelo; cada card mostra seu próprio loading/erro (D7) — um endpoint falhando não derruba os outros cards.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Dependências: T4 (contrato)
  - Arquivos: `types/dashboard.ts`, `api/dashboardApi.ts`, `hooks/useDashboard.ts`, `pages/Dashboard/DashboardPage.tsx`, `pages/Dashboard/CardIndicador.tsx`

- [ ] **T6: Gráfico de faturamento diário** (M)
  - Descrição: carregar a skill de dataviz do projeto antes de escrever o gráfico; escolher a biblioteca (única dependência nova autorizada pela spec); gráfico de linha/barra com o faturamento diário do mês.
  - Aceite: gráfico renderiza os dias do mês sem buraco (D4); segue o tema escuro Ambition (cores de `temaAmbition.ts`).
  - Verificar: `npx tsc -b`, `npx oxlint src`, `npm run build` (confirma que a dependência nova não quebra o build).
  - Dependências: T5
  - Arquivos: `pages/Dashboard/GraficoFaturamento.tsx`, `package.json` (dependência nova)

- [ ] **T7: Menu "Painel" + rota inicial** (S)
  - Descrição: item **Painel** no menu, fora da seção "Gestão Comercial"; rota `/` aponta pro Dashboard; rota desconhecida (`*`) passa a cair em `/` em vez de `/clientes`.
  - Aceite: abrir a raiz do site mostra o Dashboard; as rotas dos outros módulos continuam iguais.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Dependências: T6
  - Arquivos: `App.tsx`

### Checkpoint 3 (CP2 do plano): tela pronta
- [ ] Cards e gráfico funcionam com dados reais (conferência manual, sem Playwright nesta sessão)
- [ ] `tsc`, `oxlint`, `npm run build`, `dotnet build` (0 avisos), `dotnet test` limpos
- [ ] Revisão do Rafael antes de fechar

---

## Fase 4: Fechamento

- [ ] **T8: README, graphify e verificação final** (S)
  - Descrição: documentar o módulo, regravar o grafo e conferir os 8 critérios da spec.
  - Aceite:
    - README com a seção "etapa 6 — Dashboard"; `SPEC.md` marcada como implementada; roadmap atualizado (Login passa a etapa 7).
    - Graphify atualizado (AST-only).
    - Os **8 critérios de sucesso** conferidos um a um; registros reais intactos.
  - Verificar: `dotnet build`, `dotnet test`, `tsc`, `oxlint`, `npm run build`, E2E completo, `git status` revisado (sem segredos).
  - Dependências: T7
  - Arquivos: `README.md`, `SPEC.md`

### Checkpoint 4: pronto
- [ ] Todos os critérios da spec atendidos
- [ ] Commitado
