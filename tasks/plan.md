# Plano de implementação: Módulo Dashboard (etapa 6)

> Origem: [SPEC.md](../SPEC.md) (aprovada em 22/09/2026). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando revisão do Rafael**. Nenhum código foi escrito.

## Visão geral

Entregar o Dashboard em **8 tarefas pequenas**. Diferente dos módulos anteriores, **não há banco novo**: o módulo só lê dados que já existem (Pedidos, Estoque, Contas a Receber). A ordem: os três endpoints de resumo (um por módulo de origem, reaproveitando os serviços existentes), depois a tela.

## Grafo de dependências

```
T1 Resumo de vendas (PedidoService) + xUnit ─┐
T2 Resumo de contas a receber (ContasReceberService) ─┼─► CP1: API pronta
T3 Resumo de estoque (EstoqueService) ─────────────────┘
                                                              │
T4 DashboardController (3 endpoints)  ◄──────────────────────┘
                                                              │
T5 Base do front + cards de número ─► T6 Gráfico de faturamento diário ─► T7 Menu "Painel" + rota inicial ─► T8 Fechamento
```

**Pode andar em paralelo:** T1, T2 e T3 entre si (cada um só toca o service do seu próprio módulo, sem dependência entre eles). **Precisa ser sequencial:** T1-T3 → T4 (o controller precisa dos três métodos prontos).

## Decisões de arquitetura (além das da spec)

| Decisão | Motivo |
|---|---|
| **Sem `DashboardService` novo** — o método de resumo entra no service que já existe de cada módulo | O Dashboard não tem regra de negócio própria, só agrega o que os services já sabem calcular; criar um serviço novo só para orquestrar seria uma camada a mais sem função. |
| **`DashboardController` só delega**, sem lógica | Os três endpoints chamam um método cada, sem juntar nada — mantém os cards independentes (D7) de verdade, não só na tela. |
| Faturamento diário preenche **todos os dias do mês**, mesmo sem venda (0) | Um gráfico de linha com buraco (dia faltando) é enganoso; melhor mostrar zero explícito. |
| Biblioteca de gráfico: decidir na T6, olhando a skill de dataviz do projeto antes de escrever qualquer código de gráfico | Já autorizado pela spec como a única dependência nova permitida sem perguntar de novo. |
| Rota inicial (`/`) e item de menu fora de "Gestão Comercial" | Dashboard resume módulos, não é uma ação comercial — mesmo raciocínio discutido com o Rafael antes de especificar. |
| Dados de teste isolados (produto/cliente/pedidos `ZZT…`) para os testes de API, apagados ao final | Mesmo padrão dos módulos anteriores; aqui é ainda mais importante porque os números do Dashboard são somas — um dado de teste esquecido distorceria o card na tela real. |

## Fases e checkpoints

| Fase | Tarefas | Entrega |
|---|---|---|
| 1. Resumos por módulo | T1, T2, T3 | Três métodos de resumo, testados por xUnit e/ou E2E |
| 2. API | T4 | `DashboardController` com os 3 endpoints, verificado por E2E |
| 3. Tela | T5, T6, T7 | Cards, gráfico, menu/rota — Dashboard como página inicial |
| 4. Fechamento | T8 | README, graphify, critérios da spec conferidos |

**Checkpoints:** **CP1** após T4 (API pronta), **CP2** após T7 (tela pronta), **CP3** ao final.

## Riscos e mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| Ticket médio dividindo por zero quando não há pedido confirmado no mês | Médio (erro 500 na tela) | Testado explicitamente em T1 (0 confirmados → ticket médio 0) |
| Faturamento diário com buraco no gráfico (dia sem venda simplesmente ausente) | Baixo (visual enganoso) | Preencher todos os dias do mês com 0 antes de agrupar (D4), testado em T1 |
| Mudar a rota `*` para `/` quebrar algum link direto que hoje espera `/clientes` | Baixo (portfólio, sem usuário externo) | Só a rota **desconhecida** muda de destino; todas as rotas existentes (`/clientes`, `/produtos` etc.) continuam iguais |

## Comandos de verificação (usados em todas as tarefas)

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src
```
