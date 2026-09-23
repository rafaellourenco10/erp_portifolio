# Plano de implementação: Comissão como conta a pagar + contas avulsas (etapa 12)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando aprovação**.

## Visão geral

6 tarefas. A base é generalizar `parcelas_pagar` (origem, favorecido, descrição, total gravado) sem mudar o comportamento da compra; em cima disso entram a avulsa, o cancelar e a ponte com Comissões.

## Grafo de dependências

```
T1 Schema: origem/favorecido/descricao/vendedor/total_parcelas + comissoes.parcela_pagar_id + EmPagamento
  └─► T2 Contas a Pagar: lista generalizada, conta avulsa, cancelar (+ xUnit)
        └─► T3 Comissões → conta (gerar-conta, pagar/cancelar propagam; remove /pagar) ── CP1: API pronta (E2E)
              ├─► T4 Tela Contas a Pagar (nova conta, colunas, filtro, cancelar)
              └─► T5 Tela Comissões (gerar conta a pagar, Em pagamento)
                    └─► T6 Fechamento
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| Uma tabela `parcelas_pagar` com `origem` (em vez de uma tabela por tipo) | Lista, filtros, pagar, dashboard e resumo continuam num lugar só. |
| `total_parcelas` gravado | Avulsas não têm pedido para contar as irmãs; também simplifica a consulta. |
| Favorecido derivado na consulta (fornecedor / vendedor / texto) | Sem duplicar nome de fornecedor/vendedor na parcela. |
| Status `EmPagamento` na comissão + FK para a parcela | Mostra o que já foi para o financeiro e permite desfazer (cancelar a conta). |

## Riscos e mitigações

| Risco | Mitigação |
|---|---|
| Quebrar o fluxo de compra | Migration preserva as linhas (origem `Compra`, total preenchido); E2E refaz o fluxo de compra. |
| Front do Contas a Pagar usa `fornecedorNome` | A T2 troca o campo da API para `favorecido`; entre a T2 e a T4 a coluna Fornecedor da tela fica vazia (compila, só não mostra). A T4 vem logo em seguida e o E2E da T3 cobre a API. |

## Comandos de verificação

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src; npm run build
```
