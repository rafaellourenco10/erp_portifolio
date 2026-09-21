# Plano de implementação: Módulo Pedidos (etapa 3)

> Origem: [SPEC.md](../SPEC.md) (aprovada em 21/09/2026). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando revisão do Rafael**. Nenhum código foi escrito.

## Visão geral

Entregar o módulo de pedidos de venda em **16 tarefas pequenas**, cada uma deixando o sistema compilando e verificável. A ordem segue as dependências de baixo para cima: primeiro as **regras puras** (cálculo e status, sem banco), depois o **banco**, a **API completa** e por último a **tela**. A API inteira é fechada e testada antes do front, porque o contrato já está definido na spec e é a parte que mexe com dinheiro.

## Grafo de dependências

```
T1 Cálculo + xUnit ─┐
T2 Status/transições ┼─► T3 Entidades ─► T4 Migration ─► T5 DTOs ─► T6 Criar/obter ─► T7 Listar
                     │                                                  │
                     │                                                  ├─► T8 Editar rascunho ─► T9 Confirmar/cancelar
                     │                                                                                   │
                     │                                                             CP3: API pronta ◄─────┘
                     │
T10 Base do front (usa o contrato da API + T1 como referência de cálculo)
   ├─► T11 Lista + menu + rotas
   └─► T12 Seleções com busca ─► T13 Página: novo ─► T14 Página: editar/confirmar/cancelar ─► T15 Celular ─► T16 Fechamento
```

**Pode andar em paralelo** (se houver mais de uma sessão): T1 e T2 entre si; T10 (base do front) assim que o contrato da API estiver congelado em T5. **Precisa ser sequencial:** T3 → T4 (migration depende do modelo) e T6 → T8 → T9 (o serviço vai crescendo).

## Decisões de arquitetura (além das da spec)

| Decisão | Motivo |
|---|---|
| **Backend completo antes do front** (fatias verticais dentro do back: criar, listar, editar, confirmar) | O contrato está na spec e a regra de dinheiro fica provada por testes antes de existir tela. |
| `CalculoPedido` e `TransicoesPedido` são **classes estáticas puras** | Testáveis sem banco; o serviço só orquestra. |
| **Enums gravados como texto** (`HasConversion<string>()`) e serializados como texto no JSON (`JsonStringEnumConverter`) | O banco e a API ficam legíveis (`"Rascunho"`); o filtro `?status=Confirmado` funciona pelo nome. |
| **PUT atualiza os itens no lugar** (casa por `produtoId`: altera, remove os que saíram, adiciona os novos), não apaga e recria | Preserva o preço congelado (R3) e evita violar o índice único `(pedido_id, produto_id)` numa troca dentro do mesmo `SaveChanges`. |
| **`valor_total` gravado** por um único método de recálculo, chamado em criar e editar | A lista e a busca leem o total sem somar itens; um só ponto de escrita evita divergência. |
| **Erros de regra em campo** com `DadoInvalidoException` (400) e **transição inválida** com `ConflitoException` (409) | Reaproveita o que já existe e o mapeamento de erros que a tela já sabe mostrar. |
| **Cálculo da tela em aritmética inteira** (centavos, milésimos, centésimos de %) | `float` do JavaScript erra em casos como `1,005 × 100`; o servidor usa `decimal`. Os mesmos 6 casos valem nos dois lados. |
| **Seleção de cliente/produto com busca no servidor** (`GET /clientes?nome=&ativo=true`, `GET /produtos?busca=&ativo=true`, 20 resultados) | Esses cadastros crescem; carregar tudo não escala (diferente de Categorias, que é pequena). |
| **Formulário do pedido com `useFieldArray`** e total via `useWatch` | Padrão do react-hook-form já usado; recalcula ao digitar sem estado duplicado. |
| **Dados de teste isolados**: cliente `ZZT Pedidos` e produtos com SKU `66000001…` criados e apagados a cada rodada | Os testes precisam inativar cliente/produto (R5/R6) e **não podem tocar** nos registros reais (Rafael, MOUSE GAMER). Ordem de limpeza: pedidos → produtos → cliente. |

## Fases e checkpoints

| Fase | Tarefas | Entrega |
|---|---|---|
| 1. Regras puras | T1, T2 | Cálculo e transições provados por `dotnet test` |
| 2. Banco | T3, T4 | Tabelas `pedidos` e `pedido_itens` no banco de desenvolvimento, sem mexer nas existentes |
| 3. API | T5 a T9 | Os 6 endpoints, verificados por script ponta a ponta |
| 4. Tela | T10 a T14 | Fluxo completo: montar, salvar, confirmar, cancelar |
| 5. Fechamento | T15, T16 | Celular, README, graphify, critérios da spec conferidos |

**Checkpoints** (revisão sua antes de seguir): **CP1** após T2, **CP2** após T4, **CP3** após T9 (API pronta), **CP4** após T14 (fluxo na tela), **CP5** ao final. Critérios de cada um em [todo.md](todo.md).

## Riscos e mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| Diferença de arredondamento entre back (`decimal`) e front | Alto (dinheiro) | Mesmos 6 casos de referência nos dois lados; front em aritmética inteira (T10); a tela mostra o valor do servidor após salvar |
| Trocar itens no PUT quebrar o índice único ou perder o preço congelado | Alto | Atualização no lugar por `produtoId` (T8); teste de "preço não muda depois de editar o produto" |
| Enum saindo como número no JSON | Médio | `JsonStringEnumConverter` nos enums e teste de contrato em T6 |
| Migration mexer em tabela existente por engano | Alto | T4 só **cria**; backup do banco antes; comparar o esquema de `clientes`/`produtos`/`categorias` antes e depois; testar em banco descartável |
| Seleção com busca no servidor gerar excesso de requisições ou "piscar" | Médio | Debounce de ~300 ms e `keepPreviousData` (T12) |
| Duas abas editando o mesmo rascunho | Baixo (aceito) | "Última grava"; token `xmin` só se virar problema (pergunta antes) |
| Testes tocarem dados reais | Alto | Dados de teste com prefixo próprio e limpeza no `finally`; conferir no fim que cliente, produto e categoria reais estão iguais |
| Projeto xUnit trazer conflito de build com a solution | Baixo | T1 valida `dotnet build ErpPortfolio.slnx` com 0 avisos logo no início |
| Tela do pedido crescer demais (L) | Médio | T12 a T14 separam seleções, "novo" e "editar/confirmar"; nenhuma passa de ~5 arquivos |

## Convenções que valem para todas as tarefas

- Cabeçalho obrigatório em cada arquivo C#/TS (nome, versão, data, descrição, banco/tabelas/fontes, histórico).
- **Definição de pronto de cada tarefa:** `dotnet build ErpPortfolio.slnx` com 0 avisos, `dotnet test` verde (quando houver), `npx tsc -b` e `npx oxlint src` sem apontamentos (tarefas de front), verificação da própria tarefa executada de verdade, dados de teste apagados.
- **Não commitar** sem o Rafael pedir. Ao terminar uma tarefa que muda código, **regravar o graphify** (regra permanente) e, no fim, atualizar o README.

## Perguntas em aberto

Nenhuma. As decisões da spec foram todas fechadas em 21/09/2026.
