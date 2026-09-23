# Spec: Módulo Relatórios (etapa 9)

> Status: **rascunho, aguardando aprovação do Rafael** (23/09/2026).

## Objetivo

Nova seção **Relatórios** no menu, com três telas — **Vendas**, **Compras** e **Estoque** — que mostram os dados filtrados na tela e exportam para **Excel (.xlsx)** e **PDF** gerados pelo servidor.

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** escolha do Rafael em 23/09/2026, entre as sugestões de melhoria.
- **Sucesso:** escolher um período (e filtros), ver o relatório na tela com totais, e baixar o mesmo conteúdo em .xlsx e .pdf.

### Dentro do escopo
Três relatórios (vendas, compras, posição de estoque), cada um com filtros, resumo, tabela e exportação em Excel e PDF. Seção **Relatórios** no menu.

### Fora do escopo (entram depois)
Seletor de período no Dashboard; relatórios de contas a receber/pagar (vencidas); relatório agrupado por produto; itens dos pedidos dentro do relatório; gráficos no PDF; agendamento/envio por e-mail.

## Decisões já tomadas (com o Rafael, 23/09/2026)

| Tema | Decisão |
|---|---|
| Geração dos arquivos | **No servidor**: Excel com **ClosedXML** (licença MIT) e PDF com **QuestPDF** (licença Community, gratuita para uso individual e empresas com faturamento < US$ 1 mi — atende um portfólio). São as 2 únicas dependências novas. |
| Detalhe de Vendas/Compras | **Um pedido por linha + totais** (não lista os itens de cada pedido). |
| Dashboard | Seletor de período fica para depois. |

## Relatórios

### Vendas (R-V) e Compras (R-C) — mesmo formato

| Item | Definição |
|---|---|
| Filtros | Período (data inicial e final, **obrigatórios**), status (padrão **Confirmado**; opção "Todos"), cliente (vendas) / fornecedor (compras) opcional. |
| Linhas | Nº, data, cliente/fornecedor, quantidade de itens, total, status. Ordenadas por data (mais antiga primeiro). |
| Resumo | Quantidade de pedidos, soma dos totais e ticket médio — **das linhas listadas** (se o filtro for "Todos", o resumo inclui os cancelados; o padrão Confirmado evita isso). |

### Estoque (R-E) — posição atual

| Item | Definição |
|---|---|
| Filtros | Categoria (opcional), "só abaixo do mínimo" (padrão desligado). Só produtos **ativos**. |
| Linhas | Produto, SKU, categoria, unidade, saldo, estoque mínimo, custo, **valor em estoque** (saldo × custo), marcação "abaixo do mínimo". Ordenadas por nome. |
| Resumo | Quantidade de produtos, valor total em estoque, quantidade abaixo do mínimo. |

## Regras (R)

| # | Regra |
|---|---|
| R1 | Período: data final ≥ data inicial e no máximo **366 dias**; senão 400. Datas inclusivas (a data final vale o dia inteiro). |
| R2 | As datas do período são comparadas em **UTC**, mesma convenção do Dashboard (um pedido feito às 22h de um dia pode cair no dia seguinte). Limite conhecido, aceito. |
| R3 | Todo cálculo (totais, ticket médio, saldo, valor em estoque) é feito no servidor; a tela, o .xlsx e o .pdf mostram **os mesmos números** (saem da mesma consulta). |
| R4 | Exportação usa os mesmos filtros da tela; o arquivo traz título, data de geração, os filtros aplicados, o resumo e a tabela. Nome do arquivo: `relatorio-vendas-AAAA-MM-DD_AAAA-MM-DD.xlsx` (estoque: `relatorio-estoque-AAAA-MM-DD.xlsx`). |
| R5 | Sem paginação no relatório (é um relatório, não uma lista): o limite de 366 dias mantém o volume sob controle. |

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/relatorios/vendas?dataInicio=&dataFim=&status=&clienteId=&formato=` | `formato` = `json` (padrão), `xlsx` ou `pdf` |
| GET | `/api/relatorios/compras?dataInicio=&dataFim=&status=&fornecedorId=&formato=` | idem |
| GET | `/api/relatorios/estoque?categoriaId=&somenteAbaixoMinimo=&formato=` | idem |

Com `formato=xlsx`/`pdf` a resposta é o arquivo (download); com `json`, os dados para a tela.

## Telas

- Seção **Relatórios** no menu (depois de Financeiro): **Vendas** (`/relatorios/vendas`), **Compras** (`/relatorios/compras`), **Estoque** (`/relatorios/estoque`).
- Cada tela: filtros no topo + botão **Gerar**; cards de resumo; tabela; botões **Exportar Excel** e **Exportar PDF** (habilitados depois de gerar, usando os mesmos filtros).
- Vendas e Compras usam **um componente só** (mudam só o rótulo e o seletor de cliente/fornecedor).

## Arquitetura

- `RelatorioService`: as 3 consultas, devolvendo DTOs (linhas + resumo).
- `ExportadorRelatorio`: recebe um modelo genérico (título, filtros descritos, cards do resumo, colunas, linhas) e gera .xlsx ou .pdf. Os 3 relatórios passam por ele — **um exportador, não três**.
- `RelatoriosController`: valida os filtros, chama o service e devolve JSON ou `File(...)`.

## Testing strategy

1. **Unitário (xUnit):** validação do período (R1) e montagem do resumo (ticket médio com 0 pedidos, soma) — lógica pura.
2. **API ponta a ponta (instância temporária):** totais do JSON batem com consulta direta no banco; filtros de status/cliente/categoria; 400 para período inválido; .xlsx abre e tem as mesmas linhas/totais; .pdf é um PDF válido com o título e o total.
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; verificação visual com o Rafael.

## Boundaries

- **Sempre:** números calculados no servidor; mesma consulta para tela e arquivos; reaproveitar seletores (`SelecaoCliente`, `SelecaoFornecedor`) e o cálculo de ticket médio do Dashboard se servir.
- **Perguntar antes:** qualquer dependência além de ClosedXML/QuestPDF; relatórios além dos três.
- **Nunca:** calcular totais no front; commitar segredos; push sem pedido.

## Success criteria (testáveis)

1. Relatório de vendas de um período traz só pedidos daquele período e status, com quantidade, soma e ticket médio batendo com o banco.
2. Mesmo para compras, com filtro por fornecedor.
3. Relatório de estoque traz saldo, valor em estoque (saldo × custo) e marcação abaixo do mínimo, batendo com o banco; filtros de categoria e "só abaixo do mínimo" funcionam.
4. Período inválido (final antes do inicial, ou > 366 dias, ou faltando) retorna 400.
5. `formato=xlsx` devolve um .xlsx válido com as mesmas linhas e totais do JSON.
6. `formato=pdf` devolve um PDF válido com título, filtros e totais.
7. Menu Relatórios com as 3 telas; exportar baixa o arquivo com o nome certo.
8. `dotnet build` 0 avisos, `dotnet test` verde, `tsc -b`/`oxlint`/`npm run build` limpos.
