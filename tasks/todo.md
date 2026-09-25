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

- [x] **T4: Gráfico** (M) — *concluída em 24/09/2026*
  - SVG: barras de entradas e saídas (previsto mais claro), linha do saldo, marca de hoje, tooltip; teste de tela com Playwright.
  - Verificar: critério 6 da SPEC no navegador.
  - Resultado: `GraficoFluxoCaixa.tsx` em **dois painéis com o mesmo eixo X** (linha do saldo em cima; colunas de entradas para cima e saídas para baixo embaixo) — a skill de dataviz proíbe dois eixos Y no mesmo plot. Cores validadas pelo `validate_palette.js` no fundo escuro: azul `#3987E5` × laranja `#D95926` (CVD ΔE 26,8); verde × vermelho reprovava (ΔE 3,7 para deuteranopia). Previsto com 45% de opacidade empilhado depois do realizado (2px de fundo), ponta arredondada de 4px, marca de hoje, crosshair e tooltip por mouse e teclado, legenda; no celular rola de lado. Ajustes vistos nas capturas: rótulo final do eixo sobreposto, card "Menor saldo" quebrando o valor, rótulo "hoje" encostando na linha. Teste de tela (`.claude/ferramentas-locais/ui-fluxo-caixa-run.ps1` + `ui-fluxo-caixa.mjs`, com uma conta grande para o saldo ficar negativo): **17/17** — menu, mês atual com linha de hoje, cards e menor saldo = API, alertas de negativo e de atrasados, gráfico (legenda, alvos, hover, teclado, hoje), Mensal com 12 meses, Excel, celular sem rolagem da página, console limpo; dados de teste apagados e reais idênticos. `tsc -b`, `oxlint`, `npm run build` limpos.

## Fase 3: Fechamento

- [x] **T5: Fechamento** (S) — *concluída em 24/09/2026*
  - README (etapa 15, rota, decisões), SPEC marcada como implementada, `graphify update`, memória.
  - Verificar: critério 7 da SPEC.
  - Resultado: README com a etapa 15 (tabela de etapas, introdução, funcionalidades, rota, estrutura de pastas, seção de testes) e "Próximas etapas" sem os itens já feitos hoje (exportar Comissões, fuso de Brasília); login passa a etapa 16. SPEC com os critérios 1-7 conferidos. Grafo gravado com `grava_grafo_etapa15.py` (6 conceitos novos: decisões do fluxo, cálculo puro, gráfico em dois painéis, HorarioBrasilia, ExportadorRelatorio.Arquivo, BotoesExportar; nenhum conceito perdido, 22 hiperarestas) e HTML exportado. Build 0 avisos, `dotnet test` 246/246, `tsc -b`/`oxlint`/`npm run build` limpos.
