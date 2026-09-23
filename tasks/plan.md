# Plano de implementação: Módulo Vendedores (etapa 10)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando aprovação**.

## Visão geral

6 tarefas. O cadastro espelha Fornecedor (já testado); a parte nova de verdade é pequena: a validação só-CPF, duas colunas em `pedidos` e duas regras no `PedidoService` (salvar e confirmar).

## Grafo de dependências

```
T1 Vendedor (model) + colunas em pedidos + migration + [Cpf] com xUnit
  ├─► T2 Vendedor backend (DTOs, service, controller)
  │     └─► T3 Pedido de venda com vendedor (salvar/confirmar, % congelada) ── CP1: API pronta (E2E)
  │           ├─► T4 Tela Vendedores + menu Cadastro
  │           └─► T5 Campo Vendedor no pedido de venda (SelecaoVendedor)
  │                 └─► T6 Fechamento (README, spec, graphify)
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| `[Cpf]` novo, reaproveitando `DocumentoValidador` | Vendedor é pessoa física; o `[CpfCnpj]` aceitaria CNPJ. |
| `percentual_comissao` no pedido (não só no vendedor) | Congela a % da venda (PV3); a comissão futura lê do pedido. |
| `SelecaoVendedor` espelhando `SelecaoFornecedor` | Mesmo comportamento de busca no servidor já usado nos pedidos. |
| Sem xUnit tocando banco | Padrão do projeto; regras de pedido verificadas por E2E. |

## Riscos e mitigações

| Risco | Mitigação |
|---|---|
| Quebrar o confirmar de vendas existente | Regra nova entra junto das validações atuais; E2E confirma também que um pedido completo continua confirmando. |
| Pedidos antigos sem vendedor | Colunas nullable; E2E abre um pedido antigo. |

## Comandos de verificação

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src; npm run build
```
