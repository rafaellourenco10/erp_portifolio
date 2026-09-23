# Checklist: Módulo Comissões (etapa 11)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Model + migration + cálculo** (S) — *concluída em 23/09/2026*
  - `Models/Comissao.cs`, `Models/StatusComissao.cs`, mapeamento, migration `CriacaoTabelaComissoes` com a carga das parcelas já recebidas (CM3); `ComissaoCalculo.Valor` com xUnit.
  - Verificar: build 0 avisos; migration aplicada; `dotnet test` verde.
  - Resultado: `Models/Comissao.cs` (enum `StatusComissao` no mesmo arquivo), mapeamento com índice único em `parcela_receber_id` e FKs restrict, migration `20260923181059_CriacaoTabelaComissoes` aplicada com a carga do CM3 em SQL (0 linhas no banco de dev: não havia parcela recebida de pedido com vendedor; a SQL é exercitada no E2E da T3). `ComissaoCalculo.Valor` + 5 casos xUnit (inclusive 0,505 → 0,51): `dotnet test` 177/177. Build 0 avisos.

- [x] **T2: Gerar comissão ao receber parcela** (S) — *concluída em 23/09/2026*
  - `ContasReceberService.MarcarRecebidaAsync` gera a comissão (CM1/CM2) no mesmo SaveChanges.
  - Verificar: build; E2E na T3.
  - Resultado: `GerarComissao` no `ContasReceberService`, chamado só quando a parcela muda para Recebido (receber de novo não passa por ele); usa a % do pedido; sem vendedor/% ou valor 0 não gera. Mesmo SaveChanges do recebimento. Corrida de duas chamadas simultâneas marcada com `ponytail:` (o índice único impede duplicar). Build 0 avisos; comportamento no E2E da T3.

- [x] **T3: API de comissões** (M) — *concluída em 23/09/2026*
  - DTOs, `IComissaoService`/`ComissaoService` (listar com totais do filtro, pagar em lote), `ComissoesController`, DI.
  - Verificar: E2E dos critérios 1-6 contra instância temporária.
  - Resultado: E2E (instância temporária na 5099; dados `ZZT…` apagados; dados reais intactos): 18 verificações OK — 350 a 5% congelado gera 17,50 mesmo com o vendedor já em 9%; receber de novo não duplica; 3 parcelas de 1000 a 5,5% geram 3× 18,33; 0% e pedido sem vendedor não geram; a SQL de carga da migration (CM3) gera 16,50 para parcela recebida "antes"; lista, filtro por vendedor/status/período e totais batendo com SQL; período invertido 400; pagar em lote, idempotente, id inexistente 400 sem alterar nada, lista vazia 400; cancelar pedido mantém a comissão. Totais dos cards ignoram o filtro de status de propósito (quadro do vendedor/período).

## Fase 2: Frontend

- [x] **T4: Tela Comissões** (M) — *concluída em 23/09/2026*
  - Tipos, api, hooks, `pages/Comissoes/ComissoesListaPage.tsx` (filtros, cards, tabela com seleção, marcar como pagas), item **Comissões** em Financeiro.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.
  - Resultado: `types/comissao.ts`, `api/comissoesApi.ts`, `hooks/useComissoes.ts`, `pages/Comissoes/ComissoesListaPage.tsx`: filtros aplicados na hora (vendedor com `SelecaoVendedor`, período do recebimento, status A pagar/Pagas), cards Gerado / A pagar / Pago, tabela com seleção só das pendentes, "Marcar como pagas (N)" com o total no Popconfirm e ação por linha; seleção limpa ao filtrar/paginar/pagar. Item **Comissões** em Financeiro (`PercentageOutlined`). `tsc -b`, `oxlint` e `npm run build` limpos.

## Fase 3: Fechamento

- [x] **T5: Documentação e verificação final** (S) — *concluída em 23/09/2026*
  - README (etapa 11), SPEC marcada como implementada, `python -m graphify update .`, memória.
  - Resultado: README com funcionalidades, estrutura, API, tabela `comissoes`, testes e próximas etapas; etapa 10 marcada como telas confirmadas pelo Rafael. SPEC 7/7 ✅. Grafo atualizado.
