# Plano de implementação: Módulo Contas a Pagar (etapa 8)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando aprovação**. Aprovado, aplicar tudo em sequência com um commit por tarefa, sem push (decisão do Rafael, 23/09/2026).

## Visão geral

8 tarefas, todas espelhando o Contas a Receber (etapa 5). Código novo de verdade: 1 tabela/migration, 1 service/controller, 1 tela; o resto é ligar o que já existe (divisão em parcelas, DTO de confirmar, modal, card do dashboard).

## Grafo de dependências

```
T1 Enum + Model + Migration
  └─► T2 ContasPagarService + Controller (/api/contas-pagar)
        ├─► T3 Pedido de Compra: confirmar gera / cancelar cancela parcelas   ── CP1: API pronta
        └─► T4 Resumo no /api/dashboard/contas-pagar
              T5 Tela /contas-pagar + menu Financeiro   (depende de T2)
              T6 Modal de parcelas ao confirmar compra  (depende de T3)
              T7 Card "A pagar" no Dashboard            (depende de T4)
                    └─► T8 Fechamento (README, spec, graphify)
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| Tabela e enum próprios (`parcelas_pagar`, `StatusParcelaPagar`) em vez de uma tabela única com "tipo" | Receber liga a `pedidos`, pagar liga a `pedidos_compra`: uma tabela única precisaria de duas FKs nullable e de um "Recebido/Pago" ambíguo. Mesmo raciocínio usado para `pedidos_compra` x `pedidos`. |
| Reaproveitar `ContasReceberCalculo.Dividir` e `PedidoConfirmarDto` | Fórmula e validação idênticas; duplicar criaria duas versões para manter. |
| Modal de parcelas: extrair do `PedidoPage` para um componente usado pelas duas telas (se o código permitir sem mudar o comportamento da venda) | Evita duas cópias do mesmo modal; decidido na T6 olhando o código. |
| Sem xUnit tocando banco | Padrão do projeto: regra ligada a banco é verificada por E2E contra a API real. |

## Riscos e mitigações

| Risco | Mitigação |
|---|---|
| Cancelamento bloqueado por saldo (PC7) deixar parcelas já canceladas | Cancelar parcelas só depois da checagem de saldo, sem `SaveChanges` intermediário (mesma transação) — testado no critério 6. |
| Mudar o modal compartilhado e quebrar o confirmar da venda | T6 revalida o fluxo da venda via `tsc`/build e revisão; se extrair ficar arriscado, duplica-se o modal. |

## Comandos de verificação

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src; npm run build
```
