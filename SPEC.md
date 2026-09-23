# Spec: Comissão como conta a pagar + contas avulsas (etapa 12)

> Status: **implementada e testada em 23/09/2026** (T1 a T6, ver `tasks/todo.md`). Critérios 1-6 conferidos por E2E (28 verificações, dados `ZZT…` apagados, dados reais intactos); **telas sem verificação visual** (sem navegador nesta sessão).

## Objetivo

Integrar Comissões e Contas a Pagar: fechar as comissões de um vendedor gera **uma conta a pagar**; pagar essa conta marca as comissões como pagas. Ao abrir o Contas a Pagar para contas que não vêm de pedido de compra, entram também as **contas avulsas** (aluguel, luz, salários...).

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** escolha do Rafael em 23/09/2026, depois de Comissões.
- **Sucesso:** o dinheiro de comissão e de despesas avulsas passa pelo Contas a Pagar, como o de compras.

### Dentro do escopo
Origem da conta a pagar (Compra / Comissão / Avulsa); gerar conta a partir de comissões; pagar a conta → comissões pagas; cancelar conta avulsa/de comissão; cadastro de conta avulsa em parcelas; tela de Contas a Pagar e de Comissões ajustadas.

### Fora do escopo (entram depois)
Contas recorrentes automáticas (todo mês); categorias/plano de contas; editar conta já lançada (cancele e lance de novo); anexos/comprovantes; juros e pagamento parcial.

## Decisões já tomadas (com o Rafael, 23/09/2026)

| Tema | Decisão |
|---|---|
| Fluxo da comissão | Selecionar comissões **pendentes de um vendedor** → **Gerar conta a pagar** (com vencimento) → comissões ficam **Em pagamento** → pagar a conta em Contas a Pagar → comissões **Pagas**. |
| Pagar comissão direto | **Sai.** Só pelo Contas a Pagar. Comissões já pagas antes continuam pagas (sem conta vinculada). |
| Contas avulsas | **Entram nesta etapa**: descrição, favorecido (texto), valor, 1º vencimento, nº de parcelas e intervalo. |

## Regras

### Conta a pagar (CP)

| # | Regra |
|---|---|
| CP1 | Toda parcela a pagar tem **origem**: `Compra` (tem pedido de compra — como hoje), `Comissao` (tem vendedor) ou `Avulsa` (tem descrição). Garantido por CHECK no banco. |
| CP2 | A lista mostra para todas: **favorecido** (fornecedor, vendedor ou o texto da avulsa), **descrição** (Compra #N / "Comissões — Vendedor (N)" / texto), parcela X/Y, valor, vencimento, status. Busca por nº da compra ou por trecho de favorecido/descrição; filtro por origem. |
| CP3 | O total de parcelas (Y) passa a ser **gravado** na parcela (as avulsas não têm pedido para contar); a migration preenche as existentes. |
| CP4 | **Cancelar** uma parcela Pendente é permitido para `Avulsa` e `Comissao` (idempotente); paga → 409; de `Compra` → 409 ("cancele o pedido de compra"). |

### Conta avulsa (AV)

| # | Regra |
|---|---|
| AV1 | Descrição 3-200 (obrigatória), favorecido opcional até 150, valor total > 0 (até 2 casas), 1º vencimento obrigatório, 1-12 parcelas, intervalo 1-180 dias (padrão 1 parcela, 30 dias). |
| AV2 | Valor dividido igualmente com o resto na última (mesma regra das outras parcelas); vencimento da parcela `i` = 1º vencimento + `(i-1) × intervalo`. |

### Comissão → conta (CC)

| # | Regra |
|---|---|
| CC1 | Gerar conta exige 1+ comissões, **todas Pendentes e do mesmo vendedor**, e a data de vencimento; senão 400 sem alterar nada. |
| CC2 | Gera **uma** parcela a pagar (origem Comissão, 1/1) com a **soma** das comissões; as comissões passam a **Em pagamento** e ficam ligadas a ela. |
| CC3 | Marcar essa conta como **paga** em Contas a Pagar marca as comissões ligadas como **Pagas** (mesma data), na mesma transação. |
| CC4 | **Cancelar** essa conta devolve as comissões para **Pendente** (desligadas), para gerar de novo. |
| CC5 | O `POST /api/comissoes/pagar` direto é **removido**. |

## Modelo de dados

- **`parcelas_pagar`**: `pedido_compra_id` passa a nulo; novas `origem` varchar(20) (padrão `Compra`), `descricao` varchar(200), `favorecido` varchar(150), `vendedor_id` (FK → vendedores, restrict), `total_parcelas` int (preenchido na migration); CHECK de origem (CP1).
- **`comissoes`**: nova `parcela_pagar_id` (FK → parcelas_pagar, restrict, nula); status ganha `EmPagamento`.
- Uma migration.

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/contas-pagar?busca=&status=&origem=&...` | Passa a trazer `origem`, `favorecido`, `descricao`; `pedidoCompraId` pode ser nulo |
| POST | `/api/contas-pagar` | Lança conta **avulsa** em N parcelas |
| PATCH | `/api/contas-pagar/{id}/cancelar` | Cancela parcela Avulsa/Comissão pendente (CP4, CC4) |
| PATCH | `/api/contas-pagar/{id}/pagar` | (existente) paga; se for de comissão, paga as comissões (CC3) |
| POST | `/api/comissoes/gerar-conta` | `{ ids, vencimento }` → conta a pagar (CC1/CC2) |
| POST | `/api/comissoes/pagar` | **removido** (CC5) |

## Telas

- **Contas a Pagar**: botão **Nova conta** (avulsa); colunas Favorecido e Descrição/Origem no lugar de Fornecedor/Compra; filtro por origem; ação **Cancelar** nas pendentes avulsas/de comissão.
- **Comissões**: botão **Gerar conta a pagar** (seleção de um vendedor só; pede o vencimento e mostra o total); status **Em pagamento**; sem "Marcar como paga".

## Testing strategy

1. **Unitário (xUnit):** vencimentos e valores da conta avulsa em N parcelas; validação dos DTOs (avulsa e gerar-conta).
2. **API ponta a ponta (instância temporária, dados `ZZT…` apagados):** regressão da compra (confirmar → parcelas com X/Y certo, pagar, cancelar compra); avulsa em 3 parcelas; cancelar avulsa; cancelar compra/paga → 409; gerar conta de comissões (soma, Em pagamento), regras de CC1; pagar a conta → comissões Pagas; cancelar a conta → comissões Pendentes; `/comissoes/pagar` não existe mais; migration preencheu `total_parcelas`.
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; verificação visual com o Rafael.

## Boundaries

- **Sempre:** reaproveitar a divisão em parcelas existente; propagar status na mesma transação; manter o fluxo de compra igual.
- **Perguntar antes:** dependência nova; recorrência automática; categorias.
- **Nunca:** alterar conta já paga; commitar segredos; push sem pedido.

## Success criteria (testáveis)

Conferidos em 23/09/2026; detalhes na seção "Comissão vira conta a pagar + contas avulsas (23/09/2026)" do README.

1. ✅ O fluxo de compra continua igual (parcelas com X/Y certo, pagar, cancelar compra cancela pendentes).
2. ✅ Conta avulsa em N parcelas soma exatamente o valor, com os vencimentos certos; validações → 400.
3. ✅ Cancelar avulsa/comissão pendente funciona e é idempotente; compra ou paga → 409.
4. ✅ Gerar conta de comissões cria 1 conta com a soma e deixa as comissões Em pagamento; vendedores misturados, comissão não pendente ou id inexistente → 400 sem alterar nada.
5. ✅ Pagar a conta de comissão deixa as comissões Pagas; cancelar devolve para Pendente.
6. ✅ Lista de contas a pagar traz origem/favorecido/descrição e filtra por origem; busca por favorecido.
7. ✅ Telas ajustadas; `tsc -b`/`oxlint`/`npm run build` limpos; `dotnet build` 0 avisos e `dotnet test` 185/185 — verificação visual não feita (sem navegador).
