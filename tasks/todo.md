# Checklist: Módulo Fornecedores + Pedidos de Compra (etapa 7)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Schema

- [x] **T1: Migration + Models** (M) — *concluída em 22/09/2026*
  - Descrição: `Fornecedor.cs`, `PedidoCompra.cs`, `PedidoCompraItem.cs` (espelhando `Cliente`/`Pedido`/`PedidoItem`); `EstoqueMovimentacao.cs` ganha `PedidoCompraId`/`PedidoCompra`; mapeamento no `ErpPortfolioDbContext` (tabelas, colunas snake_case, índices únicos `ix_fornecedores_documento` e `(pedido_compra_id, produto_id)`, FKs); `dotnet ef migrations add AdicionaFornecedoresEPedidosCompra`.
  - Aceite: `dotnet ef database update` aplica sem erro; `dotnet build` 0 avisos.
  - Verificar: `dotnet build ErpPortfolio.slnx -c Release`.
  - Resultado: migration `20260923020209_AdicionaFornecedoresEPedidosCompra` gerada e aplicada no banco de dev; 2 tabelas novas + coluna `pedido_compra_id` em `estoque_movimentacoes` (nullable, FK restrict). `dotnet build` 0 avisos.
  - Dependências: nenhuma.
  - Arquivos: `Models/Fornecedor.cs`, `Models/PedidoCompra.cs`, `Models/PedidoCompraItem.cs`, `Models/EstoqueMovimentacao.cs`, `Data/ErpPortfolioDbContext.cs`, `Migrations/*`.

- [x] **T2: Fornecedor backend** (M) — *concluída em 22/09/2026*
  - Descrição: `FornecedorCriacaoDto`, `FornecedorAtualizacaoDto`, `FornecedorFiltroDto`, `FornecedorRespostaDto` (espelho dos DTOs de Cliente); `IFornecedorService`/`FornecedorService` (espelho de `ClienteService`, com `GarantirDocumentoUnicoAsync` num índice próprio); `FornecedoresController` (espelho de `ClientesController`); registro no DI (`Program.cs`).
  - Aceite: CRUD completo funcionando via API real (criar, listar com filtro, editar, inativar, documento duplicado bloqueado).
  - Verificar: `dotnet build`; teste manual via `curl`/Swagger contra a API rodando.
  - Resultado: CRUD testado via API real (criar, obter, documento duplicado → 409, editar, listar com filtro por nome, inativar → 204, `ativo=false` confirmado). Dado de teste (`ZZT Fornecedor Teste`) apagado via SQL direto (sem endpoint de exclusão física, mesmo padrão do Cliente).
  - Dependências: T1.
  - Arquivos: `DTOs/Fornecedor*.cs`, `Services/IFornecedorService.cs`, `Services/FornecedorService.cs`, `Controllers/FornecedoresController.cs`, `Program.cs`.

- [x] **T3: EstoqueService — Receber/EstornarCompra** (M) — *concluída em 22/09/2026*
  - Descrição: `IEstoqueService`/`EstoqueService` ganham `Receber(pedidoCompraId, itens)` (Entrada por item, motivo `"Compra pedido #N"`, sem checagem de saldo) e `EstornarCompraAsync(pedidoCompraId, itens, cancelamento)` (checa saldo suficiente por item ANTES de enfileirar qualquer Saída — PC7 — e lança `DadoInvalidoException` listando os itens sem saldo se faltar); `MovimentacaoRespostaDto` ganha `PedidoCompraId`.
  - Aceite: compila; comportamento (entrada gerada, estorno bloqueado por saldo insuficiente) verificado por E2E real na T4, junto do fluxo completo do Pedido de Compra — mesmo padrão do projeto, que não tem xUnit tocando banco (`BaixarAsync`/`Estornar` do Pedido de Venda também só são verificados por E2E).
  - Verificar: `dotnet build`.
  - Resultado: build limpo; comportamento validado na T4 (ver abaixo).
  - Dependências: T1.
  - Arquivos: `Services/IEstoqueService.cs`, `Services/EstoqueService.cs`, `DTOs/MovimentacaoRespostaDto.cs`.

### Checkpoint 1: schema + estoque prontos
- [x] `dotnet build` 0 avisos, `dotnet test` verde (150/150)

## Fase 2: Backend Pedido de Compra

- [x] **T4: PedidoCompra backend** (L) — *concluída em 22/09/2026*
  - Descrição: `PedidoCompraCriacaoDto`, `PedidoCompraItemEntradaDto`, `PedidoCompraRespostaDto`, `PedidoCompraResumoDto`, `PedidoCompraFiltroDto` (espelho dos DTOs de Pedido, sem `FormaPagamento`); `IPedidoCompraService`/`PedidoCompraService` reaproveitando `TransicoesPedido` e `CalculoPedido` — criar/editar rascunho, confirmar (PC5/PC6: fornecedor e produtos ativos, chama `Receber`, atualiza `Produto.Custo` por item), cancelar (PC7/PC8: chama `EstornarCompraAsync`, idempotente); `PedidosCompraController` (espelho de `PedidosController`, sem corpo no `/confirmar`).
  - Aceite: E2E real cobrindo confirmar (entrada gerada + custo atualizado), cancelar com saldo ok (estorna), cancelar sem saldo suficiente (bloqueado, 400) e cancelar duas vezes (idempotente) — mesmo padrão de verificação do `PedidoService`.
  - Verificar: `dotnet build`, E2E manual contra a API rodando.
  - Resultado: fluxo completo testado contra a API real — (1) item nasce com preço = Custo do produto; (2) confirmar gera Entrada ligada ao pedido (`"Compra pedido #N"`, saldo sobe) e **atualiza o Custo do produto para o preço congelado do item**, mesmo simulando uma mudança de custo entre criar o item e confirmar; (3) cancelar com saldo intacto gera Saída de estorno (`"Estorno cancelamento pedido de compra #N"`), saldo volta a 0; (4) cancelar de novo é idempotente (204, sem duplicar movimentação); (5) **caso novo (PC7)**: confirmar uma segunda compra, vender parte do saldo via Pedido de Venda, tentar cancelar a compra → bloqueado (400, "Estoque insuficiente para estornar"), nada alterado. `dotnet build` 0 avisos, `dotnet test` 150/150. Todos os dados de teste (fornecedor, produto, cliente, 2 pedidos de compra, 1 pedido de venda + parcela) apagados via SQL direto ao final.
  - Dependências: T1, T3.
  - Arquivos: `DTOs/PedidoCompra*.cs`, `Services/IPedidoCompraService.cs`, `Services/PedidoCompraService.cs`, `Controllers/PedidosCompraController.cs`, `Program.cs`.

### Checkpoint 2 (CP1 do plano): API pronta
- [x] Critérios 1 a 7 da spec (backend) verificados por E2E na API real
- [x] `dotnet test` verde (150/150), `dotnet build` sem avisos
- [x] Dados reais intactos; dados de teste apagados

## Fase 3: Telas

- [x] **T5: Tela Fornecedores** (M) — *concluída em 23/09/2026*
  - Descrição: `types/fornecedor.ts`, `api/fornecedoresApi.ts`, `hooks/useFornecedores.ts`, `schemas/fornecedorSchema.ts`, `pages/Fornecedores/FornecedorFormDrawer.tsx`, `pages/Fornecedores/FornecedoresListaPage.tsx` (espelho de Clientes, reaproveitando `clientes.css`); rota `/fornecedores` e item de menu (ícone `ShopOutlined`), logo depois de Clientes.
  - Aceite: lista, cria, edita, inativa fornecedor pela tela.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Resultado: espelho fiel do módulo de Clientes (mesmos componentes de filtro, drawer, validação Zod); `tsc -b` e `oxlint` limpos. Verificação visual/Playwright não feita nesta sessão (sem navegador disponível, mesma ressalva dos módulos anteriores) — a API foi validada de ponta a ponta na T2/T4.
  - Dependências: T2.
  - Arquivos: `types/fornecedor.ts`, `api/fornecedoresApi.ts`, `hooks/useFornecedores.ts`, `schemas/fornecedorSchema.ts`, `pages/Fornecedores/*`, `App.tsx`.

- [x] **T6: Tela Pedidos de Compra** (L) — *concluída em 23/09/2026*
  - Descrição: `types/pedidoCompra.ts`, `api/pedidosCompraApi.ts`, `hooks/usePedidosCompra.ts`, `schemas/pedidoCompraSchema.ts`, `pages/PedidosCompra/PedidoCompraPage.tsx`, `pages/PedidosCompra/PedidosCompraListaPage.tsx`, `pages/PedidosCompra/ItensPedidoCompraTabela.tsx` (espelho de Pedidos, reaproveitando `pedido.css`); `components/SelecaoFornecedor.tsx` (espelho de `SelecaoCliente`); `useBuscaFornecedores` em `useBuscaCadastros.ts`; rotas `/pedidos-compra`, `/pedidos-compra/novo`, `/pedidos-compra/:id` e item de menu (ícone `ShoppingOutlined`), logo depois de Pedidos.
  - Aceite: cria rascunho com itens, edita, confirma, cancela pela tela.
  - Verificar: `npx tsc -b`, `npx oxlint src`.
  - Resultado: espelho fiel do módulo de Pedidos, sem forma de pagamento nem modal de parcelas (confirmar/cancelar usam `modal.confirm` simples, como o cancelar do Pedido de Venda). `SelecaoProduto` ganhou a prop `campoPreco` (`'precoVenda' | 'custo'`) para mostrar o Custo do produto na busca em vez do preço de venda — pequena extensão do componente existente em vez de duplicá-lo. `tsc -b`, `oxlint` e `npm run build` limpos (bundle sem crescer, nenhuma dependência nova). Verificação visual/Playwright não feita nesta sessão; API já validada de ponta a ponta na T4.
  - Dependências: T4.
  - Arquivos: `types/pedidoCompra.ts`, `api/pedidosCompraApi.ts`, `hooks/usePedidosCompra.ts`, `hooks/useBuscaCadastros.ts`, `schemas/pedidoCompraSchema.ts`, `components/SelecaoFornecedor.tsx`, `components/SelecaoProduto.tsx`, `pages/PedidosCompra/*`, `App.tsx`.

- [x] **T7: Estoque mostra "Compra #N"** (S) — *concluída em 23/09/2026*
  - Descrição: componente(s) do extrato de estoque passam a exibir a origem "Compra #N" quando `pedidoCompraId` estiver preenchido (hoje só mostram "Venda #N" via `pedidoId`).
  - Aceite: uma entrada gerada por um pedido de compra confirmado aparece no extrato com o rótulo certo.
  - Verificar: `npx tsc -b`, `npx oxlint src`; conferência manual contra dado real de teste.
  - Resultado: `types/estoque.ts` ganhou `pedidoCompraId`; `MovimentacoesDrawer.tsx` ganhou uma coluna "Origem" (Venda #N / Compra #N / Manual) — mais escaneável que só o texto livre do Motivo (que já trazia "Compra pedido #N" desde a T3/T4). De quebra, corrigida uma colisão de `rowKey` (duas movimentações de compra no mesmo instante caíam na mesma chave "manual"). `tsc -b` e `oxlint` limpos; contrato conferido contra a resposta real da API na T4 (`pedidoCompraId` no formato esperado).
  - Dependências: T3, T6 (tela de estoque já existe; só ajusta a exibição).
  - Arquivos: `types/estoque.ts`, `pages/Estoque/MovimentacoesDrawer.tsx`.

### Checkpoint 3 (CP2 do plano): telas prontas
- [x] `tsc`, `oxlint`, `npm run build`, `dotnet build`, `dotnet test` limpos

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
