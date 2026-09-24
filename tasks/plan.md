# Plano de implementação: Devolução de venda (etapa 14)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aprovado e implementado** (24/09/2026), com o card de Devoluções no Dashboard.

## Visão geral

7 tarefas. O coração é um cálculo puro (valor, abatimento, reembolso, estorno), testado sem banco, e um serviço que aplica o resultado nos quatro módulos numa transação. As telas vêm depois da API validada por E2E.

## Grafo de dependências

```
T1 Schema: devolucoes + devolucao_itens; parcelas_pagar.devolucao_id + origem Devolucao; comissoes estorno
  └─► T2 DevolucaoCalculo (puro, xUnit): valor por item, "o que falta", abatimento, reembolso, estorno
        └─► T3 DevolucaoService + endpoints (POST/GET), pedido com valorDevolvido, cancelar bloqueado
              └─► T4 Contas a Pagar (origem Devolucao, não cancela) + Comissões (estorno na lista, CC6) + /dashboard/devolucoes ── CP1: API pronta (E2E)
                    ├─► T5 Tela do pedido: modal de devolução + histórico
                    └─► T6 Telas de Contas a Pagar, Comissões e card do Dashboard
                          └─► T7 Fechamento (README, SPEC, grafo, memória)
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| Devolução é registro próprio (`devolucoes` + itens) com os totais gravados | Histórico auditável (quanto abateu, quanto reembolsou) sem recalcular. |
| `DevolucaoCalculo` puro, recebendo as parcelas pendentes e devolvendo o plano de ajustes | Toda a regra de dinheiro testável em xUnit; o serviço só aplica. |
| Estorno de comissão como linha negativa em `comissoes` | Reaproveita lista, totais, gerar conta e a propagação pagar/cancelar; o desconto no fechamento sai quase de graça. |
| Reembolso como nova origem de `parcelas_pagar` | Mesmo padrão da etapa 12 (Compra/Comissão/Avulsa); lista, filtro, pagar e Dashboard já funcionam. |
| Entrada de estoque pelo `EstoqueService` com `pedido_id` e motivo | Sem coluna nova em `estoque_movimentacoes`; o extrato já mostra o pedido. |

## Riscos e mitigações

| Risco | Mitigação |
|---|---|
| Relaxar o CHECK/NOT NULL de `comissoes` quebrar a lista (parcela nula) | DTO com `numeroParcela` nulo; E2E de regressão da lista e do recebimento. |
| Centavos não fecharem após várias devoluções | Regra do "o que falta" na devolução final + teste unitário com 3 devoluções. |
| Cancelar pedido duplicar entrada de estoque | DV9 bloqueia (409) com teste E2E. |

## Comandos de verificação

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src; npm run build
```
