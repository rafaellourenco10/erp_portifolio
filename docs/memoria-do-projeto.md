# Memória do projeto

> Parte da documentação do [Ambition ERP](../README.md). Registro histórico: como o projeto evoluiu, versões exatas e o processo de trabalho.

## Nome

Os nomes técnicos (solution `ErpPortfolio`, projeto `ErpPortfolio.Api`, banco `erp_portfolio_db`) foram mantidos; "Ambition ERP" é o nome do produto exibido na interface e no Swagger.

## Organização do menu

O menu lateral agrupa as telas por departamento: **Dashboard** no topo; **Cadastro** (Clientes, Fornecedores, Vendedores, Produtos, Categorias); **Ordem Vendas/Compras** (Orçamentos, Pedidos de Venda, Pedidos de Compra); **Depósito** (Estoque); **Financeiro** (Contas a Receber, Contas a Pagar, Comissões, Fluxo de Caixa); **Fiscal** (Notas Fiscais, Empresa); **Relatórios** (Vendas, Compras, Estoque). As seções abrem e fecham, e o estado fica salvo no navegador.

## Histórico de etapas

O projeto foi construído em etapas, cada uma com especificação, plano de tarefas, implementação e testes. O detalhe de cada módulo está em [Funcionalidades](funcionalidades.md) e os testes em [Testes](testes.md).

| Etapa | Módulo | Situação |
|---|---|---|
| 1 | Clientes | Concluída e testada de ponta a ponta (18/09/2026) |
| 1.1 | Tema visual Ambition ERP + filtros por UF e status | Concluída e testada (18/09/2026) |
| 2 | Produtos (+ navegação por rotas no menu) | Concluída e testada (21/09/2026) |
| 2.1 | Categorias (cadastro próprio, escolhido por seleção no produto) | Concluída e testada (21/09/2026) |
| 2.2 | Filtro por categoria em Produtos + atalho "Nova categoria" no seletor | Concluída (22/09/2026) |
| 3 | Pedidos (cliente, itens, desconto, total calculado, confirmar/cancelar) | Concluída e testada (21/09/2026) |
| 4 | Estoque (movimentações, entrada manual, baixa/estorno automáticos) | Back-end testado de ponta a ponta; tela confirmada pelo Rafael (22/09/2026) |
| 5 | Contas a Receber (parcelas, vencimento, status de recebimento) | Back-end testado de ponta a ponta; tela não verificada visualmente (22/09/2026) |
| 6 | Dashboard (indicadores do mês) | Back-end testado com dados reais; tela não verificada visualmente (22/09/2026) |
| 7 | Fornecedores e Pedidos de Compra (entrada automática de estoque, custo atualizado) | Back-end testado de ponta a ponta; tela não verificada visualmente (23/09/2026) |
| 8 | Contas a Pagar (parcelas da compra, vencimento, status de pagamento, card no Dashboard) | Back-end testado de ponta a ponta; telas confirmadas pelo Rafael (23/09/2026) |
| 9 | Relatórios (vendas, compras, estoque) com exportação Excel/PDF | Back-end testado de ponta a ponta e PDFs conferidos; telas confirmadas pelo Rafael (23/09/2026) |
| 10 | Vendedores (cadastro + vendedor no pedido de venda, % de comissão congelada) | Back-end testado de ponta a ponta; telas confirmadas pelo Rafael (23/09/2026) |
| 11 | Comissões (geradas no recebimento da parcela, pagamento ao vendedor) | Back-end testado de ponta a ponta; tela confirmada pelo Rafael (23/09/2026) |
| 12 | Comissão vira conta a pagar + contas avulsas (origem Compra/Comissão/Avulsa, cancelar) | Back-end testado de ponta a ponta; telas não verificadas visualmente (23/09/2026) |
| 13 | Orçamentos (validade, situação Vencido calculada, gerar pedido com os preços do orçamento, marcar como perdido, PDF) | Back-end testado de ponta a ponta; tela testada com Playwright (23/09/2026) |
| 14 | Devolução de venda (parcial, estoque por item, abatimento nas parcelas, reembolso em Contas a Pagar, estorno de comissão, card no Dashboard) | Back-end testado de ponta a ponta; tela testada com Playwright (24/09/2026) |
| 15 | Fluxo de caixa (realizado + previsto, saldo acumulado, visão diária e mensal, gráfico, Excel/PDF) | Back-end testado de ponta a ponta; tela testada com Playwright (24/09/2026) |
| 16 | NF-e simulada (emitente, dados fiscais em cliente/produto, emissão pelo pedido e pela devolução, impostos, chave, XML 4.00, DANFE, lista de notas) | Back-end testado de ponta a ponta; tela testada com Playwright (28/09/2026) |
| 17+ | Login | Planejada |

## Stack e versões

| Camada | Tecnologia | Versão |
|---|---|---|
| Runtime / SDK | .NET SDK | 10.0 |
| Back-end | ASP.NET Core Web API (Controllers) | net10.0 |
| ORM | Entity Framework Core + Npgsql | EF Core 10.0.12 / Npgsql EF 10.0.3 |
| Ferramenta de migrations | dotnet-ef (ferramenta local) | 10.0.12 |
| Swagger | Swashbuckle.AspNetCore | 10.2.3 |
| Excel dos relatórios | ClosedXML (licença MIT) | 0.105.1 |
| PDF dos relatórios | QuestPDF (licença Community — gratuita para uso individual e empresas com faturamento < US$ 1 mi; declarada no `Program.cs`) | 2026.9.0 |
| Banco | PostgreSQL (Docker, imagem `postgres:18-alpine`, porta 5433 no host) | 18 |
| Front-end | React + TypeScript | React 19 / TS 6 |
| Build do front | Vite | 8 |
| Componentes | Ant Design + @ant-design/icons | 6 |
| Chamadas à API | TanStack Query + Axios | 5 / 1 |
| Formulários | React Hook Form + Zod + @hookform/resolvers | 7 / 4 / 5 |
| Datas (seletor de período) | dayjs (já vinha com o Ant Design; declarado no `package.json` na etapa 9) | 1.11 |
| Fonte | Inter (auto-hospedada via @fontsource-variable/inter) | 5 |
| Lint do front | oxlint | 1 |

Ambiente usado no desenvolvimento: Windows 11, Node.js 24, Docker Desktop com Docker Compose.

## Números

- 18 controllers na API e 163 testes unitários (xUnit) nas regras puras: cálculo do pedido, parcelas, comissões, devolução, fluxo de caixa, NF-e etc.
- Além dos unitários, os módulos foram validados com roteiros de ponta a ponta na API e testes de tela com Playwright (ver [Testes](testes.md)).

## Processo de trabalho

1. **Especificação** em [SPEC.md](../SPEC.md): objetivo, regras, decisões e critérios de aceite da etapa.
2. **Plano** em [tasks/plan.md](../tasks/plan.md) e checklist em [tasks/todo.md](../tasks/todo.md): tarefas pequenas (T1…Tn), com dependências.
3. **Implementação** tarefa a tarefa, um commit por tarefa.
4. **Testes de ponta a ponta** numa instância temporária da API (porta 5099), com dados de teste prefixados `ZZT…` que são apagados ao final, sem tocar nos dados reais.
5. **Documentação** atualizada ([Funcionalidades](funcionalidades.md), [API](api.md), [Banco](banco-de-dados.md), [Testes](testes.md)) e a SPEC marcada como implementada.

O SPEC.md e a pasta `tasks/` guardam sempre a etapa mais recente; as anteriores ficam no histórico do Git.
