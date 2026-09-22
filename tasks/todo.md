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

- [x] **T6: Cancelar pedido cancela as parcelas pendentes** (S) — *concluída em 22/09/2026*
  - Descrição: `PedidoService.Cancelar` passa a chamar `ContasReceberService.CancelarPendentesAsync` quando o status antes do cancelamento era `Confirmado` (C6).
  - Aceite:
    - Cancelar um `Confirmado` com parcelas mistas (1 `Recebido`, 2 `Pendente`): as 2 `Pendente` viram `Cancelado`; a `Recebido` **não muda**.
    - Cancelar um `Rascunho` (nunca confirmado, sem parcelas) não gera erro nem mexe em nada.
  - Verificar: E2E temporário; `dotnet build` 0 avisos.
  - Resultado: pedido de teste com 3 parcelas (R$300 ÷ 3), parcela 1 marcada como recebida, pedido cancelado: parcelas 2 e 3 (Pendentes) viraram Cancelado, a 1 continuou Recebido. Pedido rascunho nunca confirmado cancelado sem erro e sem gerar parcela nenhuma. Dados de teste apagados; reais intactos. `dotnet test` 146/146, `dotnet build` 0 avisos.
  - Dependências: T5
  - Arquivos: `Services/PedidoService.cs`, `Services/IContasReceberService.cs`, `Services/ContasReceberService.cs` (método `CancelarPendentesAsync`)

### Checkpoint 3: API pronta
- [x] Critérios 1 a 8 da spec verificados por E2E na API real (instância temporária)
- [x] `dotnet test` verde, `dotnet build` sem avisos
- [x] Dados reais intactos; dados de teste apagados
- [ ] Revisão do Rafael antes de começar a tela *(dispensada — continuação da autorização geral)*

---

## Fase 4: Tela

- [x] **T7: Base do front + lista de contas a receber** (M) — *concluída em 22/09/2026*
  - Descrição: espelho do contrato da API (`types/contaReceber.ts`, `api/contasReceberApi.ts`, `hooks/useContasReceber.ts`), item **Contas a Receber** no menu, rota `/contas-receber`, tabela com Cliente, Pedido, Parcela, Valor, Vencimento, Status e ação **Marcar como recebido**.
  - Aceite:
    - Tag de status: Pendente cinza, Atrasado vermelho, Recebido verde, Cancelado riscado.
    - Filtro por status (Segmented) e busca por cliente/nº do pedido; paginação no servidor.
  - Verificar: `npx tsc -b`, `npx oxlint src`; abrir a tela com parcelas geradas pela API e conferir filtros.
  - Resultado: `TagStatusParcela` novo (reaproveita as cores de `TagStatus.css`, com uma classe nova `.tag-status-riscado` para Cancelado); data de vencimento formatada sem passar por `Date` (evita erro de fuso numa data sem hora). `tsc -b`, `oxlint` e `npm run build` limpos. **Sem verificação visual nesta sessão** (mesma limitação do módulo Estoque — sem ferramenta de navegador); recomendo teste manual.
  - Dependências: T4 (contrato)
  - Arquivos: `types/contaReceber.ts`, `api/contasReceberApi.ts`, `hooks/useContasReceber.ts`, `components/TagStatusParcela.tsx`, `components/TagStatus.css`, `pages/ContasReceber/ContasReceberListaPage.tsx`, `App.tsx`

- [x] **T8: Modal de confirmar pedido com parcelas** (M) — *concluída em 22/09/2026*
  - Descrição: substitui o `Modal.confirm` de "Confirmar pedido" por um formulário pequeno (Número de parcelas, Intervalo em dias) que envia `{ numeroParcelas, intervaloDias }` no `PATCH /confirmar`.
  - Aceite:
    - Padrão 1 parcela / 30 dias, campos editáveis (1-12 / 1-180).
    - Confirmar cria as parcelas certas (visível em Contas a Receber depois).
    - Cancelar o modal não confirma nada.
  - Verificar: `npx tsc -b`, `npx oxlint src`; conferir contra a API real que o número de parcelas enviado bate com o que aparece em `/contas-receber`.
  - Resultado: `pedirConfirmacao` agora só valida e abre um `Modal` controlado (estado `confirmando`) com dois `InputNumber` (parcelas 1-12, intervalo 1-180 dias); `confirmar()` salva o rascunho pendente e chama `confirmarPedido` com o corpo. Em erro, o modal **fica aberto** (melhoria sobre o `modal.confirm` antigo — dá pra corrigir sem reabrir). O payload já bate com o `PedidoConfirmarDto` testado na API real nas T5/T6 (mesmo formato `{numeroParcelas, intervaloDias}`). `tsc -b`, `oxlint` e `npm run build` limpos. Mesma ressalva de falta de verificação visual desta sessão.
  - Dependências: T7, T5
  - Arquivos: `pages/Pedidos/PedidoPage.tsx`, `hooks/usePedidos.ts`, `api/pedidosApi.ts`, `types/pedido.ts`

- [x] **T9: Celular e polimento** (S) — *concluída em 22/09/2026, com ressalva*
  - Descrição: lista de Contas a Receber sem rolagem horizontal no celular; modal de confirmar utilizável em tela pequena.
  - Aceite: 390 px sem rolagem horizontal; nenhum erro de console além dos esperados.
  - Verificar: revisão de código (sem Playwright, se a limitação de ferramenta persistir nesta sessão) seguindo os padrões já usados (colunas compactas de Estoque/Produtos).
  - Resultado: **feito por revisão de código, não por Playwright** (mesma limitação já registrada nos módulos anteriores). `ContasReceberListaPage` ganhou `colunasCelular` (cliente/pedido/parcela/valor/vencimento/status numa coluna, ação em outra), mesmo padrão de `ProdutosListaPage`/`EstoqueListaPage`. O modal de confirmar usa `Flex wrap` nos dois campos, então empilha sozinho em telas estreitas (Ant `Modal` já é responsivo por padrão). `tsc -b`, `oxlint` e `npm run build` limpos. **O fluxo completo foi verificado na API real nas T5-T6** (confirmar com parcelas, cancelar cancela pendentes); falta a conferência visual/celular, pendente de teste manual.
  - Dependências: T8
  - Arquivos: `pages/ContasReceber/ContasReceberListaPage.tsx`

### Checkpoint 4: tela pronta
- [x] Fluxo completo (confirmar com parcelas, ver na lista, marcar recebido, cancelar cancela pendentes) funciona na API real (E2E das T4-T6); **verificação visual no navegador pendente** (sem ferramenta disponível nesta sessão)
- [x] `tsc`, `oxlint` limpos; `dotnet build` (0 avisos), `dotnet test` (146/146) limpos; dados de teste apagados
- [ ] Revisão do Rafael antes de fechar *(recomendo testar a tela manualmente, como já foi feito com Estoque)*

---

## Fase 5: Fechamento

- [x] **T10: README, graphify e verificação final** (S) — *concluída em 22/09/2026*
  - Descrição: documentar o módulo, regravar o grafo e conferir os 9 critérios da spec.
  - Aceite:
    - README com a seção "etapa 5 — Contas a Receber"; `SPEC.md` marcada como implementada; roadmap atualizado (Login passa a etapa 6).
    - Graphify atualizado (AST-only).
    - Os **9 critérios de sucesso** conferidos um a um; registros reais intactos.
  - Verificar: `dotnet build`, `dotnet test`, `tsc`, `oxlint`, E2E completo, `git status` revisado (sem segredos).
  - Resultado: README ganhou a seção "etapa 5 — Contas a Receber" (funcionalidades, endpoints, tabela `parcelas_receber`, 8 decisões técnicas novas, estrutura de pastas atualizada, seção "Testes realizados" com a ressalva de UI), roadmap com Contas a Receber na etapa 5 e Login movido pra etapa 6, status de Estoque atualizado (Rafael confirmou a tela funcionando), "Próximas etapas" revisada. `SPEC.md` marcada como implementada, 9 critérios conferidos com evidência apontada. Graphify atualizado (AST-only, `python -m graphify update .`, 82 arquivos, backup automático em `graphify-out/2026-09-22/`). Verificação final: `dotnet build -c Release` 0 avisos, `dotnet test` 146/146, `tsc -b` e `oxlint` limpos, `git status` revisado (só `README.md`/`SPEC.md`, sem segredos). **Ressalva que carrega pra fora desta tarefa:** a tela de Contas a Receber não foi testada visualmente nesta sessão (sem ferramenta de navegador) — registrado no README, no SPEC e no todo.md; recomendo teste manual do Rafael, como já foi feito com Estoque.
  - Dependências: T9
  - Arquivos: `README.md`, `SPEC.md`

### Checkpoint 5: pronto
- [x] Todos os critérios da spec atendidos (com a ressalva de UI documentada acima)
- [x] Commitado (autorização geral do Rafael para seguir e commitar sem precisar pedir a cada tarefa)
