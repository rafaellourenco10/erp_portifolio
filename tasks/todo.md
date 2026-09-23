# Checklist: Módulo Relatórios (etapa 9)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: RelatorioService + Controller (JSON)** (M) — *concluída em 23/09/2026*
  - DTOs de filtro (vendas/compras/estoque) e de resposta (linhas + resumo); validação do período (R1); `RelatorioService` com as 3 consultas; `RelatoriosController` com `formato=json`; xUnit do período e do resumo.
  - Verificar: build 0 avisos; `dotnet test` verde.
  - Resultado: build 0 avisos; `dotnet test` 158/158 (8 novos em `RelatorioCalculoTests`: período de 1 dia, invertido, 366/367 dias, resumo vazio e com valores, validação pelo DTO). Teste rápido numa instância temporária (5099): os 3 relatórios devolvem linhas e resumo coerentes com os dados reais; período invertido e datas ausentes → 400 no campo certo. "Abaixo do mínimo" usa a regra do Dashboard (saldo ≤ mínimo).

- [x] **T2: Exportação Excel** (M) — *concluída em 23/09/2026*
  - Pacote ClosedXML; `ExportadorRelatorio` com modelo genérico → .xlsx (título, gerado em, filtros, resumo, tabela com moeda/data formatadas); `formato=xlsx` com nome de arquivo (R4).
  - Verificar: build; E2E na T3.
  - Resultado: ClosedXML 0.105.1. Modelo genérico em `Services/RelatorioModelo.cs` (colunas/campos com tipo: texto, inteiro, quantidade, moeda, data-hora, sim/não); `ExportadorRelatorio.GerarXlsx` grava valores tipados (o Excel soma e ordena), cabeçalho destacado, filtro automático e cabeçalho congelado. Datas exibidas no horário de Brasília. Teste rápido: .xlsx de vendas lido com um leitor só de biblioteca padrão (`zipfile`) trouxe título, filtros, resumo (2 / 1400 / 700) e as 2 linhas iguais ao JSON; `Content-Disposition` com `relatorio-vendas-2026-09-01_2026-09-30.xlsx`. `dotnet build` 0 avisos.

- [x] **T3: Exportação PDF** (M) — *concluída em 23/09/2026*
  - Pacote QuestPDF (licença Community em `Program.cs`); mesmo modelo → .pdf (A4, cabeçalho, resumo, tabela, rodapé com página).
  - Verificar: E2E dos critérios 1-6 contra instância temporária da API.
  - Resultado: QuestPDF 2026.9.0 (licença Community em `Program.cs`). E2E (script Python, instância temporária na 5099, só leitura dos dados reais — nada criado nem apagado): 25 verificações OK — vendas/compras/estoque batem com SQL direto no banco (linhas, somas, ticket médio, saldo × custo, abaixo do mínimo), filtros de status/cliente/fornecedor/categoria/abaixo do mínimo, período de 1 dia inclusivo, 400 para período invertido/367 dias/datas ausentes/formato inválido, .xlsx dos 3 relatórios com as mesmas linhas e total do JSON, .pdf válido com nome certo (inclusive período sem registros). PDFs abertos e conferidos visualmente (vendas, compras, estoque em paisagem, vazio); coluna Data alargada para a hora não quebrar. `dotnet test` 158/158.

## Fase 2: Frontend

- [x] **T4: Base do front** (S) — *concluída em 23/09/2026*
  - `types/relatorio.ts`, `api/relatoriosApi.ts` (JSON + download de blob com o nome do `Content-Disposition`), hooks, seção **Relatórios** no menu com as 3 rotas; CORS expõe `Content-Disposition`.
  - Verificar: `tsc -b`, `oxlint`.
  - Resultado: tipos, `relatoriosApi` (JSON + download de blob com nome do `Content-Disposition`; erro em blob convertido para JSON antes do `lerErroApi`), `useRelatorios` (consulta só roda após "Gerar"), CORS com `WithExposedHeaders("Content-Disposition")`. `SelecaoCliente`/`SelecaoFornecedor` ganharam `aoLimpar` opcional (botão de limpar = "Todos"; telas de pedido não passam e ficam iguais). `dayjs` declarado no `package.json` (já vinha instalado como dependência do Ant Design, 1.11.23 — nada novo baixado nem no bundle) para o seletor de período. O menu e as rotas foram para as T5/T6, junto com as telas. `tsc -b`, `oxlint`, `dotnet build` limpos.

- [x] **T5: Tela de Vendas e Compras** (M) — *concluída em 23/09/2026*
  - Componente único com filtros (período, status, cliente/fornecedor), resumo, tabela e botões de exportar; duas rotas.
  - Verificar: `tsc -b`, `oxlint`.
  - Resultado: `pages/Relatorios/RelatorioPedidosPage.tsx` (prop `tipo`), rotas `/relatorios/vendas` e `/relatorios/compras` com `key` diferente (trocar de uma para outra recria a tela e não leva o cliente como fornecedor), seção **Relatórios** no menu. Período padrão: dia 1 do mês até hoje; status padrão Confirmado; cliente/fornecedor com botão de limpar ("Todos"). Exportar usa os filtros do último "Gerar". Idioma pt-BR do dayjs no `main.tsx` (calendário). `tsc -b` e `oxlint` limpos.

- [ ] **T6: Tela de Estoque** (S)
  - Filtros (categoria, só abaixo do mínimo), resumo, tabela, exportar.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T7: Documentação e verificação final** (S)
  - README (etapa 9, dependências novas e licenças), SPEC marcada como implementada, `python -m graphify update .`, memória.
