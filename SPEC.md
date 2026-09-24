# Spec: Orçamentos (etapa 13)

> Status: **implementada e testada em 23/09/2026** (T1 a T7, ver `tasks/todo.md`). Critérios 1-7 conferidos: E2E da API com 54 verificações e teste de tela com Playwright com 25 (dados `ZZT…` apagados, dados reais intactos).

## Objetivo

Registrar a proposta feita ao cliente **antes** da venda: um orçamento com itens, descontos e validade, que pode ser enviado em **PDF** e, quando o cliente aprova, vira um **pedido de venda com um clique**, mantendo os preços combinados.

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** escolha do Rafael em 23/09/2026, depois da etapa 12.
- **Sucesso:** um orçamento aprovado gera o pedido de venda sem redigitar nada, e o PDF está pronto para mandar ao cliente.

### Dentro do escopo
Cadastro de orçamento (cliente, vendedor, itens, descontos, forma de pagamento, validade, observações); lista com busca e filtro de status; situação **Vencido** calculada; **Gerar pedido** (rascunho, com os preços do orçamento); **Marcar como perdido** (com motivo); **PDF** do orçamento; tela no menu.

### Fora do escopo (entram depois)
Reabrir orçamento perdido ou aprovado; duplicar orçamento; orçamento para quem não é cliente cadastrado; envio por e-mail; orçamentos no Dashboard ou em Relatórios (taxa de conversão); desfazer a aprovação se o pedido gerado for cancelado; reservar estoque.

## Decisões já tomadas (com o Rafael, 23/09/2026)

| Tema | Decisão |
|---|---|
| Conversão | "Gerar pedido" cria um **pedido de venda em Rascunho**; a confirmação segue o fluxo normal (estoque, parcelas, vendedor). |
| Preço | O pedido gerado usa o **preço e os descontos do orçamento**, mesmo que o produto tenha mudado de preço depois. |
| Status | **Aberto**, **Aprovado** (automático ao gerar pedido) e **Perdido** (manual, com motivo opcional). **Vencido** é calculado (Aberto com a validade já passada): não gera pedido, mas pode ser editado para prorrogar a validade. |
| Cliente | Obrigatório e **cadastrado** (mesmo seletor do pedido). |

## Regras

### Orçamento (OR)

| # | Regra |
|---|---|
| OR1 | Campos: cliente (obrigatório, ativo), vendedor (opcional, ativo), forma de pagamento (opcional), desconto do orçamento 0-100 (2 casas), **validade** (data, obrigatória, ≥ hoje ao criar ou editar), observações (opcional, até 500), 1-100 itens sem produto repetido. Mesmas regras de quantidade/unidade e desconto de item do pedido (R4, R5, R8). |
| OR2 | Preço do item copiado do produto **ao adicionar o item** e congelado (igual ao pedido, R3). Total calculado pelo `CalculoPedido` (mesma conta do pedido). |
| OR3 | Tela sugere validade = hoje + 15 dias. "Hoje" = `DateOnly.FromDateTime(DateTime.UtcNow)`, a mesma referência de Contas a Receber. |
| OR4 | **Vencido** = status Aberto e validade < hoje. Não é gravado; a API devolve `vencido: true/false`. |
| OR5 | Só orçamento **Aberto** (vencido ou não) pode ser editado; Aprovado ou Perdido → 409. Editar com validade nova ≥ hoje "prorroga" o vencido. |

### Gerar pedido (GP)

| # | Regra |
|---|---|
| GP1 | Só de orçamento Aberto e **não vencido**; Aprovado/Perdido → 409; vencido → 400 ("prorrogue a validade"). |
| GP2 | Cliente precisa estar ativo; produtos inativos → 400 listando os nomes; vendedor inativo é **deixado em branco** no pedido (é opcional no rascunho). |
| GP3 | Cria o pedido **Rascunho** com cliente, vendedor, forma de pagamento, desconto e itens (quantidade, **preço unitário e desconto do orçamento**). |
| GP4 | O orçamento passa a **Aprovado** e guarda o nº do pedido gerado, na **mesma transação**. Resposta: `201` com o id do pedido. |
| GP5 | Se o pedido gerado for cancelado depois, o orçamento continua Aprovado (fora do escopo desfazer). |

### Perdido (PE)

| # | Regra |
|---|---|
| PE1 | Marcar como perdido: de Aberto (vencido ou não), com motivo opcional (até 200). Repetir em um Perdido → 204 sem mudar nada (idempotente). Aprovado → 409. |

### PDF (PD)

| # | Regra |
|---|---|
| PD1 | `GET /api/orcamentos/{id}/pdf`, qualquer status. A4 retrato (QuestPDF): cabeçalho "Ambition ERP — Orçamento Nº N", data e validade; cliente (nome, CPF/CNPJ formatado, e-mail, telefone, cidade/UF); vendedor; tabela (produto, qtd, un, preço unitário, desc. %, subtotal); subtotal dos itens, desconto do orçamento, **total**; forma de pagamento; observações; rodapé "Orçamento válido até dd/mm/aaaa · Página X de Y". |

## Modelo de dados

- **`orcamentos`**: `id`, `cliente_id` (FK restrict), `vendedor_id` (FK restrict, nulo), `data_orcamento` (timestamptz), `validade` (date), `status` varchar(20) (`Aberto`/`Aprovado`/`Perdido`, CHECK), `forma_pagamento` (nulo), `desconto_percentual` numeric(5,2), `valor_total` numeric(12,2), `observacoes` varchar(500), `motivo_perda` varchar(200), `pedido_id` (FK → pedidos, restrict, nulo, **único**).
- **`orcamento_itens`**: `id`, `orcamento_id` (FK cascade), `produto_id` (FK restrict), `quantidade` numeric(12,3), `preco_unitario` numeric(12,2), `desconto_percentual` numeric(5,2); índice único (`orcamento_id`, `produto_id`).
- Uma migration. `pedidos` não muda.

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/orcamentos?busca=&status=&pagina=&tamanhoPagina=` | Paginada; busca por nº ou nome do cliente; `status` = `Aberto` (não vencidos), `Vencido`, `Aprovado`, `Perdido` |
| GET | `/api/orcamentos/{id}` | Detalhe com itens, `vencido`, `pedidoId`, `motivoPerda` |
| POST | `/api/orcamentos` | Cria (OR1-OR3) → 201 |
| PUT | `/api/orcamentos/{id}` | Edita o Aberto (OR5); itens atualizados no lugar, como no pedido |
| POST | `/api/orcamentos/{id}/gerar-pedido` | GP1-GP4 → 201 `{ pedidoId }` |
| PATCH | `/api/orcamentos/{id}/perder` | `{ motivo? }` (PE1) |
| GET | `/api/orcamentos/{id}/pdf` | Arquivo PDF (PD1) |

## Telas

- **Menu**: "Orçamentos" em **Ordem Vendas/Compras**, antes de Pedidos de Venda (rota `/orcamentos`).
- **Lista**: nº, cliente, data, validade, total, status (tag; **Vencido** em laranja); busca e filtro de status no mesmo padrão de Pedidos.
- **Formulário** (página própria `/orcamentos/novo` e `/orcamentos/:id`, como Pedidos): cliente, vendedor, forma de pagamento, validade (padrão +15 dias), itens, desconto, observações, total ao vivo.
- **Ações**: Editar (Aberto); **Gerar pedido** (confirmação → mensagem com link para o pedido gerado); **Marcar como perdido** (modal com motivo); **Baixar PDF** (qualquer status); no Aprovado, link "Pedido #N".

## Testing strategy

1. **Unitário (xUnit):** `vencido` (validade ontem, hoje, amanhã); validação dos DTOs (validade passada, observações longas, itens repetidos); transições (editar/gerar/perder por status).
2. **API ponta a ponta (instância temporária, dados `ZZT…` apagados):** criar com preço congelado; editar e prorrogar; filtros Aberto/Vencido/Aprovado/Perdido; gerar pedido → rascunho com preços do orçamento mesmo após mudar o preço do produto; orçamento Aprovado com `pedidoId`; gerar de novo → 409; vencido → 400; produto inativo → 400; perder (idempotente, Aprovado → 409); PDF → 200 `application/pdf` com bytes `%PDF`; o pedido gerado confirma normalmente (estoque e parcelas).
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; verificação visual com o Rafael.

## Boundaries

- **Sempre:** reaproveitar `CalculoPedido`, as validações de item e os seletores do pedido; gerar pedido numa transação só; manter o fluxo do pedido igual.
- **Perguntar antes:** dependência nova; mexer na tabela `pedidos`; e-mail.
- **Nunca:** alterar orçamento Aprovado/Perdido; commitar segredos; push sem pedido.

## Success criteria (testáveis)

Conferidos em 23/09/2026; detalhes na seção "Orçamentos (23/09/2026)" do README.

1. ✅ Criar/editar orçamento com as validações de OR1 → 400 por campo; preço congelado no item.
2. ✅ Vencido calculado corretamente e filtros de status devolvem os conjuntos certos.
3. ✅ Gerar pedido cria um rascunho com os preços/descontos do orçamento e deixa o orçamento Aprovado com `pedidoId`; as regras de GP1/GP2 barram sem alterar nada.
4. ✅ O pedido gerado confirma pelo fluxo normal (estoque baixa, parcelas geradas).
5. ✅ Marcar como perdido funciona, é idempotente e bloqueia a edição.
6. ✅ PDF gerado com cliente, itens e totais corretos.
7. ✅ Tela completa (teste de tela com Playwright, 25/25); `tsc -b`/`oxlint`/`npm run build` limpos; `dotnet build` 0 avisos e `dotnet test` 207/207.
