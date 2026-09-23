# Checklist: Módulo Comissões (etapa 11)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Model + migration + cálculo** (S) — *concluída em 23/09/2026*
  - `Models/Comissao.cs`, `Models/StatusComissao.cs`, mapeamento, migration `CriacaoTabelaComissoes` com a carga das parcelas já recebidas (CM3); `ComissaoCalculo.Valor` com xUnit.
  - Verificar: build 0 avisos; migration aplicada; `dotnet test` verde.
  - Resultado: `Models/Comissao.cs` (enum `StatusComissao` no mesmo arquivo), mapeamento com índice único em `parcela_receber_id` e FKs restrict, migration `20260923181059_CriacaoTabelaComissoes` aplicada com a carga do CM3 em SQL (0 linhas no banco de dev: não havia parcela recebida de pedido com vendedor; a SQL é exercitada no E2E da T3). `ComissaoCalculo.Valor` + 5 casos xUnit (inclusive 0,505 → 0,51): `dotnet test` 177/177. Build 0 avisos.

- [ ] **T2: Gerar comissão ao receber parcela** (S)
  - `ContasReceberService.MarcarRecebidaAsync` gera a comissão (CM1/CM2) no mesmo SaveChanges.
  - Verificar: build; E2E na T3.

- [ ] **T3: API de comissões** (M)
  - DTOs, `IComissaoService`/`ComissaoService` (listar com totais do filtro, pagar em lote), `ComissoesController`, DI.
  - Verificar: E2E dos critérios 1-6 contra instância temporária.

## Fase 2: Frontend

- [ ] **T4: Tela Comissões** (M)
  - Tipos, api, hooks, `pages/Comissoes/ComissoesListaPage.tsx` (filtros, cards, tabela com seleção, marcar como pagas), item **Comissões** em Financeiro.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T5: Documentação e verificação final** (S)
  - README (etapa 11), SPEC marcada como implementada, `python -m graphify update .`, memória.
