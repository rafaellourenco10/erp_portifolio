# Checklist: Comissão como conta a pagar + contas avulsas (etapa 12)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Schema** (M) — *concluída em 23/09/2026*
  - `ParcelaPagar`: `PedidoCompraId` nulo, `Origem`, `Descricao`, `Favorecido`, `VendedorId`, `TotalParcelas`; `Comissao.ParcelaPagarId`; `StatusComissao.EmPagamento`; migration `ContasPagarOrigemEComissoes` (preenche `total_parcelas`, CHECK de origem); compra passa a gravar `TotalParcelas`.
  - Verificar: build 0 avisos; migration aplicada; linhas existentes com origem `Compra` e total certo.
  - Resultado: migration `20260923184154_ContasPagarOrigemEComissoes` aplicada, com um `UPDATE` que preenche `total_parcelas` antes do CHECK; as 2 parcelas reais (compra #12) ficaram `Compra`, 1/2 e 2/2. Enum `OrigemContaPagar` junto do model. `GerarParcelas` da compra grava origem e total; a lista passa a ler `TotalParcelas` gravado. `dotnet test` 177/177, build 0 avisos.

- [x] **T2: Contas a Pagar generalizado** (M) — *concluída em 23/09/2026*
  - Lista com origem/favorecido/descrição + filtro de origem + busca; `POST /api/contas-pagar` (avulsa, AV1/AV2); `PATCH /{id}/cancelar` (CP4); xUnit da avulsa.
  - Verificar: build; `dotnet test`; E2E na T3.
  - Resultado: `ContasPagarService` 2.0: uma projeção só (lista e respostas) com favorecido = fornecedor/vendedor/texto; filtro de origem; busca por nº da compra (só quando o texto é número; evitado o bug de `numero` nulo casar com `pedido_compra_id IS NULL`) ou ILIKE em fornecedor/vendedor/favorecido/descrição; `CriarAvulsaAsync` (`ContasPagarCalculo.ParcelasAvulsa`); `CancelarAsync` (compra/paga → 409, idempotente). A propagação para comissões ao pagar/cancelar (CC3/CC4) entrou já aqui, nos mesmos métodos. DTO: `FornecedorNome` → `Favorecido`, + `Origem`, `VendedorId`, `Descricao`. 8 testes novos (`ContasPagarCalculoTests`): `dotnet test` 185/185, build 0 avisos.

- [x] **T3: Comissões → conta a pagar** (M) — *concluída em 23/09/2026*
  - `POST /api/comissoes/gerar-conta` (CC1/CC2); pagar/cancelar a parcela propagam para as comissões (CC3/CC4); remove `/comissoes/pagar` (CC5).
  - Verificar: E2E dos critérios 1-6 contra instância temporária.
  - Resultado: `ComissaoService.GerarContaAsync` + `POST /api/comissoes/gerar-conta` (201 com id da conta, quantidade, valor e vencimento); `/comissoes/pagar` removido; totais ganham `totalEmPagamento`; lista traz `parcelaPagarId`. E2E (instância temporária na 5099, dados `ZZT…` apagados, dados reais intactos): 28 verificações OK — regressão da compra (3x com 1/3..3/3, pagar, cancelar parcela de compra 409, cancelar pedido cancela pendentes), avulsa 3x com valores/vencimentos e 4 validações 400, busca por favorecido, filtro de origem, busca por número sem trazer avulsas, cancelar avulsa (idempotente, paga 409, inexistente 404), gerar conta (misturados/inexistente/sem vencimento 400, soma 25,00, Em pagamento ligadas, favorecido = vendedor, 1/1, gerar de novo 400), pagar a conta paga as comissões com a mesma data, cancelar a conta devolve para Pendente e permite gerar de novo, `/pagar` 404.

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
