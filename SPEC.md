# Spec: Módulo Contas a Receber (etapa 5)

> Status: **em definição** (22/09/2026). Substitui a spec de Estoque (etapa 4, implementada e documentada no README). Ao mudar uma decisão depois de começar a codar, atualize esta spec **antes** do código.

## Objetivo

Fechar o ciclo financeiro de uma venda: ao confirmar um pedido, gerar as **parcelas a receber** (vencimento e valor), controlar o status de cada uma (pendente, recebida, atrasada) e permitir marcar o recebimento.

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** Pedidos e Estoque existem; falta saber **quanto e quando** vai entrar de dinheiro das vendas já confirmadas.
- **Sucesso:** confirmar um pedido gera as parcelas certas (valor e vencimento), a lista mostra o que está pendente/atrasado/recebido, e marcar uma parcela como recebida reflete na tela.

### Dentro do escopo
Geração automática de parcelas ao confirmar um pedido (quantidade e intervalo escolhidos no momento de confirmar), tela de listagem com filtro por status (incluindo "Atrasado", calculado), ação de marcar parcela como recebida, cancelamento automático das parcelas pendentes quando o pedido confirmado é cancelado.

### Fora do escopo (entram depois)
Recebimento parcial de uma parcela, juros/multa por atraso, boleto/nota fiscal de verdade, edição do valor/vencimento de uma parcela já gerada, contas a pagar, relatório financeiro/fluxo de caixa.

## Decisões já tomadas (com o Rafael, 22/09/2026)

| Tema | Decisão |
|---|---|
| Geração das parcelas | **Automática ao confirmar** o pedido — o número de parcelas e o intervalo em dias são escolhidos nesse momento (não antes, não depois). |
| Parcelamento | **N parcelas iguais** (1 a 12) **+ intervalo em dias** entre vencimentos; o valor do pedido é dividido igualmente, com o **resto na última parcela**. |
| Pedido cancelado | Cancelar um pedido **Confirmado** cancela as parcelas que ainda estão **Pendentes**; parcelas já **Recebidas** continuam como estão (histórico). |
| Marcar recebido | Botão **"Marcar como recebido"**, tudo ou nada (sem recebimento parcial); grava a data/hora do recebimento. |

## Regras de negócio

| # | Regra |
|---|---|
| C1 | Confirmar um pedido (`PATCH /pedidos/{id}/confirmar`) recebe `numeroParcelas` (1 a 12, padrão 1) e `intervaloDias` (1 a 180, padrão 30). Ao confirmar com sucesso, gera exatamente `numeroParcelas` parcelas cuja soma bate **exatamente** com `valorTotal` do pedido. |
| C2 | Valor de cada uma das primeiras `numeroParcelas − 1` parcelas = `valorTotal / numeroParcelas`, arredondado para baixo em 2 casas; a **última parcela leva o resto** (garante que a soma bate com o total, mesmo com divisão não exata). |
| C3 | Vencimento da parcela `N` (1-based) = data de confirmação (hoje, UTC) **+ `N × intervaloDias` dias**. A parcela 1 vence em `intervaloDias` dias, nunca no mesmo dia da confirmação (evita nascer "atrasada"). |
| C4 | Status de uma parcela: `Pendente` → `Recebido` (ação manual, grava `dataRecebimento`) ou `Pendente` → `Cancelado` (quando o pedido é cancelado). Ambos são finais. |
| C5 | **"Atrasado" não é um status gravado**: é uma parcela `Pendente` com `vencimento` no passado, calculado na consulta (nunca precisa de um job para "atualizar status"). |
| C6 | Cancelar um pedido que estava `Confirmado` cancela **todas as parcelas `Pendentes`** desse pedido (idempotente, como o próprio cancelamento do pedido). Parcelas `Recebidas` não mudam. |
| C7 | Marcar uma parcela como recebida é **idempotente**: marcar de novo uma já `Recebido` não gera erro e não duplica a data. Parcela `Cancelado` não pode ser recebida (409). |
| C8 | Uma parcela **nunca é excluída** — histórico definitivo, como o pedido e as movimentações de estoque. |

## Tech stack

Igual aos módulos anteriores: ASP.NET Core (.NET 10) + EF Core + PostgreSQL 18 (Docker, porta 5433) no back; React 19 + Vite + TypeScript + Ant Design 6 + TanStack Query + react-hook-form + Zod + React Router no front. Nenhuma dependência nova.

## Modelo de dados (migration só **adiciona** tabela)

`parcelas_receber`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK, identity |
| `pedido_id` | integer | FK `pedidos` (RESTRICT — histórico nunca é apagado), índice `ix_parcelas_receber_pedido_id` |
| `numero_parcela` | integer | 1-based; `CHECK` > 0 |
| `valor` | numeric(12,2) | `CHECK` > 0 |
| `vencimento` | date | sem hora (é uma data de calendário, não um instante) |
| `status` | varchar(20) | `Pendente`, `Recebido` ou `Cancelado` (texto, como `status` em pedidos) |
| `data_recebimento` | timestamptz | nulo até ser marcada como recebida |

Índice único `ux_parcelas_receber_pedido_numero (pedido_id, numero_parcela)`; índice `ix_parcelas_receber_vencimento` (para ordenar/filtrar por vencimento e achar atrasadas).

## API

Novo `ContasReceberController` (`/api/contas-receber`):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/contas-receber?busca=&status=&pagina=&tamanhoPagina=` | Lista paginada, vencimento mais próximo primeiro. `busca` = nº do pedido ou nome do cliente. `status` = `Pendente`/`Recebido`/`Cancelado`/**`Atrasado`** (filtro calculado: `Pendente` + `vencimento` no passado) | 200, 400 |
| PATCH | `/contas-receber/{id}/receber` | Marca a parcela como recebida (C7) | 200, 404, 409 |

`PedidosController` sem rota nova — `PATCH /pedidos/{id}/confirmar` ganha corpo opcional `{ numeroParcelas?, intervaloDias? }` (defaults 1 e 30) e passa a gerar as parcelas (C1); `PATCH /pedidos/{id}/cancelar` continua sem corpo, mas passa a cancelar as parcelas pendentes por dentro (C6).

- Resposta da listagem: `{ id, pedidoId, clienteNome, numeroParcela, totalParcelas, valor, vencimento, status, dataRecebimento, atrasado }` — `atrasado` é calculado no servidor (nunca confiar no relógio do navegador).
- Erros: `numeroParcelas`/`intervaloDias` fora da faixa = 400 no campo (`[ApiController]`); receber uma parcela `Cancelado` = 409 (`ConflitoException`).

## Telas

- **`/contas-receber`** (item **Contas a Receber** no menu, em Gestão Comercial): tabela com Cliente, Pedido nº, Parcela (`X/Y`), Valor, Vencimento, Status (tag: Pendente cinza, **Atrasado vermelho**, Recebido verde, Cancelado cinza riscado) e ação **Marcar como recebido**; filtro por status (Segmented: Todas / Pendentes / Atrasadas / Recebidas / Canceladas) e busca por cliente/nº do pedido.
- **Confirmar pedido** (tela do pedido, `PedidoPage`): a janela de confirmação hoje é um `Modal.confirm` simples; passa a ter um formulário pequeno com **Número de parcelas** (1 a 12, padrão 1) e **Intervalo entre parcelas (dias)** (padrão 30), enviados no `PATCH /confirmar`.
- Vencimento formatado em `pt-BR` (`dd/mm/aaaa`, sem hora, já que a coluna é `date`).

## Commands

```
# Backend (na raiz)
dotnet build ErpPortfolio.slnx
dotnet run --project backend/ErpPortfolio.Api --launch-profile http        # API em http://localhost:5065
dotnet ef migrations add NomeDaMigration --project backend/ErpPortfolio.Api --configuration Release -o Data/Migrations
dotnet ef database update --project backend/ErpPortfolio.Api --configuration Release

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
  Models/            ParcelaReceber.cs, StatusParcela.cs
  DTOs/              ParcelaRespostaDto.cs, ParcelaFiltroDto.cs, PedidoConfirmarDto.cs (numeroParcelas, intervaloDias)
  Services/          IContasReceberService.cs, ContasReceberService.cs
  Controllers/       ContasReceberController.cs
  Controllers/PedidosController.cs  # alterado: Confirmar recebe corpo
  Services/PedidoService.cs         # alterado: Confirmar gera parcelas, Cancelar cancela pendentes
  Data/Migrations/   <data>_CriacaoTabelaParcelasReceber.cs
backend/ErpPortfolio.Tests/         ContasReceberCalculoTests.cs (divisão em parcelas, resto na última)
frontend/erp-portfolio-web/src/
  api/contasReceberApi.ts   hooks/useContasReceber.ts   types/contaReceber.ts
  pages/ContasReceber/ContasReceberListaPage.tsx
  pages/Pedidos/PedidoPage.tsx        # alterado: modal de confirmação vira formulário (parcelas, intervalo)
```

## Code style

Igual ao restante do projeto: cabeçalho obrigatório em todo arquivo C#/TS (nome, versão, data, descrição, banco/tabelas/fontes, histórico), nomes em português, mensagens de erro em português.

## Testing strategy

1. **Unitário (xUnit):** divisão do valor em N parcelas (1, 2, 3, 12 parcelas; valores que não dividem exato — resto na última; soma sempre bate com o total); cálculo do vencimento por parcela.
2. **API ponta a ponta (instância temporária):** confirmar com 1/3/12 parcelas gera as parcelas certas (valor e vencimento); confirmar sem informar parcelas usa os padrões (1, 30 dias); marcar como recebida (idempotente); marcar uma `Cancelado` como recebida = 409; cancelar o pedido cancela as parcelas `Pendentes` e não mexe nas `Recebidas`; filtro `Atrasado` acha parcela `Pendente` com vencimento passado; busca por cliente/nº do pedido.
3. **Tela (revisão de código, sem Playwright nesta sessão se a limitação persistir):** formulário de confirmar com parcelas, lista com status/filtro, marcar como recebido atualizando a tela sem F5.
4. **Sempre:** `dotnet build` com 0 avisos, `tsc -b` e `oxlint` sem apontamentos; nenhum dado real alterado (cliente, produto, categoria, pedido, estoque existentes intactos).

## Boundaries

- **Sempre:** soma das parcelas de um pedido bate exatamente com `valorTotal`; toda geração/cancelamento de parcela fica na mesma transação do `SaveChangesAsync` de `PedidoService` (como Estoque); cabeçalho em cada arquivo; backup do banco antes de migration; testar em instância temporária e limpar os dados de teste; atualizar README e graphify ao terminar; `git commit` só quando o Rafael pedir (ou já autorizado, como neste módulo).
- **Perguntar antes:** dependência nova, mudar tabela existente (`pedidos`, `produtos`, `estoque_movimentacoes`), recebimento parcial, juros/multa, editar valor/vencimento de parcela já gerada.
- **Nunca:** excluir uma parcela, deixar a soma das parcelas divergir do total do pedido, marcar como recebida uma parcela cancelada, commitar segredos, forçar push.

## Success criteria (testáveis)

1. Confirmar um pedido de R$ 100,00 com 3 parcelas gera parcelas de R$ 33,33, R$ 33,33 e **R$ 33,34** (resto na última) — soma exatamente R$ 100,00.
2. Confirmar sem informar `numeroParcelas`/`intervaloDias` usa os padrões: **1 parcela**, vencimento em **30 dias**.
3. Vencimento da parcela 2 de um pedido com intervalo de 15 dias é **hoje + 30 dias** (2 × 15).
4. Marcar uma parcela `Pendente` como recebida grava `dataRecebimento` e muda o status para `Recebido`; marcar de novo não gera erro nem duplica a data.
5. Marcar uma parcela `Cancelado` como recebida retorna **409**.
6. Cancelar um pedido `Confirmado` com 3 parcelas (1 já `Recebido`, 2 `Pendentes`) cancela as 2 `Pendentes` e **não mexe** na `Recebido`.
7. Uma parcela `Pendente` com vencimento ontem aparece com `atrasado: true`; a mesma parcela com vencimento amanhã aparece com `atrasado: false`.
8. A listagem `/contas-receber` acha por nome do cliente ou número do pedido, e o filtro por status (incluindo `Atrasado`) funciona.
9. Nenhum registro real (cliente, produto, categoria, pedido, estoque) é alterado pelos testes.

## Open questions

Nenhuma em aberto — as quatro dúvidas da primeira versão foram fechadas em 22/09/2026 (ver "Decisões já tomadas"). Limites conhecidos, aceitos de propósito:

- **Sem recebimento parcial**: uma parcela é tudo ou nada; se precisar, é uma extensão do modelo (`valor_recebido` separado de `valor`) — pergunta antes, ver Boundaries.
- **Sem juros/multa por atraso**: "Atrasado" é só um rótulo visual/filtro, não altera o valor da parcela.
- **Sem edição de parcela já gerada**: se o número de parcelas escolhido ao confirmar estava errado, hoje não tem como corrigir (limite aceito, como pedido confirmado que só cancela).
