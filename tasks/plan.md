# Plano de implementação: Orçamentos (etapa 13)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aprovado e implementado** (23/09/2026).

## Visão geral

7 tarefas. O orçamento é um "irmão" do pedido de venda: mesma forma (cabeçalho + itens), mesmo cálculo e as mesmas validações de item. Por isso a base é reaproveitar o que o `PedidoService` já faz, e não copiar.

## Grafo de dependências

```
T1 Schema: orcamentos + orcamento_itens (models, DbContext, migration)
  └─► T2 API: listar/obter/criar/editar + vencido (+ xUnit)
        ├─► T3 Gerar pedido + perder ── CP1: API pronta (E2E)
        └─► T4 PDF do orçamento (QuestPDF)
              └─► T5 Front: tipos/api/hooks + lista + formulário + menu
                    └─► T6 Front: ações (gerar pedido, perder, PDF)
                          └─► T7 Fechamento
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| Tabelas próprias (`orcamentos`, `orcamento_itens`) em vez de um status "Orçamento" no pedido | Não mexe no fluxo, nos relatórios nem no dashboard de pedidos; a numeração de pedidos continua só de vendas. |
| Validações de item e cálculo do `PedidoService` extraídas para métodos estáticos compartilhados (`CalculoPedido` já é) | Uma regra só para quantidade/unidade/produto ativo; o orçamento não duplica código. |
| Gerar pedido monta o `Pedido` direto no `OrcamentoService` (mesmo `DbContext`, um `SaveChanges`) | Precisa gravar o **preço do orçamento**, e o `PedidoService.CriarAsync` copia o preço atual do produto (R3). |
| `vencido` calculado, não gravado | Sem job agendado; é sempre coerente com a data de hoje. |
| PDF em classe própria (`ExportadorOrcamento`) | O `ExportadorRelatorio` é tabular genérico; o orçamento tem layout de documento. Reaproveita a licença QuestPDF já configurada. |

## Riscos e mitigações

| Risco | Mitigação |
|---|---|
| Extrair as validações do `PedidoService` quebrar o pedido | Refatoração sem mudança de comportamento, com os testes existentes passando + E2E de regressão do pedido na T3. |
| Filtro "Aberto" x "Vencido" depender do fuso | Mesma referência de "hoje" de Contas a Receber (UTC) e teste unitário da borda (validade = hoje não está vencido). |

## Comandos de verificação

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src; npm run build
```
