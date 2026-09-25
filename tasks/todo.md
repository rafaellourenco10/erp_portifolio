# Checklist: Fluxo de caixa (etapa 15)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: FluxoCaixaCalculo** (S) — *concluída em 24/09/2026*
  - Períodos (dia/mês, com vazios e meses parciais nas pontas); realizado × previsto; atrasadas em hoje; saldo inicial (inclusive período futuro); saldo acumulado; menor saldo; validação do período (FC7). xUnit.
  - Verificar: `dotnet test`.
  - Resultado: `Services/FluxoCaixaCalculo.cs` (`ErroPeriodo` com o limite de 93 dias no diário, `DiaPrevisto` = vencimento ou hoje se atrasada, `Montar` = saldo inicial com o que vem antes do início, um período por dia/mês com vazios, saldo acumulado e menor saldo — no empate, o primeiro). `FluxoCaixaCalculoTests` (+15). Build 0 avisos, `dotnet test` 246/246.

- [x] **T2: API do fluxo de caixa** (M) — *concluída em 24/09/2026*
  - DTOs, `FluxoCaixaService` (consulta das parcelas + somas antes do início), `GET /api/fluxo-caixa` com `formato=json|xlsx|pdf`.
  - Verificar: build 0 avisos; E2E contra instância temporária na 5099 (dados `ZZT…` apagados): critérios 1-5 da SPEC.
  - Resultado: `FluxoCaixaFiltroDto` (período obrigatório, validado pelo `FluxoCaixaCalculo.ErroPeriodo`), `FluxoCaixaDto` (resumo, atrasados, `hoje` e os períodos do cálculo), `FluxoCaixaService` (2 `SUM` no banco para o realizado antes do início; recebidas/pagas do período convertidas para o dia de Brasília; pendentes até o fim no vencimento ou hoje) e `ModeloAsync` para o Excel/PDF; `FluxoCaixaController` em `/api/fluxo-caixa` (JSON ou arquivo pelo `ExportadorRelatorio.Arquivo`). E2E (`.claude/ferramentas-locais/e2e-fluxo-caixa.ps1`, venda 300,00 em 3x + avulsa 100,00 em 2x): **28/28** — cada coluna por dia comparada com a soma direta no banco; recebimento às 22:30 conta no dia anterior; atrasada em hoje e no aviso; saldo inicial de período futuro e passado; cadeia de saldos e menor saldo; mensal = diário; 6 validações; Excel/PDF. Dados de teste apagados, dados reais idênticos. Build 0 avisos.

## Fase 2: Frontend

- [x] **T3: Tela** (M) — *concluída em 24/09/2026*
  - Tipos/api/hook; rota `/fluxo-caixa` e item no menu Financeiro; período + Diário/Mensal; cards; aviso de atrasados; tabela; Excel/PDF.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.
  - Resultado: `types/fluxoCaixa.ts`, `api/fluxoCaixaApi.ts` (obter + exportar pelo `baixarArquivo`), `hooks/useFluxoCaixa.ts`, `pages/FluxoCaixa/FluxoCaixaPage.tsx` + `fluxoCaixa.css`. Abre calculado no mês atual (diário); trocar para Mensal vai para o ano atual (seletor de meses); período novo com "Gerar"; `BotoesExportar`. Cards Saldo inicial / Entradas / Saídas / Saldo final / Menor saldo (com a data, vermelho se negativo); alertas de saldo negativo e de atrasados; tabela com Entradas e Saídas (realizadas/previstas), Resultado e Saldo, linha de hoje destacada e trecho previsto mais apagado. Menu Financeiro → Fluxo de Caixa. `tsc -b`, `oxlint` e `npm run build` limpos.

- [ ] **T4: Gráfico** (M)
  - SVG: barras de entradas e saídas (previsto mais claro), linha do saldo, marca de hoje, tooltip; teste de tela com Playwright.
  - Verificar: critério 6 da SPEC no navegador.

## Fase 3: Fechamento

- [ ] **T5: Fechamento** (S)
  - README (etapa 15, rota, decisões), SPEC marcada como implementada, `graphify update`, memória.
  - Verificar: critério 7 da SPEC.
