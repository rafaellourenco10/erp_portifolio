# Checklist: Fluxo de caixa (etapa 15)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: FluxoCaixaCalculo** (S) — *concluída em 24/09/2026*
  - Períodos (dia/mês, com vazios e meses parciais nas pontas); realizado × previsto; atrasadas em hoje; saldo inicial (inclusive período futuro); saldo acumulado; menor saldo; validação do período (FC7). xUnit.
  - Verificar: `dotnet test`.
  - Resultado: `Services/FluxoCaixaCalculo.cs` (`ErroPeriodo` com o limite de 93 dias no diário, `DiaPrevisto` = vencimento ou hoje se atrasada, `Montar` = saldo inicial com o que vem antes do início, um período por dia/mês com vazios, saldo acumulado e menor saldo — no empate, o primeiro). `FluxoCaixaCalculoTests` (+15). Build 0 avisos, `dotnet test` 246/246.

- [ ] **T2: API do fluxo de caixa** (M)
  - DTOs, `FluxoCaixaService` (consulta das parcelas + somas antes do início), `GET /api/fluxo-caixa` com `formato=json|xlsx|pdf`.
  - Verificar: build 0 avisos; E2E contra instância temporária na 5099 (dados `ZZT…` apagados): critérios 1-5 da SPEC.

## Fase 2: Frontend

- [ ] **T3: Tela** (M)
  - Tipos/api/hook; rota `/fluxo-caixa` e item no menu Financeiro; período + Diário/Mensal; cards; aviso de atrasados; tabela; Excel/PDF.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

- [ ] **T4: Gráfico** (M)
  - SVG: barras de entradas e saídas (previsto mais claro), linha do saldo, marca de hoje, tooltip; teste de tela com Playwright.
  - Verificar: critério 6 da SPEC no navegador.

## Fase 3: Fechamento

- [ ] **T5: Fechamento** (S)
  - README (etapa 15, rota, decisões), SPEC marcada como implementada, `graphify update`, memória.
  - Verificar: critério 7 da SPEC.
