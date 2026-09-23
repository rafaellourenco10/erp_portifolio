# Plano de implementação: Módulo Comissões (etapa 11)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando aprovação**.

## Visão geral

5 tarefas. A comissão nasce dentro do `MarcarRecebidaAsync` do Contas a Receber (mesma transação); a tela é uma lista com totais e uma ação em lote.

## Grafo de dependências

```
T1 Comissao (model) + migration com carga das já recebidas + cálculo com xUnit
  └─► T2 Gerar comissão ao receber parcela
        └─► T3 ComissaoService + /api/comissoes (listar com totais, pagar em lote) ── CP1: API pronta (E2E)
              └─► T4 Tela Financeiro → Comissões
                    └─► T5 Fechamento (README, spec, graphify)
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| Tabela própria com base, % e valor gravados | Auditável e permite o status Pendente/Paga; calcular na hora não guardaria o pagamento ao vendedor. |
| Índice único em `parcela_receber_id` | Garante 1 comissão por parcela mesmo com corrida. |
| Carga das já recebidas em SQL na migration | Uma vez só, no mesmo lugar que cria a tabela. |
| Pagar em lote num endpoint só (`POST /pagar` com ids) | A ação da linha e a das selecionadas usam o mesmo caminho. |

## Riscos e mitigações

| Risco | Mitigação |
|---|---|
| Quebrar o "marcar como recebido" | A geração entra depois da regra atual, no mesmo SaveChanges; E2E também recebe parcela de pedido sem vendedor. |
| Arredondamento divergente | Função pura com xUnit; E2E compara com SQL. |

## Comandos de verificação

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src; npm run build
```
