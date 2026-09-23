# Tarefas: Módulo Dashboard

Contexto: [SPEC.md](../SPEC.md) · [plan.md](plan.md). Marque `[x]` ao concluir e verificar. Regras D1 a D7 e "critérios de sucesso" 1 a 8 estão na spec.

Comandos (raiz): `dotnet build ErpPortfolio.slnx -c Release` · `dotnet test backend/ErpPortfolio.Tests -c Release` · (front, em `frontend/erp-portfolio-web`) `npx tsc -b` · `npx oxlint src`.
"E2E temporário" = instância da API em porta separada (5099) verificada manualmente (`curl`), com dados de teste apagados no fim.

---

## Fase 1: Resumos por módulo (reaproveitando os services existentes)

- [x] **T1: Resumo de vendas (`PedidoService`)** (M) — *concluída em 22/09/2026*
  - Descrição: `ObterResumoVendasAsync()` — faturamento do mês, ticket médio, pedidos por status, faturamento diário (D1-D4).
  - Aceite:
    - Faturamento = soma de `valorTotal` só de pedidos `Confirmado` com `dataPedido` no mês/ano atual (UTC).
    - Ticket médio = faturamento ÷ confirmados no mês; `0` sem erro quando não há nenhum.
    - Faturamento diário tem um ponto por **cada dia** do mês, dias sem venda com `0`.
  - Verificar: `dotnet test` (ticket médio com 0 pedidos, agrupamento por dia sem buraco); `dotnet build` 0 avisos.
  - Resultado: 150 testes passando (146 anteriores + 4 novos), 0 avisos. `DashboardCalculo` pura (ticket médio, preencher dias) reaproveitável pelos outros resumos se precisar. Pedidos do mês trazidos para memória e agrupados em C# (escala de portfólio, sem necessidade de SQL agregado complexo).
  - Dependências: nenhuma
  - Arquivos: `DTOs/VendasResumoDto.cs`, `DTOs/FaturamentoDiaDto.cs`, `DTOs/PedidosPorStatusDto.cs`, `Services/DashboardCalculo.cs`, `Services/IPedidoService.cs`, `Services/PedidoService.cs`, `backend/ErpPortfolio.Tests/DashboardResumoTests.cs`

- [x] **T2: Resumo de contas a receber (`ContasReceberService`)** (S) — *concluída em 22/09/2026*
  - Descrição: `ObterResumoAsync()` — total e quantidade pendente/atrasado (D5), sem filtro de mês.
  - Aceite: soma e contagem batem com as parcelas `Pendente`; "atrasado" é o subconjunto com `vencimento` no passado, mesmo cálculo já usado na listagem de Contas a Receber.
  - Verificar: `dotnet build` 0 avisos; conferido junto com o E2E da T4.
  - Resultado: `dotnet build` 0 avisos. Sem lógica pura nova (reaproveita o mesmo cálculo de "atrasado" já usado em `ListarAsync`); verificação fica pro E2E da T4.
  - Dependências: nenhuma
  - Arquivos: `DTOs/ContasReceberResumoDto.cs`, `Services/IContasReceberService.cs`, `Services/ContasReceberService.cs`

- [x] **T3: Resumo de estoque (`EstoqueService`)** (S) — *concluída em 22/09/2026*
  - Descrição: `ObterResumoAsync()` — quantidade de produtos ativos com saldo ≤ `LimiteSaldoBaixo` (5) (D6).
  - Aceite: produto com saldo exatamente 5 conta; saldo 6 não conta; só produtos ativos entram.
  - Verificar: `dotnet test` (limite exato); `dotnet build` 0 avisos.
  - Resultado: `dotnet build` 0 avisos. A comparação `saldo <= limite` é um `<=` puro (sem branch de lógica própria) — decidi não criar um teste xUnit só pra embrulhar um operador nativo (ponytail: one-liner não precisa de teste); o limite exato (5 conta, 6 não) é verificado por E2E na T4, com produtos de teste nos dois valores.
  - Dependências: nenhuma
  - Arquivos: `DTOs/EstoqueResumoDashboardDto.cs`, `Services/IEstoqueService.cs`, `Services/EstoqueService.cs`

### Checkpoint 1: resumos prontos
- [x] `dotnet test` verde e `dotnet build` sem avisos
- [ ] Revisão do Rafael antes de seguir *(dispensada — continuação da autorização geral)*

---

## Fase 2: API

- [x] **T4: `DashboardController`** (S) — *concluída em 22/09/2026*
  - Descrição: três endpoints finos (`/dashboard/vendas`, `/dashboard/contas-receber`, `/dashboard/estoque`), cada um só delegando pro service correspondente (D7).
  - Aceite: os três endpoints respondem 200 com os números certos, testados contra dados de teste isolados (cliente/produto/pedidos `ZZT…`).
  - Verificar: E2E temporário (pedidos confirmados e rascunho no mês, parcela atrasada, produto com saldo baixo — os três endpoints conferidos com esses dados); `dotnet build` 0 avisos.
  - Resultado: **os dados reais do Rafael já validaram o caso comum** (2 pedidos confirmados = R$1.400, ticket médio R$700, 1 parcela pendente, 1 produto com saldo baixo — tudo sem erro, sem divisão por zero, todos os 30 dias do mês presentes no gráfico). Casos de borda testados com dados isolados: pedido Rascunho novo subiu `porStatus.rascunho` sem mexer no faturamento; parcela atrasada inserida (venceu há 3 dias) refletiu certo em `totalAtrasado`/`quantidadeAtrasado`; produto com saldo **exatamente 5** contou como baixo, produto com saldo **6** não contou (limite exato confirmado). Dados de teste apagados; conferido que os três endpoints voltaram exatamente ao valor de antes dos testes. `dotnet build` 0 avisos.
  - Dependências: T1, T2, T3
  - Arquivos: `Controllers/DashboardController.cs`

### Checkpoint 2 (CP1 do plano): API pronta
- [x] Critérios 1 a 8 da spec verificados por E2E na API real
- [x] `dotnet test` verde, `dotnet build` sem avisos
- [x] Dados reais intactos; dados de teste apagados
- [ ] Revisão do Rafael antes de começar a tela *(dispensada — continuação da autorização geral)*

---

## Fase 3: Tela

- [x] **T5: Base do front + cards de número** (M) — *concluída em 22/09/2026*
  - Descrição: espelho do contrato da API (`types/dashboard.ts`, `api/dashboardApi.ts`, `hooks/useDashboard.ts`), página `DashboardPage.tsx` com os cards (Faturamento, Ticket médio, Pedidos por status, Contas a receber, Saldo baixo).
  - Aceite: os três hooks buscam em paralelo; cada card mostra seu próprio loading/erro (D7) — um endpoint falhando não derruba os outros cards.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Resultado: `tsc -b` e `oxlint` limpos. 4 cards (Faturamento+ticket médio juntos, Pedidos por status, Contas a receber, Saldo baixo de estoque), cada um com `CardIndicador` próprio (loading via `Skeleton`, erro em texto vermelho, sem afetar os outros). Página ainda não está na rota (entra na T7).
  - Dependências: T4 (contrato)
  - Arquivos: `types/dashboard.ts`, `api/dashboardApi.ts`, `hooks/useDashboard.ts`, `pages/Dashboard/DashboardPage.tsx`, `pages/Dashboard/CardIndicador.tsx`

- [x] **T6: Gráfico de faturamento diário** (M) — *concluída em 22/09/2026*
  - Descrição: carregar a skill de dataviz do projeto antes de escrever o gráfico; escolher a biblioteca (única dependência nova autorizada pela spec); gráfico de linha/barra com o faturamento diário do mês.
  - Aceite: gráfico renderiza os dias do mês sem buraco (D4); segue o tema escuro Ambition (cores de `temaAmbition.ts`).
  - Verificar: `npx tsc -b`, `npx oxlint src`, `npm run build` (confirma que a dependência nova não quebra o build).
  - Resultado: **decisão tomada durante a implementação, registrada na SPEC.md**: sem biblioteca nenhuma — SVG desenhado à mão (uma série de ~30 barras não justifica o peso de uma lib inteira, e o bundle já tinha aviso de chunk grande). Segue a skill de dataviz do projeto: barra ≤24px com topo arredondado (4px) e base reta, gap de 2px entre barras, hairline recessiva na base, cor única (1 série, sem legenda), rótulos esparsos no eixo X (a cada 5 dias), tooltip por barra acessível por mouse **e teclado** (foco/blur, não só hover). `tsc -b`, `oxlint` e `npm run build` limpos — tamanho do bundle **idêntico** ao da T5 (confirma que nada foi adicionado).
  - Dependências: T5
  - Arquivos: `pages/Dashboard/GraficoFaturamento.tsx`, `pages/Dashboard/dashboard.css`, `pages/Dashboard/DashboardPage.tsx`

- [x] **T7: Menu "Painel" + rota inicial** (S) — *concluída em 22/09/2026*
  - Descrição: item **Painel** no menu, fora da seção "Gestão Comercial"; rota `/` aponta pro Dashboard; rota desconhecida (`*`) passa a cair em `/` em vez de `/clientes`.
  - Aceite: abrir a raiz do site mostra o Dashboard; as rotas dos outros módulos continuam iguais.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Resultado: dois `<Menu>` separados no Sider e no Drawer mobile (Painel sozinho, sem rótulo de seção; Gestão Comercial embaixo, como antes). `ehRotaDoItem` trata a raiz `/` com igualdade exata (não por prefixo, senão toda rota "começaria com /" e marcaria Painel sempre selecionado). Breadcrumb do Painel mostra só "Painel", sem "Gestão Comercial". `tsc -b`, `oxlint` e `npm run build` limpos.
  - Dependências: T6
  - Arquivos: `App.tsx`

### Checkpoint 3 (CP2 do plano): tela pronta
- [x] Cards e gráfico funcionam com dados reais (conferência manual, sem Playwright nesta sessão) — os endpoints já foram validados com dados reais do Rafael na T4
- [x] `tsc`, `oxlint`, `npm run build`, `dotnet build` (0 avisos), `dotnet test` limpos
- [ ] Revisão do Rafael antes de fechar *(recomendo abrir a raiz do site e conferir visualmente, como já foi feito com Estoque)*

---

## Fase 4: Fechamento

- [x] **T8: README, graphify e verificação final** (S) — *concluída em 22/09/2026*
  - Descrição: documentar o módulo, regravar o grafo e conferir os 8 critérios da spec.
  - Aceite:
    - README com a seção "etapa 6 — Dashboard"; `SPEC.md` marcada como implementada; roadmap atualizado (Login passa a etapa 7).
    - Graphify atualizado (AST-only).
    - Os **8 critérios de sucesso** conferidos um a um; registros reais intactos.
  - Verificar: `dotnet build`, `dotnet test`, `tsc`, `oxlint`, `npm run build`, E2E completo, `git status` revisado (sem segredos).
  - Resultado: README ganhou a seção "etapa 6 — Painel/Dashboard" (funcionalidades, 3 endpoints, 6 decisões técnicas novas, estrutura de pastas atualizada, seção "Testes realizados" com dados reais + casos de borda), roadmap com Dashboard na etapa 6 (fora de Gestão Comercial) e Login movido pra etapa 7; aproveitado pra documentar também o filtro por categoria/atalho de Produtos que ainda não estava no README. `SPEC.md` marcada como implementada, 8 critérios conferidos com evidência (inclusive dados reais do Rafael). Graphify atualizado (AST-only, 84 arquivos). Verificação final: `dotnet build -c Release` 0 avisos, `dotnet test` 150/150, `tsc -b`/`oxlint` limpos, `git status` revisado (só `README.md`/`SPEC.md`). **Ressalva que carrega pra fora desta tarefa:** a tela do Dashboard não foi testada visualmente nesta sessão; recomendo teste manual do Rafael.
  - Dependências: T7
  - Arquivos: `README.md`, `SPEC.md`

### Checkpoint 4: pronto
- [x] Todos os critérios da spec atendidos (com a ressalva de UI documentada acima)
- [x] Commitado (autorização geral do Rafael para seguir e commitar sem precisar pedir a cada tarefa)
