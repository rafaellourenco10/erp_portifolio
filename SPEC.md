# Spec: Módulo Estoque (etapa 4)

> Status: **implementada e testada em 22/09/2026** (T1 a T10 do plano, ver `tasks/todo.md`). Os 10 critérios de sucesso abaixo foram conferidos um a um contra o código atual. A tela foi verificada por revisão de código (tipos, lint, build), **sem verificação visual/Playwright** — sem ferramenta de navegador disponível nesta sessão. Ao mudar uma decisão depois disso, atualize esta spec **antes** do código.

## Objetivo

Controlar o **saldo de estoque** de cada produto no Ambition ERP, através de um histórico de **movimentações** (entradas e saídas), com baixa automática ao confirmar um pedido e devolução automática ao cancelar um pedido confirmado.

- **Quem usa:** o dono do ERP de portfólio (sem login, sem múltiplos depósitos).
- **Por que agora:** Pedidos existe, mas confirmar uma venda hoje não mexe em estoque nenhum — não dá para saber o que ainda tem disponível.
- **Sucesso:** cada produto mostra um saldo correto (soma das movimentações), dá para lançar uma entrada manual (compra), confirmar um pedido baixa o estoque dos itens automaticamente e recusa se faltar saldo, e cancelar um pedido confirmado devolve a quantidade.

### Dentro do escopo
Tabela de movimentações (entrada manual, saída automática por venda, entrada automática por estorno de cancelamento), tela de estoque por produto com saldo, tela/drawer de nova entrada manual, extrato de movimentações por produto, bloqueio de confirmação de pedido sem saldo suficiente.

### Fora do escopo (entram depois)
Fornecedores, pedido de compra formal, múltiplos depósitos/armazéns, custo médio/CMV, inventário por contagem física, saída manual (perda/quebra/ajuste negativo), estoque mínimo/alerta de ruptura.

## Decisões já tomadas (com o Rafael, 22/09/2026)

| Tema | Decisão |
|---|---|
| Modelo de dados | **Histórico de movimentações** (não só um saldo no produto). O saldo é sempre a soma (Entrada − Saída), nunca gravado direto. |
| Estoque insuficiente ao confirmar | **Bloqueia** (400 no campo, como cliente/produto inativo em Pedidos — R6). Pedido continua rascunho. |
| Cancelar pedido Confirmado | **Devolve automaticamente**: gera uma entrada de estorno por item. Cancelar um Rascunho não gera movimentação (nunca teve saída). |
| Entrada manual | **Produto + quantidade + motivo (texto livre, opcional)**. Sem fornecedor por enquanto. |
| Saída manual (perda/quebra) | **Fora do escopo** — só existe saída automática por venda confirmada. |

## Regras de negócio

| # | Regra |
|---|---|
| E1 | Toda mudança de estoque gera um registro em `estoque_movimentacoes`; o saldo de um produto é `Σ Entrada − Σ Saída` das suas movimentações, calculado na hora (nunca gravado). |
| E2 | Confirmar um pedido (`PATCH /pedidos/{id}/confirmar`) gera uma **Saída** por item, com a quantidade do item e referência ao pedido. Se **qualquer** item não tiver saldo suficiente, a confirmação inteira é recusada (**400**, campo `Itens`) e **nenhuma** movimentação é gravada — checagem e gravação na mesma transação. |
| E3 | Cancelar um pedido que estava **Confirmado** gera uma **Entrada** de estorno por item (mesma quantidade, mesma referência ao pedido). Cancelar um pedido que estava em **Rascunho** não gera movimentação nenhuma. |
| E4 | Entrada manual (`POST /api/estoque/entradas`) exige produto **ativo** e quantidade **> 0**; motivo é opcional (até 200 caracteres). |
| E5 | Quantidade da movimentação segue a mesma faixa de Pedidos: `0,001` a `999.999,999`, até 3 casas decimais. |
| E6 | Editar um rascunho (PUT) **não** mexe em estoque — só Confirmar consome e só Cancelar-de-Confirmado devolve. |
| E7 | Um produto **inativo** pode ter saldo e aparecer no extrato normalmente; só a **entrada manual** exige produto ativo (E4). Baixa/estorno por pedido usa o produto do item, ativo ou não (mesma regra de Pedidos: inativar depois não trava o histórico). |

## Tech stack

Igual aos módulos anteriores: ASP.NET Core (.NET 10) + EF Core + PostgreSQL 18 (Docker, porta 5433) no back; React 19 + Vite + TypeScript + Ant Design 6 + TanStack Query + react-hook-form + Zod + React Router no front. Nenhuma dependência nova.

## Modelo de dados (migration só **adiciona** tabela)

`estoque_movimentacoes`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK, identity |
| `produto_id` | integer | FK `produtos` (RESTRICT), índice `ix_estoque_movimentacoes_produto_id` |
| `tipo` | varchar(20) | `Entrada` ou `Saida` (texto, como `status` em pedidos) |
| `quantidade` | numeric(12,3) | `CHECK` > 0 (o tipo é que define entrada/saída; quantidade sempre positiva) |
| `motivo` | varchar(200) | opcional; automático nas movimentações geradas por pedido (ex.: `"Venda pedido #123"`, `"Estorno cancelamento pedido #123"`) |
| `pedido_id` | integer | FK `pedidos` (RESTRICT), **opcional** (nulo em entrada manual), índice `ix_estoque_movimentacoes_pedido_id` |
| `data_movimentacao` | timestamptz | UTC, padrão `now()`, índice `ix_estoque_movimentacoes_data_movimentacao` |

Saldo do produto = query agregada (`SUM` condicional por `tipo`), não é coluna própria — mesmo padrão do subtotal do item de pedido ("não é gravado, é calculado na hora").

## API

Novo `EstoqueController` (`/api/estoque`):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/estoque?busca=&pagina=&tamanhoPagina=` | Lista paginada de produtos com saldo atual (nome ou SKU) | 200, 400 |
| GET | `/estoque/{produtoId}/movimentacoes?pagina=&tamanhoPagina=` | Extrato do produto, mais recente primeiro | 200, 404 |
| POST | `/estoque/entradas` | Lança uma entrada manual (E4) | 201, 400, 404 |

`PedidosController` sem rota nova — `PATCH /pedidos/{id}/confirmar` e `/cancelar` passam a chamar `IEstoqueService` internamente (E2, E3); resposta de erro de estoque insuficiente usa o mesmo formato 400 já usado pelas outras validações de confirmar.

- Corpo do POST `/estoque/entradas`: `{ produtoId, quantidade, motivo? }`.
- Resposta da listagem: `{ produtoId, produtoNome, sku, unidade, saldo }` paginado (mesmo envelope `ResultadoPaginadoDto` dos outros módulos).
- Resposta do extrato: `{ tipo, quantidade, motivo, pedidoId?, dataMovimentacao }` paginado.

## Telas

- **`/estoque`** (item **Estoque** no menu, em Gestão Comercial): tabela com Produto, SKU, Unidade e **Saldo**; busca por nome/SKU (mesmo padrão de busca dos outros módulos, sem o painel Filtrar completo — não há status aqui); botão **Nova entrada** abre um drawer (produto por seleção com busca no servidor — reaproveita `SelecaoProduto`, quantidade, motivo opcional); ação **Ver movimentações** por linha abre um drawer com o extrato paginado daquele produto (tipo, quantidade, motivo/origem, data), mais recente primeiro.
- Saldo negativo (não deveria acontecer dado E2, mas fica visível se acontecer por dado antigo) aparece em vermelho, mesmo padrão da margem negativa em Produtos.
- Sem alteração visível na tela de Pedidos além do próprio efeito colateral: confirmar sem saldo mostra erro 400 no resumo do pedido (mesmo padrão dos outros erros de confirmar, ex. cliente inativo).

## Commands

```
# Backend (na raiz)
dotnet build ErpPortfolio.slnx
dotnet run --project backend/ErpPortfolio.Api --launch-profile http        # API em http://localhost:5065
dotnet ef migrations add NomeDaMigration --project backend/ErpPortfolio.Api -o Data/Migrations
dotnet ef database update --project backend/ErpPortfolio.Api

# Frontend (em frontend/erp-portfolio-web)
npm run dev          # http://localhost:5173
npx tsc -b
npx oxlint src
npm run build

# Testes automatizados
dotnet test backend/ErpPortfolio.Tests
```

## Project structure (só o que é novo/alterado)

```
backend/ErpPortfolio.Api/
  Models/            EstoqueMovimentacao.cs, TipoMovimentacao.cs
  DTOs/               EstoqueEntradaDto.cs, EstoqueResumoDto.cs, EstoqueFiltroDto.cs, MovimentacaoRespostaDto.cs
  Services/          IEstoqueService.cs, EstoqueService.cs
  Controllers/       EstoqueController.cs
  Data/Migrations/   <data>_CriacaoTabelaEstoqueMovimentacoes.cs
  Services/PedidoService.cs        # alterado: Confirmar chama BaixarEstoque, Cancelar chama EstornarEstoque
backend/ErpPortfolio.Tests/        EstoqueServiceTests.cs (saldo, baixa, estorno, bloqueio)
frontend/erp-portfolio-web/src/
  api/estoqueApi.ts   hooks/useEstoque.ts   types/estoque.ts   schemas/estoqueEntradaSchema.ts
  pages/Estoque/EstoqueListaPage.tsx, EntradaEstoqueDrawer.tsx, MovimentacoesDrawer.tsx
```

## Code style

Igual ao restante do projeto: cabeçalho obrigatório em todo arquivo C#/TS (nome, versão, data, descrição, banco/tabelas/fontes, histórico), nomes em português, mensagens de erro em português.

## Testing strategy

1. **Unitário (xUnit):** saldo calculado corretamente a partir de uma lista de movimentações (casos: só entradas, só saídas, misto, saldo zero); confirmar com saldo suficiente gera as saídas certas; confirmar com saldo insuficiente **não grava nada** e retorna erro; cancelar um Confirmado gera as entradas de estorno certas; cancelar um Rascunho não gera nada.
2. **API ponta a ponta (instância temporária):** entrada manual (201), entrada em produto inativo (400), extrato paginado, confirmar pedido baixa saldo dos itens, confirmar sem saldo suficiente retorna 400 e o pedido continua rascunho (e a checagem foi feita sem gravar nada), cancelar confirmado devolve o saldo, cancelar rascunho não mexe em saldo, listagem de estoque com busca por nome/SKU.
3. **Tela (Playwright):** lista de estoque com saldo, lançar entrada manual e ver o saldo mudar, abrir extrato e ver a movimentação, tentar confirmar um pedido sem saldo e ver o erro, confirmar com saldo e ver o saldo cair, cancelar confirmado e ver o saldo voltar, celular sem rolagem horizontal.
4. **Sempre:** `dotnet build` com 0 avisos, `tsc -b` e `oxlint` sem apontamentos; nenhum dado real alterado (cliente, produto, categoria, pedido existentes intactos).

## Boundaries

- **Sempre:** toda mudança de saldo passa por uma movimentação (nunca um `UPDATE` direto de um campo saldo); checagem de saldo suficiente e gravação da saída na **mesma transação** (E2); cabeçalho em cada arquivo; backup do banco antes de migration; testar em instância temporária e limpar os dados de teste; atualizar README e graphify ao terminar; `git commit` só quando o Rafael pedir.
- **Perguntar antes:** dependência nova, mudar tabela existente (`pedidos`, `produtos`, etc.), saída manual (perda/ajuste), estoque mínimo/alerta, múltiplos depósitos, qualquer forma de estoque negativo permitido.
- **Nunca:** gravar saldo como campo direto no produto, deixar confirmar um pedido sem saldo suficiente, excluir uma movimentação (histórico é definitivo, como o pedido), commitar segredos, forçar push.

## Success criteria (testáveis)

Conferidos um a um em 22/09/2026 contra o código final (T10), com `dotnet build -c Release` (0 avisos), `dotnet test` (124/124), `tsc -b` e `oxlint` limpos. Evidência: testes unitários (`EstoqueCalculoTests`, `ModeloEstoqueTests`) e scripts de API ponta a ponta rodados manualmente numa instância temporária (porta 5099), com dados de teste isolados (produtos/clientes `ZZT…`, SKUs `6700000x`) e os dados reais conferidos idênticos ao final de cada rodada.

1. ✅ Saldo de um produto sem nenhuma movimentação é **0** — `GET /estoque` com produto sem movimentação (T4).
2. ✅ Uma entrada manual de 10 unidades faz o saldo do produto virar **10** — `POST /estoque/entradas` (T5).
3. ✅ Confirmar um pedido com 1 item de 3 unidades (saldo 10) baixa o saldo para **7**, e gera uma movimentação `Saida` de 3 referenciando o pedido — pedido de 3 unidades confirmado, saldo caiu de 5 para 2 no teste da T6 (mesma lógica, valores do cenário testado).
4. ✅ Confirmar um pedido cujo item pede mais que o saldo disponível retorna **400** no campo certo, o pedido **continua Rascunho**, e **nenhuma** movimentação é gravada (saldo inalterado) — pedido de 10 unidades com saldo 2 recusado, saldo e extrato inalterados (T6).
5. ✅ Cancelar um pedido **Confirmado** devolve o saldo (gera `Entrada` de estorno igual à saída original) — saldo voltou de 6 para 10 ao cancelar (T7).
6. ✅ Cancelar um pedido que ainda estava em **Rascunho** não gera nenhuma movimentação (saldo inalterado) — saldo e extrato (3 linhas) inalterados (T7).
7. ✅ Entrada manual em produto **inativo** retorna 400 — 400 no campo `ProdutoId` (T5).
8. ✅ O extrato de um produto lista as movimentações mais recentes primeiro, com tipo, quantidade, motivo/origem e data — conferido em T4, T6 e T7.
9. ✅ A listagem `/estoque` acha um produto por nome ou por SKU e mostra o saldo correto — busca por SKU testada em T4.
10. ✅ Nenhum registro real (cliente, produto, categoria, pedido) é alterado pelos testes — contagens conferidas idênticas ao final de cada rodada (T3, T5, T6, T7).

## Open questions

Nenhuma em aberto — as quatro dúvidas da primeira versão foram fechadas em 22/09/2026 (ver "Decisões já tomadas"). Limites conhecidos, aceitos de propósito:

- **Sem saída manual** (perda/quebra/ajuste): se precisar, é uma extensão natural do mesmo modelo (`TipoMovimentacao` já é um enum) — pergunta antes, ver Boundaries.
- **Sem estoque mínimo/alerta de ruptura**: pode ser acrescentado depois como um campo em `produtos` + destaque na lista de Estoque.
- **Sem múltiplos depósitos**: um saldo único por produto, suficiente pro escopo de portfólio.
