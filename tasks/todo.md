# Checklist: Devolução de venda (etapa 14)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Schema** (M) — *concluída em 24/09/2026*
  - `Devolucao`, `DevolucaoItem`; `ParcelaPagar.DevolucaoId` + origem `Devolucao`; `Comissao.ParcelaReceberId` nulo + `DevolucaoId`; CHECKs; migration `AdicionaDevolucoes`.
  - Verificar: build 0 avisos; migration aplicada; dados existentes intactos (comissões e contas continuam válidas nos CHECKs novos).
  - Resultado: backup `backup_pre_devolucao_20260924.dump` (ferramentas locais) antes; migration `20260924004530_AdicionaDevolucoes` aplicada: tabelas `devolucoes` (CHECK total = abatido + reembolso) e `devolucao_itens`; `parcelas_pagar.devolucao_id` + CHECK de origem com `Devolucao`; `comissoes.parcela_receber_id` nulo + `devolucao_id` e CHECK `ck_comissoes_valor` (normal > 0 com parcela ou estorno < 0 com devolução). As 2 comissões e as 4 contas reais passaram nos CHECKs novos. Build 0 avisos, `dotnet test` 207/207.

- [x] **T2: DevolucaoCalculo** (S) — *concluída em 24/09/2026*
  - Valor por item com descontos; "o que falta" na devolução final; abatimento das pendentes (da última, zera → cancela); reembolso; estorno de comissão. xUnit.
  - Verificar: `dotnet test`.
  - Resultado: `Services/DevolucaoCalculo.cs` (`ValorItem`, `ValorTotal` com "o que falta" e teto no restante, `Abater` → ajustes/abatido/reembolso, `Estorno` via `ComissaoCalculo`). `DevolucaoCalculoTests` (+17): 3 devoluções de um pedido de 97,00 fecham exatamente; abatimento da última, parcela zerada = cancelar, excedente = reembolso; estorno −reembolso × % e zero sem vendedor/%. `dotnet test` 224/224.

- [x] **T3: API de devolução** (M) — *concluída em 24/09/2026*
  - DTOs, `DevolucaoService` (DV1-DV8), `POST/GET /api/pedidos/{id}/devolucoes`, `valorDevolvido`/`quantidadeDevolvida` no pedido, cancelar pedido com devolução → 409 (DV9).
  - Verificar: build; `dotnet test`.
  - Resultado: `DevolucaoService` valida (pedido confirmado, item do pedido, unidade, disponível, vencimento ≥ hoje), calcula pelo `DevolucaoCalculo` e aplica numa **transação explícita com dois SaveChanges** (o nº da devolução entra na descrição do reembolso e no motivo do estoque): devolução + itens, parcelas reduzidas/canceladas, conta `Devolucao`, comissão negativa, entrada via `IEstoqueService.DevolverAoEstoque`. `DevolucoesController` em `/api/pedidos/{pedidoId}/devolucoes` (POST 201, GET). Pedido: `quantidadeDevolvida` por item e `valorDevolvido` (parâmetros com padrão, o orçamento não muda); cancelar com devolução → 409. +5 xUnit do DTO: `dotnet test` 229/229, build 0 avisos. E2E na T4.

- [x] **T4: Contas a Pagar e Comissões** (S) — *concluída em 24/09/2026*
  - Origem `Devolucao` (lista/filtro, não cancela); lista de comissões com estorno; gerar conta inclui estornos (CC6); `GET /api/dashboard/devolucoes` (DB1).
  - Verificar: E2E dos critérios 1-5 contra instância temporária (dados `ZZT…` apagados), com regressão.
  - Resultado: cancelar conta `Devolucao` → 409; lista de comissões com `numeroParcela` nulo e `devolucaoId`; `GerarContaAsync` puxa os estornos pendentes do vendedor e recusa soma ≤ 0; `GET /api/dashboard/devolucoes` (`DevolucoesResumoDto`). **Bug achado pelo E2E e corrigido:** `PATCH /pedidos/{id}/cancelar` não tratava `ConflitoException` (virava 500); agora 409. E2E (`.claude/ferramentas-locais/e2e-devolucoes.ps1`, pedido de 275,50 em 3x com vendedor a 10%): **36/36** — parcial só abate (91,84 → 6,34), estoque +1, pedido com `quantidadeDevolvida`/`valorDevolvido`, faturamento inalterado; 8 validações sem alterar nada; misto com perda (6,34 abatido + 79,16 reembolso, parcela 3 cancelada, conta `Devolucao` com favorecido/descrição/vencimento, estorno −7,92); devolução final fecha 275,50 (estorno −10,45); cancelar pedido/reembolso 409; fechamento com estornos (−0,01 → 400; com mais uma comissão → 5 itens, 9,99; cancelar/pagar propagam); card do mês +3/+275,50; regressão de cancelar sem devolução. Dados de teste apagados; dados reais e soma de comissões idênticos.

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
