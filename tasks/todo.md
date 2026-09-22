# Tarefas: Módulo Contas a Receber

Contexto: [SPEC.md](../SPEC.md) · [plan.md](plan.md). Marque `[x]` ao concluir e verificar. Regras C1 a C8 e "critérios de sucesso" 1 a 9 estão na spec.

Comandos (raiz): `dotnet build ErpPortfolio.slnx -c Release` · `dotnet test backend/ErpPortfolio.Tests -c Release` · (front, em `frontend/erp-portfolio-web`) `npx tsc -b` · `npx oxlint src`.
"E2E temporário" = instância da API em porta separada (5099) verificada manualmente (`curl`), com dados de teste apagados no fim (cliente/produto/pedido de teste, sem tocar nos reais).

---

## Fase 1: Regra pura (sem banco)

- [x] **T1: Enum `StatusParcela` + divisão em parcelas (função pura)** (S) — *concluída em 22/09/2026*
  - Descrição: `StatusParcela { Pendente, Recebido, Cancelado }` e `ContasReceberCalculo.Dividir(decimal valorTotal, int numeroParcelas)` → lista de valores por parcela, com o resto na última (C2).
  - Aceite:
    - `Dividir(100.00, 3)` → `[33.33, 33.33, 33.34]` (soma = 100.00 exato).
    - `Dividir(100.00, 1)` → `[100.00]`.
    - `Dividir(valor, 12)` sempre soma exatamente `valor`, para vários valores de teste.
  - Verificar: `dotnet test` verde; `dotnet build` 0 avisos.
  - Resultado: 132 testes passando (124 anteriores + 8 novos), 0 avisos. 8 casos: 1 parcela, resto na última (100÷3), divisão exata (300÷3), 4 casos de soma sempre batendo (valores grandes, pequenos, primos), 12 parcelas com valores conferidos ponto a ponto.
  - Dependências: nenhuma
  - Arquivos: `backend/ErpPortfolio.Api/Models/StatusParcela.cs`, `backend/ErpPortfolio.Api/Services/ContasReceberCalculo.cs`, `backend/ErpPortfolio.Tests/ContasReceberCalculoTests.cs`

### Checkpoint 1: regra pura
- [x] `dotnet test` verde e `dotnet build` sem avisos
- [ ] Revisão do Rafael antes de seguir *(dispensada — continuação da autorização geral)*

---

## Fase 2: Banco

- [x] **T2: Entidade `ParcelaReceber` e mapeamento EF** (S) — *concluída em 22/09/2026*
  - Descrição: entidade e mapeamento no `ErpPortfolioDbContext` (snake_case, FK `pedido_id` RESTRICT, `CHECK valor > 0` e `numero_parcela > 0`, `vencimento` como `date`/`DateOnly`, status como texto).
  - Aceite:
    - Índice único `ux_parcelas_receber_pedido_numero (pedido_id, numero_parcela)`.
    - Índice `ix_parcelas_receber_vencimento`.
    - `data_recebimento` aceita nulo.
  - Verificar: `dotnet build` 0 avisos; teste de modelo em memória (como `ModeloEstoqueTests`) confere tabela, FK, índices e CHECK.
  - Resultado: 146 testes passando (132 anteriores + 14 novos), 0 avisos. `Vencimento` mapeado como `DateOnly`/`date`, sem hora.
  - Dependências: T1
  - Arquivos: `Models/ParcelaReceber.cs`, `Data/ErpPortfolioDbContext.cs`, `backend/ErpPortfolio.Tests/ModeloParcelaReceberTests.cs`

- [x] **T3: Migration `CriacaoTabelaParcelasReceber`** (S) — *concluída em 22/09/2026*
  - Descrição: gerar a migration, testar num banco descartável (subida e volta) e aplicar no de desenvolvimento **com backup**.
  - Aceite:
    - A migration **só cria** `parcelas_receber`; nada em `pedidos`, `produtos`, `clientes`, `categorias`, `estoque_movimentacoes` muda.
    - `Down` remove só a tabela nova.
  - Verificar: script aplicado e revertido num banco descartável; `dotnet ef database update` no dev; `\d parcelas_receber` conferido; esquema das outras tabelas idêntico.
  - Resultado: migration com só 1 `CreateTable` + 2 `CreateIndex` (Down: 1 `DropTable`). Banco descartável (restaurado do backup completo): insert válido, `CHECK` de valor 0 recusado, índice único recusando parcela duplicada, FK recusando pedido inexistente, `Down` removendo só a tabela nova. Backup em `.claude/ferramentas-locais/backup_pre_contas_receber_20260922.dump`. Dev: aplicada, contagens de clientes/produtos/categorias idênticas (1/1/2); pedidos (2) e estoque_movimentacoes (2) refletem uso real do Rafael entre sessões, não dados de teste; `parcelas_receber` criada vazia.
  - Dependências: T2
  - Arquivos: `Data/Migrations/20260922190422_CriacaoTabelaParcelasReceber.cs`, `.Designer.cs`, `ErpPortfolioDbContextModelSnapshot.cs`

### Checkpoint 2: banco
- [x] Tabela criada, esquema das tabelas antigas idêntico, backup feito
- [ ] Revisão do Rafael antes de seguir *(dispensada — continuação da autorização geral)*

---

## Fase 3: API

- [x] **T4: Consulta (listar com filtro/status) + marcar recebido** (M) — *concluída em 22/09/2026*
  - Descrição: `GET /api/contas-receber?busca=&status=&pagina=&tamanhoPagina=` (join com pedidos/clientes, `atrasado` calculado no servidor) e `PATCH /api/contas-receber/{id}/receber`.
  - Aceite:
    - `busca` acha por nome do cliente **ou** nº do pedido.
    - `status=Atrasado` acha só `Pendente` com `vencimento` no passado; os outros status filtram exato.
    - Marcar como recebida grava `dataRecebimento`; marcar de novo é idempotente (C7); marcar uma `Cancelado` = 409.
    - `id` inexistente no PATCH = 404.
  - Verificar: E2E temporário (inserindo parcelas via SQL direto, já que a geração automática só vem na T5); `dotnet build` 0 avisos.
  - Resultado: pedido de teste (id 149) com 3 parcelas inseridas via SQL (1 atrasada, 1 recebida, 1 pendente futura). Confirmado: `totalParcelas` correto, `atrasado: true` só na vencida, filtros `Atrasado`/`Recebido` certos, busca por nome e por `#149`, marcar como recebida (200, `dataRecebimento` gravado), marcar de novo idempotente (mesma data), 404 em parcela inexistente, 409 ao tentar receber uma `Cancelado`. Dados de teste apagados; reais intactos. **Achado corrigido:** `switch` sobre `FiltroStatusParcela?` gerava aviso CS8524 (enum não coberto para valores fora dos nomeados); trocado o `null =>` por `_ =>` no branch padrão. `dotnet test` 146/146, `dotnet build` 0 avisos.
  - Dependências: T3
  - Arquivos: `DTOs/ParcelaRespostaDto.cs`, `DTOs/ParcelaFiltroDto.cs`, `DTOs/FiltroStatusParcela.cs`, `Services/IContasReceberService.cs`, `Services/ContasReceberService.cs`, `Controllers/ContasReceberController.cs`, `Program.cs`

- [x] **T5: Confirmar pedido gera as parcelas** (M) — *concluída em 22/09/2026*
  - Descrição: `PATCH /pedidos/{id}/confirmar` passa a receber `{ numeroParcelas?, intervaloDias? }` (defaults 1 e 30) e, ao confirmar com sucesso, chama `ContasReceberService.GerarParcelas` (C1, C2, C3).
  - Aceite:
    - Confirmar com 3 parcelas gera 3 parcelas cuja soma bate com `valorTotal`; vencimentos em `intervaloDias`, `2×intervaloDias`, `3×intervaloDias` dias.
    - Confirmar sem informar nada usa 1 parcela / 30 dias.
    - `numeroParcelas` fora de 1-12 ou `intervaloDias` fora de 1-180 = 400.
    - As validações de confirmar que já existiam (forma de pagamento, cliente/produto ativo, saldo de estoque) continuam valendo.
  - Verificar: E2E temporário (1, 3 e 12 parcelas; valor que não divide exato); `dotnet build` 0 avisos.
  - Resultado: cliente/produto de teste isolados (SKU `67000005`) com 100 unidades em estoque. Confirmado: 3 parcelas (intervalo 15) com vencimentos em 22/09+15/30/45 dias e soma 100,00 exata; confirmar **sem corpo** usou o padrão (1 parcela, 30 dias); 12 parcelas de um pedido de R$ 200,00 somaram exatamente 200,00 (11× R$16,66 + R$16,74 na última); `numeroParcelas: 13` recusado com 400 no campo. Dados de teste apagados; reais intactos (pedidos=2, estoque_movimentacoes=2, como antes). `dotnet test` 146/146, `dotnet build` 0 avisos.
  - Dependências: T4
  - Arquivos: `DTOs/PedidoConfirmarDto.cs`, `Controllers/PedidosController.cs`, `Services/IPedidoService.cs`, `Services/PedidoService.cs`, `Services/IContasReceberService.cs`, `Services/ContasReceberService.cs` (método `GerarParcelas`)

- [ ] **T6: Cancelar pedido cancela as parcelas pendentes** (S)
  - Descrição: `PedidoService.Cancelar` passa a chamar `ContasReceberService.CancelarPendentesAsync` quando o status antes do cancelamento era `Confirmado` (C6).
  - Aceite:
    - Cancelar um `Confirmado` com parcelas mistas (1 `Recebido`, 2 `Pendente`): as 2 `Pendente` viram `Cancelado`; a `Recebido` **não muda**.
    - Cancelar um `Rascunho` (nunca confirmado, sem parcelas) não gera erro nem mexe em nada.
  - Verificar: E2E temporário; `dotnet build` 0 avisos.
  - Dependências: T5
  - Arquivos: `Services/PedidoService.cs`, `Services/ContasReceberService.cs` (método `CancelarPendentesAsync`)

### Checkpoint 3: API pronta
- [ ] Critérios 1 a 8 da spec verificados por E2E na API real (instância temporária)
- [ ] `dotnet test` verde, `dotnet build` sem avisos
- [ ] Dados reais intactos; dados de teste apagados
- [ ] Revisão do Rafael antes de começar a tela

---

## Fase 4: Tela

- [ ] **T7: Base do front + lista de contas a receber** (M)
  - Descrição: espelho do contrato da API (`types/contaReceber.ts`, `api/contasReceberApi.ts`, `hooks/useContasReceber.ts`), item **Contas a Receber** no menu, rota `/contas-receber`, tabela com Cliente, Pedido, Parcela, Valor, Vencimento, Status e ação **Marcar como recebido**.
  - Aceite:
    - Tag de status: Pendente cinza, Atrasado vermelho, Recebido verde, Cancelado riscado.
    - Filtro por status (Segmented) e busca por cliente/nº do pedido; paginação no servidor.
  - Verificar: `npx tsc -b`, `npx oxlint src`; abrir a tela com parcelas geradas pela API e conferir filtros.
  - Dependências: T4 (contrato)
  - Arquivos: `types/contaReceber.ts`, `api/contasReceberApi.ts`, `hooks/useContasReceber.ts`, `pages/ContasReceber/ContasReceberListaPage.tsx`, `App.tsx`

- [ ] **T8: Modal de confirmar pedido com parcelas** (M)
  - Descrição: substitui o `Modal.confirm` de "Confirmar pedido" por um formulário pequeno (Número de parcelas, Intervalo em dias) que envia `{ numeroParcelas, intervaloDias }` no `PATCH /confirmar`.
  - Aceite:
    - Padrão 1 parcela / 30 dias, campos editáveis (1-12 / 1-180).
    - Confirmar cria as parcelas certas (visível em Contas a Receber depois).
    - Cancelar o modal não confirma nada.
  - Verificar: `npx tsc -b`, `npx oxlint src`; conferir contra a API real que o número de parcelas enviado bate com o que aparece em `/contas-receber`.
  - Dependências: T7, T5
  - Arquivos: `pages/Pedidos/PedidoPage.tsx`, `hooks/usePedidos.ts` (useConfirmarPedido aceita corpo), `api/pedidosApi.ts`

- [ ] **T9: Celular e polimento** (S)
  - Descrição: lista de Contas a Receber sem rolagem horizontal no celular; modal de confirmar utilizável em tela pequena.
  - Aceite: 390 px sem rolagem horizontal; nenhum erro de console além dos esperados.
  - Verificar: revisão de código (sem Playwright, se a limitação de ferramenta persistir nesta sessão) seguindo os padrões já usados (colunas compactas de Estoque/Produtos).
  - Dependências: T8
  - Arquivos: `pages/ContasReceber/ContasReceberListaPage.tsx`, `pages/Pedidos/PedidoPage.tsx`

### Checkpoint 4: tela pronta
- [ ] Fluxo completo (confirmar com parcelas, ver na lista, marcar recebido, cancelar cancela pendentes) funciona na API real
- [ ] `tsc`, `oxlint`, `dotnet build` (0 avisos), `dotnet test` limpos; dados de teste apagados
- [ ] Revisão do Rafael antes de fechar

---

## Fase 5: Fechamento

- [ ] **T10: README, graphify e verificação final** (S)
  - Descrição: documentar o módulo, regravar o grafo e conferir os 9 critérios da spec.
  - Aceite:
    - README com a seção "etapa 5 — Contas a Receber"; `SPEC.md` marcada como implementada; roadmap atualizado (Login passa a etapa 6).
    - Graphify atualizado (AST-only).
    - Os **9 critérios de sucesso** conferidos um a um; registros reais intactos.
  - Verificar: `dotnet build`, `dotnet test`, `tsc`, `oxlint`, E2E completo, `git status` revisado (sem segredos).
  - Dependências: T9
  - Arquivos: `README.md`, `SPEC.md`

### Checkpoint 5: pronto
- [ ] Todos os critérios da spec atendidos
- [ ] Commitado
