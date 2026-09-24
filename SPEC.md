# Spec: Fluxo de caixa (etapa 15)

> Status: **aprovada em 24/09/2026** (padrões confirmados pelo Rafael), em implementação. Tarefas em `tasks/plan.md` e `tasks/todo.md`.

## Objetivo

Mostrar o dinheiro que **entrou e saiu** (realizado) e o que **vai entrar e sair** (previsto), dia a dia ou mês a mês, com o **saldo acumulado** — para responder "quanto tenho?" e "vai faltar dinheiro em algum dia?".

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** escolha do Rafael em 24/09/2026 (primeiro da lista de módulos futuros).
- **Sucesso:** abrir a tela e ver, para o mês atual, o saldo de cada dia, onde o realizado termina e o previsto começa, e o dia em que o saldo fica mais baixo.

### Dentro do escopo
Entradas = parcelas a receber; saídas = parcelas a pagar (todas as origens: compra, comissão, avulsa, reembolso de devolução). Realizado pela data do recebimento/pagamento; previsto pelo vencimento das pendentes. Saldo acumulado desde o início do ERP. Visão **diária** e **mensal**. Tela própria no Financeiro com resumo, gráfico, tabela e exportação Excel/PDF.

### Fora do escopo (entram depois)
Contas bancárias/caixas e saldo inicial digitado; categorias de despesa (próximo módulo da lista); detalhar os lançamentos de um dia ao clicar; card no Dashboard; cenários ("e se atrasar X"); juros/multa de atraso.

## Decisões já tomadas (com o Rafael, 24/09/2026)

| Tema | Decisão |
|---|---|
| Escopo | **Realizado + previsto**, com saldo projetado. |
| Saldo inicial | **Acumulado desde o início**: tudo que entrou menos tudo que saiu antes do período. Sem cadastro novo. |
| Visão | **Diária e mensal**, com gráfico e tabela nas duas. |
| Onde | **Tela própria no Financeiro** (sem card no Dashboard por enquanto). |

### Decididas por padrão (confirmadas pelo Rafael em 24/09/2026)

| Tema | Padrão proposto | Motivo |
|---|---|---|
| Parcelas atrasadas (pendentes com vencimento < hoje) | Entram como **previstas no dia de hoje** e aparecem num aviso "atrasado a receber / a pagar". | Ainda não entraram nem saíram; o mais realista é esperar que aconteçam já. Deixá-las no passado sumiria com elas do saldo projetado. |
| Limite do período | Até **366 dias** (mesma regra dos relatórios); na visão **diária**, até **93 dias**. | Mais de ~3 meses dia a dia não cabe no gráfico. |
| Período ao abrir | Diário: **mês atual**. Mensal: **ano atual**. | Os dois cortes mais pedidos. |
| "Hoje" e dias | **Horário de Brasília** (`HorarioBrasilia`), como o resto do ERP. | Recebimento às 22h conta no mesmo dia. |

## Regras

### Fluxo de caixa (FC)

| # | Regra |
|---|---|
| FC1 | **Entrada realizada:** parcela a receber `Recebido`, valor da parcela, no dia (Brasília) de `data_recebimento`. **Saída realizada:** parcela a pagar `Pago`, no dia de `data_pagamento`. Parcelas `Cancelado` nunca entram. |
| FC2 | **Previsto:** parcela `Pendente` no dia do `vencimento`, só para dias **≥ hoje**. Pendente com vencimento < hoje (atrasada) entra no **dia de hoje**. |
| FC3 | **Saldo inicial** = entradas realizadas − saídas realizadas **antes** do início do período. Se o período começa depois de hoje, soma também o previsto de hoje até a véspera do início (inclui atrasadas). |
| FC4 | **Saldo de cada período** (dia ou mês) = saldo do anterior + entradas (realizadas + previstas) − saídas (realizadas + previstas). O primeiro parte do saldo inicial. |
| FC5 | Uma linha **por período, mesmo sem movimento** (zeros), do início ao fim. Na visão mensal, o primeiro e o último mês consideram só os dias dentro do período. |
| FC6 | **Resumo:** saldo inicial, total de entradas, total de saídas, saldo final, **menor saldo** e o dia/mês em que ele ocorre, e o total **atrasado** a receber e a pagar (só quando hoje está no período). |
| FC7 | **Período:** `dataInicio` e `dataFim` obrigatórios, fim ≥ início, até 366 dias; `agrupamento=Dia` até 93 dias (senão 400 em `DataFim`). |
| FC8 | `formato=xlsx|pdf` devolve o arquivo com os mesmos dados (resumo + tabela), pelo `ExportadorRelatorio`. |

## Modelo de dados

Sem mudança no banco: lê `parcelas_receber` e `parcelas_pagar` (índices existentes em vencimento/status). Sem migration.

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/fluxo-caixa?dataInicio=&dataFim=&agrupamento=Dia\|Mes` | `{ saldoInicial, totalEntradas, totalSaidas, saldoFinal, menorSaldo, dataMenorSaldo, atrasadoReceber, atrasadoPagar, periodos: [{ inicio, entradasRealizadas, saidasRealizadas, entradasPrevistas, saidasPrevistas, saldo }] }` (FC1-FC7) |
| GET | mesma rota + `&formato=xlsx\|pdf` | Arquivo `fluxo-caixa-AAAA-MM-DD_AAAA-MM-DD` (FC8) |

## Telas

- **Financeiro → Fluxo de Caixa** (`/fluxo-caixa`): período (RangePicker) + **Diário/Mensal**, botão Gerar e Excel/PDF.
- **Cards:** Saldo inicial, Entradas, Saídas, Saldo final e **Menor saldo** (vermelho se negativo, com a data). Aviso quando houver atrasados.
- **Gráfico (SVG próprio, como o do Dashboard):** barras de entradas e saídas por período, **previsto com cor mais clara** que o realizado, e a **linha do saldo**; marcação de "hoje"; tooltip por período.
- **Tabela:** período, entradas (realizado/previsto), saídas (realizado/previsto), resultado e saldo; saldo negativo em vermelho; linha de hoje destacada.

## Testing strategy

1. **Unitário (xUnit), `FluxoCaixaCalculo` puro:** agrupamento por dia/mês com dias vazios; realizado × previsto em volta de hoje; atrasadas caindo em hoje; saldo inicial (passado e período futuro); saldo acumulado e menor saldo; mês parcial nas pontas; validação do período.
2. **API ponta a ponta (instância temporária na 5099, dados `ZZT…` apagados):** criar venda e compra com parcelas passadas/futuras, receber e pagar algumas, conferir cada número contra a soma direta; filtros inválidos → 400; Excel/PDF.
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; teste de tela com Playwright.

## Boundaries

- **Sempre:** reaproveitar `HorarioBrasilia`, `RelatorioCalculo.ErroPeriodo`, `ExportadorRelatorio`, `BotoesExportar` e o estilo do gráfico do Dashboard; somas no servidor.
- **Perguntar antes:** criar tabela nova (contas bancárias, saldo inicial); mexer no Dashboard.
- **Nunca:** gravar nada (a tela é só leitura); commitar segredos; push sem pedido.

## Success criteria (testáveis)

1. Entradas/saídas realizadas de cada dia batem com a soma direta das parcelas recebidas/pagas no banco (dia de Brasília).
2. Previstos batem com as pendentes por vencimento; atrasadas aparecem em hoje e no aviso.
3. Saldo inicial + entradas − saídas = saldo final; menor saldo e sua data corretos, inclusive com período no futuro.
4. Visão mensal soma igual à diária do mesmo período.
5. Período inválido → 400 sem calcular; Excel e PDF com os mesmos números da tela.
6. Tela no Financeiro com cards, gráfico (realizado × previsto + saldo), tabela e exportação, testada no navegador.
7. `dotnet build` 0 avisos, `dotnet test` verde, `tsc -b`/`oxlint`/`npm run build` limpos.
