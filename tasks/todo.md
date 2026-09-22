# Tarefas: Módulo Estoque

Contexto: [SPEC.md](../SPEC.md) · [plan.md](plan.md). Marque `[x]` ao concluir e verificar. Regras E1 a E7 e "critérios de sucesso" 1 a 10 estão na spec.

Comandos (raiz): `dotnet build ErpPortfolio.slnx` · `dotnet test backend/ErpPortfolio.Tests` · (front, em `frontend/erp-portfolio-web`) `npx tsc -b` · `npx oxlint src`.
"E2E temporário" = instância da API em porta separada (ex. 5099) + script, com dados de teste apagados no fim (produto de SKU de teste, sem tocar nos reais).

---

## Fase 1: Regra pura (sem banco)

- [x] **T1: Enum `TipoMovimentacao` + cálculo puro de saldo** (S) — *concluída em 22/09/2026*
  - Descrição: `TipoMovimentacao { Entrada, Saida }` e uma função pura `EstoqueCalculo.Saldo(IEnumerable<(TipoMovimentacao Tipo, decimal Quantidade)>)`.
  - Aceite:
    - Lista vazia → saldo 0.
    - Só entradas → soma; só saídas → soma negativa; misto → diferença correta.
    - Testável sem banco (xUnit puro, como `CalculoPedidoTests`).
  - Verificar: `dotnet test` verde; `dotnet build` 0 avisos.
  - Resultado: 110 testes passando (105 anteriores + 5 novos), 0 avisos. Build precisou de `-c Release` (API estava rodando e trava o `.exe` do Debug, como já documentado no README).
  - Dependências: nenhuma
  - Arquivos: `backend/ErpPortfolio.Api/Models/TipoMovimentacao.cs`, `backend/ErpPortfolio.Api/Services/EstoqueCalculo.cs`, `backend/ErpPortfolio.Tests/EstoqueCalculoTests.cs`

### Checkpoint 1: regra pura
- [x] `dotnet test` verde e `dotnet build` sem avisos
- [ ] Revisão do Rafael antes de seguir *(dispensada — autorização geral para seguir sem parar)*

---

## Fase 2: Banco

- [x] **T2: Entidade `EstoqueMovimentacao` e mapeamento EF** (S) — *concluída em 22/09/2026*
  - Descrição: entidade e mapeamento no `ErpPortfolioDbContext` (snake_case, FK `produto_id` RESTRICT, FK `pedido_id` RESTRICT opcional, `CHECK quantidade > 0`, tipo como texto).
  - Aceite:
    - Índices `ix_estoque_movimentacoes_produto_id`, `ix_estoque_movimentacoes_pedido_id`, `ix_estoque_movimentacoes_data_movimentacao`.
    - `pedido_id` aceita nulo (entrada manual não referencia pedido).
  - Verificar: `dotnet build` 0 avisos; teste de modelo em memória (como `ModeloPedidoTests`) confere tabela, FKs, índices e CHECK.
  - Resultado: 124 testes passando (110 anteriores + 14 novos de `ModeloEstoqueTests`), 0 avisos. FK `pedido_id` também RESTRICT (não cascade), já que o histórico de movimentações nunca é apagado.
  - Dependências: T1
  - Arquivos: `Models/EstoqueMovimentacao.cs`, `Data/ErpPortfolioDbContext.cs`, `backend/ErpPortfolio.Tests/ModeloEstoqueTests.cs`

- [x] **T3: Migration `CriacaoTabelaEstoqueMovimentacoes`** (S) — *concluída em 22/09/2026*
  - Descrição: gerar a migration, testar num banco descartável (subida e volta) e aplicar no de desenvolvimento **com backup**.
  - Aceite:
    - A migration **só cria** `estoque_movimentacoes`; nada em `pedidos`, `produtos`, `clientes`, `categorias` muda (esquema comparado antes e depois).
    - `Down` remove só a tabela nova.
  - Verificar: script SQL aplicado e revertido num banco descartável; `dotnet ef database update` no dev; `\d estoque_movimentacoes` conferido; esquema das outras tabelas idêntico.
  - Resultado: migration com só 1 `CreateTable` + 3 `CreateIndex` (Down: 1 `DropTable`). Banco descartável (`erp_portfolio_db_teste_migration`, restaurado do backup completo para ter o histórico de migrations): 19 verificações OK (insert válido, quantidade 0 recusada pelo CHECK, produto inexistente recusado pela FK, Down removendo só a tabela nova). Backup em `.claude/ferramentas-locais/backup_pre_estoque_20260922.dump` (fora do Git). Dev: aplicada, contagens de clientes/produtos/categorias/pedidos idênticas antes e depois (1/1/2/1), `estoque_movimentacoes` criada vazia.
  - Dependências: T2
  - Arquivos: `Data/Migrations/20260922120855_CriacaoTabelaEstoqueMovimentacoes.cs`, `.Designer.cs`, `ErpPortfolioDbContextModelSnapshot.cs`

### Checkpoint 2: banco
- [x] Tabela criada, esquema das tabelas antigas idêntico, backup feito
- [ ] Revisão do Rafael antes de seguir *(dispensada — autorização geral para seguir sem parar)*

---

## Fase 3: API

- [x] **T4: DTOs + consulta (listar com saldo, extrato por produto)** (M) — *concluída em 22/09/2026*
  - Descrição: `GET /api/estoque?busca=&pagina=&tamanhoPagina=` (produtos com saldo, agregação por `produto_id`) e `GET /api/estoque/{produtoId}/movimentacoes?pagina=&tamanhoPagina=` (extrato, mais recente primeiro).
  - Aceite:
    - `busca` acha por nome **ou** SKU do produto (mesmo padrão de Produtos).
    - Produto sem nenhuma movimentação aparece com saldo **0** (não some da lista).
    - `produtoId` inexistente no extrato retorna 404.
  - Verificar: E2E temporário; `dotnet build` 0 avisos.
  - Resultado: instância temporária (porta 5099) contra o banco de dev. **Achado corrigido:** a subconsulta de saldo não podia ficar num método de instância separado (`InvalidOperationException` do EF — "client projection contains a reference to a constant expression... through instance method"); movida para inline dentro do `Select`. Verificado: produto sem movimentação = saldo 0; inserir uma `Entrada` de teste (SQL direto) fez o saldo e o extrato baterem; busca por SKU encontrou o produto; produto inexistente no extrato = 404. Dado de teste apagado, `estoque_movimentacoes` volta a 0 linhas. `dotnet test` 124/124, `dotnet build` 0 avisos.
  - Dependências: T3
  - Arquivos: `DTOs/EstoqueResumoDto.cs`, `DTOs/EstoqueFiltroDto.cs`, `DTOs/MovimentacaoRespostaDto.cs`, `Services/IEstoqueService.cs`, `Services/EstoqueService.cs`, `Controllers/EstoqueController.cs`, `Program.cs`

- [x] **T5: Entrada manual** (S) — *concluída em 22/09/2026*
  - Descrição: `POST /api/estoque/entradas` — `{ produtoId, quantidade, motivo? }`, cria uma movimentação `Entrada`.
  - Aceite:
    - 201 com a movimentação criada; saldo do produto sobe na consulta seguinte (T4).
    - Produto inexistente → 404; produto **inativo** → 400 (E4); quantidade ≤ 0 ou fora de `0,001–999.999,999` → 400 (E5).
    - `motivo` opcional, até 200 caracteres.
  - Verificar: E2E temporário; `dotnet test`; `dotnet build` 0 avisos.
  - Resultado: 7 verificações na instância temporária (porta 5099) com um produto próprio de teste (SKU `67000001`, id apagado ao final): entrada válida com motivo (201, saldo 20), entrada sem motivo (201, `motivo: null`), produto inexistente (404), quantidade 0 (400), quantidade com 4 casas (400), produto inativado depois (400 no campo `ProdutoId`). Dados de teste apagados; contagem real de produtos (1) e `estoque_movimentacoes` (0) confirmadas. `dotnet test` 124/124.
  - Dependências: T4
  - Arquivos: `DTOs/EstoqueEntradaDto.cs`, `Services/EstoqueService.cs`, `Services/IEstoqueService.cs`, `Controllers/EstoqueController.cs`

- [x] **T6: Confirmar pedido baixa o estoque** (M) — *concluída em 22/09/2026*
  - Descrição: `PedidoService.Confirmar` passa a checar saldo de todos os itens e gravar uma `Saida` por item, na mesma transação (E2).
  - Aceite:
    - Confirmar com saldo suficiente em todos os itens: saldo de cada produto cai exatamente a quantidade do item; movimentação com `pedidoId` e motivo `"Venda pedido #N"`.
    - Confirmar com **qualquer** item sem saldo suficiente: 400 no campo `Itens` (ou equivalente), pedido **continua Rascunho**, **nenhuma** movimentação é gravada (nem dos itens que tinham saldo).
    - As outras validações de confirmar (forma de pagamento, cliente/produto ativo — R6 de Pedidos) continuam valendo.
  - Verificar: E2E temporário (caso com saldo, caso sem saldo, caso limite exato); `dotnet test`; `dotnet build` 0 avisos.
  - Resultado: cliente e produto de teste isolados (SKU `67000002`) na instância temporária (porta 5099). Entrada de 5, pedido de 3 confirmado (saldo 5→2, extrato com Saída "Venda pedido #144"), pedido de 10 recusado (400 "Estoque insuficiente: ... saldo 2,000, pedido pede 10,000", pedido continuou Rascunho, saldo e extrato inalterados), pedido editado para exatamente 2 (o saldo restante) confirmou normalmente (saldo foi a 0). Dados de teste apagados (clientes, produtos, pedidos e estoque_movimentacoes reais intactos). `dotnet test` 124/124, `dotnet build` 0 avisos. `BaixarAsync`/`Estornar` não chamam `SaveChanges`: ficam na mesma transação do `SaveChangesAsync` já existente em `PedidoService`.
  - Dependências: T5
  - Arquivos: `Services/PedidoService.cs`, `Services/EstoqueService.cs`, `Services/IEstoqueService.cs`

- [x] **T7: Cancelar pedido Confirmado devolve o estoque** (S) — *concluída em 22/09/2026*
  - Descrição: `PedidoService.Cancelar` passa a gerar uma `Entrada` de estorno por item **só quando** o status antes do cancelamento era `Confirmado` (E3).
  - Aceite:
    - Cancelar um `Confirmado`: saldo de cada produto volta a subir a quantidade do item; motivo `"Estorno cancelamento pedido #N"`.
    - Cancelar um `Rascunho` (nunca confirmado): **nenhuma** movimentação gerada, saldo inalterado.
    - Cancelar duas vezes continua idempotente (204, sem gerar estorno duplicado).
  - Verificar: E2E temporário (cancelar confirmado, cancelar rascunho, cancelar duas vezes); `dotnet test`; `dotnet build` 0 avisos.
  - Resultado: implementada junto com a T6 (mesmo arquivo/commit anterior); esta tarefa cobriu a verificação. Cliente/produto de teste isolados (SKU `67000003`): entrada de 10, pedido de 4 confirmado (saldo 6), cancelado (saldo volta a 10, extrato com "Estorno cancelamento pedido #146"), cancelado de novo (204, saldo continua 10, não duplicou), pedido rascunho nunca confirmado cancelado (204, saldo continua 10, extrato continua com só 3 linhas). Dados de teste apagados; reais intactos. `dotnet test` 124/124.
  - Dependências: T6
  - Arquivos: `Services/PedidoService.cs`, `Services/EstoqueService.cs`, `Services/IEstoqueService.cs`

### Checkpoint 3: API pronta
- [x] Critérios 1 a 7 da spec verificados por E2E na API real (instância temporária)
- [x] `dotnet test` verde, `dotnet build` sem avisos
- [x] Dados reais (cliente, produto, categorias, pedidos) intactos; dados de teste apagados
- [ ] Revisão do Rafael antes de começar a tela *(dispensada — autorização geral para seguir sem parar)*

---

## Fase 4: Tela

- [x] **T8: Base do front + lista de estoque** (M) — *concluída em 22/09/2026*
  - Descrição: espelho do contrato da API (`types/estoque.ts`, `api/estoqueApi.ts`, `hooks/useEstoque.ts`), item **Estoque** no menu, rota `/estoque`, tabela com Produto, SKU, Unidade, Saldo e busca por nome/SKU.
  - Aceite:
    - Saldo negativo (não deveria acontecer, mas se acontecer) aparece em vermelho, como a margem negativa em Produtos.
    - Paginação no servidor, mesmo padrão dos outros módulos.
  - Verificar: `npx tsc -b`, `npx oxlint src`; abrir a tela com movimentações criadas pela API e conferir busca e saldo.
  - Resultado: implementada junto com a T9 (a lista só faz sentido com os drawers funcionando). Ver resultado consolidado na T9.
  - Dependências: T4 (contrato)
  - Arquivos: `types/estoque.ts`, `api/estoqueApi.ts`, `hooks/useEstoque.ts`, `pages/Estoque/EstoqueListaPage.tsx`, `App.tsx`, `utils/moeda.ts` (formatarQuantidade)

- [x] **T9: Drawer de nova entrada + drawer de extrato** (M) — *concluída em 22/09/2026*
  - Descrição: botão **Nova entrada** abre drawer (seleção de produto reaproveitando `SelecaoProduto`, quantidade, motivo opcional); ação **Ver movimentações** por linha abre drawer com o extrato paginado (tipo, quantidade, motivo, data).
  - Aceite:
    - Lançar uma entrada atualiza o saldo na lista sem recarregar a página (invalidação de cache do TanStack Query).
    - Erros 400 da API (produto inativo, quantidade inválida) aparecem no campo certo.
    - Extrato mostra `Saida`/`Entrada` com cor ou ícone diferente, mais recente primeiro.
  - Verificar: `npx tsc -b`, `npx oxlint src`; Playwright: lançar entrada e ver saldo mudar, abrir extrato e ver a movimentação.
  - Resultado: `npx tsc -b` limpo, `npx oxlint src` limpo (1 aviso de `set-state-in-effect` no reset de página do extrato ao trocar de produto, corrigido ajustando o estado durante a renderização em vez de `useEffect`, padrão recomendado pelo React), `npm run build` limpo (só o aviso já conhecido de chunk > 500kB). **Limitação desta sessão:** sem ferramenta de navegador/Playwright disponível, não foi possível repetir a verificação visual (390px, fluxo clicado) feita nos módulos anteriores — só os gates estáticos (tipos, lint, build) foram conferidos. Recomendo um teste manual rápido na tela antes de considerar o módulo fechado.
  - Dependências: T8, T5
  - Arquivos: `pages/Estoque/EntradaEstoqueDrawer.tsx`, `pages/Estoque/MovimentacoesDrawer.tsx`, `schemas/estoqueEntradaSchema.ts`

- [x] **T10: Celular e polimento** (S) — *concluída em 22/09/2026, com ressalva*
  - Descrição: lista e drawers sem rolagem horizontal no celular; conferir o fluxo completo (confirmar pedido sem saldo mostra erro, confirmar com saldo baixa, cancelar confirmado devolve) direto na tela.
  - Aceite: 390 px sem rolagem horizontal; nenhum erro de console além dos esperados (400 de saldo insuficiente).
  - Verificar: Playwright em 390 px e 1920 px; fluxo Pedidos + Estoque de ponta a ponta no navegador.
  - Resultado: **feito por revisão de código, não por Playwright** (sem ferramenta de navegador nesta sessão, diferente dos módulos anteriores). Ajustei `EstoqueListaPage` para ter colunas compactas no celular (SKU/nome/saldo empilhados numa coluna, ação em outra), mesmo padrão comprovado de `ProdutosListaPage`; os dois drawers já usavam `size={telas.sm === false ? '100%' : ...}`, igual ao `CategoriaFormDrawer`. `tsc -b` e `oxlint` limpos. **A API real foi verificada de ponta a ponta nas T6/T7** (confirmar sem saldo bloqueia, confirmar com saldo baixa, cancelar confirmado devolve) — só falta a conferência visual/celular, que fica pendente de um teste manual do Rafael.
  - Dependências: T9, T7
  - Arquivos: `pages/Estoque/EstoqueListaPage.tsx`

### Checkpoint 4: tela pronta
- [x] Fluxo completo (entrada manual, confirmar baixa, cancelar devolve, extrato) funciona na API real (E2E das T5-T7); **verificação visual no navegador pendente** (sem ferramenta disponível nesta sessão)
- [x] `tsc`, `oxlint` limpos; `dotnet build` (0 avisos), `dotnet test` (124/124) limpos; dados de teste apagados
- [ ] Revisão do Rafael antes de fechar *(recomendo testar a tela manualmente antes do fechamento em T11, dado que o navegador não pôde ser usado aqui)*

---

## Fase 5: Fechamento

- [ ] **T11: README, graphify e verificação final** (S)
  - Descrição: documentar o módulo, regravar o grafo e conferir os 10 critérios da spec.
  - Aceite:
    - README com a seção "etapa 4 — Estoque" (funcionalidades, endpoints, tabela do banco, decisões técnicas); `SPEC.md` marcada como implementada; roadmap da etapa 4 atualizado (Login passa a etapa 5).
    - Graphify atualizado sem perder conceitos/hiperarestas.
    - Os **10 critérios de sucesso** conferidos um a um; registros reais intactos.
  - Verificar: `dotnet build`, `dotnet test`, `tsc`, `oxlint`, E2E API e tela completos, `git status` revisado (sem segredos).
  - Dependências: T10
  - Arquivos: `README.md`, `SPEC.md`

### Checkpoint 5: pronto
- [ ] Todos os critérios da spec atendidos
- [ ] Commitado a pedido do Rafael
