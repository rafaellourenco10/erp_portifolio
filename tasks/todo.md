# Checklist: Módulo Contas a Pagar (etapa 8)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Enum + Model + Migration** (S) — *concluída em 23/09/2026*
  - `Models/StatusParcelaPagar.cs`, `Models/ParcelaPagar.cs`, mapeamento no `ErpPortfolioDbContext` (espelho de `ParcelaReceber`), migration `CriacaoTabelaParcelasPagar` aplicada.
  - Verificar: `dotnet build` 0 avisos; `dotnet ef database update` sem erro.
  - Resultado: migration `20260923131654_CriacaoTabelaParcelasPagar` gerada e aplicada no banco de dev (com `--configuration Release`, porque a API em Debug estava rodando e travava a DLL). `dotnet build -c Release` 0 avisos.

- [ ] **T2: ContasPagarService + Controller** (M)
  - `IContasPagarService`/`ContasPagarService` (Listar, MarcarPaga, GerarParcelas, CancelarPendentesAsync, ObterResumo), DTOs (`ParcelaPagarRespostaDto`, `FiltroStatusParcelaPagar`, `ContasPagarResumoDto`; filtro reaproveita o formato de `ParcelaFiltroDto`), `ContasPagarController`, registro no DI.
  - Verificar: build; E2E de listar/pagar feito na T3 (precisa de parcelas geradas).

- [ ] **T3: Pedido de Compra gera/cancela parcelas** (M)
  - `PedidoCompraService.ConfirmarAsync` recebe nº de parcelas/intervalo e chama `GerarParcelas`; `CancelarAsync` de Confirmado chama `CancelarPendentesAsync` depois da checagem de saldo; controller aceita `PedidoConfirmarDto`.
  - Verificar: E2E dos critérios 1-6 da spec contra a API real.

- [ ] **T4: Resumo no Dashboard (API)** (S)
  - `GET /api/dashboard/contas-pagar`.
  - Verificar: E2E do critério 7.

## Fase 2: Frontend

- [ ] **T5: Tela Contas a Pagar** (M)
  - `types/contaPagar.ts`, `api/contasPagarApi.ts`, `hooks/useContasPagar.ts`, `pages/ContasPagar/ContasPagarListaPage.tsx`, tag de status com "Pago", rota `/contas-pagar` e item no menu Financeiro.
  - Verificar: `tsc -b`, `oxlint`.

- [ ] **T6: Modal de parcelas ao confirmar compra** (S)
  - Confirmar pedido de compra abre o modal de parcelas; `pedidosCompraApi` envia o corpo.
  - Verificar: `tsc -b`, `oxlint`; fluxo da venda inalterado.

- [ ] **T7: Card "A pagar" no Dashboard** (S)
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T8: Documentação e verificação final** (S)
  - README (seção etapa 8), SPEC.md marcada como implementada com os 8 critérios conferidos, `python -m graphify update .`, memória atualizada.
