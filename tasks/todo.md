# Checklist: Comissão como conta a pagar + contas avulsas (etapa 12)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Schema** (M) — *concluída em 23/09/2026*
  - `ParcelaPagar`: `PedidoCompraId` nulo, `Origem`, `Descricao`, `Favorecido`, `VendedorId`, `TotalParcelas`; `Comissao.ParcelaPagarId`; `StatusComissao.EmPagamento`; migration `ContasPagarOrigemEComissoes` (preenche `total_parcelas`, CHECK de origem); compra passa a gravar `TotalParcelas`.
  - Verificar: build 0 avisos; migration aplicada; linhas existentes com origem `Compra` e total certo.
  - Resultado: migration `20260923184154_ContasPagarOrigemEComissoes` aplicada, com um `UPDATE` que preenche `total_parcelas` antes do CHECK; as 2 parcelas reais (compra #12) ficaram `Compra`, 1/2 e 2/2. Enum `OrigemContaPagar` junto do model. `GerarParcelas` da compra grava origem e total; a lista passa a ler `TotalParcelas` gravado. `dotnet test` 177/177, build 0 avisos.

- [ ] **T2: Contas a Pagar generalizado** (M)
  - Lista com origem/favorecido/descrição + filtro de origem + busca; `POST /api/contas-pagar` (avulsa, AV1/AV2); `PATCH /{id}/cancelar` (CP4); xUnit da avulsa.
  - Verificar: build; `dotnet test`; E2E na T3.

- [ ] **T3: Comissões → conta a pagar** (M)
  - `POST /api/comissoes/gerar-conta` (CC1/CC2); pagar/cancelar a parcela propagam para as comissões (CC3/CC4); remove `/comissoes/pagar` (CC5).
  - Verificar: E2E dos critérios 1-6 contra instância temporária.

## Fase 2: Frontend

- [ ] **T4: Tela Contas a Pagar** (M)
  - Colunas Favorecido e Descrição/Origem, filtro de origem, **Nova conta** (avulsa), **Cancelar**.
  - Verificar: `tsc -b`, `oxlint`.

- [ ] **T5: Tela Comissões** (S)
  - **Gerar conta a pagar** (um vendedor, vencimento, total), status Em pagamento, sem "Marcar como paga".
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T6: Documentação e verificação final** (S)
  - README (etapa 12), SPEC marcada como implementada, `python -m graphify update .`, memória.
