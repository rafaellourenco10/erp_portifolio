# Spec: Módulo Fornecedores + Pedidos de Compra (etapa 7)

> Status: **implementada e testada em 23/09/2026** (T1 a T8 do plano, ver `tasks/todo.md`). Os 8 critérios de sucesso abaixo foram conferidos um a um contra a API real, com dados de teste isolados (apagados ao final) e os dados reais intactos. Backend testado de ponta a ponta; **sem verificação visual/Playwright** das telas de Fornecedores e Pedidos de Compra — sem ferramenta de navegador disponível nesta sessão.

## Objetivo

Cadastro de Fornecedores e um Pedido de Compra (fornecedor + itens) que, ao ser confirmado, dá entrada automática no estoque e atualiza o custo dos produtos comprados — fechando o lado "compra" do estoque, que hoje só tem entrada manual.

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** combinado com o Rafael em 22/09/2026, como a etapa depois do Dashboard; Estoque e Produtos (com campo `Custo`) já existem e sustentam o módulo.
- **Sucesso:** cadastrar um fornecedor, montar um pedido de compra com itens, confirmar dá entrada de estoque por item (ligada ao pedido) e atualiza o custo dos produtos comprados; cancelar um pedido confirmado estorna a entrada (vira saída), bloqueando se o saldo já foi consumido.

### Dentro do escopo
CRUD de Fornecedor (mesmos campos e validações do Cliente). Pedido de Compra: cabeçalho (fornecedor, status Rascunho/Confirmado/Cancelado, desconto) + itens (produto, quantidade, preço unitário = custo do produto, desconto do item), total calculado no servidor — reaproveitando a mesma fórmula do Pedido de Venda. Confirmar dá entrada em estoque e atualiza `Produto.Custo`. Extrato de estoque passa a mostrar também a origem "compra" (hoje só mostra "venda").

### Fora do escopo (entram depois)
**Forma de pagamento e Contas a Pagar** — o pedido de compra não tem forma de pagamento nem gera parcela; isso só faz sentido quando existir um módulo de Contas a Pagar (mesmo raciocínio do Pedido de Venda com Contas a Receber, que veio numa etapa depois). Edição do pedido depois de confirmado. Recebimento parcial de mercadoria (o pedido é recebido inteiro, de uma vez, ao confirmar). Reverter `Produto.Custo` ao cancelar um pedido de compra (o custo fica com o último valor pago; ver PC7).

## Decisões já tomadas (com o Rafael, 22/09/2026)

| Tema | Decisão |
|---|---|
| Campos do Fornecedor | Mesmo padrão do Cliente: Nome, Documento (CPF/CNPJ), Email opcional, Telefone opcional, Cidade, UF, Ativo. |
| Entrada em estoque | Confirmar o Pedido de Compra dá entrada automática por item, ligada ao pedido de compra (mesma ideia da saída automática do Pedido de Venda). |
| Preço do item | Nasce com o `Custo` atual do produto (editável no rascunho); confirmar atualiza `Produto.Custo` com o valor pago em cada item — mantém o custo sempre no preço da última compra. |
| Status do pedido | Reaproveita o mesmo enum e as mesmas regras de transição do Pedido de Venda (`StatusPedido`, `TransicoesPedido`) — não é um fluxo diferente, só um cabeçalho diferente. |
| Cálculo do total | Reaproveita `CalculoPedido` (mesma fórmula de subtotal/total/desconto do Pedido de Venda) — por isso o Pedido de Compra também tem desconto por item e por pedido, mesmo sem ser um requisito novo: é o preço de reusar o cálculo já testado em vez de escrever um paralelo. |

## Regras de negócio

### Fornecedor (F)

| # | Regra |
|---|---|
| F1 | Mesmos campos e validações do Cliente: Nome (3-150), Documento (CPF/CNPJ, com ou sem máscara), Email opcional, Telefone opcional, Cidade (2-100), UF (sigla válida). |
| F2 | Documento único **entre fornecedores** (índice próprio, independente do de clientes — um mesmo CNPJ pode ser cliente e fornecedor). Conflito com fornecedor ativo bloqueia; conflito com inativo sugere reativar (mesma mensagem do Cliente). |
| F3 | Inativar não apaga; só bloqueia novo pedido de compra para esse fornecedor (mesma regra do Cliente/Pedido). |

### Pedido de Compra (PC)

| # | Regra |
|---|---|
| PC1 | Rascunho → pode editar, confirmar e cancelar. Confirmado → só cancelar. Cancelado → nada (estado final). Reaproveita `TransicoesPedido`. |
| PC2 | Preço do item = `Custo` do produto no momento em que o item é adicionado; fica congelado no item, editável enquanto o pedido é rascunho. |
| PC3 | Total = mesma fórmula do Pedido de Venda (soma dos subtotais com desconto do item, menos desconto do pedido), via `CalculoPedido`. |
| PC4 | 1 a 100 itens, produto não repetido no mesmo pedido, descontos de 0 a 100 com até 2 casas — mesmas regras do Pedido de Venda (R4/R8). |
| PC5 | Confirmar exige fornecedor ativo e todos os produtos do pedido ativos (sem exigir forma de pagamento, que não existe aqui). |
| PC6 | Confirmar gera uma **Entrada** de estoque por item, ligada ao pedido de compra (`EstoqueMovimentacao.PedidoCompraId`), com motivo `"Compra pedido #N"`; e atualiza `Produto.Custo` de cada produto do pedido para o preço pago no item. |
| PC7 | Cancelar um pedido que estava Confirmado exige saldo suficiente em **cada item** (o que já foi vendido/consumido não pode ser estornado) — senão bloqueia o cancelamento com a lista dos itens sem saldo; se passar, gera uma **Saída** de estorno por item, ligada ao pedido de compra, motivo `"Estorno cancelamento pedido de compra #N"`. O `Produto.Custo` não volta ao valor anterior (reverter seria ambíguo se houve outra compra depois). |
| PC8 | Cancelar um pedido que já está Cancelado é sucesso (idempotente), igual ao Pedido de Venda (R7). |

## Tech stack

Igual aos módulos anteriores: ASP.NET Core (.NET 10) + EF Core + PostgreSQL 18 no back; React 19 + Vite + TypeScript + Ant Design 6 + TanStack Query no front. Nenhuma dependência nova.

## Modelo de dados

Duas tabelas novas + uma coluna nova:

- **`public.fornecedores`** — id, nome, documento, email, telefone, cidade, uf, ativo, data_cadastro. Índice único `ix_fornecedores_documento`. Espelho exato de `public.clientes`.
- **`public.pedidos_compra`** — id, fornecedor_id (FK → fornecedores), data_pedido, status, desconto_percentual, valor_total. Espelho de `public.pedidos`, sem `forma_pagamento`.
- **`public.pedido_compra_itens`** — id, pedido_compra_id (FK → pedidos_compra), produto_id (FK → produtos), quantidade, preco_unitario, desconto_percentual. Índice único `(pedido_compra_id, produto_id)`, espelho de `pedido_itens`.
- **`public.estoque_movimentacoes`** ganha a coluna **`pedido_compra_id`** (nullable, FK → pedidos_compra), irmã da `pedido_id` já existente. Uma movimentação tem no máximo uma das duas preenchidas (nunca as duas).

## API

Fornecedores — espelho exato de `/api/clientes`:

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/fornecedores` | Listagem paginada (filtros: nome, UFs, ativo) |
| GET | `/api/fornecedores/{id}` | Consulta por id |
| POST | `/api/fornecedores` | Inclusão |
| PUT | `/api/fornecedores/{id}` | Edição |
| PATCH | `/api/fornecedores/{id}/inativar` | Inativação |

Pedidos de Compra — espelho de `/api/pedidos`, sem o corpo de confirmar (não há parcelas):

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/pedidos-compra` | Listagem paginada (filtros: busca por número/fornecedor, status) |
| GET | `/api/pedidos-compra/{id}` | Consulta por id, com fornecedor e itens |
| POST | `/api/pedidos-compra` | Cria um rascunho |
| PUT | `/api/pedidos-compra/{id}` | Edita o rascunho (fornecedor, itens, desconto) |
| PATCH | `/api/pedidos-compra/{id}/confirmar` | Rascunho → Confirmado (PC5/PC6); sem corpo |
| PATCH | `/api/pedidos-compra/{id}/cancelar` | Rascunho/Confirmado → Cancelado (PC7/PC8) |

`MovimentacaoRespostaDto` (extrato de estoque) ganha o campo `pedidoCompraId`, ao lado do `pedidoId` já existente.

## Telas

- **`/fornecedores`** — lista + drawer de cadastro/edição, espelho de `/clientes`. Item **Fornecedores** no menu, logo depois de Clientes, dentro de "Gestão Comercial".
- **`/pedidos-compra`**, **`/pedidos-compra/novo`**, **`/pedidos-compra/:id`** — lista + formulário com tabela de itens, espelho de `/pedidos`. Item **Pedidos de Compra** no menu, logo depois de Pedidos.
- **Estoque**: o extrato de movimentações passa a exibir "Compra #N" para as linhas com `pedidoCompraId`, do mesmo jeito que já exibe "Venda #N" para `pedidoId`.

## Commands

```
# Backend (na raiz)
dotnet build ErpPortfolio.slnx -c Release
dotnet ef migrations add NomeDaMigration --project backend/ErpPortfolio.Api
dotnet run --project backend/ErpPortfolio.Api --launch-profile http

# Frontend (em frontend/erp-portfolio-web)
npm run dev
npx tsc -b
npx oxlint src
npm run build

# Testes automatizados
dotnet test backend/ErpPortfolio.Tests
```

## Project structure (só o que é novo/alterado)

```
backend/ErpPortfolio.Api/
  Models/            Fornecedor.cs, PedidoCompra.cs, PedidoCompraItem.cs
                      EstoqueMovimentacao.cs  # + PedidoCompraId/PedidoCompra
  DTOs/               FornecedorCriacaoDto.cs, FornecedorAtualizacaoDto.cs, FornecedorFiltroDto.cs, FornecedorRespostaDto.cs
                      PedidoCompraCriacaoDto.cs, PedidoCompraItemEntradaDto.cs, PedidoCompraRespostaDto.cs, PedidoCompraResumoDto.cs, PedidoCompraFiltroDto.cs
                      MovimentacaoRespostaDto.cs  # + PedidoCompraId
  Services/           IFornecedorService.cs / FornecedorService.cs
                      IPedidoCompraService.cs / PedidoCompraService.cs
                      IEstoqueService.cs / EstoqueService.cs  # + ReceberAsync / EstornarCompra
  Controllers/        FornecedoresController.cs, PedidosCompraController.cs
  Data/               ErpPortfolioDbContext.cs  # + DbSets e mapeamento das 2 tabelas novas + coluna nova
  Migrations/         <nova migration>
backend/ErpPortfolio.Tests/  PedidoCompraServiceTests.cs (ou equivalente) — confirmar/cancelar, custo atualizado, saldo insuficiente ao estornar
frontend/erp-portfolio-web/src/
  api/                fornecedoresApi.ts, pedidosCompraApi.ts
  hooks/              useFornecedores.ts, usePedidosCompra.ts
  types/              fornecedor.ts, pedidoCompra.ts
  pages/Fornecedores/ FornecedorFormDrawer.tsx, FornecedoresListaPage.tsx
  pages/PedidosCompra/ PedidoCompraPage.tsx, PedidosCompraListaPage.tsx, ItensPedidoCompraTabela.tsx
  App.tsx             # rotas e itens de menu novos
```

## Code style

Igual ao restante do projeto: cabeçalho obrigatório em todo arquivo C#/TS, nomes em português, mensagens em português.

## Testing strategy

1. **Unitário (xUnit):** confirmar gera entrada por item e atualiza `Produto.Custo`; cancelar de Confirmado com saldo suficiente gera saída de estorno; cancelar de Confirmado sem saldo suficiente bloqueia (DadoInvalidoException) e não altera nada.
2. **API ponta a ponta (instância temporária):** fluxo completo — cria fornecedor, cria pedido de compra, confirma (saldo sobe, custo do produto muda), cancela (saldo volta), com dados de teste isolados e apagados ao final.
3. **Tela:** revisão de código (sem navegador nesta sessão, mesma limitação dos módulos anteriores) — recomendo verificação visual do Rafael antes de considerar pronto de verdade.
4. **Sempre:** `dotnet build` 0 avisos, `tsc -b`/`oxlint` sem apontamentos.

## Boundaries

- **Sempre:** todo cálculo (total, custo atualizado) no servidor; reaproveitar `StatusPedido`, `TransicoesPedido` e `CalculoPedido` em vez de duplicar; testar em instância temporária e limpar os dados de teste; atualizar README e graphify ao terminar; `git commit` só quando o Rafael pedir.
- **Perguntar antes:** qualquer dependência nova; forma de pagamento/Contas a Pagar no Pedido de Compra; recebimento parcial de mercadoria.
- **Nunca:** editar um pedido de compra depois de confirmado; reverter `Produto.Custo` automaticamente ao cancelar; commitar segredos; forçar push.

## Success criteria (testáveis)

Conferidos um a um em 23/09/2026 contra a API real (instância local, dados de teste `ZZT…` apagados ao final; dados reais intactos), com `dotnet build -c Release` (0 avisos), `dotnet test` (150/150), `tsc -b`/`oxlint` limpos e `npm run build` (bundle sem crescer).

1. ✅ Cadastrar, editar e inativar um fornecedor funciona igual ao Cliente, com documento único **só entre fornecedores** — testado: criar, obter, documento duplicado (409), editar, listar com filtro, inativar (204, `ativo=false`).
2. ✅ Criar um pedido de compra em rascunho com 1+ itens; o preço de cada item nasce igual ao `Custo` atual do produto e é editável enquanto rascunho — confirmado com item nascendo em R$ 10,00 (custo do produto de teste).
3. ✅ Confirmar um pedido de compra: gera uma Entrada de estoque por item ligada ao pedido (visível no extrato como "Compra #N"), o saldo do produto sobe, e `Produto.Custo` passa a ser o preço pago no item — testado simulando uma mudança de custo (10 → 15) entre montar o item e confirmar: confirmar devolveu o custo para 10 (o preço congelado no item), não manteve o 15.
4. ✅ Confirmar com fornecedor ou algum produto inativo é bloqueado (400), sem gravar nada — testado nos dois casos separadamente (400 em `FornecedorId` e em `Itens`), pedido permaneceu Rascunho.
5. ✅ Cancelar um pedido confirmado com saldo intacto: gera Saída de estorno por item, saldo volta ao valor de antes da compra.
6. ✅ Cancelar um pedido confirmado cujo saldo já foi parcialmente consumido (por uma venda, por exemplo) é bloqueado (400), sem gravar nada e sem mexer no saldo — testado vendendo 3 de um saldo de 5 e tentando cancelar a compra: bloqueado com a mensagem do saldo insuficiente, saldo e status inalterados.
7. ✅ Cancelar duas vezes o mesmo pedido é idempotente (a segunda chamada também retorna sucesso, sem gerar movimentação duplicada).
8. ✅ Menu e rotas novas (Fornecedores, Pedidos de Compra) funcionam e não quebram nenhuma rota existente — `tsc -b`/`oxlint`/`npm run build` limpos; verificação visual/Playwright não feita nesta sessão (sem navegador disponível).

## Open questions

Nenhuma em aberto — as três decisões da primeira versão foram fechadas com o Rafael em 22/09/2026 (ver "Decisões já tomadas"). Limites conhecidos, aceitos de propósito:

- **Sem forma de pagamento / Contas a Pagar**: entra quando esse módulo existir (mesmo caminho que Pedidos → Contas a Receber).
- **Sem recebimento parcial**: o pedido de compra é recebido inteiro ao confirmar; recebimento em partes é uma extensão natural (viraria um novo status ou um saldo "a receber" por item).
- **Custo não reverte ao cancelar**: aceito porque reverter exigiria guardar o custo anterior por item, e o valor "correto" depois de outra compra no meio é ambíguo.
