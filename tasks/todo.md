# Checklist: Módulo Contas a Pagar (etapa 8)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Enum + Model + Migration** (S) — *concluída em 23/09/2026*
  - `Models/StatusParcelaPagar.cs`, `Models/ParcelaPagar.cs`, mapeamento no `ErpPortfolioDbContext` (espelho de `ParcelaReceber`), migration `CriacaoTabelaParcelasPagar` aplicada.
  - Verificar: `dotnet build` 0 avisos; `dotnet ef database update` sem erro.
  - Resultado: migration `20260923131654_CriacaoTabelaParcelasPagar` gerada e aplicada no banco de dev (com `--configuration Release`, porque a API em Debug estava rodando e travava a DLL). `dotnet build -c Release` 0 avisos.

- [x] **T2: ContasPagarService + Controller** (M) — *concluída em 23/09/2026*
  - `IContasPagarService`/`ContasPagarService` (Listar, MarcarPaga, GerarParcelas, CancelarPendentesAsync, ObterResumo), DTOs (`ParcelaPagarRespostaDto`, `FiltroStatusParcelaPagar`, `ContasPagarResumoDto`; filtro reaproveita o formato de `ParcelaFiltroDto`), `ContasPagarController`, registro no DI.
  - Verificar: build; E2E de listar/pagar feito na T3 (precisa de parcelas geradas).
  - Resultado: `dotnet build -c Release` 0 avisos. O filtro virou um DTO próprio (`ParcelaPagarFiltroDto`), porque o `ParcelaFiltroDto` usa o enum com "Recebido". Comportamento verificado por E2E na T3.

- [x] **T3: Pedido de Compra gera/cancela parcelas** (M) — *concluída em 23/09/2026*
  - `PedidoCompraService.ConfirmarAsync` recebe nº de parcelas/intervalo e chama `GerarParcelas`; `CancelarAsync` de Confirmado chama `CancelarPendentesAsync` depois da checagem de saldo; controller aceita `PedidoConfirmarDto`.
  - Verificar: E2E dos critérios 1-6 da spec contra a API real.
  - Resultado: E2E contra instância temporária (Release, porta 5099; a API do Rafael na 5065 ficou intacta) com dados `ZZT…`: 26 verificações OK cobrindo os critérios 1-7 (3x30 → 333,33/333,33/333,34 em +30/+60/+90; sem corpo → 1x30; 0/13 parcelas e 0/181 dias → 400 sem confirmar; busca por nº e fornecedor; filtros Pendente/Pago/Atrasado; pagar idempotente, 404, 409 em cancelada; cancelar compra → Pago intacta e Pendentes canceladas; cancelamento bloqueado por saldo não mexe nas parcelas). Dados de teste apagados, `parcelas_pagar` vazia, pedido #4 intacto. `dotnet test` 150/150.

- [x] **T4: Resumo no Dashboard (API)** (S) — *concluída em 23/09/2026*
  - `GET /api/dashboard/contas-pagar`.
  - Verificar: E2E do critério 7.
  - Resultado: testado no mesmo E2E da T3 — resumo (600,00 em 3 pendentes, 100,00 em 1 atrasada) bateu com a soma direto no banco.

## Fase 2: Frontend

- [x] **T5: Tela Contas a Pagar** (M) — *concluída em 23/09/2026*
  - `types/contaPagar.ts`, `api/contasPagarApi.ts`, `hooks/useContasPagar.ts`, `pages/ContasPagar/ContasPagarListaPage.tsx`, tag de status com "Pago", rota `/contas-pagar` e item no menu Financeiro.
  - Verificar: `tsc -b`, `oxlint`.
  - Resultado: tela gerada a partir da de Contas a Receber (mesmo layout e colunas, trocando cliente/pedido por fornecedor/compra e "Recebido" por "Pago"); `TagStatusParcela` passou a aceitar "Pago" em vez de ganhar uma cópia; item Contas a Pagar (`WalletOutlined`) em Financeiro. `tsc -b` e `oxlint` limpos.

- [x] **T6: Modal de parcelas ao confirmar compra** (S) — *concluída em 23/09/2026*
  - Confirmar pedido de compra abre o modal de parcelas; `pedidosCompraApi` envia o corpo.
  - Verificar: `tsc -b`, `oxlint`; fluxo da venda inalterado.
  - Resultado: modal extraído para `components/ModalParcelas.tsx` e usado pelas duas telas (a venda só trocou o JSX do modal pelo componente: mesmo título, texto, limites e padrão 1/30; os campos agora voltam ao padrão no `afterClose` em vez de ao abrir). `pedidosCompraApi.confirmar` e `useConfirmarPedidoCompra` enviam o corpo. `tsc -b` e `oxlint` limpos.

- [x] **T7: Card "A pagar" no Dashboard** (S) — *concluída em 23/09/2026*
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.
  - Resultado: card "Contas a pagar" ao lado do "Contas a receber"; o corpo dos dois virou a função `ResumoContas` (mesmo visual). Com 5 cards, a grade passou a 2 linhas no desktop: financeiro (Faturamento, A receber, A pagar — `lg=8`) e operação (Pedidos por status, Saldo baixo — `lg=12`). `tsc -b`, `oxlint` e `npm run build` limpos (o aviso de chunk > 500 kB já existia).

## Fase 3: Fechamento

- [x] **T8: Documentação e verificação final** (S) — *concluída em 23/09/2026*
  - README (seção etapa 8), SPEC.md marcada como implementada com os 8 critérios conferidos, `python -m graphify update .`, memória atualizada.
  - Resultado: README com funcionalidades, estrutura, API, tabela `parcelas_pagar` e testes da etapa 8; introdução e menções ao menu atualizadas para os departamentos (Cadastro, Ordem Vendas/Compras, Depósito, Financeiro). SPEC marcada como implementada, 8/8 critérios ✅. Grafo atualizado.
