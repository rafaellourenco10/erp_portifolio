# Plano de implementação: Fluxo de caixa (etapa 15)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aprovado e implementado** (24/09/2026).

## Visão geral

5 tarefas, sem mudança no banco. O coração é um cálculo puro (períodos, realizado × previsto, saldo acumulado, menor saldo), testado sem banco; o serviço só busca as parcelas e entrega ao cálculo. A tela vem depois da API validada por E2E.

## Grafo de dependências

```
T1 FluxoCaixaCalculo (puro, xUnit): períodos dia/mês, realizado × previsto, atrasadas em hoje, saldos, menor saldo
  └─► T2 API: DTOs + FluxoCaixaService + GET /api/fluxo-caixa (JSON, xlsx, pdf) ── CP1: API pronta (E2E na 5099)
        └─► T3 Tela: rota/menu Financeiro, filtros, cards, tabela, exportar
              └─► T4 Gráfico SVG: barras entradas/saídas (realizado × previsto) + linha do saldo
                    └─► T5 Fechamento (README, SPEC, grafo, memória)
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| `FluxoCaixaCalculo` puro, recebendo movimentos `(dia, valor, entrada/saída, realizado/previsto)` já no dia de Brasília | Toda a regra testável em xUnit; o serviço só consulta e converte datas. |
| Sem tabela nova | Entradas e saídas já existem nas parcelas; saldo acumulado dispensa saldo inicial digitado. |
| Uma rota com `formato=` (como os Relatórios) | Arquivo e tela saem da mesma consulta; reaproveita `ExportadorRelatorio.Arquivo`. |
| Gráfico SVG próprio | Mesmo motivo do Dashboard: sem lib de gráfico no bundle. |

## Riscos

| Risco | Mitigação |
|---|---|
| Somar o realizado inteiro para o saldo inicial fica lento com muitos dados | Um `SUM` no banco por tabela (antes do início), não carregar linhas. |
| Diferença de dia UTC × Brasília | Converter com `HorarioBrasilia` e testar recebimento às 22h. |
