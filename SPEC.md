# Spec: Módulo Vendedores (etapa 10)

> Status: **implementada e testada em 23/09/2026** (T1 a T6, ver `tasks/todo.md`). Critérios 1-5 conferidos por E2E (19 verificações, dados `ZZT…` apagados, dados reais intactos); **telas sem verificação visual** (sem navegador nesta sessão).

## Objetivo

Cadastro de **Vendedores** e o vendedor no **Pedido de Venda**, deixando pronta a base para o módulo de **Comissão** (próxima etapa): ao confirmar, o pedido guarda a % de comissão do vendedor naquele momento.

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** pedido do Rafael em 23/09/2026, como passo antes da comissão.
- **Sucesso:** cadastrar vendedores com % de comissão; escolher o vendedor no pedido de venda; confirmar exige vendedor ativo e congela a % no pedido.

### Dentro do escopo
CRUD de Vendedor (tela em **Cadastro**), vendedor no pedido de venda (seleção com busca), regra de confirmar e % congelada.

### Fora do escopo (entram depois)
**Cálculo/relatório de comissão** (próxima etapa); vendedor no pedido de compra; filtro/coluna de vendedor nos relatórios e na lista de pedidos; login do vendedor; metas.

## Decisões já tomadas (com o Rafael, 23/09/2026)

| Tema | Decisão |
|---|---|
| Campos | Nome, CPF, e-mail e telefone opcionais, **% de comissão padrão** (0 a 100) e ativo. Sem endereço. |
| Vendedor no pedido | **Obrigatório só para confirmar** (igual à forma de pagamento): o rascunho pode ficar sem; pedidos antigos continuam sem vendedor. |
| Comissão | **Congelar a % no pedido ao confirmar**; mudar a % do vendedor depois não altera vendas já confirmadas. |

## Regras de negócio

### Vendedor (V)

| # | Regra |
|---|---|
| V1 | Nome 3-150; **CPF** obrigatório e válido (só CPF, não CNPJ; com ou sem máscara, gravado só com dígitos); e-mail opcional (válido, até 150); telefone opcional (mesmo formato do Cliente); % de comissão de 0 a 100 com até 2 casas (padrão 0). |
| V2 | CPF único **entre vendedores**. Conflito com vendedor ativo bloqueia (409); com inativo, sugere reativar (mesma mensagem do Cliente/Fornecedor). |
| V3 | Inativar não apaga; só impede escolher o vendedor em novos pedidos e confirmar pedidos com ele. |

### Vendedor no pedido de venda (PV)

| # | Regra |
|---|---|
| PV1 | Rascunho aceita vendedor vazio. Se informado ao salvar, o vendedor precisa existir e estar **ativo** (senão 400 no campo `VendedorId`) — mesma regra já usada para o cliente. |
| PV2 | Confirmar exige vendedor informado e **ativo** (400 no campo `VendedorId`, pedido continua Rascunho) — junto das outras exigências já existentes (forma de pagamento, cliente/produtos ativos, estoque). |
| PV3 | Ao confirmar, o pedido grava `percentual_comissao` = % atual do vendedor. Depois disso, não muda mais (nem se a % do vendedor for alterada). |
| PV4 | Rascunho não guarda % (fica vazio); pedidos confirmados antes deste módulo ficam sem vendedor e sem %. |

## Modelo de dados

- **`public.vendedores`** — id, nome, cpf (índice único), email, telefone, percentual_comissao numeric(5,2) (CHECK 0-100), ativo, data_cadastro.
- **`public.pedidos`** ganha `vendedor_id` (nullable, FK → vendedores, restrict) e `percentual_comissao` numeric(5,2) (nullable, CHECK 0-100).
- Uma migration só.

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/vendedores?nome=&ativo=&pagina=&tamanhoPagina=` | Lista paginada |
| GET | `/api/vendedores/{id}` | Consulta |
| POST | `/api/vendedores` | Inclusão |
| PUT | `/api/vendedores/{id}` | Edição |
| PATCH | `/api/vendedores/{id}/inativar` | Inativação |
| POST/PUT | `/api/pedidos` | Passam a aceitar `vendedorId` (opcional) |
| GET | `/api/pedidos/{id}` | Passa a devolver `vendedorId`, `vendedorNome` e `percentualComissao` |

## Telas

- **Cadastro → Vendedores** (`/vendedores`): lista + drawer de cadastro/edição, no padrão de Fornecedores (sem cidade/UF, com % de comissão).
- **Pedido de Venda**: campo **Vendedor** (seleção com busca, só ativos) ao lado do cliente; no pedido confirmado, mostra o vendedor e a % de comissão congelada.

## Testing strategy

1. **Unitário (xUnit):** validação de CPF só-CPF (aceita CPF válido com/sem máscara, recusa CNPJ e CPF inválido) e do DTO do vendedor (% fora de 0-100).
2. **API ponta a ponta (instância temporária, dados `ZZT…` apagados ao final):** CRUD do vendedor (CPF duplicado 409, inativar); rascunho com e sem vendedor; vendedor inativo no rascunho → 400; confirmar sem vendedor → 400; confirmar com vendedor → % gravada; mudar a % do vendedor depois não altera o pedido.
3. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; verificação visual com o Rafael.

## Boundaries

- **Sempre:** espelhar o padrão de Fornecedor (DTOs, service, controller, tela); % congelada só no confirmar, calculada no servidor.
- **Perguntar antes:** dependência nova; qualquer coisa de cálculo de comissão.
- **Nunca:** recalcular a % de pedidos já confirmados; commitar segredos; push sem pedido.

## Success criteria (testáveis)

Conferidos em 23/09/2026; detalhes na seção "Vendedores (23/09/2026)" do README.

1. ✅ CRUD de vendedor funciona; CPF inválido, CNPJ ou % fora de 0-100 → 400; CPF repetido → 409.
2. ✅ Rascunho salva com ou sem vendedor; vendedor inativo ou inexistente → 400 em `VendedorId`.
3. ✅ Confirmar sem vendedor (ou com vendedor inativo) → 400 em `VendedorId`, pedido continua Rascunho.
4. ✅ Confirmar com vendedor grava a % de comissão dele no pedido; alterar a % do vendedor depois não muda o pedido confirmado.
5. ✅ `GET /api/pedidos/{id}` traz vendedor e % congelada; pedidos antigos continuam abrindo (sem vendedor).
6. ✅ Tela Vendedores no menu Cadastro; campo Vendedor no pedido de venda; `tsc -b`/`oxlint`/`npm run build` limpos; `dotnet build` 0 avisos e `dotnet test` 172/172 — verificação visual não feita (sem navegador).
