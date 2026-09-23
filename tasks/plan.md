# Plano de implementação: Módulo Fornecedores + Pedidos de Compra (etapa 7)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **autorizado a implementar tudo e commitar a cada tarefa, sem pausar para revisão** (autorização do Rafael, 22/09/2026 — mesmo padrão usado no fechamento do módulo Pedidos).

## Visão geral

8 tarefas. A ideia central da spec é **reaproveitar** o que já existe (`StatusPedido`, `TransicoesPedido`, `CalculoPedido`, o padrão do Cliente) em vez de duplicar — então o volume de código novo de verdade é: 1 migration, 2 métodos novos no `EstoqueService`, e os CRUDs/telas espelhados.

## Grafo de dependências

```
T1 Migration + Models (Fornecedor, PedidoCompra, PedidoCompraItem, +coluna) ──┐
                                                                                 ├─► T3 EstoqueService: Receber/EstornarCompra
T2 Fornecedor backend (DTOs, Service, Controller) ◄─── T1 ─────────────────────┘
                                                                                       │
                                                                    T4 PedidoCompra backend (Service, Controller) ◄─┘
                                                                                       │
                                                                          CP1: API pronta
                                                                                       │
              T5 Tela Fornecedores ──┐
                                       ├─► T7 Estoque mostra "Compra #N" ─► T8 Fechamento
              T6 Tela Pedidos de Compra ─┘
```

**Sequencial:** T1 → T2/T3 → T4 → (T5, T6 podem andar em paralelo) → T7 → T8.

## Decisões de arquitetura (além das da spec)

| Decisão | Motivo |
|---|---|
| **Uma migration só**, cobrindo as 4 mudanças de schema (2 tabelas novas + 1 coluna) | É uma feature coesa; várias migrations pequenas para a mesma entrega não ganham nada. |
| **Sem `IPedidoCompraService` duplicando `TransicoesPedido`/`CalculoPedido`** | Já são funções puras independentes de `Pedido`; a spec já decidiu reaproveitar (ver "Decisões já tomadas"). |
| **Sem testes xUnit para Fornecedor** (só verificação manual via API real) | Mesmo padrão do Cliente, que também não tem xUnit dedicado (README: "Testes automatizados de integração para a API de Clientes... ainda pendente") — CRUD simples, sem regra de negócio própria. |
| **xUnit só para `PedidoCompraService`** (confirmar/cancelar/custo/saldo insuficiente) | É onde mora a regra de negócio nova de verdade (PC5-PC8); mesmo padrão do `PedidoService`. |
| Ícones do menu: Fornecedores = `ShopOutlined`, Pedidos de Compra = `ShoppingOutlined` | Distintos dos já usados (`TeamOutlined` Clientes, `ShoppingCartOutlined` Pedidos), decidido na T5/T6. |

## Fases e checkpoints

| Fase | Tarefas | Entrega |
|---|---|---|
| 1. Schema | T1 | Migration aplicada, models mapeados |
| 2. Backend | T2, T3, T4 | Fornecedores + Pedidos de Compra completos na API, testados (xUnit + E2E real) |
| 3. Frontend | T5, T6, T7 | Telas novas + extrato de estoque atualizado |
| 4. Fechamento | T8 | README, graphify, critérios da spec conferidos |

**Checkpoints:** **CP1** após T4 (API pronta), **CP2** ao final (T8).

## Riscos e mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| Cancelar um pedido de compra confirmado cujo saldo já foi parcialmente vendido | Médio (estornaria estoque inexistente, saldo ficaria negativo sem querer) | PC7: checar saldo suficiente por item ANTES de enfileirar qualquer estorno; testado em T4 |
| `Produto.Custo` sendo sobrescrito por engano em pedido ainda rascunho | Baixo | Custo só é escrito em `ConfirmarAsync`, nunca em `CriarAsync`/`AtualizarAsync` |
| Confundir `PedidoId` (venda) com `PedidoCompraId` (compra) na mesma movimentação | Médio (dado errado no extrato) | Uma movimentação preenche só uma FK das duas; `NovaMovimentacaoCompra` isolado do `NovaMovimentacao` de venda |

## Comandos de verificação (usados em todas as tarefas)

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src
```
