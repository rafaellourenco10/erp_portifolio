# Checklist: Devolução de venda (etapa 14)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Schema** (M) — *concluída em 24/09/2026*
  - `Devolucao`, `DevolucaoItem`; `ParcelaPagar.DevolucaoId` + origem `Devolucao`; `Comissao.ParcelaReceberId` nulo + `DevolucaoId`; CHECKs; migration `AdicionaDevolucoes`.
  - Verificar: build 0 avisos; migration aplicada; dados existentes intactos (comissões e contas continuam válidas nos CHECKs novos).
  - Resultado: backup `backup_pre_devolucao_20260924.dump` (ferramentas locais) antes; migration `20260924004530_AdicionaDevolucoes` aplicada: tabelas `devolucoes` (CHECK total = abatido + reembolso) e `devolucao_itens`; `parcelas_pagar.devolucao_id` + CHECK de origem com `Devolucao`; `comissoes.parcela_receber_id` nulo + `devolucao_id` e CHECK `ck_comissoes_valor` (normal > 0 com parcela ou estorno < 0 com devolução). As 2 comissões e as 4 contas reais passaram nos CHECKs novos. Build 0 avisos, `dotnet test` 207/207.

- [ ] **T2: DevolucaoCalculo** (S)
  - Valor por item com descontos; "o que falta" na devolução final; abatimento das pendentes (da última, zera → cancela); reembolso; estorno de comissão. xUnit.
  - Verificar: `dotnet test`.

- [ ] **T3: API de devolução** (M)
  - DTOs, `DevolucaoService` (DV1-DV8), `POST/GET /api/pedidos/{id}/devolucoes`, `valorDevolvido`/`quantidadeDevolvida` no pedido, cancelar pedido com devolução → 409 (DV9).
  - Verificar: build; `dotnet test`.

- [ ] **T4: Contas a Pagar e Comissões** (S)
  - Origem `Devolucao` (lista/filtro, não cancela); lista de comissões com estorno; gerar conta inclui estornos (CC6); `GET /api/dashboard/devolucoes` (DB1).
  - Verificar: E2E dos critérios 1-5 contra instância temporária (dados `ZZT…` apagados), com regressão.

## Fase 2: Frontend

- [ ] **T5: Tela do pedido** (M)
  - Tipos/api/hooks; modal "Registrar devolução" com prévia; seção Devoluções; "Cancelar pedido" some com devolução.
  - Verificar: `tsc -b`, `oxlint`.

- [ ] **T6: Contas a Pagar, Comissões e Dashboard na tela** (S)
  - Origem Devolução (filtro/tag, sem cancelar); estorno na lista de comissões; aviso no modal de gerar conta; card Devoluções do mês.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`; teste de tela (Playwright).

## Fase 3: Fechamento

- [ ] **T7: Documentação e verificação final** (S)
  - README (etapa 14; Login passa a 15), SPEC marcada como implementada, grafo, memória.
