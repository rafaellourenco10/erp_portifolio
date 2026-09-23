# Checklist: Módulo Fornecedores + Pedidos de Compra (etapa 7)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Schema

- [ ] **T1: Migration + Models** (M)
  - Descrição: `Fornecedor.cs`, `PedidoCompra.cs`, `PedidoCompraItem.cs` (espelhando `Cliente`/`Pedido`/`PedidoItem`); `EstoqueMovimentacao.cs` ganha `PedidoCompraId`/`PedidoCompra`; mapeamento no `ErpPortfolioDbContext` (tabelas, colunas snake_case, índices únicos `ix_fornecedores_documento` e `(pedido_compra_id, produto_id)`, FKs); `dotnet ef migrations add AdicionaFornecedoresEPedidosCompra`.
  - Aceite: `dotnet ef database update` aplica sem erro; `dotnet build` 0 avisos.
  - Verificar: `dotnet build ErpPortfolio.slnx -c Release`.
  - Dependências: nenhuma.
  - Arquivos: `Models/Fornecedor.cs`, `Models/PedidoCompra.cs`, `Models/PedidoCompraItem.cs`, `Models/EstoqueMovimentacao.cs`, `Data/ErpPortfolioDbContext.cs`, `Migrations/*`.

- [ ] **T2: Fornecedor backend** (M)
  - Descrição: `FornecedorCriacaoDto`, `FornecedorAtualizacaoDto`, `FornecedorFiltroDto`, `FornecedorRespostaDto` (espelho dos DTOs de Cliente); `IFornecedorService`/`FornecedorService` (espelho de `ClienteService`, com `GarantirDocumentoUnicoAsync` num índice próprio); `FornecedoresController` (espelho de `ClientesController`); registro no DI (`Program.cs`).
  - Aceite: CRUD completo funcionando via API real (criar, listar com filtro, editar, inativar, documento duplicado bloqueado).
  - Verificar: `dotnet build`; teste manual via `curl`/Swagger contra a API rodando.
  - Dependências: T1.
  - Arquivos: `DTOs/Fornecedor*.cs`, `Services/IFornecedorService.cs`, `Services/FornecedorService.cs`, `Controllers/FornecedoresController.cs`, `Program.cs`.

- [ ] **T3: EstoqueService — Receber/EstornarCompra** (M)
  - Descrição: `IEstoqueService`/`EstoqueService` ganham `ReceberAsync(pedidoCompraId, itens)` (Entrada por item, motivo `"Compra pedido #N"`, sem checagem de saldo) e `EstornarCompra(pedidoCompraId, itens)` (checa saldo suficiente por item ANTES de enfileirar qualquer Saída — PC7 — e lança `DadoInvalidoException` listando os itens sem saldo se faltar); `MovimentacaoRespostaDto` ganha `PedidoCompraId`.
  - Aceite: xUnit cobrindo o estorno bloqueado por saldo insuficiente (o caso novo que `Estornar`/`BaixarAsync` não tinham).
  - Verificar: `dotnet test`.
  - Dependências: T1.
  - Arquivos: `Services/IEstoqueService.cs`, `Services/EstoqueService.cs`, `DTOs/MovimentacaoRespostaDto.cs`, `ErpPortfolio.Tests/EstoqueCalculoTests.cs` (ou novo arquivo).

### Checkpoint 1: schema + estoque prontos
- [ ] `dotnet build` 0 avisos, `dotnet test` verde

## Fase 2: Backend Pedido de Compra

- [ ] **T4: PedidoCompra backend** (L)
  - Descrição: `PedidoCompraCriacaoDto`, `PedidoCompraItemEntradaDto`, `PedidoCompraRespostaDto`, `PedidoCompraResumoDto`, `PedidoCompraFiltroDto` (espelho dos DTOs de Pedido, sem `FormaPagamento`); `IPedidoCompraService`/`PedidoCompraService` reaproveitando `TransicoesPedido` e `CalculoPedido` — criar/editar rascunho, confirmar (PC5/PC6: fornecedor e produtos ativos, chama `ReceberAsync`, atualiza `Produto.Custo` por item), cancelar (PC7/PC8: chama `EstornarCompra`, idempotente); `PedidosCompraController` (espelho de `PedidosController`, sem corpo no `/confirmar`).
  - Aceite: xUnit para confirmar (entrada gerada + custo atualizado) e cancelar (estorno com saldo ok / bloqueado sem saldo / idempotente); E2E real do fluxo completo.
  - Verificar: `dotnet build`, `dotnet test`, E2E manual contra a API rodando.
  - Dependências: T1, T3.
  - Arquivos: `DTOs/PedidoCompra*.cs`, `Services/IPedidoCompraService.cs`, `Services/PedidoCompraService.cs`, `Controllers/PedidosCompraController.cs`, `Program.cs`, `ErpPortfolio.Tests/PedidoCompraServiceTests.cs`.

### Checkpoint 2 (CP1 do plano): API pronta
- [ ] Critérios 1 a 7 da spec (backend) verificados por E2E na API real
- [ ] `dotnet test` verde, `dotnet build` sem avisos
- [ ] Dados reais intactos; dados de teste apagados

## Fase 3: Telas

- [ ] **T5: Tela Fornecedores** (M)
  - Descrição: `types/fornecedor.ts`, `api/fornecedoresApi.ts`, `hooks/useFornecedores.ts`, `pages/Fornecedores/FornecedorFormDrawer.tsx`, `pages/Fornecedores/FornecedoresListaPage.tsx` (espelho de Clientes, reaproveitando `clientes.css`); rota `/fornecedores` e item de menu (ícone `ShopOutlined`), logo depois de Clientes.
  - Aceite: lista, cria, edita, inativa fornecedor pela tela.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Dependências: T2.
  - Arquivos: `types/fornecedor.ts`, `api/fornecedoresApi.ts`, `hooks/useFornecedores.ts`, `pages/Fornecedores/*`, `App.tsx`.

- [ ] **T6: Tela Pedidos de Compra** (L)
  - Descrição: `types/pedidoCompra.ts`, `api/pedidosCompraApi.ts`, `hooks/usePedidosCompra.ts`, `pages/PedidosCompra/PedidoCompraPage.tsx`, `pages/PedidosCompra/PedidosCompraListaPage.tsx`, `pages/PedidosCompra/ItensPedidoCompraTabela.tsx` (espelho de Pedidos, reaproveitando `pedido.css`); rotas `/pedidos-compra`, `/pedidos-compra/novo`, `/pedidos-compra/:id` e item de menu (ícone `ShoppingOutlined`), logo depois de Pedidos.
  - Aceite: cria rascunho com itens, edita, confirma, cancela pela tela.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Dependências: T4.
  - Arquivos: `types/pedidoCompra.ts`, `api/pedidosCompraApi.ts`, `hooks/usePedidosCompra.ts`, `pages/PedidosCompra/*`, `App.tsx`.

- [ ] **T7: Estoque mostra "Compra #N"** (S)
  - Descrição: componente(s) do extrato de estoque passam a exibir a origem "Compra #N" quando `pedidoCompraId` estiver preenchido (hoje só mostram "Venda #N" via `pedidoId`).
  - Aceite: uma entrada gerada por um pedido de compra confirmado aparece no extrato com o rótulo certo.
  - Verificar: `npx tsc -b`, `npx oxlint src`; conferência manual contra dado real de teste.
  - Dependências: T3, T6 (tela de estoque já existe; só ajusta a exibição).
  - Arquivos: `types/estoque.ts`, componente do extrato em `pages/Estoque/`.

### Checkpoint 3 (CP2 do plano): telas prontas
- [ ] `tsc`, `oxlint`, `npm run build`, `dotnet build`, `dotnet test` limpos

## Fase 4: Fechamento

- [ ] **T8: README, graphify e verificação final** (S)
  - Descrição: README ganha a seção "etapa 7 — Fornecedores e Pedidos de Compra"; tabela de etapas atualizada (Login vira etapa 8+); `SPEC.md` marcada como implementada; graphify atualizado (`graphify update .`, AST-only — nenhum doc/imagem mudou além de SPEC.md/README.md, que entram na próxima extração semântica).
  - Aceite: os **8 critérios de sucesso** da spec conferidos um a um; registros reais intactos; dados de teste apagados.
  - Verificar: `dotnet build`, `dotnet test`, `tsc`, `oxlint`, `npm run build`, E2E completo, `git status` revisado (sem segredos).
  - Dependências: T7.
  - Arquivos: `README.md`, `SPEC.md`.

### Checkpoint 4: pronto
- [ ] Todos os critérios da spec atendidos
- [ ] Commitado (autorização geral do Rafael, 22/09/2026)
