# Banco de dados

> Parte da documentação do [Ambition ERP](../README.md).


- Banco: `erp_portfolio_db` / usuário: `erp_user` / porta no host: **5433**
- Container: `erp_portfolio_postgres` / volume: `erp_portfolio_pgdata`

Tabela `public.clientes`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_clientes`, identity (generated always) |
| `nome` | varchar(150) | obrigatório, índice `ix_clientes_nome` |
| `documento` | varchar(14) | CPF/CNPJ sem máscara, índice **único** `ix_clientes_documento` |
| `email` | varchar(150) | opcional |
| `telefone` | varchar(20) | opcional |
| `cidade` | varchar(100) | obrigatório |
| `uf` | char(2) | obrigatório, sempre em maiúsculas |
| `ativo` | boolean | obrigatório |
| `data_cadastro` | timestamptz | UTC, padrão `now()` |

Migration inicial: `20260918172909_CriacaoTabelaClientes`.

Tabela `public.produtos`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_produtos`, identity (generated always) |
| `nome` | varchar(150) | obrigatório, índice `ix_produtos_nome` |
| `sku` | varchar(30) | obrigatório, somente dígitos (número de série), índice **único** `ix_produtos_sku` |
| `categoria_id` | integer | opcional, FK `fk_produtos_categorias` → `categorias.id` (restrict), índice `ix_produtos_categoria_id` |
| `unidade` | varchar(2) | UN, KG, L, M ou CX |
| `preco_venda` | numeric(12,2) | obrigatório |
| `custo` | numeric(12,2) | obrigatório |
| `ativo` | boolean | obrigatório |
| `data_cadastro` | timestamptz | UTC, padrão `now()` |

Migration: `20260921174744_CriacaoTabelaProdutos` (só cria a tabela e os índices; não altera `clientes`). Para aplicar em um banco já existente: `dotnet ef database update --project backend/ErpPortfolio.Api`.

Tabela `public.categorias`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_categorias`, identity (generated always) |
| `nome` | varchar(60) | obrigatório, índice **único** `ix_categorias_nome` (exato; o "sem diferenciar maiúsculas" é garantido pelo serviço) |
| `ativo` | boolean | obrigatório |
| `data_cadastro` | timestamptz | UTC, padrão `now()` |

Migration: `20260921182807_CategoriasComoRegistro`. **Foi editada à mão**: o EF gerava a remoção da coluna `produtos.categoria` (texto) antes de criar a tabela nova, o que apagaria as categorias já cadastradas. Agora ela cria `categorias`, converte cada texto distinto (sem diferenciar maiúsculas/minúsculas e sem espaços nas pontas) em um registro, liga os produtos a ele e só então remove a coluna antiga; o `Down` devolve os nomes como texto. Foi testada num banco descartável (variações de maiúsculas, nulos e vazios, subida e volta) antes de ir para o banco de desenvolvimento.

Tabela `public.pedidos`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_pedidos`, identity (generated always); também é o **número** do pedido |
| `cliente_id` | integer | obrigatório, FK `fk_pedidos_clientes` → `clientes.id` (restrict), índice `ix_pedidos_cliente_id` |
| `data_pedido` | timestamptz | UTC, padrão `now()`, índice `ix_pedidos_data_pedido` |
| `status` | varchar(20) | `Rascunho`, `Confirmado` ou `Cancelado` (gravado como texto) |
| `forma_pagamento` | varchar(20) | opcional (nulo até confirmar); `Dinheiro`, `Pix`, `Boleto` ou `Cartao` |
| `desconto_percentual` | numeric(5,2) | obrigatório, `CHECK` entre 0 e 100 |
| `valor_total` | numeric(12,2) | gravado por um único método de recálculo (nunca calculado na query) |

Tabela `public.pedido_itens`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_pedido_itens`, identity (generated always) |
| `pedido_id` | integer | FK `fk_pedido_itens_pedidos` → `pedidos.id` (**cascade**: apagar o pedido apaga os itens) |
| `produto_id` | integer | FK `fk_pedido_itens_produtos` → `produtos.id` (restrict) |
| `quantidade` | numeric(12,3) | `CHECK` > 0 |
| `preco_unitario` | numeric(12,2) | **copiado do produto** ao adicionar o item; nunca muda depois |
| `desconto_percentual` | numeric(5,2) | `CHECK` entre 0 e 100 |

Índice único `ux_pedido_itens_pedido_produto (pedido_id, produto_id)`: impede o mesmo produto duas vezes no mesmo pedido no banco (reforço; a API já recusa antes). O subtotal do item **não é gravado** — é sempre `quantidade × preço × (1 − desconto/100)`, calculado na hora.

Migration: `20260921193153_CriacaoTabelasPedidos` (só cria as duas tabelas nova; não altera `clientes`, `produtos` nem `categorias`).

Tabela `public.estoque_movimentacoes`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_estoque_movimentacoes`, identity (generated always) |
| `produto_id` | integer | FK `fk_estoque_movimentacoes_produtos` → `produtos.id` (restrict), índice `ix_estoque_movimentacoes_produto_id` |
| `tipo` | varchar(20) | `Entrada` ou `Saida` (gravado como texto) |
| `quantidade` | numeric(12,3) | `CHECK` > 0; o `tipo` é que define se soma ou subtrai do saldo |
| `motivo` | varchar(200) | opcional; automático nas movimentações geradas por pedido (`"Venda pedido #N"` / `"Estorno cancelamento pedido #N"`) |
| `pedido_id` | integer | FK `fk_estoque_movimentacoes_pedidos` → `pedidos.id` (restrict), opcional (nulo em entrada manual), índice `ix_estoque_movimentacoes_pedido_id` |
| `data_movimentacao` | timestamptz | UTC, padrão `now()`, índice `ix_estoque_movimentacoes_data_movimentacao` |

O saldo de um produto **não é gravado**: é sempre `Σ Entrada − Σ Saída` das suas movimentações, calculado na consulta (subconsulta agregada por `produto_id`). Migration: `20260922120855_CriacaoTabelaEstoqueMovimentacoes` (só cria essa tabela; não altera `pedidos`, `produtos`, `clientes` nem `categorias`).

Tabela `public.parcelas_receber`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_parcelas_receber`, identity (generated always) |
| `pedido_id` | integer | FK `fk_parcelas_receber_pedidos` → `pedidos.id` (restrict) |
| `numero_parcela` | integer | 1-based, `CHECK` > 0 |
| `valor` | numeric(12,2) | `CHECK` > 0 |
| `vencimento` | date | sem hora (é uma data de calendário) |
| `status` | varchar(20) | `Pendente`, `Recebido` ou `Cancelado` (gravado como texto); "Atrasado" não é gravado |
| `data_recebimento` | timestamptz | nulo até ser marcada como recebida |

Índice único `ux_parcelas_receber_pedido_numero (pedido_id, numero_parcela)` e índice `ix_parcelas_receber_vencimento`. Migration: `20260922190422_CriacaoTabelaParcelasReceber` (só cria essa tabela; não altera `pedidos`, `produtos`, `clientes`, `categorias` nem `estoque_movimentacoes`).

Tabela `public.vendedores`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_vendedores`, identity (generated always) |
| `nome` | varchar(150) | índice `ix_vendedores_nome` |
| `cpf` | char(11) | só dígitos; índice único `ix_vendedores_cpf` |
| `email` / `telefone` | varchar(150) / varchar(20) | opcionais |
| `percentual_comissao` | numeric(5,2) | padrão 0, `CHECK` entre 0 e 100 |
| `ativo` | boolean | inativação lógica |
| `data_cadastro` | timestamptz | UTC, padrão `now()` |

`public.pedidos` ganhou `vendedor_id` (integer, opcional, FK `fk_pedidos_vendedores` → `vendedores.id` restrict, índice `ix_pedidos_vendedor_id`) e `percentual_comissao` (numeric(5,2), opcional, `CHECK` 0-100; preenchido só ao confirmar). Migration: `20260923163031_AdicionaVendedores`.

Tabela `public.comissoes`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_comissoes`, identity (generated always) |
| `parcela_receber_id` | integer | FK `fk_comissoes_parcelas_receber` (restrict); **índice único** `ux_comissoes_parcela_receber_id` (1 comissão por parcela) |
| `pedido_id` / `vendedor_id` | integer | FKs restrict; índices `ix_comissoes_pedido_id`, `ix_comissoes_vendedor_id` |
| `valor_base` | numeric(12,2) | valor da parcela recebida |
| `percentual` | numeric(5,2) | % congelada no pedido; `CHECK` > 0 e ≤ 100 |
| `valor` | numeric(12,2) | `CHECK` > 0 |
| `data_geracao` | timestamptz | data do recebimento; índice `ix_comissoes_data_geracao` |
| `status` | varchar(20) | `Pendente` ou `Paga` |
| `data_pagamento` | timestamptz | nula até pagar |

Migration: `20260923181059_CriacaoTabelaComissoes` (cria a tabela e insere, em SQL, as comissões das parcelas já recebidas de pedidos com vendedor).

Tabela `public.orcamentos` (etapa 13):

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_orcamentos`, identity (generated always); é o número do orçamento |
| `cliente_id` / `vendedor_id` | integer | FKs restrict (`fk_orcamentos_clientes`, `fk_orcamentos_vendedores`; vendedor opcional); índices `ix_orcamentos_cliente_id`, `ix_orcamentos_vendedor_id` |
| `data_orcamento` | timestamptz | UTC, padrão `now()`; índice `ix_orcamentos_data_orcamento` |
| `validade` | date | último dia em que vale; "vencido" = `Aberto` com validade antes de hoje (calculado) |
| `status` | varchar(20) | `Aberto`, `Aprovado` ou `Perdido` |
| `forma_pagamento` | varchar(20) | opcional |
| `desconto_percentual` / `valor_total` | numeric(5,2) / numeric(12,2) | `CHECK` 0-100 / ≥ 0 (total calculado pelo servidor) |
| `observacoes` / `motivo_perda` | varchar(500) / varchar(200) | opcionais |
| `pedido_id` | integer | FK `fk_orcamentos_pedidos` (restrict), **único** (`ux_orcamentos_pedido_id`); `CHECK ck_orcamentos_status`: preenchido **só** no `Aprovado` |

`public.orcamento_itens` é o espelho de `pedido_itens` (`orcamento_id` com FK cascade, `produto_id` restrict, `quantidade`, `preco_unitario` congelado, `desconto_percentual`, índice único `ux_orcamento_itens_orcamento_produto`). Migration: `20260923235828_CriacaoTabelasOrcamentos`.

Tabelas `public.devolucoes` e `public.devolucao_itens` (etapa 14):

| Coluna | Tipo | Observação |
|---|---|---|
| `devolucoes.id` | integer | PK `pk_devolucoes`, identity; é o número da devolução |
| `devolucoes.pedido_id` | integer | FK `fk_devolucoes_pedidos` (restrict); índice `ix_devolucoes_pedido_id` |
| `devolucoes.data_devolucao` | timestamptz | UTC, padrão `now()`; índice `ix_devolucoes_data_devolucao` |
| `devolucoes.motivo` | varchar(200) | opcional |
| `devolucoes.valor_total` / `valor_abatido` / `valor_reembolso` | numeric(12,2) | `CHECK ck_devolucoes_valores`: total > 0, os dois ≥ 0 e total = abatido + reembolso |
| `devolucao_itens.devolucao_id` / `pedido_item_id` | integer | FKs cascade / restrict; único `ux_devolucao_itens_devolucao_item` |
| `devolucao_itens.quantidade` / `valor` / `volta_estoque` | numeric(12,3) / numeric(12,2) / boolean | `CHECK` quantidade > 0 e valor ≥ 0 |

Na mesma migration (`20260924004530_AdicionaDevolucoes`), `parcelas_pagar` ganhou `devolucao_id` (FK `fk_parcelas_pagar_devolucoes`, restrict) e o `CHECK ck_parcelas_pagar_origem` passou a aceitar `Devolucao` (exige `devolucao_id`); `comissoes.parcela_receber_id` passou a ser opcional e ganhou `devolucao_id` (FK `fk_comissoes_devolucoes`, restrict), com o `CHECK ck_comissoes_valor` exigindo **ou** comissão normal (parcela, valor > 0) **ou** estorno (devolução, valor < 0).

Etapa 16 (migration `20260928122428_AdicionaNotasFiscais`, só adiciona): `clientes` ganhou `logradouro`, `numero`, `complemento`, `bairro`, `cep` (char 8), `codigo_municipio` (char 7) e `inscricao_estadual`, todos opcionais; `produtos` ganhou `ncm` (char 8, opcional). Tabelas novas:

| Tabela / coluna | Tipo | Observação |
|---|---|---|
| `empresa` | — | uma linha só (`CHECK ck_empresa_id`: id = 1): razão social, fantasia, `cnpj` char(14), IE, endereço, `codigo_municipio`, `uf`, `serie_nfe` (0-999) |
| `notas_fiscais.tipo` | varchar(10) | `Saida` ou `Entrada`; `CHECK ck_notas_fiscais_tipo`: a entrada exige `devolucao_id` e `nota_referenciada_id` |
| `notas_fiscais.serie` / `numero` | integer | único `ux_notas_fiscais_serie_numero`; o próximo número é o maior da série + 1, com a linha da empresa travada (`FOR UPDATE`) |
| `notas_fiscais.chave` / `protocolo` | char(44) / char(15) | chave única `ux_notas_fiscais_chave` |
| `notas_fiscais.cliente_id` / `pedido_id` / `devolucao_id` / `nota_referenciada_id` | integer | FKs restrict; uma saída por pedido (`ux_notas_fiscais_pedido_saida`, índice filtrado) e uma nota por devolução |
| `notas_fiscais.destinatario_*` | texto | nome, documento e UF congelados na emissão |
| `notas_fiscais.valor_*` / `base_icms` | numeric(12,2) | produtos, desconto, base, ICMS, PIS, COFINS, total |
| `notas_fiscais.xml` | text | `nfeProc` completo (o DANFE sai dele) |
| `nota_fiscal_itens` | — | FK cascade na nota; produto (restrict), código, descrição, NCM, CFOP, unidade, quantidade, valores e impostos congelados; único (nota, numero_item) |

Tabela `public.fornecedores` (espelho exato de `public.clientes`):

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_fornecedores`, identity (generated always) |
| `nome` | varchar(150) | obrigatório, índice `ix_fornecedores_nome` |
| `documento` | varchar(14) | CPF/CNPJ sem máscara, índice **único** `ix_fornecedores_documento` (próprio, independente do de clientes) |
| `email` | varchar(150) | opcional |
| `telefone` | varchar(20) | opcional |
| `cidade` | varchar(100) | obrigatório |
| `uf` | char(2) | obrigatório, sempre em maiúsculas |
| `ativo` | boolean | obrigatório |
| `data_cadastro` | timestamptz | UTC, padrão `now()` |

Tabela `public.pedidos_compra`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_pedidos_compra`, identity (generated always); também é o **número** do pedido de compra |
| `fornecedor_id` | integer | obrigatório, FK `fk_pedidos_compra_fornecedores` → `fornecedores.id` (restrict), índice `ix_pedidos_compra_fornecedor_id` |
| `data_pedido` | timestamptz | UTC, padrão `now()`, índice `ix_pedidos_compra_data_pedido` |
| `status` | varchar(20) | `Rascunho`, `Confirmado` ou `Cancelado` (mesmo enum `StatusPedido` do Pedido de Venda) |
| `desconto_percentual` | numeric(5,2) | obrigatório, `CHECK` entre 0 e 100 |
| `valor_total` | numeric(12,2) | gravado por um único método de recálculo (reaproveita `CalculoPedido`) |

Sem `forma_pagamento`: não há Contas a Pagar ainda.

Tabela `public.pedido_compra_itens`:

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_pedido_compra_itens`, identity (generated always) |
| `pedido_compra_id` | integer | FK `fk_pedido_compra_itens_pedidos_compra` → `pedidos_compra.id` (**cascade**) |
| `produto_id` | integer | FK `fk_pedido_compra_itens_produtos` → `produtos.id` (restrict) |
| `quantidade` | numeric(12,3) | `CHECK` > 0 |
| `preco_unitario` | numeric(12,2) | **copiado do `custo` do produto** ao adicionar o item; nunca muda depois |
| `desconto_percentual` | numeric(5,2) | `CHECK` entre 0 e 100 |

Índice único `ux_pedido_compra_itens_pedido_produto (pedido_compra_id, produto_id)`, mesmo reforço de `pedido_itens`.

`public.estoque_movimentacoes` ganhou a coluna `pedido_compra_id` (integer, opcional, FK `fk_estoque_movimentacoes_pedidos_compra` → `pedidos_compra.id` restrict, índice `ix_estoque_movimentacoes_pedido_compra_id`), irmã da `pedido_id` já existente — uma movimentação preenche no máximo uma das duas. O `motivo` automático passa a incluir `"Compra pedido #N"` e `"Estorno cancelamento pedido de compra #N"`.

Migration: `20260923020209_AdicionaFornecedoresEPedidosCompra` (cria as 3 tabelas novas e a coluna em `estoque_movimentacoes`; não altera as demais tabelas).

Tabela `public.parcelas_pagar` (espelho de `public.parcelas_receber`):

| Coluna | Tipo | Observação |
|---|---|---|
| `id` | integer | PK `pk_parcelas_pagar`, identity (generated always) |
| `pedido_compra_id` | integer | FK `fk_parcelas_pagar_pedidos_compra` → `pedidos_compra.id` (restrict) |
| `numero_parcela` | integer | 1-based, `CHECK` > 0 |
| `valor` | numeric(12,2) | `CHECK` > 0 |
| `vencimento` | date | sem hora (é uma data de calendário) |
| `status` | varchar(20) | `Pendente`, `Pago` ou `Cancelado` (gravado como texto); "Atrasado" não é gravado |
| `data_pagamento` | timestamptz | nulo até ser marcada como paga |

Índice único `ux_parcelas_pagar_pedido_compra_numero (pedido_compra_id, numero_parcela)` e índice `ix_parcelas_pagar_vencimento`. Migration: `20260923131654_CriacaoTabelaParcelasPagar` (só cria essa tabela).

Na etapa 12 (migration `20260923184154_ContasPagarOrigemEComissoes`), `parcelas_pagar` ganhou `origem` (varchar(20), padrão `Compra`), `vendedor_id` (FK `fk_parcelas_pagar_vendedores`, restrict, índice `ix_parcelas_pagar_vendedor_id`), `descricao` (varchar(200)), `favorecido` (varchar(150)) e `total_parcelas` (preenchido para as existentes); `pedido_compra_id` passou a ser opcional. O CHECK `ck_parcelas_pagar_origem` exige o vínculo de cada origem (Compra → pedido de compra; Comissao → vendedor; Avulsa → descrição) e `ck_parcelas_pagar_total_parcelas` exige `total_parcelas >= numero_parcela`. `comissoes` ganhou `parcela_pagar_id` (FK `fk_comissoes_parcelas_pagar`, restrict, índice `ix_comissoes_parcela_pagar_id`) e o status `EmPagamento`.


