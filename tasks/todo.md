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

- [ ] **T3: Migration `CriacaoTabelaEstoqueMovimentacoes`** (S)
  - Descrição: gerar a migration, testar num banco descartável (subida e volta) e aplicar no de desenvolvimento **com backup**.
  - Aceite:
    - A migration **só cria** `estoque_movimentacoes`; nada em `pedidos`, `produtos`, `clientes`, `categorias` muda (esquema comparado antes e depois).
    - `Down` remove só a tabela nova.
  - Verificar: script SQL aplicado e revertido num banco descartável; `dotnet ef database update` no dev; `\d estoque_movimentacoes` conferido; esquema das outras tabelas idêntico.
  - Dependências: T2
  - Arquivos: `Data/Migrations/<data>_CriacaoTabelaEstoqueMovimentacoes.cs`, `.Designer.cs`, `ErpPortfolioDbContextModelSnapshot.cs`

### Checkpoint 2: banco
- [ ] Tabela criada, esquema das tabelas antigas idêntico, backup feito
- [ ] Revisão do Rafael antes de seguir

---

## Fase 3: API

- [ ] **T4: DTOs + consulta (listar com saldo, extrato por produto)** (M)
  - Descrição: `GET /api/estoque?busca=&pagina=&tamanhoPagina=` (produtos com saldo, agregação por `produto_id`) e `GET /api/estoque/{produtoId}/movimentacoes?pagina=&tamanhoPagina=` (extrato, mais recente primeiro).
  - Aceite:
    - `busca` acha por nome **ou** SKU do produto (mesmo padrão de Produtos).
    - Produto sem nenhuma movimentação aparece com saldo **0** (não some da lista).
    - `produtoId` inexistente no extrato retorna 404.
  - Verificar: E2E temporário; `dotnet build` 0 avisos.
  - Dependências: T3
  - Arquivos: `DTOs/EstoqueResumoDto.cs`, `DTOs/EstoqueFiltroDto.cs`, `DTOs/MovimentacaoRespostaDto.cs`, `Services/IEstoqueService.cs`, `Services/EstoqueService.cs`, `Controllers/EstoqueController.cs`, `Program.cs`

- [ ] **T5: Entrada manual** (S)
  - Descrição: `POST /api/estoque/entradas` — `{ produtoId, quantidade, motivo? }`, cria uma movimentação `Entrada`.
  - Aceite:
    - 201 com a movimentação criada; saldo do produto sobe na consulta seguinte (T4).
    - Produto inexistente → 404; produto **inativo** → 400 (E4); quantidade ≤ 0 ou fora de `0,001–999.999,999` → 400 (E5).
    - `motivo` opcional, até 200 caracteres.
  - Verificar: E2E temporário; `dotnet test`; `dotnet build` 0 avisos.
  - Dependências: T4
  - Arquivos: `DTOs/EstoqueEntradaDto.cs`, `Services/EstoqueService.cs`, `Controllers/EstoqueController.cs`

- [ ] **T6: Confirmar pedido baixa o estoque** (M)
  - Descrição: `PedidoService.Confirmar` passa a checar saldo de todos os itens e gravar uma `Saida` por item, na mesma transação (E2).
  - Aceite:
    - Confirmar com saldo suficiente em todos os itens: saldo de cada produto cai exatamente a quantidade do item; movimentação com `pedidoId` e motivo `"Venda pedido #N"`.
    - Confirmar com **qualquer** item sem saldo suficiente: 400 no campo `Itens` (ou equivalente), pedido **continua Rascunho**, **nenhuma** movimentação é gravada (nem dos itens que tinham saldo).
    - As outras validações de confirmar (forma de pagamento, cliente/produto ativo — R6 de Pedidos) continuam valendo.
  - Verificar: E2E temporário (caso com saldo, caso sem saldo, caso limite exato); `dotnet test`; `dotnet build` 0 avisos.
  - Dependências: T5
  - Arquivos: `Services/PedidoService.cs`, `Services/EstoqueService.cs` (método `Baixar`)

- [ ] **T7: Cancelar pedido Confirmado devolve o estoque** (S)
  - Descrição: `PedidoService.Cancelar` passa a gerar uma `Entrada` de estorno por item **só quando** o status antes do cancelamento era `Confirmado` (E3).
  - Aceite:
    - Cancelar um `Confirmado`: saldo de cada produto volta a subir a quantidade do item; motivo `"Estorno cancelamento pedido #N"`.
    - Cancelar um `Rascunho` (nunca confirmado): **nenhuma** movimentação gerada, saldo inalterado.
    - Cancelar duas vezes continua idempotente (204, sem gerar estorno duplicado).
  - Verificar: E2E temporário (cancelar confirmado, cancelar rascunho, cancelar duas vezes); `dotnet test`; `dotnet build` 0 avisos.
  - Dependências: T6
  - Arquivos: `Services/PedidoService.cs`, `Services/EstoqueService.cs` (método `Estornar`)

### Checkpoint 3: API pronta
- [ ] Critérios 1 a 7 da spec verificados por E2E na API real (instância temporária)
- [ ] `dotnet test` verde, `dotnet build` sem avisos
- [ ] Dados reais (cliente, produto, categorias, pedidos) intactos; dados de teste apagados
- [ ] Revisão do Rafael antes de começar a tela

---

## Fase 4: Tela

- [ ] **T8: Base do front + lista de estoque** (M)
  - Descrição: espelho do contrato da API (`types/estoque.ts`, `api/estoqueApi.ts`, `hooks/useEstoque.ts`), item **Estoque** no menu, rota `/estoque`, tabela com Produto, SKU, Unidade, Saldo e busca por nome/SKU.
  - Aceite:
    - Saldo negativo (não deveria acontecer, mas se acontecer) aparece em vermelho, como a margem negativa em Produtos.
    - Paginação no servidor, mesmo padrão dos outros módulos.
  - Verificar: `npx tsc -b`, `npx oxlint src`; abrir a tela com movimentações criadas pela API e conferir busca e saldo.
  - Dependências: T4 (contrato)
  - Arquivos: `types/estoque.ts`, `api/estoqueApi.ts`, `hooks/useEstoque.ts`, `pages/Estoque/EstoqueListaPage.tsx`, `App.tsx`

- [ ] **T9: Drawer de nova entrada + drawer de extrato** (M)
  - Descrição: botão **Nova entrada** abre drawer (seleção de produto reaproveitando `SelecaoProduto`, quantidade, motivo opcional); ação **Ver movimentações** por linha abre drawer com o extrato paginado (tipo, quantidade, motivo, data).
  - Aceite:
    - Lançar uma entrada atualiza o saldo na lista sem recarregar a página (invalidação de cache do TanStack Query).
    - Erros 400 da API (produto inativo, quantidade inválida) aparecem no campo certo.
    - Extrato mostra `Saida`/`Entrada` com cor ou ícone diferente, mais recente primeiro.
  - Verificar: `npx tsc -b`, `npx oxlint src`; Playwright: lançar entrada e ver saldo mudar, abrir extrato e ver a movimentação.
  - Dependências: T8, T5
  - Arquivos: `pages/Estoque/EntradaEstoqueDrawer.tsx`, `pages/Estoque/MovimentacoesDrawer.tsx`, `schemas/estoqueEntradaSchema.ts`

- [ ] **T10: Celular e polimento** (S)
  - Descrição: lista e drawers sem rolagem horizontal no celular; conferir o fluxo completo (confirmar pedido sem saldo mostra erro, confirmar com saldo baixa, cancelar confirmado devolve) direto na tela.
  - Aceite: 390 px sem rolagem horizontal; nenhum erro de console além dos esperados (400 de saldo insuficiente).
  - Verificar: Playwright em 390 px e 1920 px; fluxo Pedidos + Estoque de ponta a ponta no navegador.
  - Dependências: T9, T7
  - Arquivos: `pages/Estoque/*.tsx`, CSS reaproveitado dos outros módulos

### Checkpoint 4: tela pronta
- [ ] Fluxo completo (entrada manual, confirmar baixa, cancelar devolve, extrato) funciona no navegador
- [ ] `tsc`, `oxlint`, `dotnet build` (0 avisos), `dotnet test` limpos; dados de teste apagados
- [ ] Revisão do Rafael antes de fechar

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
