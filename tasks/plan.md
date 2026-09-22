# Plano de implementação: Módulo Estoque (etapa 4)

> Origem: [SPEC.md](../SPEC.md) (aprovada em 22/09/2026). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando revisão do Rafael**. Nenhum código foi escrito.

## Visão geral

Entregar o controle de estoque em **11 tarefas pequenas**, cada uma deixando o sistema compilando e verificável. Ordem de baixo para cima: **regra pura** de saldo (sem banco), depois o **banco**, a **API** (consulta, entrada manual, e só então a integração com Pedidos) e por último a **tela**. A integração com Pedidos vem depois da API de estoque estar pronta e testada sozinha, para não misturar os dois problemas.

## Grafo de dependências

```
T1 Saldo (função pura) + xUnit ─► T2 Entidade/mapeamento ─► T3 Migration ─► T4 DTOs + consulta (GET) ─► T5 Entrada manual (POST)
                                                                                                              │
                                                                                    T6 Confirmar baixa estoque ◄┘
                                                                                              │
                                                                                    T7 Cancelar estorna estoque
                                                                                              │
                                                                CP3: API pronta ◄─────────────┘
                                                                          │
T8 Base do front + lista/estoque ─► T9 Drawers (entrada, extrato) ─► T10 Celular/polimento ─► T11 Fechamento
```

**Precisa ser sequencial:** T2 → T3 (migration depende do modelo); T4 → T5 (entrada usa o mesmo `EstoqueService`); T5 → T6 → T7 (o serviço de baixa/estorno precisa existir antes de plugar em `PedidoService`, e cancelar reaproveita o método de estorno criado em T6).

## Decisões de arquitetura (além das da spec)

| Decisão | Motivo |
|---|---|
| **API de estoque fechada e testada antes de tocar em `PedidoService`** | Isola o risco: se algo quebrar na baixa/estorno, o bug está claramente na integração, não na regra de saldo. |
| Saldo é uma **função pura** (`EstoqueCalculo.Saldo(IEnumerable<Movimentacao>)`) sem banco | Testável por xUnit sem banco, mesmo padrão do `CalculoPedido` de Pedidos. |
| `EstoqueService.Baixar` e `.Estornar` chamados **dentro da mesma transação** de `PedidoService.Confirmar`/`Cancelar` | Se a baixa falhar (saldo insuficiente), nada do pedido muda — `PedidoService` já abre transação para gravar `valor_total`, reaproveita. |
| `TipoMovimentacao` gravado como texto (`HasConversion<string>()`), igual a `StatusPedido` | Consistência com o padrão já usado; legível direto no banco. |
| Saldo **calculado por query agregada** (`GROUP BY produto_id`), nunca gravado | Evita divergência entre um campo `saldo` e a soma real das movimentações — mesmo raciocínio do `valor_total` vs. subtotal do item. |
| Motivo automático nas movimentações de pedido (`"Venda pedido #123"` / `"Estorno cancelamento pedido #123"`) | O extrato fica legível sem precisar abrir o pedido para saber a origem. |
| Tela de estoque em **lista + 2 drawers** (nova entrada, extrato), não uma página própria como Pedidos | Não há um "formulário grande" aqui — as duas ações são pontuais, o padrão Drawer dos outros módulos já serve. |
| Dados de teste isolados: produto próprio com SKU de teste (`67000001…`), sem tocar nos produtos/pedidos reais | Mesma regra de Pedidos: os testes de confirmar/cancelar mexem em estoque, não podem afetar saldo de produtos reais. |

## Fases e checkpoints

| Fase | Tarefas | Entrega |
|---|---|---|
| 1. Regra pura | T1 | Cálculo de saldo provado por `dotnet test`, sem banco |
| 2. Banco | T2, T3 | Tabela `estoque_movimentacoes` no banco de desenvolvimento, sem mexer nas existentes |
| 3. API | T4 a T7 | Consulta, entrada manual e baixa/estorno automáticos, verificados por script ponta a ponta |
| 4. Tela | T8 a T10 | Lista com saldo, lançar entrada, ver extrato, celular |
| 5. Fechamento | T11 | README, graphify, critérios da spec conferidos |

**Checkpoints** (revisão sua antes de seguir): **CP1** após T1, **CP2** após T3, **CP3** após T7 (API pronta, com Pedidos já integrado), **CP4** após T10 (tela pronta), **CP5** ao final. Critérios de cada um em [todo.md](todo.md).

## Riscos e mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| Confirmar um pedido baixar estoque parcialmente (2 de 3 itens) antes de descobrir que falta saldo no 3º | Alto (saldo inconsistente) | Checar o saldo de **todos** os itens antes de gravar qualquer movimentação (E2); mesma transação do `SaveChanges` de confirmar |
| Cancelar um pedido que nunca foi Confirmado gerar estorno indevido | Médio | `PedidoService.Cancelar` só chama `Estornar` quando o status **anterior** era `Confirmado` (E3), testado nos dois caminhos |
| Saldo calculado por agregação ficar lento com muitas movimentações | Baixo (escopo de portfólio) | Índice em `produto_id`; aceitável sem cache por enquanto |

## Comandos de verificação (usados em todas as tarefas)

```
dotnet build ErpPortfolio.slnx
dotnet test backend/ErpPortfolio.Tests
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src
```
