# API e regras de validação

> Parte da documentação do [Ambition ERP](../README.md).

## API

URL base em desenvolvimento: `http://localhost:5065/api`

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/clientes?nome=&ufs=&ativo=&pagina=1&tamanhoPagina=10` | Lista paginada, ordenada por nome, com filtros opcionais | 200, 400 |
| GET | `/clientes/{id}` | Obtém um cliente | 200, 404 |
| POST | `/clientes` | Cadastra um cliente | 201, 400, 409 |
| PUT | `/clientes/{id}` | Edita um cliente (inclusive o campo `ativo`) | 200, 400, 404, 409 |
| PATCH | `/clientes/{id}/inativar` | Inativa um cliente; repetir a chamada também retorna 204 | 204, 404 |

Produtos (mesmo desenho de respostas):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/produtos?busca=&ativo=&categoriaId=&pagina=1&tamanhoPagina=10` | Lista paginada, ordenada por nome; `busca` procura no nome **ou** no SKU; `categoriaId` filtra por categoria | 200, 400 |
| GET | `/produtos/{id}` | Obtém um produto | 200, 404 |
| POST | `/produtos` | Cadastra um produto | 201, 400, 409 |
| PUT | `/produtos/{id}` | Edita um produto; `ativo` é opcional (ausente = mantém) | 200, 400, 404, 409 |
| PATCH | `/produtos/{id}/inativar` | Inativa um produto; repetir a chamada também retorna 204 | 204, 404 |

Regras de validação de Produto (API e tela): nome de 3 a 150 caracteres (com `trim` **antes** de contar); SKU de 2 a 30 **dígitos** (somente números; letras e símbolos retornam 400); `categoriaId` opcional (precisa ser uma categoria **existente e ativa**, senão 400 no campo `categoriaId`; a categoria que o produto já tem continua aceita mesmo se inativada depois); unidade em `UN`, `KG`, `L`, `M`, `CX`; preço e custo entre 0 e 9.999.999.999,99 com no máximo 2 casas. A resposta traz `categoriaId` e `categoriaNome`.

Categorias:

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/categorias?busca=&ativo=&pagina=1&tamanhoPagina=10` | Lista paginada, ordenada por nome | 200, 400 |
| GET | `/categorias/{id}` | Obtém uma categoria | 200, 404 |
| POST | `/categorias` | Cadastra uma categoria (nome de 2 a 60 caracteres, com `trim`) | 201, 400, 409 |
| PUT | `/categorias/{id}` | Edita uma categoria; `ativo` é opcional (ausente = mantém) | 200, 400, 404, 409 |
| PATCH | `/categorias/{id}/inativar` | Inativa uma categoria; repetir a chamada também retorna 204 | 204, 404 |

Pedidos:

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/pedidos?busca=&status=&pagina=1&tamanhoPagina=10` | Lista paginada, mais recente primeiro; `busca` procura pelo número (com ou sem `#`) **ou** por trecho do nome do cliente | 200, 400 |
| GET | `/pedidos/{id}` | Obtém um pedido com os itens | 200, 404 |
| POST | `/pedidos` | Cria um **rascunho** com os itens (preço copiado do produto) | 201, 400 |
| PUT | `/pedidos/{id}` | Substitui cliente, itens, desconto e forma de pagamento (**só em rascunho**) | 200, 400, 404, 409 |
| PATCH | `/pedidos/{id}/confirmar` | Rascunho → Confirmado; exige forma de pagamento e cliente/produtos ativos | 200, 400, 404, 409 |
| PATCH | `/pedidos/{id}/cancelar` | Rascunho ou Confirmado → Cancelado; cancelar duas vezes também retorna 204 | 204, 404 |

Regras de validação de Pedido (API e tela): 1 a 100 itens, sem produto repetido (soma-se a quantidade na tela antes de enviar); quantidade de 0,001 a 999.999,999 (até 3 casas; `UN`/`CX` só inteiro); descontos (item e pedido) de 0 a 100 com até 2 casas; total até R$ 9.999.999.999,99 (acima disso, 400 no campo `Itens`); cliente e produtos precisam estar **ativos** ao criar o pedido ou adicionar/trocar um item; confirmar exige forma de pagamento preenchida e cliente/produtos ainda ativos. O preço de cada item é copiado do produto no momento em que é adicionado e nunca muda depois, mesmo que o preço do produto mude ou o cliente/produto seja inativado.

Estoque:

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/estoque?busca=&pagina=1&tamanhoPagina=10` | Lista paginada de produtos com saldo atual; `busca` procura no nome **ou** no SKU | 200, 400 |
| GET | `/estoque/{produtoId}/movimentacoes?pagina=1&tamanhoPagina=10` | Extrato paginado do produto, mais recente primeiro | 200, 404 |
| POST | `/estoque/entradas` | Lança uma entrada manual (`produtoId`, `quantidade`, `motivo` opcional) | 201, 400, 404 |

`PATCH /pedidos/{id}/confirmar` e `/cancelar` não ganharam rota nova: por dentro, confirmar chama a baixa de estoque (uma saída por item, checando saldo de **todos** os itens antes de gravar qualquer coisa) e cancelar um pedido que estava Confirmado chama o estorno (uma entrada por item). Estoque insuficiente ao confirmar retorna 400 no campo `Itens`, no mesmo formato dos outros erros de confirmar, e o pedido continua Rascunho.

Regras de validação de Estoque (API): quantidade de 0,001 a 999.999,999 (até 3 casas), igual a Pedidos; motivo opcional até 200 caracteres; entrada manual exige produto **ativo** (senão 400 no campo `ProdutoId`); saldo é sempre `Σ Entrada − Σ Saída` das movimentações do produto, nunca uma coluna gravada.

Contas a Receber:

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/contas-receber?busca=&status=&pagina=1&tamanhoPagina=10` | Lista paginada, vencimento mais próximo primeiro; `busca` = nº do pedido ou nome do cliente; `status` = `Pendente`/`Recebido`/`Cancelado`/**`Atrasado`** (calculado) | 200, 400 |
| PATCH | `/contas-receber/{id}/receber` | Marca a parcela como recebida; repetir é idempotente | 200, 404, 409 |
| GET | `/contas-receber/exportar?busca=&status=&formato=xlsx` | Todas as parcelas do filtro (sem paginação) em Excel ou PDF (`formato=pdf`), no layout dos relatórios; o nome vem no `Content-Disposition` | 200, 400 |

`PATCH /pedidos/{id}/confirmar` passou a aceitar corpo **opcional** `{ numeroParcelas, intervaloDias }` (padrão `{1, 30}`): ao confirmar com sucesso, gera essa quantidade de parcelas cuja soma bate exatamente com `valorTotal` (resto na última) e vencimento em `N × intervaloDias` dias. `PATCH /pedidos/{id}/cancelar` continua sem corpo, mas cancela por dentro as parcelas **Pendentes** do pedido quando ele estava Confirmado; parcelas já Recebidas não mudam.

Regras de validação de Contas a Receber (API): `numeroParcelas` de 1 a 12; `intervaloDias` de 1 a 180; receber uma parcela `Cancelado` retorna 409.

Dashboard:

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/dashboard/vendas` | Faturamento, ticket médio, pedidos por status e faturamento diário do **mês atual** (só pedidos Confirmados contam para faturamento/ticket médio) | 200 |
| GET | `/dashboard/contas-receber` | Total e quantidade de parcelas pendentes/atrasadas, sem filtro de mês | 200 |
| GET | `/dashboard/contas-pagar` | Mesmo formato, para as parcelas a pagar | 200 |
| GET | `/dashboard/vencimentos-pagar` | Parcelas a pagar pendentes atrasadas ou que vencem nos próximos 7 dias: `{ quantidade, total, quantidadeAtrasadas, itens: [{ id, titulo, detalhe, valor, vencimento, dias }] }` (`dias` negativo = atrasada; até 10 itens) | 200 |
| GET | `/dashboard/vencimentos-receber` | Mesmo formato, para as parcelas a receber | 200 |
| GET | `/dashboard/estoque` | Quantidade de produtos ativos com saldo de estoque ≤ 5 | 200 |

Sem parâmetros — o mês é sempre calculado no servidor (horário de Brasília, `HorarioBrasilia.Hoje()`), nunca enviado pelo cliente. Cada rota só delega pro service do módulo de origem (`PedidoService`, `ContasReceberService`, `ContasPagarService`, `EstoqueService`); não existe um "DashboardService" com lógica própria.

Fornecedores (mesmo desenho de respostas de Clientes):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/fornecedores?nome=&ufs=&ativo=&pagina=1&tamanhoPagina=10` | Lista paginada, ordenada por nome, com filtros opcionais | 200, 400 |
| GET | `/fornecedores/{id}` | Obtém um fornecedor | 200, 404 |
| POST | `/fornecedores` | Cadastra um fornecedor | 201, 400, 409 |
| PUT | `/fornecedores/{id}` | Edita um fornecedor (inclusive o campo `ativo`) | 200, 400, 404, 409 |
| PATCH | `/fornecedores/{id}/inativar` | Inativa um fornecedor; repetir a chamada também retorna 204 | 204, 404 |

Documento único **num índice próprio**, independente do de Clientes: o mesmo CPF/CNPJ pode estar cadastrado como cliente e como fornecedor.

Vendedores (mesmo desenho de Fornecedores, sem cidade/UF e com `percentualComissao`):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/vendedores?nome=&ativo=&pagina=1&tamanhoPagina=10` | Lista paginada, ordenada por nome | 200, 400 |
| GET | `/vendedores/{id}` | Obtém um vendedor | 200, 404 |
| POST | `/vendedores` | Cadastra um vendedor (CPF válido, só pessoa física; comissão 0-100 com até 2 casas) | 201, 400, 409 |
| PUT | `/vendedores/{id}` | Edita (inclusive `ativo`) | 200, 400, 404, 409 |
| PATCH | `/vendedores/{id}/inativar` | Inativa; repetir também retorna 204 | 204, 404 |

`POST`/`PUT /pedidos` aceitam `vendedorId` (opcional; se vier, precisa existir e estar ativo, senão 400 em `VendedorId`). `PATCH /pedidos/{id}/confirmar` exige vendedor ativo (400 em `VendedorId`) e grava `percentualComissao` = % do vendedor naquele momento. `GET /pedidos/{id}` devolve `vendedorId`, `vendedorNome` e `percentualComissao`.

Comissões:

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/comissoes?vendedorId=&status=&dataInicio=&dataFim=&pagina=1&tamanhoPagina=10` | `{ resultado: <lista paginada, mais recente primeiro>, totais: { totalGerado, totalPendente, totalPago } }`; `status` = `Pendente`/`Paga`; datas = data do recebimento, inclusivas; os totais são do vendedor/período (não dependem do status) | 200, 400 |
| POST | `/comissoes/gerar-conta` | Corpo `{ "ids": [1, 2], "vencimento": "2026-10-10" }`: comissões **Pendentes de um vendedor** viram **uma** conta a pagar com a soma e ficam `EmPagamento`; id inexistente, comissão não pendente ou vendedores misturados → 400 em `Ids` e nada muda (etapa 12; substitui o antigo `POST /comissoes/pagar`) | 201, 400 |
| GET | `/comissoes/exportar?vendedorId=&status=&dataInicio=&dataFim=&formato=xlsx` | Todas as comissões do filtro (sem paginação) em Excel ou PDF (`formato=pdf`), com os totais | 200, 400 |

Orçamentos (etapa 13; itens com o mesmo formato dos de pedido):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/orcamentos?busca=&status=&pagina=1&tamanhoPagina=10` | Lista paginada (mais recentes primeiro) com `validade`, `vencido` e `pedidoId`; `busca` = número (`12` ou `#12`) ou nome do cliente; `status` = `Aberto` (só os dentro da validade), `Vencido`, `Aprovado` ou `Perdido` | 200, 400 |
| GET | `/orcamentos/{id}` | Detalhe com itens, `vencido`, `observacoes`, `motivoPerda`, `pedidoId` | 200, 404 |
| POST | `/orcamentos` | Corpo do pedido + `validade` (AAAA-MM-DD, não antes de hoje) e `observacoes`; preço copiado do produto | 201, 400 |
| PUT | `/orcamentos/{id}` | Edita o **Aberto** (inclusive vencido, para prorrogar); itens já existentes mantêm o preço | 200, 400, 404, 409 |
| POST | `/orcamentos/{id}/gerar-pedido` | Cria o pedido de venda **Rascunho** com os preços do orçamento e aprova o orçamento (mesma transação); `201` com `{ "pedidoId": N }` e `Location: /api/pedidos/N`; vencido, cliente inativo ou produto inativo → 400; já aprovado/perdido → 409 | 201, 400, 404, 409 |
| PATCH | `/orcamentos/{id}/perder` | Corpo opcional `{ "motivo": "..." }` (até 200); repetir num perdido → 204 sem mudar o motivo; aprovado → 409 | 204, 400, 404, 409 |
| GET | `/orcamentos/{id}/pdf` | Arquivo `orcamento-N.pdf` (qualquer situação) | 200, 404 |

Devoluções de venda (etapa 14):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/pedidos/{id}/devolucoes` | Histórico do pedido, com itens, conta de reembolso e estorno | 200, 404 |
| POST | `/pedidos/{id}/devolucoes` | Corpo `{ "itens": [{ "pedidoItemId": 10, "quantidade": 1, "voltaEstoque": true }], "motivo": "...", "vencimentoReembolso": "2026-09-30" }` (motivo e vencimento opcionais). Pedido não confirmado → 409; item de outro pedido, quantidade acima do disponível ou fracionada em UN/CX, vencimento passado → 400 e nada muda. Resposta com `valorTotal`, `valorAbatido`, `valorReembolso`, `parcelaPagarId` e `estornoComissao` | 201, 400, 404, 409 |
| GET | `/dashboard/devolucoes` | `{ valorTotal, quantidade }` das devoluções do mês atual | 200 |

`GET /pedidos/{id}` passou a trazer `valorDevolvido` e, por item, `quantidadeDevolvida`; `PATCH /pedidos/{id}/cancelar` responde 409 se o pedido tem devolução. Em Contas a Pagar, a origem `Devolucao` entra no filtro e cancelar essa conta → 409. Em Comissões, o estorno vem com `devolucaoId`, `numeroParcela` nulo e valor negativo, e `POST /comissoes/gerar-conta` inclui os estornos pendentes do vendedor (soma ≤ 0 → 400).

`PATCH /contas-receber/{id}/receber` passou a gerar a comissão por dentro (mesma transação). Status da comissão: `Pendente` → `EmPagamento` (conta gerada) → `Paga` (conta paga); os totais incluem `totalEmPagamento`.

Pedidos de Compra (mesmo desenho de Pedidos, sem forma de pagamento):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/pedidos-compra?busca=&status=&pagina=1&tamanhoPagina=10` | Lista paginada, mais recente primeiro; `busca` procura pelo número (com ou sem `#`) **ou** por trecho do nome do fornecedor | 200, 400 |
| GET | `/pedidos-compra/{id}` | Obtém um pedido de compra com os itens | 200, 404 |
| POST | `/pedidos-compra` | Cria um **rascunho** com os itens (preço copiado do **Custo** do produto) | 201, 400 |
| PUT | `/pedidos-compra/{id}` | Substitui fornecedor, itens e desconto (**só em rascunho**) | 200, 400, 404, 409 |
| PATCH | `/pedidos-compra/{id}/confirmar` | Rascunho → Confirmado; exige fornecedor/produtos ativos; corpo opcional `{ numeroParcelas, intervaloDias }` (padrão `{1, 30}`) gera as parcelas a pagar | 200, 400, 404, 409 |
| PATCH | `/pedidos-compra/{id}/cancelar` | Rascunho ou Confirmado → Cancelado; cancelar duas vezes também retorna 204 | 204, 400, 404 |

Confirmar dá entrada de estoque por item (ligada ao pedido de compra) e **atualiza o `custo` do produto** para o preço pago em cada item — tudo por dentro do `PedidoCompraService`, sem rota própria. Cancelar um pedido que estava Confirmado estorna a entrada (uma saída por item), mas **exige saldo suficiente em cada item**: se o que entrou já foi vendido, a API recusa com **400** no campo `Itens` e não altera nada (diferente do cancelamento de um Pedido de Venda, que nunca falha).

Contas a Pagar (espelho de Contas a Receber):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/contas-pagar?busca=&status=&origem=&pagina=1&tamanhoPagina=10` | Lista paginada, vencimento mais próximo primeiro; cada linha traz `origem` (`Compra`/`Comissao`/`Avulsa`), `favorecido` e `descricao`; `busca` = nº da compra ou trecho do favorecido/descrição; `status` = `Pendente`/`Pago`/`Cancelado`/**`Atrasado`** (calculado) | 200, 400 |
| PATCH | `/contas-pagar/{id}/pagar` | Marca a parcela como paga; repetir é idempotente; parcela cancelada retorna 409; se for de comissão, as comissões ligadas ficam Pagas | 200, 404, 409 |
| POST | `/contas-pagar` | Conta **avulsa**: `{ descricao, favorecido?, valorTotal, primeiroVencimento, numeroParcelas (1-12), intervaloDias (1-180) }`; devolve as parcelas criadas | 201, 400 |
| PATCH | `/contas-pagar/{id}/cancelar` | Cancela parcela pendente avulsa ou de comissão (de comissão devolve as comissões para Pendente); repetir retorna 200; de compra ou paga → 409 | 200, 404, 409 |
| GET | `/contas-pagar/exportar?busca=&status=&origem=&formato=xlsx` | Todas as parcelas do filtro (sem paginação) em Excel ou PDF (`formato=pdf`), no layout dos relatórios | 200, 400 |

Confirmar um pedido de compra gera as parcelas (mesmas regras de `numeroParcelas`/`intervaloDias` do Pedido de Venda); cancelar um que estava Confirmado cancela as parcelas **Pendentes** depois da checagem de saldo — se o cancelamento for recusado por saldo, nenhuma parcela muda.

Relatórios (só leitura; `formato` = `json` (padrão), `xlsx` ou `pdf` — com arquivo, a resposta é o download com `Content-Disposition`):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/relatorios/vendas?dataInicio=&dataFim=&status=&clienteId=&formato=` | Pedidos de venda do período (datas `AAAA-MM-DD`, inclusivas, obrigatórias, no máximo 366 dias), um por linha, com quantidade, valor total e ticket médio | 200, 400 |
| GET | `/relatorios/compras?dataInicio=&dataFim=&status=&fornecedorId=&formato=` | Mesmo formato, para pedidos de compra | 200, 400 |
| GET | `/relatorios/estoque?categoriaId=&somenteAbaixoMinimo=&formato=` | Posição atual dos produtos ativos: saldo, custo, valor em estoque, abaixo do mínimo | 200, 400 |

Fluxo de caixa (só leitura; mesmo `formato` dos relatórios):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/fluxo-caixa?dataInicio=&dataFim=&agrupamento=Dia\|Mes&formato=` | `{ saldoInicial, totalEntradas, totalSaidas, saldoFinal, menorSaldo, dataMenorSaldo, atrasadoReceber, atrasadoPagar, hoje, periodos: [{ inicio, entradasRealizadas, saidasRealizadas, entradasPrevistas, saidasPrevistas, saldo }] }`; datas obrigatórias, até 366 dias (93 no `Dia`); arquivo `fluxo-caixa-<inicio>_<fim>` | 200, 400 |

NF-e simulada (etapa 16):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET / PUT | `/empresa` | Dados do emitente; o PUT cria ou atualiza (CNPJ, IE, endereço, IBGE × UF, série 0-999) | 200, 400, 404 (GET sem cadastro) |
| POST | `/pedidos/{id}/nfe` | Emite a NF-e de saída do pedido confirmado; faltando dado fiscal → 400 com `errors.Pendencias` (lista) | 201, 400, 404, 409 (não confirmado ou já emitida) |
| POST | `/devolucoes/{id}/nfe` | Emite a NF-e de devolução (entrada, `finNFe=4`, `refNFe` da nota do pedido) | 201, 400, 404, 409 (venda sem NF-e ou já emitida) |
| GET | `/notas-fiscais?dataInicio=&dataFim=&clienteId=&tipo=Saida\|Entrada&busca=&pagina=&tamanhoPagina=&formato=` | Lista paginada (mais recente primeiro); `busca` = número ou trecho da chave; `formato=xlsx\|pdf` exporta todas as do filtro | 200, 400 |
| GET | `/notas-fiscais/{id}` | Detalhe com itens, impostos e a chave referenciada | 200, 404 |
| GET | `/notas-fiscais/{id}/xml` e `/danfe` | Arquivos `NFe{chave}.xml` e `NFe{chave}.pdf` | 200, 404 |

`PATCH /pedidos/{id}/cancelar` devolve **409** quando o pedido tem NF-e. O detalhe do pedido traz `notaFiscalId`, `numeroNfe` e `chaveNfe`; a lista de devoluções, `notaFiscalId` e `numeroNfe`.

O CORS expõe o cabeçalho `Content-Disposition` para o front ler o nome do arquivo.

### Filtros da listagem

| Parâmetro | Exemplo | Efeito |
|---|---|---|
| `nome` | `nome=silva` | Trecho do nome, sem diferenciar maiúsculas/minúsculas |
| `ufs` | `ufs=SP&ufs=MG` | Uma ou mais UFs (repita o parâmetro); aceita minúsculas; UF inválida retorna 400 |
| `ativo` | `ativo=true` / `ativo=false` | Só ativos / só inativos; sem o parâmetro, traz todos |
| `pagina`, `tamanhoPagina` | `pagina=2&tamanhoPagina=20` | Paginação (página 1 a 100.000; 1 a 100 por página) |

Os filtros podem ser combinados.

### Exemplo de resposta da listagem

```json
{
  "itens": [
    {
      "id": 3,
      "nome": "Silva Comércio Ltda",
      "documento": "11222333000181",
      "email": null,
      "telefone": null,
      "cidade": "Belo Horizonte",
      "uf": "MG",
      "ativo": true,
      "dataCadastro": "2026-09-18T17:39:34.336345Z"
    }
  ],
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalItens": 1,
  "totalPaginas": 1
}
```

### Formato dos erros

Todos os erros seguem o padrão **ProblemDetails** (RFC 9110):

```json
// 400 - validação
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Documento": ["Documento inválido. Informe um CPF ou CNPJ válido."],
    "Uf": ["UF inválida."]
  }
}

// 409 - documento duplicado
{
  "title": "Conflito",
  "status": 409,
  "detail": "Já existe um cliente cadastrado com este CPF/CNPJ."
}

// 409 - transição de pedido inválida (ex.: confirmar um pedido já cancelado)
{
  "title": "Conflito",
  "status": 409,
  "detail": "Pedido cancelado não pode ser confirmado."
}
```

### CORS

Liberado apenas para as origens de `Cors:OrigensPermitidas`. Em desenvolvimento (`appsettings.Development.json`) a única origem é `http://localhost:5180`. Em produção a lista fica vazia até ser configurada.

---

## Regras de validação

As mesmas regras são aplicadas no front (Zod, em `clienteSchema.ts`) e na API (DataAnnotations nos DTOs).

| Campo | Obrigatório | Regra |
|---|---|---|
| Nome | Sim | 3 a 150 caracteres |
| Documento | Sim | CPF (11 dígitos) ou CNPJ (14 caracteres) com dígitos verificadores válidos; aceita máscara; único no cadastro |
| E-mail | Não | Formato de e-mail, até 150 caracteres; vazio é gravado como `null` |
| Telefone | Não | 8 a 20 caracteres entre dígitos, espaço, `(`, `)`, `+` e `-`; vazio é gravado como `null` |
| Cidade | Sim | 2 a 100 caracteres |
| UF | Sim | Uma das 27 siglas; aceita minúsculas e grava em maiúsculas |
| Ativo | — | Só na edição; na inclusão é sempre `true` |

Parâmetros da listagem: `pagina` de 1 a 100.000, `tamanhoPagina` de 1 a 100, `nome` até 150 caracteres.

### Como o total do pedido é calculado

```
subtotal_do_item = arredonda2( quantidade × preço × (1 − desconto_do_item/100) )
soma              = Σ subtotais dos itens
total             = arredonda2( soma × (1 − desconto_do_pedido/100) )
```

Arredondamento em 2 casas, sempre para cima na metade (`AwayFromZero`, ex.: 1,005 → 1,01). O servidor calcula em `decimal` (`CalculoPedido.cs`); a tela recalcula em **aritmética inteira** (`calculoPedido.ts`, com `BigInt`) só para mostrar o total enquanto o pedido é montado — o número decimal do JavaScript erraria casos como `1.005 * 100`. Os dois lados são comparados por um teste de paridade com 2.010 casos gerados aleatoriamente (`ParidadeCalculoFrontTests.cs`); ao salvar, vale sempre o valor devolvido pelo servidor.

### Como o CPF/CNPJ é validado

- **CPF**: 11 dígitos, não todos iguais; os dois dígitos verificadores são calculados com pesos 10→2 e 11→2 (módulo 11).
- **CNPJ**: 12 caracteres `[0-9A-Z]` + 2 dígitos verificadores numéricos. O valor de cada caractere é o **código ASCII − 48** (então `0`–`9` valem 0–9 e `A` vale 17). Os pesos são 5,4,3,2,9,8,7,6,5,4,3,2 e 6,5,4,3,2,9,8,7,6,5,4,3,2 (módulo 11). Com isso, o mesmo cálculo cobre o CNPJ numérico tradicional e o novo CNPJ alfanumérico.
- A máscara (`.`, `/`, `-`, espaços) é removida antes de validar e de gravar.

---

