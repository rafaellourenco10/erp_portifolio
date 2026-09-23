# Spec: Módulo Contas a Pagar (etapa 8)

> Status: **implementada e testada em 23/09/2026** (T1 a T8 do plano, ver `tasks/todo.md`). Os 8 critérios abaixo foram conferidos contra a API real numa instância temporária (dados de teste `ZZT…` apagados ao final, dados reais intactos). **Sem verificação visual** das telas — sem navegador nesta sessão.

## Objetivo

Espelho do Contas a Receber do lado da compra: confirmar um Pedido de Compra gera as parcelas a pagar ao fornecedor, e uma tela lista essas parcelas para marcar como pagas — fechando o "Fora do escopo" deixado pela etapa 7 ("Forma de pagamento e Contas a Pagar").

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** pedido do Rafael em 23/09/2026, logo depois de fechar Fornecedores + Pedidos de Compra.
- **Sucesso:** confirmar um pedido de compra escolhendo N parcelas gera N contas a pagar com valores e vencimentos certos; a tela `/contas-pagar` lista, filtra (inclusive Atrasado) e marca como paga; cancelar a compra cancela as parcelas ainda pendentes; o Dashboard mostra o total a pagar.

### Dentro do escopo
Tabela de parcelas a pagar ligadas ao pedido de compra; modal de parcelas ao confirmar o pedido de compra; tela `/contas-pagar` no menu **Financeiro**; marcar parcela como paga; cancelar pedido de compra cancela as parcelas pendentes; card "A pagar" no Dashboard.

### Fora do escopo (entram depois)
Pagamento parcial de parcela; editar valor/vencimento de uma parcela; contas a pagar avulsas (sem pedido de compra — aluguel, luz etc.); estorno/devolução de parcelas já pagas ao cancelar; **gerar parcelas para pedidos de compra que já estavam confirmados antes deste módulo** (ex.: o #4 da Kabum fica sem parcelas — mesmo comportamento que os pedidos de venda antigos tiveram na etapa 5).

## Decisões já tomadas (com o Rafael, 23/09/2026)

| Tema | Decisão |
|---|---|
| Geração das parcelas | Modal igual ao do pedido de venda: Nº de parcelas (1-12) e intervalo em dias (1-180), padrão 1 parcela em 30 dias. |
| Dashboard | Ganha um card "A pagar" (total pendente e atrasado), ao lado do "A receber". |
| Execução | Aprovada a spec, aplicar todas as tarefas em sequência, um commit por tarefa, sem push. |

## Regras de negócio (P)

| # | Regra |
|---|---|
| P1 | Confirmar um pedido de compra gera N parcelas Pendentes: valor dividido igualmente (arredondado para baixo no centavo, resto na última) e vencimento em `hoje + i × intervalo` dias — mesma fórmula do Contas a Receber (`ContasReceberCalculo.Dividir`, reaproveitada). |
| P2 | Nº de parcelas 1-12, intervalo 1-180 dias; corpo opcional (ausente = 1 parcela, 30 dias). Reaproveita `PedidoConfirmarDto`. |
| P3 | Status gravado: Pendente, Pago, Cancelado. **Atrasado** não é gravado: é Pendente com vencimento < hoje, calculado no servidor. |
| P4 | Marcar como paga: Pendente → Pago (grava a data do pagamento); marcar de novo uma já paga é sucesso (idempotente); parcela Cancelada não pode ser paga (409). |
| P5 | Cancelar um pedido de compra Confirmado cancela as parcelas ainda Pendentes; as já Pagas não mudam. Tudo na mesma transação do estorno de estoque — se o cancelamento for bloqueado por saldo insuficiente (PC7), nenhuma parcela muda. |
| P6 | Busca da lista: por número do pedido de compra (`4` ou `#4`) ou nome do fornecedor; ordenada por vencimento. |

## Modelo de dados

- **`public.parcelas_pagar`** — id, pedido_compra_id (FK → pedidos_compra), numero_parcela, valor, vencimento, status, data_pagamento. Espelho de `public.parcelas_receber`.
- Enum novo **`StatusParcelaPagar`** (Pendente, Pago, Cancelado) — não reaproveita `StatusParcela` porque "Recebido" não faz sentido numa conta a pagar.

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/contas-pagar` | Lista paginada (busca, status incl. Atrasado) |
| PATCH | `/api/contas-pagar/{id}/pagar` | Marca a parcela como paga (P4) |
| PATCH | `/api/pedidos-compra/{id}/confirmar` | **Passa a aceitar** corpo `{ numeroParcelas, intervaloDias }` (P2) |
| GET | `/api/dashboard/contas-pagar` | Resumo: total/quantidade pendente e atrasado |

## Telas

- **`/contas-pagar`** — espelho de `/contas-receber` (fornecedor, pedido de compra, parcela X/Y, valor, vencimento, status, ação "Marcar como paga"). Item **Contas a Pagar** no menu **Financeiro**, depois de Contas a Receber.
- **Pedido de Compra** — "Confirmar pedido" abre o modal de parcelas (igual ao do pedido de venda).
- **Dashboard** — card "A pagar".

## Testing strategy

1. **Unitário (xUnit):** só lógica pura, igual ao resto do projeto (a divisão já é coberta por `ContasReceberCalculoTests`); validação do enum/DTO se houver lógica nova.
2. **API ponta a ponta (instância local):** confirmar compra com 3 parcelas → valores e vencimentos; marcar paga (e de novo, idempotente); cancelar compra → pendentes canceladas, paga intacta; cancelamento bloqueado por saldo → parcelas intactas; filtro Atrasado; resumo do dashboard. Dados de teste `ZZT…` apagados ao final.
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; verificação visual fica com o Rafael (sem navegador nesta sessão).

## Boundaries

- **Sempre:** cálculo de parcelas e de atraso no servidor; reaproveitar `ContasReceberCalculo.Dividir` e `PedidoConfirmarDto`; parcelas geradas/canceladas na mesma transação do pedido; atualizar README e graphify ao terminar.
- **Perguntar antes:** dependência nova; contas a pagar avulsas; pagamento parcial; backfill de parcelas para compras antigas.
- **Nunca:** gravar "Atrasado" no banco; alterar parcelas já pagas ao cancelar; commitar segredos; push sem pedido.

## Success criteria (testáveis)

Conferidos em 23/09/2026 por um script E2E (26 verificações) contra a API real; detalhes na seção "Contas a Pagar (23/09/2026)" do README.

1. ✅ Confirmar um pedido de compra com 3 parcelas / 30 dias gera 3 parcelas Pendentes cuja soma é exatamente o total do pedido, vencendo em +30, +60 e +90 dias.
2. ✅ Confirmar sem corpo gera 1 parcela de 30 dias; valores fora da faixa (0 ou 13 parcelas, 0 ou 181 dias) retornam 400 sem confirmar.
3. ✅ `/api/contas-pagar` lista com busca por número/fornecedor e filtro por status, incluindo Atrasado calculado.
4. ✅ Marcar como paga funciona, é idempotente, e em parcela cancelada retorna 409.
5. ✅ Cancelar um pedido de compra confirmado cancela as parcelas Pendentes e mantém as Pagas.
6. ✅ Cancelamento bloqueado por saldo insuficiente (PC7) não altera nenhuma parcela.
7. ✅ Resumo do dashboard bate com as parcelas pendentes/atrasadas no banco.
8. ✅ Menu Financeiro mostra Contas a Pagar; `tsc -b`/`oxlint`/`npm run build` limpos; `dotnet build` 0 avisos e `dotnet test` verde (150/150) — verificação visual não feita (sem navegador).
