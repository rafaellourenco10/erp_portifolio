# Spec: Devolução de venda (etapa 14)

> Status: **implementada e testada em 24/09/2026** (T1 a T7, ver `tasks/todo.md`). Critérios 1-7 conferidos: E2E da API com 36 verificações e teste de tela com Playwright com 14 (dados `ZZT…` apagados, dados reais intactos).

## Objetivo

Registrar a devolução (total ou parcial) de um pedido de venda confirmado e propagar as consequências para os outros módulos numa transação só: **estoque** (o item volta, se estiver em condições), **contas a receber** (as parcelas pendentes diminuem), **contas a pagar** (reembolso do que o cliente já pagou a mais) e **comissões** (estorno sobre o valor reembolsado).

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** escolha do Rafael em 24/09/2026, depois de Orçamentos.
- **Sucesso:** devolver 1 de 3 itens de uma venda deixa estoque, parcelas, reembolso e comissão coerentes, sem nenhum ajuste manual.

### Dentro do escopo
Devolução parcial por item/quantidade (várias devoluções no mesmo pedido até devolver tudo); valor devolvido calculado com os descontos do pedido; abatimento nas parcelas pendentes; reembolso como conta a pagar; estorno de comissão descontado no próximo fechamento; escolha por item de "volta ao estoque"; histórico de devoluções no pedido; card **Devoluções do mês** no Dashboard; ajustes nas telas de Pedido, Contas a Pagar e Comissões.

### Fora do escopo (entram depois)
Desfazer/editar uma devolução; crédito do cliente para pedidos futuros; troca (devolução + novo pedido num passo só); devolução de compra ao fornecedor; faturamento líquido (o Dashboard e os Relatórios continuam mostrando o valor bruto do pedido; as devoluções aparecem num card próprio); indicador de devolução na lista de pedidos.

## Decisões já tomadas (com o Rafael, 24/09/2026)

| Tema | Decisão |
|---|---|
| Escopo | **Parcial por item e quantidade**; várias devoluções no mesmo pedido até devolver tudo. |
| Dinheiro | **Abate das parcelas pendentes** (da última para a primeira); o que passar disso (o cliente já tinha pago) vira **conta a pagar "Reembolso"** ao cliente. |
| Comissão | Parcelas reduzidas já geram menos comissão quando forem recebidas. Sobre o valor **reembolsado**, nasce um **estorno** (comissão negativa) que é **descontado no próximo fechamento** do vendedor, mesmo que a comissão original já tenha sido paga. |
| Estoque | **Escolha por item**: "volta ao estoque" marcado por padrão; desmarcado = perda (produto com defeito). |

### Decididas por padrão (confirmadas pelo Rafael em 24/09/2026)

| Tema | Padrão proposto | Motivo |
|---|---|---|
| Desfazer | Devolução é **definitiva** (não edita nem exclui). | Já moveu estoque, parcelas, conta e comissão; errou → registre uma venda nova. |
| Cancelar pedido com devolução | **Bloqueado (409)**. | Cancelar estorna todos os itens ao estoque e duplicaria a entrada dos devolvidos. Devolva o restante em vez de cancelar. |
| Conta de reembolso | **Não pode ser cancelada** (409), só paga. | Faz parte da devolução, que é definitiva. |
| Faturamento | Dashboard/Relatórios **continuam brutos**; o Dashboard ganha um **card de Devoluções do mês** (pedido do Rafael). | O bruto continua comparável com o que foi vendido; a devolução fica visível ao lado. |

## Regras

### Devolução (DV)

| # | Regra |
|---|---|
| DV1 | Só de pedido **Confirmado**; Rascunho/Cancelado → 409. |
| DV2 | 1+ itens, cada um um item **deste** pedido, sem repetir; quantidade > 0, até 3 casas, inteira em UN/CX, e **≤ vendida − já devolvida** (senão 400 em `Itens`). Motivo opcional (até 200). |
| DV3 | Valor de cada item devolvido = `quantidade × preço × (1 − desc. item) × (1 − desc. pedido)`, arredondado a 2 casas. Na devolução que zera o pedido (tudo devolvido), o valor total é **o que falta** (`valor_total do pedido − já devolvido`), para os centavos fecharem. |
| DV4 | **Abatimento:** o valor desconta das parcelas **Pendentes**, da de maior número para a menor; parcela que chega a zero fica **Cancelada** (mantém o valor original para histórico); parcela reduzida guarda o novo valor. |
| DV5 | **Reembolso** = valor da devolução − abatido. Se > 0, gera **uma** conta a pagar origem **`Devolucao`** (1/1, favorecido = nome do cliente, descrição "Reembolso devolução #D — pedido #P", vencimento informado ou hoje). A tela manda o vencimento vazio quando é "hoje" (o "hoje" do servidor é UTC). |
| DV6 | **Estorno de comissão:** se o pedido tem vendedor com % > 0 e houve reembolso, nasce uma comissão **negativa** (`valor = −reembolso × %`, base = reembolso), status Pendente, ligada à devolução. |
| DV7 | **Estoque:** item com "volta ao estoque" gera **Entrada** (motivo "Devolução #D pedido #P"); sem, não gera nada (fica registrado no item da devolução). |
| DV8 | Tudo (devolução, itens, parcelas, conta, estorno, estoque) no **mesmo SaveChanges**; se qualquer regra falhar, nada muda. |
| DV9 | **Cancelar pedido** que tem devolução → 409 ("devolva o restante"). |

### Dashboard (DB)

| # | Regra |
|---|---|
| DB1 | Card **Devoluções do mês**: soma de `valor_total` e quantidade das devoluções com `data_devolucao` no mês atual (UTC, mesma convenção dos outros cards); o faturamento do mês **não muda**. |

### Contas a pagar e comissões (CP/CC)

| # | Regra |
|---|---|
| CP5 | Nova origem `Devolucao` (CHECK: exige `devolucao_id`); aparece no filtro de origem; **cancelar** essa conta → 409. |
| CC6 | **Gerar conta de comissões** inclui automaticamente **todos os estornos Pendentes** do vendedor; se a soma final for ≤ 0 → 400 ("os estornos superam as comissões selecionadas") e nada muda. Pagar/cancelar a conta propaga aos estornos como às comissões (CC3/CC4). |
| CC7 | A lista de comissões mostra o estorno com valor negativo e a origem "Estorno · devolução #D" (sem nº de parcela). |

## Modelo de dados

- **`devolucoes`**: `id`, `pedido_id` (FK restrict), `data_devolucao` (timestamptz), `motivo` varchar(200), `valor_total`, `valor_abatido`, `valor_reembolso` (numeric(12,2), CHECK `valor_total = valor_abatido + valor_reembolso`, todos ≥ 0, total > 0).
- **`devolucao_itens`**: `id`, `devolucao_id` (FK cascade), `pedido_item_id` (FK restrict), `quantidade` numeric(12,3) > 0, `valor` numeric(12,2), `volta_estoque` bool; único (`devolucao_id`, `pedido_item_id`).
- **`parcelas_pagar`**: + `devolucao_id` (FK restrict, nulo); CHECK de origem ganha `Devolucao`.
- **`comissoes`**: `parcela_receber_id` passa a nulo; + `devolucao_id` (FK restrict, nulo); CHECK: comissão normal (`parcela_receber_id` preenchido, valor > 0) **ou** estorno (`devolucao_id` preenchido, valor < 0). O índice único em `parcela_receber_id` continua (nulos não conflitam).
- Uma migration.

## API

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/pedidos/{id}/devolucoes` | `{ itens: [{ pedidoItemId, quantidade, voltaEstoque }], motivo?, vencimentoReembolso? }` → 201 com valor total, abatido, reembolso, id da conta de reembolso e valor do estorno (DV1-DV8) |
| GET | `/api/pedidos/{id}/devolucoes` | Histórico do pedido (com itens) |
| GET | `/api/pedidos/{id}` | Passa a trazer `valorDevolvido` e, por item, `quantidadeDevolvida` |
| PATCH | `/api/pedidos/{id}/cancelar` | Com devolução → 409 (DV9) |
| GET/PATCH | `/api/contas-pagar…` | Origem `Devolucao`; cancelar → 409 (CP5) |
| POST | `/api/comissoes/gerar-conta` | Inclui os estornos pendentes do vendedor (CC6); a resposta traz a quantidade e o valor final |
| GET | `/api/comissoes` | Estorno com `devolucaoId`, `numeroParcela` nulo, valor negativo (CC7) |
| GET | `/api/dashboard/devolucoes` | `{ valorTotal, quantidade }` do mês atual (DB1) |

## Telas

- **Pedido confirmado**: botão **Registrar devolução** → modal com os itens (vendido, já devolvido, quantidade a devolver, "volta ao estoque"), motivo, vencimento do reembolso e **prévia** do valor; ao salvar, mensagem com abatido/reembolso/estorno. Seção **Devoluções** com o histórico. "Cancelar pedido" some quando houver devolução.
- **Contas a Pagar**: origem "Devolução" no filtro e na tag; sem "Cancelar" nessas contas.
- **Dashboard**: card **Devoluções do mês** (valor e quantidade), com o mesmo loading/erro próprio dos outros cards.
- **Comissões**: estorno em vermelho (negativo) com "Estorno · devolução #D"; o modal de gerar conta avisa que os estornos pendentes do vendedor são descontados.

## Testing strategy

1. **Unitário (xUnit):** valor da devolução (descontos, arredondamento, "o que falta" na última); abatimento das parcelas (da última, zera → cancela, reembolso do excedente); estorno de comissão; validação do DTO.
2. **API ponta a ponta (instância temporária, dados `ZZT…` apagados):** devolução parcial com parcelas pendentes (só abate); depois de receber tudo (só reembolso + estorno); misto; quantidade acima do disponível, item de outro pedido, pedido rascunho/cancelado → erro sem mudar nada; estoque com e sem "volta"; devolver tudo fecha os centavos; cancelar pedido com devolução → 409; conta de reembolso não cancela; gerar conta de comissões desconta o estorno; soma ≤ 0 → 400; regressão: confirmar/cancelar pedido sem devolução, receber parcela gera comissão.
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; teste de tela com Playwright (modal de devolução, histórico, contas a pagar, comissões).

## Boundaries

- **Sempre:** tudo numa transação; reaproveitar `CalculoPedido`, `ComissaoCalculo`, `EstoqueService` e a propagação existente de pagar/cancelar conta de comissão.
- **Perguntar antes:** mudar o faturamento do Dashboard/Relatórios; crédito do cliente; desfazer devolução.
- **Nunca:** alterar parcela já recebida; apagar comissão já paga (estorna com negativa); commitar segredos; push sem pedido.

## Success criteria (testáveis)

Conferidos em 24/09/2026; detalhes na seção "Devolução de venda (24/09/2026)" do README.

1. ✅ Devolução parcial com parcelas pendentes: estoque volta (só itens marcados), parcelas pendentes reduzem da última para a primeira, sem reembolso nem estorno.
2. ✅ Devolução depois de tudo recebido: reembolso vira conta a pagar `Devolucao` e nasce o estorno de comissão (−reembolso × %).
3. ✅ Validações de DV1/DV2 → erro sem alterar nada; devolver tudo fecha exatamente o valor do pedido.
4. ✅ Cancelar pedido com devolução → 409; conta de reembolso não cancela.
5. ✅ Gerar conta de comissões desconta os estornos pendentes; soma ≤ 0 → 400; pagar/cancelar a conta propaga ao estorno.
6. ✅ Telas de Pedido, Contas a Pagar, Comissões e o card do Dashboard ajustados e testados no navegador; o faturamento do mês não muda com a devolução.
7. ✅ `dotnet build` 0 avisos, `dotnet test` 229/229, `tsc -b`/`oxlint`/`npm run build` limpos.
