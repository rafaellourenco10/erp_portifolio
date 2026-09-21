# Spec: Módulo Pedidos (etapa 3)

> Status: **aprovada pelo Rafael em 21/09/2026**. Nada foi implementado ainda. Ao mudar uma decisão, atualize esta spec **antes** do código.

## Objetivo

Registrar **vendas** no Ambition ERP: um pedido liga um **cliente** a um ou mais **produtos** (itens), com quantidade, desconto e **total calculado pelo servidor**.

- **Quem usa:** o dono do ERP de portfólio (sem login por enquanto, então o pedido não guarda vendedor).
- **Por que agora:** Clientes e Produtos já existem; Pedidos é o coração do ERP e pré-requisito de Contas a receber, Estoque e Dashboard.
- **Sucesso:** dá para montar um pedido com vários itens, ver o total mudar na hora, confirmá-lo (fica travado) ou cancelá-lo, e o histórico de preços das vendas nunca muda sozinho.

### Dentro do escopo
Lista de pedidos com filtros, tela de pedido (novo / rascunho / consulta), itens com quantidade decimal e desconto %, desconto no pedido todo, forma de pagamento (lista fixa, sem parcelas), confirmar e cancelar.

### Fora do escopo (entram depois)
Estoque, parcelas e contas a receber, nota fiscal, impressão/PDF, vendedor/usuário, edição de pedido confirmado, exclusão de pedido, filtro por período.

## Decisões já tomadas (com o Rafael, 21/09/2026)

| Tema | Decisão |
|---|---|
| Pedido confirmado | **Não edita, só cancela.** Se errou: cancela e cria outro. |
| Quantidade | **Decimal, até 3 casas.** Produto em `UN` ou `CX` exige quantidade **inteira**; `KG`, `L`, `M` aceitam decimal. |
| Desconto | **Por item e no pedido todo** (ambos em %). |
| Pagamento | **Campo simples no pedido** (lista fixa, sem parcelas). Obrigatório só para **confirmar** (opcional no rascunho). |
| Preço do item | **Congelado ao adicionar** ao rascunho; confirmar **não** atualiza preços. Para usar o preço novo, remove e adiciona o item de novo. |
| Item repetido | **Recusado** na API (400); a tela soma a quantidade em vez de criar outra linha. |
| Testes | **Projeto xUnit** (`backend/ErpPortfolio.Tests`) só para o cálculo e as transições de status, além dos testes ponta a ponta. |
| Scripts ponta a ponta | Continuam **fora do repositório**; o README lista o que foi verificado. |

## Regras de negócio

| # | Regra |
|---|---|
| R1 | Status: `Rascunho` → `Confirmado`; `Rascunho` ou `Confirmado` → `Cancelado`. `Cancelado` é final. |
| R2 | Só o **rascunho** pode ser editado. Editar/confirmar fora do estado permitido retorna **409**. |
| R3 | O **preço unitário é copiado** de `produtos.preco_venda` para o item quando o item é adicionado e **fica congelado**. O cliente da API **nunca envia** preço nem total. |
| R4 | Um pedido tem **1 a 100 itens**; o mesmo produto **não pode repetir** no pedido (soma-se a quantidade). |
| R5 | Cliente e produtos de **pedidos novos / itens novos** precisam estar **ativos**. Inativar depois não afeta pedidos já feitos. |
| R6 | Confirmar exige: forma de pagamento preenchida, cliente ativo e produtos ativos. Senão **400** no campo. |
| R7 | Cancelar um pedido já cancelado retorna 204 (idempotente, como "inativar"). Pedido **nunca é excluído**. |
| R8 | Quantidade: `0,001` a `999.999,999`. Desconto do item e do pedido: `0` a `100`, no máximo 2 casas. |
| R9 | Número do pedido = `id` sequencial. Data do pedido = data/hora de criação (UTC), não editável. |
| R10 | O **total do pedido não pode passar de 9.999.999.999,99** (limite da coluna `numeric(12,2)`); acima disso a API retorna **400** no campo `Itens`, em vez de estourar no banco. *(Incluída em 21/09/2026, durante a T1: quantidade máxima × preço máximo passa de 10 quatrilhões.)* |

### Cálculo (a regra que mais importa)

```
subtotal do item = arredonda2( quantidade × preço_unitário × (1 − desconto_item/100) )
soma             = Σ subtotais dos itens
total            = arredonda2( soma × (1 − desconto_pedido/100) )
arredonda2       = 2 casas, metade para cima (MidpointRounding.AwayFromZero)
```

Casos de referência (usados nos testes do back **e** do front):

| Caso | Entrada | Resultado |
|---|---|---|
| 1 | 2 × 350,00 com 10%; 1 × 200,00 com 0%; pedido 0% | subtotais 630,00 e 200,00; total **830,00** |
| 2 | mesmo pedido com 5% no pedido | total **788,50** |
| 3 | 3 × 33,33; 0% | subtotal **99,99** |
| 4 | 1,5 KG × 10,00; 0% | subtotal **15,00** |
| 5 | 1 × 0,05 com 50% | subtotal **0,03** (0,025 arredonda para cima) |
| 6 | 1 × 100,00 com 100% | subtotal **0,00** |

## Tech stack

Igual aos módulos anteriores: ASP.NET Core (.NET 10) + EF Core + PostgreSQL 18 (Docker, porta 5433) no back; React 19 + Vite + TypeScript + Ant Design 6 + TanStack Query + react-hook-form + Zod + React Router no front. **Nenhuma dependência nova**, salvo a decisão da pergunta 2 (projeto de testes xUnit).

## Modelo de dados (migration só **adiciona** tabelas)

`pedidos`: `id` (identity), `cliente_id` (FK `clientes`, RESTRICT), `data_pedido` (timestamptz, `now()`), `status` (varchar(20)), `forma_pagamento` (varchar(20), nulo no rascunho), `desconto_percentual` (numeric(5,2), padrão 0), `valor_total` (numeric(12,2)).

`pedido_itens`: `id`, `pedido_id` (FK `pedidos`, CASCADE), `produto_id` (FK `produtos`, RESTRICT), `quantidade` (numeric(12,3)), `preco_unitario` (numeric(12,2)), `desconto_percentual` (numeric(5,2)).

Índices/restrições: `ix_pedidos_cliente_id`, `ix_pedidos_data_pedido`, único `ux_pedido_itens_pedido_produto (pedido_id, produto_id)`, `CHECK` de quantidade > 0 e descontos entre 0 e 100. `valor_total` é gravado a cada salvamento por **um único método** de recálculo; o subtotal de cada item é derivado (não gravado).

## API (`/api/pedidos`)

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/pedidos?busca=&status=&pagina=&tamanhoPagina=` | Lista paginada (mais recentes primeiro). `busca` = nº do pedido ou nome do cliente | 200, 400 |
| GET | `/pedidos/{id}` | Pedido com itens | 200, 404 |
| POST | `/pedidos` | Cria um **rascunho** com os itens | 201, 400 |
| PUT | `/pedidos/{id}` | Substitui cliente, itens, desconto e pagamento (**só rascunho**) | 200, 400, 404, 409 |
| PATCH | `/pedidos/{id}/confirmar` | Rascunho → Confirmado (R6) | 200, 400, 404, 409 |
| PATCH | `/pedidos/{id}/cancelar` | → Cancelado (R7) | 204, 404 |

- Corpo do POST/PUT: `clienteId`, `formaPagamento?` (`Dinheiro`, `Pix`, `Boleto`, `Cartao`), `descontoPercentual`, `itens[] { produtoId, quantidade, descontoPercentual }`.
- No PUT, item cujo produto **já estava** no pedido mantém o preço congelado; produto novo copia o preço atual (R3).
- Resposta: cabeçalho + `clienteNome`, `subtotalItens`, `valorTotal` e itens com `produtoNome`, `sku`, `unidade`, `precoUnitario`, `subtotal`.
- Erros: 400 no formato do `[ApiController]` (campos `ClienteId`, `Itens`, `FormaPagamento`); transição inválida = 409 (`ConflitoException`).

## Telas

- **`/pedidos`** (item **Pedidos** no menu, em Gestão Comercial): tabela com Nº, Cliente, Data, Itens, Total, Status e ação **Abrir**; filtro Filtrar (busca + status) e tags removíveis, como em Produtos; no celular vira cartões.
- **`/pedidos/novo`** e **`/pedidos/:id`**: página (não painel lateral, por causa da tabela de itens). Cliente por seleção com **busca no servidor** (só ativos); tabela de itens editável (produto por seleção com busca no servidor, quantidade, preço só leitura, desconto %, subtotal); desconto do pedido, forma de pagamento e **resumo** (soma, desconto, total) recalculado na hora.
- Rascunho: **Salvar rascunho**, **Confirmar pedido** (salva pendências e confirma) e **Cancelar pedido**. Confirmado/Cancelado: tudo somente leitura; confirmado ainda tem **Cancelar**.
- Tag de status própria (Rascunho, Confirmado, Cancelado) no tema Ambition.
- O cálculo da tela é só **pré-visualização**; vale sempre o que o servidor devolve ao salvar.

## Commands

```
# Backend (na raiz)
dotnet build ErpPortfolio.slnx
dotnet run --project backend/ErpPortfolio.Api --launch-profile http        # API em http://localhost:5065
dotnet ef migrations add NomeDaMigration --project backend/ErpPortfolio.Api -o Data/Migrations
dotnet ef database update --project backend/ErpPortfolio.Api
#   (com a API rodando, o build trava o .exe: acrescente --configuration Release aos comandos dotnet ef)

# Frontend (em frontend/erp-portfolio-web)
npm run dev          # http://localhost:5173
npx tsc -b           # tipos
npx oxlint src       # lint
npm run build

# Testes automatizados (se a pergunta 2 for aprovada)
dotnet test backend/ErpPortfolio.Tests
```

## Project structure (só o que é novo)

```
backend/ErpPortfolio.Api/
  Models/            Pedido.cs, PedidoItem.cs, StatusPedido.cs, FormaPagamento.cs
  DTOs/              Pedido{Criacao,Atualizacao,Resposta,Resumo,Filtro}Dto.cs, PedidoItemDto.cs
  Services/          IPedidoService.cs, PedidoService.cs, CalculoPedido.cs   # CalculoPedido = funções puras
  Controllers/       PedidosController.cs
  Data/Migrations/   <data>_CriacaoTabelasPedidos.cs
backend/ErpPortfolio.Tests/      (opcional) CalculoPedidoTests.cs, TransicoesPedidoTests.cs
frontend/erp-portfolio-web/src/
  api/pedidosApi.ts   hooks/usePedidos.ts   types/pedido.ts   schemas/pedidoSchema.ts
  utils/calculoPedido.ts          # mesma fórmula do back, com os mesmos casos de referência
  components/TagStatusPedido.tsx
  pages/Pedidos/PedidosListaPage.tsx, PedidoPage.tsx, ItensPedidoTabela.tsx
```

## Code style

Igual ao restante do projeto: **cabeçalho obrigatório** em todo arquivo C#/TS (nome, versão, data, descrição, banco/tabelas/fontes, histórico), nomes em português, `trim` antes de validar, mensagens de erro em português. Exemplo do padrão (o cálculo fica em funções puras, sem banco):

```csharp
public static class CalculoPedido
{
    public static decimal Subtotal(decimal quantidade, decimal precoUnitario, decimal descontoPercentual) =>
        Arredondar(quantidade * precoUnitario * (1 - descontoPercentual / 100));

    public static decimal Total(IEnumerable<decimal> subtotais, decimal descontoPedidoPercentual) =>
        Arredondar(subtotais.Sum() * (1 - descontoPedidoPercentual / 100));

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
```

## Testing strategy

O projeto **ainda não tem testes automatizados**; a verificação vem sendo feita numa instância temporária da API (porta 5099) + scripts + Playwright, apagando os dados de teste. Para Pedidos:

1. **Unitário (xUnit, se aprovado):** `CalculoPedido` com os 6 casos de referência e as transições de status (R1/R2). É a regra de dinheiro; merece um teste que rode sozinho.
2. **API ponta a ponta (instância temporária):** criação, todos os 400 (cliente/produto inativo ou inexistente, item repetido, quantidade decimal em `UN`, desconto 101, sem itens), PUT só em rascunho, confirmar/cancelar e os 409, preço congelado após mudar o preço do produto, total confere com os casos de referência, busca e filtro de status, 404.
3. **Tela (Playwright):** montar pedido, total ao vivo, salvar, confirmar (fica travado), cancelar, filtros, URL direta, celular sem rolagem horizontal.
4. **Sempre:** `dotnet build` com 0 avisos, `tsc -b` e `oxlint` sem apontamentos; nenhum dado real alterado (produto, categoria e cliente existentes intactos).

## Boundaries

- **Sempre:** o servidor calcula preço e total; validar tudo na API (a tela é só conveniência); cabeçalho em cada arquivo; backup do banco antes de migration; testar em instância temporária e limpar os dados de teste; atualizar README e graphify ao terminar; `git commit` só quando o Rafael pedir.
- **Perguntar antes:** dependência nova (xUnit, biblioteca de datas), mudar tabela existente (`clientes`, `produtos`, `categorias`), token de concorrência (`xmin`), estoque/parcelas/PDF, qualquer forma de editar pedido confirmado.
- **Nunca:** aceitar preço ou total vindo do cliente, excluir pedido, editar pedido confirmado, commitar segredos, forçar push, rodar teste contra dados reais sem limpeza.

## Success criteria (testáveis)

1. Os 6 casos de referência dão exatamente os valores da tabela, no back **e** no front.
2. Criar pedido com 2 itens devolve 201, status `Rascunho`, preços copiados do produto e `valorTotal` correto.
3. Mudar o preço do produto **não** altera o preço dos itens de pedidos existentes.
4. `PUT` em pedido confirmado ou cancelado retorna 409 e nada muda.
5. Confirmar sem forma de pagamento, com cliente inativo ou com produto inativo retorna 400 no campo correto.
6. Confirmar duas vezes retorna 409; cancelar duas vezes retorna 204.
7. Quantidade `2,5` em produto `UN` é recusada (400); em produto `KG` é aceita.
8. Item repetido, 0 itens, mais de 100 itens, desconto acima de 100 e quantidade ≤ 0 retornam 400.
9. Cliente e produto inativados **depois** continuam aparecendo nos pedidos antigos.
10. Na tela, o total muda ao editar quantidade/desconto e, ao salvar, é substituído pelo valor do servidor sem diferença.
11. Confirmado fica somente leitura (sem botão de salvar), mas ainda pode ser cancelado.
12. Nenhum registro real (cliente, produto, categoria) é alterado pelos testes.

## Open questions

Nenhuma em aberto: as cinco dúvidas da primeira versão foram fechadas em 21/09/2026 (ver "Decisões já tomadas"). Limites conhecidos, aceitos de propósito:

- **Sem controle de concorrência:** duas abas editando o mesmo rascunho valem "a última grava". Se virar problema, token `xmin` (pergunta antes, ver Boundaries).
- **Cálculo duplicado** no back e no front: mitigado pelos mesmos 6 casos de referência nos dois lados.
- **Seleção com busca no servidor** para cliente e produto (sem carregar tudo), porque esses cadastros crescem.
