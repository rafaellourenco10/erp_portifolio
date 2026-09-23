# Checklist: Módulo Relatórios (etapa 9)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: RelatorioService + Controller (JSON)** (M) — *concluída em 23/09/2026*
  - DTOs de filtro (vendas/compras/estoque) e de resposta (linhas + resumo); validação do período (R1); `RelatorioService` com as 3 consultas; `RelatoriosController` com `formato=json`; xUnit do período e do resumo.
  - Verificar: build 0 avisos; `dotnet test` verde.
  - Resultado: build 0 avisos; `dotnet test` 158/158 (8 novos em `RelatorioCalculoTests`: período de 1 dia, invertido, 366/367 dias, resumo vazio e com valores, validação pelo DTO). Teste rápido numa instância temporária (5099): os 3 relatórios devolvem linhas e resumo coerentes com os dados reais; período invertido e datas ausentes → 400 no campo certo. "Abaixo do mínimo" usa a regra do Dashboard (saldo ≤ mínimo).

- [ ] **T2: Exportação Excel** (M)
  - Pacote ClosedXML; `ExportadorRelatorio` com modelo genérico → .xlsx (título, gerado em, filtros, resumo, tabela com moeda/data formatadas); `formato=xlsx` com nome de arquivo (R4).
  - Verificar: build; E2E na T3.

- [ ] **T3: Exportação PDF** (M)
  - Pacote QuestPDF (licença Community em `Program.cs`); mesmo modelo → .pdf (A4, cabeçalho, resumo, tabela, rodapé com página).
  - Verificar: E2E dos critérios 1-6 contra instância temporária da API.

## Fase 2: Frontend

- [ ] **T4: Base do front + menu** (S)
  - `types/relatorio.ts`, `api/relatoriosApi.ts` (JSON + download de blob com o nome do `Content-Disposition`), hooks, seção **Relatórios** no menu com as 3 rotas; CORS expõe `Content-Disposition`.
  - Verificar: `tsc -b`, `oxlint`.

- [ ] **T5: Tela de Vendas e Compras** (M)
  - Componente único com filtros (período, status, cliente/fornecedor), resumo, tabela e botões de exportar; duas rotas.
  - Verificar: `tsc -b`, `oxlint`.

- [ ] **T6: Tela de Estoque** (S)
  - Filtros (categoria, só abaixo do mínimo), resumo, tabela, exportar.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T7: Documentação e verificação final** (S)
  - README (etapa 9, dependências novas e licenças), SPEC marcada como implementada, `python -m graphify update .`, memória.
