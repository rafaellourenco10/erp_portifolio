# Plano de implementação: Módulo Contas a Receber (etapa 5)

> Origem: [SPEC.md](../SPEC.md) (aprovada em 22/09/2026). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando revisão do Rafael**. Nenhum código foi escrito.

## Visão geral

Entregar Contas a Receber em **10 tarefas pequenas**, mesma ordem de baixo para cima dos módulos anteriores: **regra pura** de divisão em parcelas (sem banco), **banco**, **API** (consulta e marcar recebido antes de tocar em Pedidos) e por último a **integração com Pedidos** (confirmar gera parcelas, cancelar cancela pendentes) e a **tela**.

## Grafo de dependências

```
T1 Divisão em parcelas (função pura) + xUnit ─► T2 Entidade/mapeamento ─► T3 Migration ─► T4 Consulta (GET) + marcar recebido (PATCH)
                                                                                                    │
                                                                          T5 Confirmar gera parcelas ◄┘
                                                                                    │
                                                                          T6 Cancelar cancela pendentes
                                                                                    │
                                                        CP3: API pronta ◄───────────┘
                                                                  │
T7 Base do front + lista ─► T8 Modal de confirmar com parcelas ─► T9 Celular/polimento ─► T10 Fechamento
```

**Precisa ser sequencial:** T2 → T3 (migration depende do modelo); T4 → T5 → T6 (a integração com Pedidos usa o `ContasReceberService` já pronto e testado sozinho); T7 → T8 (o modal de confirmar precisa do contrato da T5 já fechado, mas pode ser feito em paralelo à T4-T6 já que consome `/pedidos/confirmar`, não `/contas-receber`).

## Decisões de arquitetura (além das da spec)

| Decisão | Motivo |
|---|---|
| **API de Contas a Receber fechada e testada antes de tocar em `PedidoService`** | Mesmo raciocínio de Estoque: isola o risco da geração de parcelas do risco de mexer num serviço já em produção. |
| Divisão em parcelas é uma **função pura** (`ContasReceberCalculo.Dividir(valorTotal, numeroParcelas)`) sem banco | Testável por xUnit sem banco, mesmo padrão do `CalculoPedido` e do `EstoqueCalculo`. |
| `ContasReceberService.GerarParcelas` (síncrono, só monta entidades) e `.CancelarPendentesAsync` (precisa consultar as parcelas do pedido) chamados **dentro da mesma transação** de `PedidoService.Confirmar`/`Cancelar` | Não chamam `SaveChanges` sozinhos; reaproveita o `SaveChangesAsync` que `PedidoService` já faz, igual ao Estoque. |
| `StatusParcela` gravado como texto (`HasConversion<string>()`) | Consistência com `StatusPedido` e `TipoMovimentacao`. |
| `vencimento` como `date` (não `timestamptz`) | É uma data de calendário; `DateOnly` no C#, sem hora nem fuso pra confundir "venceu hoje". |
| **Modal de confirmar do pedido vira um formulário pequeno** (não mais `Modal.confirm` imperativo) | Precisa capturar 2 campos (parcelas, intervalo) antes de confirmar; `Modal.confirm` não tem estado de formulário de forma limpa. |
| `atrasado` calculado **no servidor**, devolvido pronto no DTO | Nunca confiar no relógio/fuso do navegador para uma regra de negócio (mesmo raciocínio de nunca confiar em total calculado no cliente). |
| Dados de teste isolados: cliente e produto de teste próprios (como nos módulos anteriores), pedidos de teste apagados junto com as parcelas geradas | As parcelas têm FK `RESTRICT` pro pedido; a ordem de limpeza é parcelas → itens → pedido → produto/cliente. |

## Fases e checkpoints

| Fase | Tarefas | Entrega |
|---|---|---|
| 1. Regra pura | T1 | Divisão em parcelas provada por `dotnet test`, sem banco |
| 2. Banco | T2, T3 | Tabela `parcelas_receber` no banco de desenvolvimento, sem mexer nas existentes |
| 3. API | T4 a T6 | Consulta, marcar recebido, geração ao confirmar e cancelamento ao cancelar, verificados por E2E |
| 4. Tela | T7 a T9 | Lista, modal de confirmar com parcelas, celular |
| 5. Fechamento | T10 | README, graphify, critérios da spec conferidos |

**Checkpoints** (revisão sua antes de seguir, dispensada se a autorização geral continuar valendo): **CP1** após T1, **CP2** após T3, **CP3** após T6 (API pronta), **CP4** após T9 (tela pronta), **CP5** ao final.

## Riscos e mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| Soma das parcelas não bater com `valorTotal` por arredondamento | Alto (dinheiro) | Resto sempre na última parcela (C2), testado com valores que não dividem exato (ex.: R$ 100 ÷ 3) |
| Cancelar um pedido cancelar parcelas já **Recebidas** por engano | Alto (perderia histórico de recebimento) | `CancelarPendentesAsync` filtra explicitamente por `Status == Pendente`, testado com um cenário misto (1 recebida + 2 pendentes) |
| "Atrasado" calculado errado por fuso horário (servidor em UTC, usuário em BRT) | Médio | Comparar `vencimento` (date, sem hora) com a data de hoje também em UTC, consistente com o resto do projeto (`DateTime.UtcNow` em todo lugar) |

## Comandos de verificação (usados em todas as tarefas)

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src
```
