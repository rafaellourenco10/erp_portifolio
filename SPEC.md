# Spec: Módulo Dashboard (etapa 6)

> Status: **em definição** (22/09/2026). Substitui a spec de Contas a Receber (etapa 5, implementada e documentada no README). Ao mudar uma decisão depois de começar a codar, atualize esta spec **antes** do código.

## Objetivo

Tela inicial com indicadores agregados de Pedidos, Estoque e Contas a Receber, para responder "como estou indo" sem precisar abrir cada módulo.

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** Pedidos, Estoque e Contas a Receber existem e têm dado real; antes disso o Dashboard só mostraria número fictício.
- **Sucesso:** abrir o Dashboard mostra faturamento e ticket médio do mês, pedidos por status, contas a receber pendentes/atrasadas e produtos com saldo baixo, tudo batendo com o que aparece nos módulos de origem.

### Dentro do escopo
Cards de indicador (faturamento do mês, ticket médio, pedidos por status, contas a receber pendente/atrasado, produtos com saldo baixo) e um gráfico de faturamento diário do mês atual. Só leitura — nenhuma ação a partir do Dashboard.

### Fora do escopo (entram depois)
Seletor de período (o mês é sempre o atual), drill-down/exportação, estoque mínimo configurável por produto (o limite de "saldo baixo" é fixo no código por enquanto), cache/agregação pré-calculada (tudo é consulta direta, sem tabela nova).

## Decisões já tomadas (com o Rafael, 22/09/2026)

| Tema | Decisão |
|---|---|
| Indicadores da v1 | Faturamento + ticket médio, pedidos por status, contas a receber pendente/atrasado, produtos com saldo baixo de estoque. |
| Período | **Mês atual, fixo** — sem seletor de data nesta versão. |
| Contas a receber no Dashboard | Mostra o **saldo em aberto agora** (não filtrado por mês) — parcelas vencem em datas futuras variadas, filtrar por "mês da venda" não faz sentido para elas. |
| Visual | Cards de número **+ 1 gráfico**: faturamento diário do mês atual (linha/barra). |
| Saldo baixo de estoque | Limite **fixo no código** (`saldo ≤ 5`), igual para todos os produtos — não existe estoque mínimo por produto hoje. |
| Arquitetura dos cards | Cada card **independente**: se um indicador falhar, os outros continuam aparecendo. Endpoints agrupados por módulo de origem (Pedidos, Estoque, Contas a Receber), não um endpoint único. |

## Regras de negócio

| # | Regra |
|---|---|
| D1 | "Faturamento do mês" = soma de `valorTotal` dos pedidos **Confirmados** com `dataPedido` no mês/ano atual (UTC). Pedidos Rascunho e Cancelado não entram. |
| D2 | "Ticket médio" = faturamento do mês ÷ quantidade de pedidos Confirmados no mês; `0` se não houver nenhum. |
| D3 | "Pedidos por status" conta **todos** os pedidos (qualquer status) com `dataPedido` no mês atual, agrupados por status. |
| D4 | "Faturamento diário" = faturamento (D1) agrupado por dia do mês atual, um ponto por dia (dias sem venda confirmada entram com 0, para o gráfico não ter buracos). |
| D5 | "Contas a receber pendente" = soma de `valor` das parcelas com status `Pendente` (todas, não só do mês); "atrasado" = subconjunto com `vencimento` no passado (mesmo cálculo do módulo de Contas a Receber). |
| D6 | "Saldo baixo de estoque" = quantidade de produtos **ativos** cujo saldo (Σ Entrada − Σ Saída) é **≤ 5** (constante `LimiteSaldoBaixo`). |
| D7 | Cada indicador vem de um endpoint próprio, agrupado por módulo de origem; a tela busca os três em paralelo e mostra cada card assim que a resposta dele chega (não espera todos). |

## Tech stack

Igual aos módulos anteriores: ASP.NET Core (.NET 10) + EF Core + PostgreSQL 18 no back; React 19 + Vite + TypeScript + Ant Design 6 + TanStack Query no front. **Uma dependência nova no front**: biblioteca de gráfico (a decidir na implementação, seguindo a skill de dataviz do projeto — provavelmente algo leve tipo Ant Design Charts ou Recharts).

## Modelo de dados

**Nenhuma tabela nova.** Tudo consulta agregada sobre `pedidos`, `parcelas_receber`, `produtos` e `estoque_movimentacoes` já existentes.

## API

Três endpoints pequenos, um por módulo de origem, sem controller/service novo — cada método entra no service que já existe (`PedidoService`, `ContasReceberService`, `EstoqueService`), atrás de um `DashboardController` fino que só delega:

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/dashboard/vendas` | Faturamento do mês, ticket médio, pedidos por status, faturamento diário (D1-D4) | 200 |
| GET | `/dashboard/contas-receber` | Total e quantidade pendente/atrasado (D5) | 200 |
| GET | `/dashboard/estoque` | Quantidade de produtos com saldo baixo (D6) | 200 |

Sem parâmetros (mês atual é sempre calculado no servidor, `DateTime.UtcNow`). Sem entrada do cliente, sem erro de validação esperado.

## Telas

- **`/` (nova rota inicial)** — item **Painel** no menu, **fora** da seção "Gestão Comercial" (fica no topo, sozinho — é um resumo entre módulos, não uma ação comercial). A rota desconhecida (`*`) passa a cair em `/` em vez de `/clientes`.
- Grid de cards: Faturamento do mês, Ticket médio, Pedidos por status (3 números: Rascunho/Confirmado/Cancelado), Contas a receber pendente (com o atrasado destacado), Produtos com saldo baixo.
- Gráfico de faturamento diário do mês, abaixo dos cards.
- Cada card mostra seu próprio loading/erro (D7) — um indicador falhando não derruba a tela inteira.

## Commands

```
# Backend (na raiz)
dotnet build ErpPortfolio.slnx -c Release
dotnet run --project backend/ErpPortfolio.Api --launch-profile http

# Frontend (em frontend/erp-portfolio-web)
npm install <biblioteca-de-grafico-escolhida>
npm run dev
npx tsc -b
npx oxlint src
npm run build

# Testes automatizados
dotnet test backend/ErpPortfolio.Tests
```

## Project structure (só o que é novo/alterado)

```
backend/ErpPortfolio.Api/
  DTOs/              VendasResumoDto.cs, FaturamentoDiaDto.cs, ContasReceberResumoDto.cs, EstoqueResumoDashboardDto.cs
  Services/          IPedidoService.cs / PedidoService.cs       # + ObterResumoVendasAsync
                     IContasReceberService.cs / ContasReceberService.cs  # + ObterResumoAsync
                     IEstoqueService.cs / EstoqueService.cs     # + ObterResumoAsync
  Controllers/       DashboardController.cs
backend/ErpPortfolio.Tests/  DashboardResumoTests.cs (agrupamento por dia/status, cálculo de ticket médio)
frontend/erp-portfolio-web/src/
  api/dashboardApi.ts   hooks/useDashboard.ts   types/dashboard.ts
  pages/Dashboard/DashboardPage.tsx, CardIndicador.tsx, GraficoFaturamento.tsx
  App.tsx  # rota "/" e item de menu "Painel" fora de Gestão Comercial
```

## Code style

Igual ao restante do projeto: cabeçalho obrigatório em todo arquivo C#/TS, nomes em português, mensagens em português.

## Testing strategy

1. **Unitário (xUnit):** ticket médio com 0 pedidos (não divide por zero), agrupamento de faturamento por dia preenchendo dias sem venda com 0, contagem de saldo baixo com o limite exato (5 entra, 6 não).
2. **API ponta a ponta (instância temporária):** os três endpoints batendo com pedidos/parcelas/produtos de teste criados na mesma rodada (faturamento e ticket médio conferem com pedidos confirmados no mês, contas a receber bate com parcelas pendentes/atrasadas, saldo baixo bate com produtos de teste no limite).
3. **Tela:** revisão de código (mesma limitação das últimas rodadas — sem navegador nesta sessão).
4. **Sempre:** `dotnet build` 0 avisos, `tsc -b`/`oxlint` sem apontamentos; nenhum dado real alterado (o módulo é só leitura, mas os dados de teste usados para conferir os números precisam ser limpos).

## Boundaries

- **Sempre:** todo cálculo de dinheiro/contagem no servidor (a tela só exibe); cada endpoint independente dos outros; cabeçalho em cada arquivo; testar em instância temporária e limpar os dados de teste; atualizar README e graphify ao terminar; `git commit` só quando o Rafael pedir (ou autorização já dada, como nos módulos anteriores).
- **Perguntar antes:** dependência nova de gráfico (decidir qual na hora, mas é a única autorizada por esta spec — qualquer outra dependência nova precisa perguntar de novo), seletor de período, estoque mínimo por produto, qualquer ação (não só leitura) no Dashboard.
- **Nunca:** gravar um valor agregado como se fosse fonte de verdade (sempre recalcular), commitar segredos, forçar push.

## Success criteria (testáveis)

1. Faturamento do mês soma exatamente o `valorTotal` dos pedidos Confirmados com `dataPedido` no mês atual; pedidos Rascunho/Cancelado não entram.
2. Ticket médio = faturamento ÷ quantidade de Confirmados no mês; com 0 confirmados, o valor é `0` (sem erro).
3. Pedidos por status conta certo os três status, só os do mês atual.
4. Faturamento diário tem um ponto por dia do mês, incluindo dias sem venda (valor 0), sem buraco no gráfico.
5. Contas a receber pendente/atrasado bate com a soma das parcelas `Pendente`/atrasadas reais, sem filtro de mês.
6. Produto com saldo exatamente 5 conta como saldo baixo; saldo 6 não conta.
7. Cada endpoint responde de forma independente; um erro num não impede os outros dois de aparecer na tela.
8. Nenhum registro real é alterado pelos testes (módulo só leitura; dados de teste usados na verificação são apagados).

## Open questions

Nenhuma em aberto — as quatro dúvidas da primeira versão foram fechadas em 22/09/2026 (ver "Decisões já tomadas"). Limites conhecidos, aceitos de propósito:

- **Sem seletor de período**: sempre o mês atual; um filtro de data é uma extensão natural (mais um parâmetro nos três endpoints).
- **Limite de saldo baixo fixo (5)**: até existir um campo de estoque mínimo por produto.
- **Biblioteca de gráfico** a escolher na implementação, seguindo a skill de dataviz do projeto.
