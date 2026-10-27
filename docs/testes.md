# Testes realizados

> Parte da documentação do [Ambition ERP](../README.md).


Testes manuais de ponta a ponta executados em 18/09/2026, com o banco em Docker, a API rodando e o front no Microsoft Edge (automatizado com Playwright).

## API

| Cenário | Resultado esperado | OK |
|---|---|---|
| POST com CPF válido | 201 | ✅ |
| POST com CPF já cadastrado | 409 | ✅ |
| POST com nome curto, CPF inválido, e-mail, telefone, cidade e UF inválidos | 400 com mensagem em cada campo | ✅ |
| POST com CNPJ alfanumérico (`12.abc.345/01de-35`) | 201, gravado como `12ABC34501DE35` | ✅ |
| POST com CNPJ numérico | 201 | ✅ |
| GET com `nome=SILVA` | Encontra "Silva Comércio Ltda" | ✅ |
| GET com `nome=%` | Nenhum resultado (curinga escapado) | ✅ |
| GET com `pagina=0` | 400 | ✅ |
| GET `/clientes/999` | 404 | ✅ |
| PUT usando o documento de outro cliente | 409 | ✅ |
| PUT com e-mail e telefone vazios | 200, gravados como `null` | ✅ |
| PUT com `ativo: true` em cliente inativo | 200, cliente reativado | ✅ |
| PUT / PATCH em id inexistente | 404 | ✅ |
| PATCH inativar | 204 e `ativo = false` | ✅ |
| Preflight CORS vindo de `http://localhost:5180` | Cabeçalhos `Access-Control-Allow-*` presentes | ✅ |
| Requisição de origem não permitida | Sem cabeçalhos de CORS | ✅ |
| Swagger (`/swagger`) | Página no ar com os 3 caminhos de `/api/clientes` | ✅ |
| GET `ufs=SP` / `ufs=SP&ufs=mg` | Só clientes dessas UFs (minúscula aceita) | ✅ |
| GET `ativo=false` / `ativo=true&ufs=ES` | Só inativos / combinação sem resultado | ✅ |
| GET `nome=silva&ufs=SP` | Filtros combinados | ✅ |
| GET `ufs=XX` | 400 "UF inválida." | ✅ |

## Tela

| Cenário | OK |
|---|---|
| Listagem carrega com total de registros | ✅ |
| Busca por nome filtra a tabela e o **x** limpa o filtro | ✅ |
| Salvar formulário vazio/inválido mostra as mensagens do Zod nos campos | ✅ |
| CPF digitado sem máscara é formatado ao sair do campo | ✅ |
| Inclusão válida fecha o painel, mostra "Cliente cadastrado com sucesso." e a linha aparece | ✅ |
| CPF duplicado mostra o erro 409 embaixo do campo CPF/CNPJ | ✅ |
| Edição carrega os dados no painel e atualiza a linha | ✅ |
| Inativação com confirmação muda a tag para "Inativo", esmaece a linha e desabilita o botão | ✅ |
| Reativação pela chave "Status do cadastro" no painel de edição | ✅ |
| Filtro de UF: "sao" encontra "São Paulo (SP)"; "RN" seleciona Rio Grande do Norte; contador "2 selecionadas" | ✅ |
| Opções do dropdown de UF com espaçamento normal (36 px por opção) | ✅ |
| Filtro SP + MG, status Inativos, combinação sem resultado ("Nenhum cliente corresponde aos filtros.") e Limpar filtros | ✅ |
| Botão **Filtrar** mostra o badge com a quantidade de filtros aplicados | ✅ |
| Filtros aplicados aparecem como tags abaixo do cabeçalho; reabrir o painel mostra os mesmos valores | ✅ |
| Remover uma tag individual (ex.: só a UF "MG") atualiza a lista sem precisar reabrir o painel | ✅ |
| "Limpar tudo" remove todas as tags e volta a listar todos os clientes | ✅ |
| Painel de filtros funciona no celular (390 px) sem estourar a largura da tela | ✅ |
| Fonte Inter carregada e fundo `#0F1112` aplicado | ✅ |
| Em 1920, 1440, 1024, 800 e 390 px a página não rola na horizontal; a tabela cabe inteira a partir de 800 px | ✅ |
| Menu lateral: 256 px em 1024 px+, 72 px (ícones) entre 768 e 991 px, gaveta no celular | ✅ |
| Nenhum erro no console além do 409 esperado | ✅ |

Os testes do tema (etapa 1.1) rodaram numa cópia isolada (banco `erp_portfolio_teste`, API na porta 5075 e front na 5174), apagada ao final, sem tocar nos dados de desenvolvimento.

Verificações de build: `dotnet build` sem avisos, `tsc -b` sem erros e `oxlint` sem apontamentos.

## Categorias (21/09/2026)

Também numa API temporária (porta 5099) e num Vite temporário (5174); os dados de teste (`ZZT…` e SKUs `77…`/`88…`) foram apagados ao final e o produto e a categoria reais ficaram intactos.

| Verificação | Resultado |
|---|---|
| Migration num banco descartável: "PERIFÉRICO"/"periférico" e "Cabos"/"  Cabos  " viram uma categoria; nulos e vazios ficam sem categoria; o banco recusa excluir categoria em uso; o `Down` devolve os nomes | ✅ |
| Migration no banco de desenvolvimento (com backup antes): o produto existente continuou ligado à categoria "PERIFÉRICO" e os clientes ficaram intactos | ✅ |
| API de Categorias e da integração com Produtos: 34 verificações (criar com `trim`, 4 casos de 400, nome duplicado inclusive com outra caixa, aviso para categoria inativa, `PUT` sem `ativo`, trocar só a caixa do próprio nome, busca, `%` sem virar curinga, status, paginação, 404; produto com/sem categoria, categoria inexistente → 400, trocar/remover categoria, categoria inativa, FK) | ✅ |
| Tela: cadastro com validação e nome com `trim`, nome duplicado com outra caixa no campo, categoria aparecendo na seleção do produto e na lista, limpar e trocar a categoria, inativar (produto mantém o nome; seleção deixa de oferecer; edição mostra "(inativa)"), renomear, filtro Inativas, celular sem rolagem horizontal | ✅ |
| Regressão: testes de Produtos (API e tela) continuam passando | ✅ |

## Produtos e correções de Clientes (21/09/2026)

Executados numa segunda instância da API (porta 5099) e num Vite temporário (porta 5174), com os dados de teste apagados ao final; a API e o front de desenvolvimento não foram tocados.

| Verificação | Resultado |
|---|---|
| API de Produtos: 31 verificações (criação com `trim` do SKU e normalização de unidade/categoria, 11 casos de 400 — inclusive SKU com letras, hífen, 1 dígito e 31 dígitos —, SKU duplicado com espaços diferentes, inativar, `PUT` sem `ativo`, conflito com produto inativo, troca de SKU no `PUT`, busca por nome/SKU, `%` sem virar curinga, filtro de status, paginação, 404) | ✅ |
| Clientes: `PUT` sem `ativo` mantém o cliente inativo; `ativo=true` continua reativando | ✅ |
| Clientes: conflito de documento avisa quando o cadastro existente está inativo | ✅ |
| Clientes: e-mail com espaço no fim passa a ser aceito no formulário | ✅ |
| Tela: `/` redireciona para `/clientes`; menu, breadcrumb e URL acompanham a navegação; `/produtos` direto e rota desconhecida | ✅ |
| Tela: validação do formulário vazio, campo SKU descartando letras e símbolos, cadastro com dinheiro em vírgula, erro de SKU duplicado no campo, edição com campos preenchidos | ✅ |
| Tela: margem calculada (53,6%) e margem negativa (−25,3%) em vermelho; inativar; busca por SKU; filtro de status; "Limpar tudo" | ✅ |
| Celular (390 px): gaveta do menu navega até Produtos, sem rolagem horizontal | ✅ |
| Nenhum erro no console além do 409 esperado | ✅ |

## Pedidos (21/09/2026)

Backend testado com 105 testes unitários (xUnit, `backend/ErpPortfolio.Tests`) e ponta a ponta numa API temporária (porta 5099); a tela testada com Playwright (Edge) contra essa API e um Vite temporário (porta 5174). Dados de teste (clientes/produtos `ZZT…`, SKUs `660000xx`) apagados ao final; dados reais (cliente, produto, categorias) conferidos idênticos antes e depois de cada rodada.

| Verificação | Resultado |
|---|---|
| Testes unitários: cálculo (subtotal, total, limite, arredondamento), transições de status, validação dos DTOs, modelo (FKs, CHECKs, índice único), paridade com o front (2.010 casos aleatórios) | ✅ 105/105 |
| API ponta a ponta: criação, os 400 (cliente/produto inativo ou inexistente, item repetido, quantidade decimal em `UN`, desconto acima de 100, sem itens, total acima do limite), `PUT` só em rascunho, confirmar/cancelar e os 409, preço congelado após mudar o preço do produto, total batendo com os 6 casos de referência da spec, busca e filtro de status, 404 | ✅ |
| Seletores de cliente e produto (T12): só ativos, busca com espera de 300 ms (uma consulta por busca, não uma por tecla), nome+documento / SKU+preço+unidade nas opções, produto já no pedido desabilitado, o escolhido mantém o nome mesmo com a busca zerada (`labelInValue`) | ✅ |
| Página do pedido — novo, itens e total (T13): casos de referência (R$ 830,00 e R$ 788,50) montados na tela, produto repetido soma a quantidade, quantidade decimal por unidade, remover item, quantidade vazia e total acima do limite bloqueiam o envio, salvar cria o rascunho com o total do servidor, preço congelado ao reabrir, erro 400 do servidor no campo certo | ✅ |
| Confirmar e cancelar (T14): janela de confirmação, confirmar salva o pendente e então confirma, erros de confirmação (sem forma de pagamento, cliente/produto inativado) no campo certo com o pedido seguindo Rascunho, cancelar rascunho e confirmado, 409 (pedido mudou por fora) recarrega a tela | ✅ |
| Celular (T15): itens em cartão abaixo de 768 px com tudo visível, sem rolagem horizontal em 390/768/1024/1920 px na lista e na página do pedido, valores muito grandes não estouram o cartão | ✅ |
| Nenhum erro no console além dos 400/404/409 esperados | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (105 aprovados), `tsc -b` sem erros e `oxlint` sem apontamentos.

## Estoque (22/09/2026)

Back-end testado com 5 testes unitários novos de cálculo de saldo, 14 de modelo (mapeamento EF), e ponta a ponta numa API temporária (porta 5099), com cliente/produtos de teste (`ZZT…`, SKUs `6700000x`) apagados ao final; dados reais conferidos idênticos antes e depois.

| Verificação | Resultado |
|---|---|
| Testes unitários: saldo (lista vazia, só entradas, só saídas, misto, negativo), mapeamento EF (colunas, FKs, índices, CHECK) | ✅ 124/124 (total do projeto) |
| Migration num banco descartável: insert válido, `CHECK` de quantidade rejeitando 0, FK rejeitando produto inexistente, `Down` removendo só a tabela nova | ✅ |
| Migration no banco de desenvolvimento (com backup antes): contagens de clientes/produtos/categorias/pedidos idênticas antes e depois | ✅ |
| Consulta: saldo agregado corretamente (produto sem movimentação = 0), busca por nome/SKU, extrato paginado mais recente primeiro, produto inexistente no extrato = 404 | ✅ |
| Entrada manual: 201 com motivo, 201 sem motivo, produto inexistente = 404, produto inativo = 400 no campo, quantidade 0 ou com 4 casas = 400 | ✅ |
| Confirmar pedido: saldo suficiente baixa exatamente a quantidade do item; saldo insuficiente recusa com 400 no campo `Itens`, pedido continua Rascunho e nenhuma movimentação é gravada; caso limite (saldo = quantidade pedida) confirma normalmente | ✅ |
| Cancelar pedido: cancelar um Confirmado devolve o saldo (estorno); cancelar um Rascunho que nunca foi confirmado não gera movimentação; cancelar duas vezes não duplica o estorno | ✅ |
| Nenhum registro real (cliente, produto, categoria, pedido) alterado pelos testes | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (124 aprovados), `tsc -b` sem erros e `oxlint` sem apontamentos, `npm run build` sem erros.

**Limitação desta rodada:** sem ferramenta de navegador/Playwright disponível na sessão em que o módulo foi construído, a tela (lista, drawer de entrada, drawer de extrato, layout no celular) **não foi verificada visualmente** pela IA — só o back-end foi testado de ponta a ponta contra a API real. **Atualização:** o Rafael testou a tela manualmente logo em seguida e confirmou que está funcionando (lista com saldo real, navegação pelo menu).

## Contas a Receber (22/09/2026)

Back-end testado com 8 testes unitários novos de divisão em parcelas, 14 de modelo (mapeamento EF), e ponta a ponta numa API temporária (porta 5099), com cliente/produtos/pedidos de teste (`ZZT…`, SKUs `6700000x`) apagados ao final; dados reais conferidos idênticos antes e depois.

| Verificação | Resultado |
|---|---|
| Testes unitários: divisão em parcelas (1 parcela, resto na última, divisão exata, 12 parcelas, valores diversos), mapeamento EF (colunas, FK, índices, CHECK) | ✅ 146/146 (total do projeto) |
| Migration num banco descartável: insert válido, `CHECK` de valor, índice único de parcela duplicada, FK de pedido inexistente, `Down` removendo só a tabela nova | ✅ |
| Migration no banco de desenvolvimento (com backup antes): contagens das outras tabelas idênticas antes e depois | ✅ |
| Consulta: `totalParcelas` correto, `atrasado` calculado certo (só `Pendente` com vencimento passado), busca por cliente e por nº do pedido, filtros de status | ✅ |
| Marcar como recebida: grava `dataRecebimento`; marcar de novo é idempotente (mesma data); marcar uma `Cancelado` = 409; parcela inexistente = 404 | ✅ |
| Confirmar pedido: 3 parcelas com soma exata e vencimentos em `N × intervalo` dias; confirmar sem informar nada usa o padrão (1 parcela, 30 dias); 12 parcelas de um valor não divisível somam exatamente o total; `numeroParcelas` fora de 1-12 = 400 | ✅ |
| Cancelar pedido: pedido com 1 parcela recebida + 2 pendentes — cancelar deixa a recebida intacta e cancela as 2 pendentes; cancelar um rascunho nunca confirmado não gera parcela nenhuma | ✅ |
| Nenhum registro real (cliente, produto, categoria, pedido, estoque) alterado pelos testes | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (146 aprovados), `tsc -b` sem erros e `oxlint` sem apontamentos, `npm run build` sem erros.

**Limitação desta rodada:** mesma situação de Estoque — sem ferramenta de navegador/Playwright nesta sessão, a tela (lista de contas a receber, modal de confirmar com parcelas, layout no celular) **não foi verificada visualmente**. O back-end foi testado de ponta a ponta contra a API real, e o código da tela segue os mesmos padrões já usados (e já confirmados funcionando) nos módulos anteriores. Recomenda-se um teste manual: confirmar um pedido escolhendo parcelas, ver a lista em `/contas-receber`, marcar uma parcela como recebida e cancelar um pedido confirmado para ver as parcelas pendentes cancelarem.

## Filtro por categoria em Produtos (22/09/2026)

Testado na API real (instância temporária, porta 5099): `GET /api/produtos?categoriaId=` achou só o produto da categoria de teste; `categoriaId` inexistente devolveu lista vazia. Produto e categoria de teste apagados ao final; dados reais intactos.

O atalho "Nova categoria" no seletor do formulário (parte visual, sem chamada de API própria pra verificar por fora do navegador) não foi testado nesta sessão.

## Dashboard (22/09/2026)

Os três endpoints (`/dashboard/vendas`, `/dashboard/contas-receber`, `/dashboard/estoque`) foram validados de duas formas:

| Verificação | Resultado |
|---|---|
| **Dados reais** do Rafael: 2 pedidos confirmados (R$ 1.400, ticket médio R$ 700), 1 parcela pendente, 1 produto com saldo baixo — os três endpoints responderam certo, sem erro, sem divisão por zero, com os 30 dias do mês presentes no gráfico | ✅ |
| Pedido **Rascunho** novo no mês: contou em `porStatus.rascunho` sem alterar faturamento/ticket médio | ✅ |
| Parcela vencida há 3 dias (inserida via SQL de teste): refletiu certo em `totalAtrasado`/`quantidadeAtrasado` | ✅ |
| Saldo baixo de estoque: produto com saldo **exatamente 5** contou; produto com saldo **6** não contou (limite exato) | ✅ |
| Cada endpoint responde de forma independente (D7) | ✅ (verificado por desenho — cada rota só delega pro service do módulo de origem, sem depender dos outros) |
| Nenhum registro real alterado pelos testes; dados de teste apagados e os três endpoints conferidos batendo com o valor de antes | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (150 aprovados), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros (bundle do gráfico SVG não aumentou o tamanho final, confirmando que nenhuma dependência nova foi instalada).

**Limitação desta rodada:** mesma situação dos módulos anteriores — sem ferramenta de navegador/Playwright nesta sessão, a tela (cards, gráfico, item de menu "Painel") **não foi verificada visualmente**. Os três endpoints foram validados com dados reais e casos de borda isolados, mas ninguém abriu `/` no navegador. Recomenda-se um teste manual.

## Fornecedores e Pedidos de Compra (23/09/2026)

Back-end testado ponta a ponta numa instância local da API (porta 5065), com fornecedor/produto/cliente de teste (`ZZT…`) e dois pedidos de compra + um pedido de venda de apoio, todos apagados via SQL ao final (não há endpoint de exclusão física para nenhum dos dois cadastros, mesmo padrão de Cliente/Produto).

| Verificação | Resultado |
|---|---|
| CRUD de Fornecedor: criar, obter, documento duplicado (409), editar, listar com filtro por nome, inativar (204, `ativo=false`) | ✅ |
| Item do Pedido de Compra nasce com `precoUnitario` = `Custo` atual do produto | ✅ |
| Confirmar: gera Entrada de estoque por item ligada ao pedido (`"Compra pedido #N"`, `pedidoCompraId` preenchido), saldo sobe, e **atualiza o `Custo` do produto** para o preço congelado no item — testado simulando uma mudança de custo entre montar o item e confirmar (custo mudou pra 15, confirmar voltou pra 10, o preço do item) | ✅ |
| Confirmar com fornecedor inativo, depois com produto inativo (fornecedor reativado) | 400 no campo certo nos dois casos (`FornecedorId` / `Itens`), pedido continua Rascunho | ✅ |
| Cancelar um Confirmado com saldo intacto: gera Saída de estorno por item (`"Estorno cancelamento pedido de compra #N"`), saldo volta a 0 | ✅ |
| Cancelar duas vezes o mesmo pedido: idempotente (204 nas duas, sem duplicar movimentação) | ✅ |
| **Caso novo (PC7):** confirmar uma segunda compra (saldo 5), vender 3 unidades num Pedido de Venda confirmado (saldo fica 2), tentar cancelar a compra → **400** ("Estoque insuficiente para estornar: ... saldo 2,000, a compra tinha entrado com 5,000"), saldo e status do pedido de compra **inalterados** | ✅ |
| Extrato de estoque (`GET /estoque/{id}/movimentacoes`) traz `pedidoCompraId` no formato esperado pelo tipo do front | ✅ |
| Nenhum registro real alterado pelos testes; todos os dados de teste (fornecedor, produto, cliente, 2 pedidos de compra, 1 pedido de venda + parcela) apagados via SQL ao final | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (150 aprovados — o projeto não tem xUnit tocando o `DbContext`, só lógica pura; a regra de negócio do Pedido de Compra foi verificada pelo E2E acima, mesmo padrão do `PedidoService`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros (bundle sem crescer — nenhuma dependência nova).

**Limitação desta rodada:** mesma situação dos módulos anteriores — sem ferramenta de navegador/Playwright nesta sessão, as telas de Fornecedores e Pedidos de Compra (listas, drawer, formulário, celular) **não foram verificadas visualmente**. O back-end foi testado de ponta a ponta contra a API real, e o código das telas segue exatamente os mesmos padrões já usados (e já confirmados funcionando) nos módulos de Clientes e Pedidos. Recomenda-se um teste manual: cadastrar um fornecedor, montar um pedido de compra, confirmar e ver a entrada aparecer no extrato de estoque com "Compra #N" e o custo do produto atualizado.

## Contas a Pagar (23/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099 — a API de desenvolvimento na 5065 continuou rodando intacta), por um script Python com 26 verificações, usando fornecedor/produto de teste (`ZZT…`) e 4 pedidos de compra, todos apagados via SQL ao final.

| Verificação | Resultado |
|---|---|
| Confirmar com 3 parcelas / 30 dias num pedido de R$ 1.000,00 → 333,33 / 333,33 / 333,34 (soma exata), vencendo em +30/+60/+90 dias, Pendentes, `X/3`, com o nome do fornecedor | ✅ |
| Confirmar sem corpo → 1 parcela em 30 dias | ✅ |
| 0 ou 13 parcelas, 0 ou 181 dias → 400; pedido continua Rascunho e sem parcelas | ✅ |
| Busca por nº da compra e por nome do fornecedor; filtros Pendente, Pago e **Atrasado** (vencimento forçado no passado via SQL: só a vencida aparece, com `atrasado=true`); status inválido (`Recebido`) → 400 | ✅ |
| Marcar como paga → `Pago` com data; de novo → 200 com a mesma data; parcela inexistente → 404 | ✅ |
| Cancelar a compra: parcela Paga fica `Pago`, as Pendentes viram `Cancelado`; pagar uma cancelada → 409; cancelar de novo não muda nada | ✅ |
| Cancelamento **bloqueado por saldo** (saldo consumido via SQL) → 400; parcelas continuam Pendentes e o pedido continua Confirmado | ✅ |
| `GET /dashboard/contas-pagar` bate com a soma direta no banco (pendente e atrasado) | ✅ |
| Dados de teste apagados; `parcelas_pagar` vazia; pedido de compra real (#4) intacto | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (150 aprovados), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros (nenhuma dependência nova).

**Limitação desta rodada:** sem navegador nesta sessão, a tela de Contas a Pagar, o modal de parcelas no Pedido de Compra (e o da venda, que passou a usar o mesmo componente) e o novo layout do Dashboard **não foram verificados visualmente**. Recomenda-se um teste manual: confirmar um pedido de compra escolhendo 2 ou 3 parcelas, ver as parcelas em `/contas-pagar`, marcar uma como paga, cancelar a compra e conferir o card "Contas a pagar" no Dashboard; e confirmar um pedido de venda para garantir que o modal continua igual.

## Relatórios (23/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script Python com 25 verificações. Como os relatórios só leem, a conferência foi feita contra os **dados reais**, comparando cada resposta com SQL direto no banco — nada foi criado nem apagado.

| Verificação | Resultado |
|---|---|
| Vendas e compras de setembro/2026: mesmos pedidos, soma, quantidade e ticket médio que o SQL; sem status traz todos; filtros por cliente e por fornecedor | ✅ |
| Período de 1 dia inclui o dia inteiro (data final inclusiva) | ✅ |
| Estoque: produtos ativos, saldo, valor em estoque (saldo × custo) e abaixo do mínimo batem por produto; total = soma das linhas; filtros de categoria e "só abaixo do mínimo" | ✅ |
| Período invertido, 367 dias, datas ausentes e formato inválido → 400 no campo certo | ✅ |
| `.xlsx` dos 3 relatórios (lido com um leitor só de biblioteca padrão): mesmas linhas e mesmo total do JSON; nome do arquivo conforme R4 | ✅ |
| `.pdf` dos 3 relatórios e de um período sem registros: PDF válido, nome certo; **abertos e conferidos visualmente** (título, filtros, cards, tabela, estoque em paisagem, "Nenhum registro...") | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (158 aprovados, 8 novos em `RelatorioCalculoTests`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

**Limitação desta rodada:** sem navegador nesta sessão, as **telas** de relatório (filtros, seletor de período, cards, tabela, botões de exportar, download pelo navegador) **não foram verificadas visualmente** — os arquivos gerados pelo servidor, sim. Recomenda-se: gerar cada relatório, baixar o Excel e o PDF pela tela e abrir os dois.

## Vendedores (23/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script Python com 19 verificações, usando vendedores/cliente/produto de teste (`ZZT…`), todos apagados via SQL ao final; os 3 pedidos reais ficaram intactos.

| Verificação | Resultado |
|---|---|
| CRUD: criar (CPF com máscara gravado só com dígitos), CPF repetido → 409, CNPJ → 400, comissão 100,01% → 400, listar com filtro, editar, reativar pelo PUT | ✅ |
| Rascunho com e sem vendedor; vendedor inexistente ou trocado por um inativo → 400 em `VendedorId` | ✅ |
| Confirmar sem vendedor, ou com vendedor inativado depois → 400 em `VendedorId`, pedido continua Rascunho | ✅ |
| Confirmar grava a % do vendedor (5,50); mudar a % do vendedor para 9 **não** altera esse pedido; o pedido confirmado depois pega 9 | ✅ |
| Pedido antigo (sem vendedor) continua abrindo; lista de pedidos inalterada | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (172 aprovados, 14 novos em `VendedorValidacaoTests`: CPF com/sem máscara, CNPJ e CPF inválido recusados, comissão 0-100 com 2 casas), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

**Limitação desta rodada:** sem navegador nesta sessão, a tela de Vendedores e o campo Vendedor no pedido **não foram verificados visualmente**. Recomenda-se: cadastrar um vendedor com 5%, criar um pedido de venda escolhendo esse vendedor, confirmar e ver a comissão congelada no pedido; depois mudar a % do vendedor e reabrir o pedido.

## Comissões (23/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script Python com 18 verificações, usando vendedores/cliente/produto de teste (`ZZT…`) e 5 pedidos, todos apagados via SQL ao final; os dados reais ficaram intactos.

| Verificação | Resultado |
|---|---|
| Parcela de R$ 350 de pedido confirmado a 5% gera R$ 17,50 — mesmo com a % do vendedor já alterada para 9% depois da confirmação; receber de novo não duplica | ✅ |
| Pedido de R$ 1.000 em 3 parcelas a 5,5%: 3 comissões de R$ 18,33 (arredondamento) | ✅ |
| Vendedor com 0% e pedido sem vendedor: recebem normalmente e não geram comissão | ✅ |
| A SQL de carga da migration, rodada sobre uma parcela marcada como recebida "antes", gera R$ 16,50 (300 × 5,5%) | ✅ |
| Lista: vendedor, pedido, parcela X/Y, base e valor; filtros por vendedor, status e período; totais batendo com SQL; período invertido → 400; período vazio → totais 0 | ✅ |
| Pagar 2 em lote → Paga com data; pagar de novo mantém a data; id inexistente → 400 sem alterar nada; lista vazia → 400 | ✅ |
| Cancelar pedido com parcela recebida mantém a comissão | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (177 aprovados, 5 novos em `ComissaoCalculoTests`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

**Limitação desta rodada:** sem navegador nesta sessão, a tela de Comissões **não foi verificada visualmente**. Recomenda-se: confirmar um pedido com vendedor, receber uma parcela em Contas a Receber, abrir Financeiro → Comissões, filtrar pelo vendedor e marcar como paga (uma e depois várias).

## Comissão vira conta a pagar + contas avulsas (23/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script Python com 28 verificações, usando fornecedor/cliente/vendedores/produto de teste (`ZZT…`), todos apagados via SQL ao final; as contas e comissões reais ficaram intactas (a migration preencheu as 2 parcelas reais da compra #12 com `Compra`, 1/2 e 2/2).

| Verificação | Resultado |
|---|---|
| Regressão da compra: 3 parcelas com origem Compra, favorecido = fornecedor, 1/3..3/3 e valores certos; pagar funciona; cancelar parcela de compra direto → 409; cancelar o pedido de compra cancela as pendentes | ✅ |
| Conta avulsa de R$ 1.000 em 3x a partir de 05/10: 333,33 / 333,33 / 333,34, vencimentos 05/10, 04/11, 04/12; descrição curta, valor 0, sem vencimento e 13 parcelas → 400 no campo | ✅ |
| Busca pelo favorecido, filtro de origem, e busca por número não traz avulsas/comissões por engano | ✅ |
| Cancelar avulsa pendente (e de novo, idempotente); paga → 409; inexistente → 404 | ✅ |
| Gerar conta: vendedores misturados / id inexistente / sem vencimento → 400 sem alterar nada; 3 comissões de um vendedor → 1 conta de R$ 25,00 (1/1, favorecido = vendedor, descrição "Comissões — … (3)"), comissões Em pagamento e ligadas; gerar de novo → 400; totais com Em pagamento | ✅ |
| Pagar a conta de comissão → as 3 comissões Pagas com a mesma data; cancelar outra conta de comissão → comissão volta a Pendente, desligada, e gera conta de novo | ✅ |
| `POST /comissoes/pagar` não existe mais (404) | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (185 aprovados, 8 novos em `ContasPagarCalculoTests`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

**Limitação desta rodada:** sem navegador nesta sessão, as telas de Contas a Pagar (Nova conta, colunas, filtro, cancelar) e de Comissões (Gerar conta a pagar) **não foram verificadas visualmente**. Recomenda-se: lançar uma conta avulsa em 2 parcelas; em Comissões, gerar a conta de um vendedor; pagar essa conta em Contas a Pagar e ver as comissões virarem Pagas; gerar outra e cancelar para vê-las voltar a "A pagar".

## Orçamentos (23/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script PowerShell com 54 verificações, e a **tela** com Playwright (Edge headless) contra essa API e um Vite temporário (porta 5174), com 25 verificações. Dados de teste (`ZZT Orc…`, SKUs `690000xx`, vendedores de teste) apagados via SQL ao final; dados reais e contagens de pedidos/orçamentos/estoque/parcelas idênticos antes e depois.

| Verificação | Resultado |
|---|---|
| Criar: preço copiado do produto, total 185,25 (itens com desconto + 5% geral), observações aparadas; validade passada / ausente, sem itens, produto repetido, UN fracionada, observações com 501, cliente inativo → 400 no campo; validade = hoje aceita | ✅ |
| Editar: item existente mantém o preço (100) com o produto já a 130; item novo pega o preço atual; item removido sai | ✅ |
| Vencido calculado no detalhe e na lista; filtros Aberto/Vencido/Aprovado/Perdido e busca `#N`; prorrogar pela edição | ✅ |
| Gerar pedido: vencido → 400; produto inativo → 400 sem gravar nada; gera o rascunho com cliente, vendedor, forma, descontos e **preço do orçamento**, total igual; orçamento Aprovado com `pedidoId`; gerar de novo / editar / perder o aprovado → 409; vendedor inativo fica em branco; cliente inativado → 400 | ✅ |
| O pedido gerado confirma pelo fluxo normal: 2 saídas de estoque e 2 parcelas somando o total | ✅ |
| Perder: motivo aparado, idempotente (sem trocar o motivo), sem corpo, motivo com 201 → 400, vencido pode, inexistente → 404 | ✅ |
| PDF: 200 `application/pdf` (`%PDF`), `orcamento-N.pdf`, 404 para inexistente; arquivo aberto e conferido | ✅ |
| Tela: lista com as 4 situações e link do pedido; filtro Vencido; preço congelado; vencido desabilita "Gerar pedido" e prorrogar reabilita; perdido/aprovado somente leitura; novo com validade hoje + 15, erros de campo e breadcrumb; marcar como perdido; gerar pedido abre o rascunho com o preço do orçamento; celular sem rolagem horizontal; console sem erros | ✅ |

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (207 aprovados, 22 novos em `OrcamentoTests`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

## Devolução de venda (24/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script PowerShell com 36 verificações, e a **tela** com Playwright (Edge headless) contra essa API e um Vite temporário (porta 5174), com 14 verificações. Pedido de teste: 3 mouses a R$ 100 (10% no item) + 2 kg de arroz a R$ 10, 5% no pedido = R$ 275,50 em 3 parcelas, vendedor a 10%. Dados de teste (`ZZT Dev…`, SKUs `680000xx`) apagados via SQL ao final; dados reais, contagens e soma das comissões idênticos antes e depois. Backup do banco antes da migration em `.claude/ferramentas-locais/`.

| Verificação | Resultado |
|---|---|
| Parcial com parcelas pendentes: 1 mouse = R$ 85,50, abatido da última parcela (91,84 → 6,34), sem reembolso nem estorno; estoque +1; pedido com `quantidadeDevolvida`/`valorDevolvido`; faturamento do mês inalterado | ✅ |
| 8 validações (acima do disponível, UN fracionada, repetido, sem itens, vencimento passado, item de outro pedido, rascunho/cancelado 409, inexistente 404) sem alterar nada | ✅ |
| Misto depois de receber 2 parcelas, como perda: 6,34 abatido (parcela cancelada mantendo o valor) + 79,16 de reembolso (conta `Devolucao`, favorecido = cliente, vencimento informado) + estorno −7,92; estoque inalterado | ✅ |
| Devolução do restante fecha exatamente 275,50 (estorno −10,45); nada mais a devolver → 400; histórico com as 3 devoluções | ✅ |
| Cancelar pedido com devolução → 409 (antes virava 500: o endpoint não tratava a exceção — corrigido); cancelar reembolso → 409; pagar reembolso funciona | ✅ |
| Fechamento de comissões: estornos maiores que as comissões → 400; com mais uma comissão, a conta inclui os estornos (5 itens, R$ 9,99); cancelar/pagar a conta propaga aos estornos | ✅ |
| Card do Dashboard (+3 devoluções, +R$ 275,50); regressão do cancelar sem devolução e da lista de comissões | ✅ |
| Tela: botões e histórico antes/depois, prévias R$ 85,50 e R$ 190,00, mensagem com abatido/reembolso/estorno, perda e motivo no histórico, reembolso em Contas a Pagar sem cancelar, estorno em Comissões, card do Dashboard, celular, console limpo | ✅ |

Achados da tela, corrigidos: o vencimento "hoje" do modal era recusado entre 21h e 24h (o servidor usa UTC) — agora "hoje" vai vazio e o servidor usa o dele; rótulos do modal espremiam a data; aviso do antd já existente em Contas a Pagar (`Tag bordered`).

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (229 aprovados, 22 novos em `DevolucaoCalculoTests`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

## Fluxo de caixa (24/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script PowerShell com 28 verificações, e a **tela** com Playwright (Edge headless) contra essa API e um Vite temporário (porta 5174), com 17 verificações. Fixtures: venda de R$ 300,00 em 3x (uma recebida ontem às 22h30 de Brasília, uma atrasada, uma futura) e conta avulsa de R$ 100,00 em 2x (uma paga hoje); na tela, mais uma conta grande para o saldo ficar negativo. Dados de teste (`ZZT Fx…`, SKU `690000xx`) apagados via SQL ao final; dados reais idênticos antes e depois.

| Verificação | Resultado |
|---|---|
| Entradas e saídas realizadas de cada dia = soma direta no banco pelo dia de Brasília; recebimento às 22h30 conta no dia anterior (UTC já é o dia seguinte) | ✅ |
| Previstos = pendentes pelo vencimento; atrasada em hoje e no aviso; nenhum previsto antes de hoje; período passado só com realizado | ✅ |
| Saldo inicial = realizado antes do início; período futuro soma o previsto até a véspera; cadeia de saldos, saldo final e menor saldo | ✅ |
| Mensal soma igual ao diário; ano inteiro com 12 meses | ✅ |
| 6 validações de período (sem datas, fim antes do início, 94 dias no diário, 367 no mensal, agrupamento inválido) → 400; 93 dias → 200; Excel e PDF | ✅ |
| Tela: menu, mês atual com a linha de hoje, cards e menor saldo = API, alertas de saldo negativo e de atrasados, gráfico (legenda, hover, teclado, hoje), Mensal, Excel, celular, console limpo | ✅ |

Achados das capturas de tela, corrigidos: rótulos finais do eixo sobrepostos, card "Menor saldo" quebrando o valor negativo, gráfico ilegível no celular (agora rola de lado). As cores do gráfico foram validadas para daltonismo (azul × laranja; verde × vermelho reprovava).

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (246 aprovados, 15 novos em `FluxoCaixaCalculoTests`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

## NF-e simulada (28/09/2026)

Back-end testado ponta a ponta numa **instância temporária** da API (build Release, porta 5099), por um script PowerShell com 47 verificações, e a **tela** com Playwright (Edge headless) contra essa API e um Vite temporário (porta 5174), com 25 verificações. A empresa de teste usa a **série 999** e a linha real da empresa (se houver) é restaurada no fim. Dados de teste (`ZZT Nf…`, SKU `691000xx`) apagados via SQL; dados reais idênticos antes e depois.

| Verificação | Resultado |
|---|---|
| Pendências: sem empresa, sem endereço do cliente e sem NCM → 400 com as três, sem gravar nada; empresa com IBGE de outra UF → 400 | ✅ |
| Numeração 1-2-3 na série; total da nota = total do pedido; chave com 44 posições, cUF, CNPJ, modelo, série e **DV recalculado pelo script** | ✅ |
| Impostos conferidos à mão: SP 18% (CFOP 5102), RJ 12% com desconto no item, BA 7% com rateio do desconto do pedido (5,00 + 2,50); soma dos itens = cabeçalho | ✅ |
| XML: `Id` = NFe+chave, protNFe da mesma chave, homologação, destinatário contribuinte com IE, interestadual, `tPag`, ICMSTot; DANFE em PDF | ✅ |
| 409 ao emitir de novo, ao cancelar pedido com nota e em rascunho; 404; editar cliente/produto/empresa não muda a nota (md5 do XML e dos itens) | ✅ |
| Devolução: entrada nº 4, CFOP 2202, mesma alíquota (7%), valores da devolução, **nome e NCM de antes da edição do produto**, `finNFe=4`, `refNFe`, `tPag 90`; 409 para venda sem NF-e | ✅ |
| Lista: filtros por período, cliente, tipo e trecho da chave, paginação, 400 no período invertido, Excel e PDF | ✅ |
| Tela: Empresa (Zod, erro de IBGE da API no campo, normalização), janela de pendências, dados fiscais do cliente e NCM do produto pelas telas, emitir no pedido e na devolução, downloads `NFe{chave}.xml/.pdf`, lista, detalhe com a nota devolvida clicável, celular sem rolagem lateral, console limpo | ✅ |

Achados, corrigidos: `<Link>` dentro da janela de pendências derrubava a tela (o modal do antd fica fora do Router); IE com letra era aceita porque a máscara tirava tudo que não era dígito; DANFE com CEP sem máscara e bordas coladas; aviso do antd no `Descriptions`. Os DANFEs de venda e de devolução foram conferidos visualmente.

Verificações de build: `dotnet build -c Release` sem avisos, `dotnet test` (292 aprovados, 46 novos em `NfeCalculoTests`, `NfeXmlTests` e `DadosFiscaisValidacaoTests`), `tsc -b` sem erros, `oxlint` sem apontamentos, `npm run build` sem erros.

---

