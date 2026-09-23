# Spec: Módulo Comissões (etapa 11)

> Status: **rascunho, aguardando aprovação do Rafael** (23/09/2026).

## Objetivo

Calcular a comissão dos vendedores **conforme o cliente paga** e dar um lugar para acompanhar e **marcar como paga ao vendedor** — fechando o que a etapa 10 (Vendedores) preparou.

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** pedido do Rafael em 23/09/2026, logo depois de Vendedores.
- **Sucesso:** receber uma parcela de um pedido com vendedor gera a comissão dela; a tela **Financeiro → Comissões** mostra o que está a pagar e o que já foi pago, por vendedor e período, e permite marcar como paga.

### Dentro do escopo
Tabela de comissões; geração automática ao receber parcela; carga das parcelas já recebidas; tela com filtros, totais e "marcar como paga" (uma ou várias).

### Fora do escopo (entram depois)
Gerar conta a pagar da comissão no Financeiro; estornar comissão (hoje uma parcela recebida não volta a pendente); comissão sobre pedido de compra; exportação Excel/PDF da tela; metas e faixas de comissão.

## Decisões já tomadas (com o Rafael, 23/09/2026)

| Tema | Decisão |
|---|---|
| Quando a comissão existe | **Quando o cliente paga a parcela** (não ao confirmar o pedido). |
| Controle | Consultar **e** marcar como **paga ao vendedor** (status Pendente → Paga). |
| Menu | **Financeiro → Comissões**. |

## Regras (CM)

| # | Regra |
|---|---|
| CM1 | Marcar uma parcela a receber como **recebida** gera **uma** comissão, se o pedido tiver vendedor e % de comissão > 0: `valor = arredondar(valor da parcela × % / 100, 2)` (meio para cima), com a % **congelada no pedido** (etapa 10), não a atual do vendedor. |
| CM2 | Uma comissão por parcela (índice único): receber de novo (já idempotente) não duplica. Gerada na mesma transação do recebimento. |
| CM3 | Parcelas **já recebidas** antes deste módulo, de pedidos com vendedor e %, ganham a comissão na migration (data = data do recebimento). |
| CM4 | Status: **Pendente** (a pagar ao vendedor) → **Paga** (grava a data). Marcar como paga é idempotente; aceita várias de uma vez. |
| CM5 | Cancelar um pedido não mexe em comissões (só parcelas **Pendentes** são canceladas; as recebidas — e suas comissões — ficam). |
| CM6 | Filtros da tela: vendedor, status e período (pela **data do recebimento**, datas inclusivas, mesma regra dos relatórios); totais (gerado, a pagar, pago) são **do filtro**, calculados no servidor. |

## Modelo de dados

**`public.comissoes`** — id, parcela_receber_id (FK, **único**), pedido_id (FK), vendedor_id (FK), valor_base numeric(12,2) (valor da parcela), percentual numeric(5,2), valor numeric(12,2) (CHECK > 0), data_geracao timestamptz (= data do recebimento), status varchar(20) (`Pendente`/`Paga`), data_pagamento timestamptz nula. Guarda base e % para a conta ficar auditável mesmo se algo mudar depois. Uma migration (com a carga do CM3).

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/comissoes?vendedorId=&status=&dataInicio=&dataFim=&pagina=&tamanhoPagina=` | Lista paginada (mais recente primeiro) + totais do filtro |
| POST | `/api/comissoes/pagar` | Corpo `{ ids: [...] }`: marca como pagas (idempotente); ids inexistentes → 400 |
| PATCH | `/api/contas-receber/{id}/receber` | (existente) passa a gerar a comissão por dentro |

## Tela

- **Financeiro → Comissões** (`/comissoes`): filtros (vendedor, status, período), cards **Gerado / A pagar / Pago**, tabela (vendedor, pedido #, cliente, parcela X/Y, valor recebido, %, comissão, data do recebimento, status, data do pagamento), seleção das pendentes + botão **Marcar como pagas** e ação por linha.

## Testing strategy

1. **Unitário (xUnit):** cálculo da comissão (arredondamento, 2 casas, % 0).
2. **API ponta a ponta (instância temporária, dados `ZZT…` apagados ao final):** receber parcela de pedido com vendedor → comissão certa; receber de novo não duplica; pedido sem vendedor não gera; % congelada vale mesmo mudando a do vendedor; filtros e totais; pagar em lote e de novo (idempotente); id inexistente → 400; cancelar pedido não mexe na comissão; carga do CM3 conferida no banco.
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; verificação visual com o Rafael.

## Boundaries

- **Sempre:** cálculo no servidor; % lida do pedido; geração na mesma transação do recebimento.
- **Perguntar antes:** dependência nova; ligar comissão ao Contas a Pagar.
- **Nunca:** recalcular comissão já gerada; commitar segredos; push sem pedido.

## Success criteria (testáveis)

1. Receber uma parcela de pedido com vendedor gera 1 comissão com valor = parcela × % congelada, arredondado a 2 casas; receber de novo não duplica.
2. Pedido sem vendedor (ou com 0%) não gera comissão.
3. Parcelas já recebidas antes do módulo, de pedidos com vendedor, têm comissão após a migration.
4. `GET /api/comissoes` filtra por vendedor/status/período e traz totais batendo com o banco.
5. `POST /api/comissoes/pagar` marca como pagas (data gravada), é idempotente e recusa id inexistente sem alterar nada.
6. Cancelar um pedido com parcela recebida mantém a comissão.
7. Tela em Financeiro → Comissões; `tsc -b`/`oxlint`/`npm run build` limpos; `dotnet build` 0 avisos e `dotnet test` verde.
