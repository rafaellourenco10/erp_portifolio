# Funcionalidades por módulo

> Parte da documentação do [Ambition ERP](../README.md).

## Funcionalidades (etapa 1 — Clientes)

- Listagem em tabela com **paginação no servidor** (10, 20, 50 ou 100 por página)
- **Filtros**: botão **Filtrar** (com contador de filtros ativos) abre um painel com busca por nome (sem diferenciar maiúsculas/minúsculas), **Estado (UF) com seleção múltipla** em dropdown (busca sem acentos: "sao" encontra São Paulo) e **status** (Todos / Ativos / Inativos); os filtros aplicados aparecem como tags removíveis abaixo do cabeçalho
- **Inclusão e edição** em painel lateral, com validação no front (Zod) e na API (DataAnnotations)
- **Inativação** (exclusão lógica) com confirmação; o cliente continua na lista com a tag "Inativo" e pode ser **reativado** pela edição
- Validação de **CPF e CNPJ** pelos dígitos verificadores, incluindo o **CNPJ alfanumérico** (emitido pela Receita desde julho/2026)
- CPF/CNPJ aceito **com ou sem máscara**; é gravado sem máscara e exibido formatado
- **Documento único**: tentar cadastrar um CPF/CNPJ repetido retorna **409 Conflict**, e a tela mostra o erro no próprio campo
- Erros de validação da API aparecem campo a campo no formulário
- Layout responsivo: menu lateral recolhível (vira gaveta no celular), colunas secundárias ocultadas em telas menores e, no celular, cada cliente exibido como cartão
- Documentação interativa da API com **Swagger**

## Funcionalidades (etapa 2 — Produtos)

- **Navegação por rotas** (React Router): o menu lateral troca a tela por URL (`/clientes`, `/produtos`); a página aberta sobrevive ao recarregar e uma rota desconhecida cai no Painel (`/`)
- Cadastro com **nome**, **SKU** (número de série do produto: único e somente dígitos; o campo da tela nem deixa digitar letras), **categoria** (opcional, escolhida numa seleção com as categorias cadastradas), **unidade** (UN, KG, L, M ou CX), **preço de venda** e **custo** (obrigatórios, em reais, com 2 casas decimais)
- Listagem com paginação no servidor, **busca por nome ou SKU** e filtro de status e **categoria** (mesmo painel **Filtrar** e tags removíveis de Clientes)
- Coluna **Margem** calculada na tela: (preço − custo) ÷ preço; margem negativa aparece em vermelho
- **Inativação** (exclusão lógica) e reativação pela edição, como em Clientes
- **SKU único**: repetir um SKU retorna **409**; se o produto existente estiver **inativo**, a mensagem orienta a reativá-lo em vez de criar outro
- Um `PUT` sem o campo `ativo` **mantém** o status atual do produto
- O seletor de categoria do formulário tem um atalho **"+ Nova categoria"**: abre o cadastro de categoria por cima e já seleciona a categoria criada, sem sair da tela

## Funcionalidades (etapa 2.1 — Categorias)

- Tela **Categorias** no menu (Cadastro): cadastro só com o **nome**, busca por nome, filtro Todas / Ativas / Inativas, edição e inativação
- **Nome único sem diferenciar maiúsculas/minúsculas** ("Cabos" e "cabos" são a mesma categoria); se a existente estiver **inativa**, a mensagem orienta a reativá-la
- No cadastro de **Produtos** o campo Categoria é uma **seleção** (com busca) das categorias ativas; é opcional e pode ser limpo
- Inativar uma categoria **não mexe nos produtos**: eles continuam nela (a lista mostra o nome) e, ao editar, ela aparece como "(inativa)"; só deixa de ser oferecida em novos cadastros. Mover um produto para uma categoria inativa é recusado (400)
- O produto que tinha categoria em texto livre **foi convertido pela migration**: cada texto vira uma categoria (sem duplicar por maiúsculas/minúsculas) e o produto continua ligado a ela

---

## Funcionalidades (etapa 3 — Pedidos)

- Pedido de venda: **cliente** (seleção com busca no servidor), **itens** (produto por seleção com busca no servidor, quantidade, desconto %), **desconto no pedido todo**, **forma de pagamento** e **total calculado pelo servidor**
- Preço do produto é **copiado e congelado** no item ao adicionar; mudar o preço ou inativar o cliente/produto depois não altera pedidos já feitos
- Status **Rascunho → Confirmado** ou **Cancelado**; só o rascunho é editável; confirmado ainda pode ser cancelado; cancelado é definitivo (pedido nunca é excluído)
- **Confirmar pedido** salva o que estiver pendente na tela e então confirma; exige forma de pagamento e cliente/produtos ativos, senão erro 400 no campo certo
- **Cancelar pedido** é idempotente (cancelar duas vezes também retorna sucesso) e tem confirmação em janela
- O mesmo produto **não pode repetir** no pedido: adicioná-lo de novo soma a quantidade no item existente
- Quantidade decimal (até 3 casas) para produtos em `KG`, `L` e `M`; `UN` e `CX` só aceitam inteiro
- Limite de **1 a 100 itens** e total máximo de R$ 9.999.999.999,99 por pedido
- O total é recalculado **na hora** enquanto o pedido é montado (aritmética inteira no front, para não errar arredondamento); ao salvar, vale sempre o valor devolvido pelo servidor
- Lista com filtro por número ou nome do cliente e por status, no mesmo padrão **Filtrar** dos outros módulos
- No celular, cada item do pedido vira um **cartão** (quantidade, desconto, preço e subtotal juntos); em tela larga é uma tabela

---

## Funcionalidades (etapa 4 — Estoque)

- Saldo de cada produto é sempre **calculado** a partir de um histórico de **movimentações** (entrada/saída), nunca gravado direto
- Tela **Estoque** (menu Depósito): tabela com produto, SKU, unidade e **saldo**, busca por nome ou SKU
- **Nova entrada** (compra/ajuste): produto (seleção com busca no servidor), quantidade e motivo em texto livre, opcional
- **Ver movimentações**: extrato paginado de um produto, mais recente primeiro, com tipo, quantidade, motivo/origem e data
- **Confirmar um pedido baixa o estoque** automaticamente: uma saída por item, com a quantidade do item
- Confirmar um pedido **sem saldo suficiente é recusado** (400 no campo, como cliente/produto inativo); nenhuma movimentação é gravada, nem a dos itens que tinham saldo
- **Cancelar um pedido que estava Confirmado devolve o estoque** automaticamente (uma entrada de estorno por item); cancelar um rascunho que nunca foi confirmado não mexe em estoque
- Saldo negativo (não deveria acontecer, dado o bloqueio acima) aparece em vermelho na lista, mesmo padrão da margem negativa em Produtos
- Fora do escopo por enquanto: fornecedores, pedido de compra, saída manual (perda/ajuste), múltiplos depósitos, estoque mínimo/alerta

---

## Funcionalidades (etapa 5 — Contas a Receber)

- **Confirmar um pedido gera as parcelas a receber** automaticamente: o número de parcelas (1 a 12) e o intervalo em dias entre vencimentos são escolhidos na hora de confirmar, num modal próprio
- Valor dividido **igualmente entre as parcelas**, com o resto (se a divisão não for exata) na última — a soma sempre bate com o total do pedido
- Vencimento da parcela `N` = data da confirmação + `N × intervalo` dias (a primeira nunca vence no mesmo dia)
- Tela **Contas a Receber** (menu Financeiro): tabela com cliente, pedido, parcela (`X/Y`), valor, vencimento e status; busca por cliente ou nº do pedido; filtro por status
- Status: **Pendente**, **Recebido** (ação "Marcar como recebido", idempotente) ou **Cancelado**; **Atrasado** não é um status gravado — é uma parcela Pendente com vencimento no passado, calculado no servidor
- **Cancelar um pedido que estava Confirmado cancela as parcelas ainda Pendentes** automaticamente; parcelas já Recebidas continuam como estão (histórico preservado)
- Fora do escopo por enquanto: recebimento parcial, juros/multa por atraso, edição de parcela já gerada, contas a pagar

---

## Funcionalidades (etapa 6 — Painel/Dashboard)

- Tela inicial (`/`, item **Dashboard** no topo do menu) com indicadores do **mês atual** — sem seletor de período nesta versão
- **Faturamento e ticket médio**: soma do `valorTotal` só dos pedidos **Confirmados** no mês, e a média por pedido
- **Pedidos por status** no mês: Rascunho / Confirmado / Cancelado
- **Contas a receber**: total pendente e, dentro dele, quanto está atrasado (sem filtro de mês — parcelas vencem em datas futuras variadas)
- **Saldo baixo de estoque**: quantidade de produtos ativos com saldo ≤ 5 (limite fixo por enquanto; não existe estoque mínimo por produto ainda)
- **Gráfico de faturamento diário** do mês, com todos os dias presentes (dias sem venda aparecem com R$ 0, sem buraco no gráfico); tooltip por barra, acessível por mouse e teclado
- Cada card busca seu próprio indicador; se um endpoint falhar, os outros continuam aparecendo normalmente
- **Vencimentos** (23/09/2026): dois quadros, **A pagar** e **A receber**, com as parcelas pendentes **atrasadas** e as que **vencem nos próximos 7 dias**, mais urgentes primeiro — quem, de onde (compra/pedido/descrição e parcela X/Y), valor e etiqueta "Atrasada há N dias" (vermelha) ou "Vence hoje / em N dias" (amarela); total, quantidade de atrasadas e link "Ver todas". Mostra até 10 linhas por quadro (o total conta todas)
- Só leitura — nenhuma ação a partir do Dashboard
- Fora do escopo por enquanto: seletor de período, estoque mínimo por produto, drill-down/exportação

---

## Funcionalidades (etapa 7 — Fornecedores e Pedidos de Compra)

- **Fornecedor**: mesmo formato do Cliente (nome, CPF/CNPJ, e-mail/telefone opcionais, cidade, UF, ativo); documento **único só entre fornecedores** (um CNPJ já usado por um cliente pode ser cadastrado como fornecedor)
- **Pedido de compra**: fornecedor (seleção com busca no servidor), itens (produto, quantidade, desconto %), desconto no pedido todo e total calculado pelo servidor — reaproveita a mesma fórmula, status e transições do Pedido de Venda; sem forma de pagamento (não há Contas a Pagar ainda)
- Preço de cada item nasce do **Custo** do produto (não do preço de venda) e fica congelado, do mesmo jeito que o Pedido de Venda congela o preço de venda
- **Confirmar** dá **entrada automática no estoque** por item, ligada ao pedido de compra, e **atualiza o Custo do produto** para o preço pago em cada item — mesmo que o custo do produto tenha mudado entre montar o rascunho e confirmar
- **Cancelar um pedido confirmado** exige saldo suficiente em cada item (o que já foi vendido não pode ser estornado): sem saldo, a API recusa com 400 e nada é alterado; com saldo, gera uma saída de estorno por item
- Cancelar é **idempotente** (cancelar duas vezes também retorna sucesso), mesmo padrão do Pedido de Venda
- O extrato de estoque (painel "Movimentações" de um produto) ganhou uma coluna **Origem** (Venda #N / Compra #N / Manual)
- Fora do escopo por enquanto: forma de pagamento, recebimento parcial de mercadoria (o pedido é recebido inteiro ao confirmar)

---

## Funcionalidades (etapa 8 — Contas a Pagar)

- Espelho do Contas a Receber, do lado da compra: **confirmar um pedido de compra gera as parcelas a pagar** ao fornecedor. O modal de confirmar é o mesmo da venda (número de parcelas 1 a 12, intervalo 1 a 180 dias, padrão 1 parcela em 30 dias) — agora um componente só, `ModalParcelas`, usado pelas duas telas
- Mesma divisão do Contas a Receber: valor dividido igualmente, resto na última parcela; vencimento da parcela `N` = data da confirmação + `N × intervalo` dias
- Tela **Contas a Pagar** (menu Financeiro): fornecedor, compra, parcela (`X/Y`), valor, vencimento e status; busca por fornecedor ou nº da compra; filtro Pendentes/Atrasadas/Pagas/Canceladas
- Status: **Pendente**, **Pago** (ação "Marcar como paga", idempotente) ou **Cancelado**; **Atrasado** é calculado no servidor (Pendente com vencimento no passado), não gravado
- **Cancelar uma compra confirmada cancela as parcelas ainda Pendentes**; as já Pagas não mudam. Se o cancelamento for bloqueado por falta de saldo de estoque, nenhuma parcela muda (mesma transação)
- **Dashboard** ganhou o card **Contas a pagar** (total pendente e atrasado); os cards passaram a ocupar duas linhas: financeiro (Faturamento, A receber, A pagar) e operação (Pedidos por status, Saldo baixo)
- Compras confirmadas **antes** deste módulo não ganharam parcelas (mesma situação das vendas antigas na etapa 5)
- Fora do escopo por enquanto: pagamento parcial, editar parcela, contas a pagar avulsas (sem pedido de compra, ex.: aluguel), estorno de parcela já paga

---

## Funcionalidades (etapa 9 — Relatórios)

- Seção **Relatórios** no menu com três telas: **Vendas**, **Compras** e **Estoque**. Cada uma tem filtros, botão **Gerar**, cards de resumo, tabela e botões **Excel** e **PDF**
- **Vendas / Compras** (mesma tela, muda o cliente/fornecedor): período obrigatório (padrão: do dia 1 do mês até hoje; máximo 366 dias), status (padrão Confirmado, ou Todos) e cliente/fornecedor opcional. Um pedido por linha (nº, data, nome, itens, total, status); resumo com quantidade, valor total e ticket médio
- **Estoque**: posição atual dos produtos ativos — saldo, estoque mínimo, custo, **valor em estoque** (saldo × custo) e situação (abaixo do mínimo = saldo ≤ mínimo, mesma regra do Dashboard); filtros por categoria e "só abaixo do mínimo"
- Os arquivos são **gerados pelo servidor** a partir da mesma consulta da tela, então os números são sempre iguais: **Excel** (.xlsx, ClosedXML) com valores de verdade (moeda, data e número somam/ordenam no Excel), cabeçalho destacado e filtro automático; **PDF** (QuestPDF) em A4, paisagem quando a tabela é larga, com filtros, cards de resumo e "Página X de Y"
- Nome do arquivo: `relatorio-vendas-AAAA-MM-DD_AAAA-MM-DD.xlsx` / `.pdf` (estoque: `relatorio-estoque-AAAA-MM-DD`)
- Datas exibidas no horário de Brasília; o filtro de período também usa os dias de Brasília (mesma convenção do Dashboard)
- Fora do escopo por enquanto: seletor de período no Dashboard, relatórios de contas a receber/pagar, relatório agrupado por produto, itens dos pedidos dentro do relatório

---

## Funcionalidades (etapa 10 — Vendedores)

- Tela **Vendedores** (menu Cadastro): nome, **CPF** (só pessoa física — CNPJ é recusado), e-mail e telefone opcionais, **% de comissão padrão** (0 a 100, até 2 casas) e ativo/inativo; mesmo padrão de filtros, tabela e painel de Fornecedores
- CPF único **entre vendedores**; repetido com um vendedor inativo sugere reativar o cadastro
- **Pedido de Venda** ganhou o campo **Vendedor** (seleção com busca, só ativos): opcional no rascunho, **obrigatório e ativo para confirmar** (igual à forma de pagamento)
- Ao confirmar, o pedido **guarda a % de comissão do vendedor naquele momento**; mudar a % do vendedor depois não altera vendas já confirmadas. O pedido confirmado mostra o vendedor e a % congelada
- Pedidos confirmados antes deste módulo continuam sem vendedor
- Fora do escopo por enquanto: vendedor no pedido de compra, coluna/filtro de vendedor na lista de pedidos e nos relatórios (o cálculo da comissão veio na etapa 11)

---

## Funcionalidades (etapa 11 — Comissões)

- **A comissão nasce quando o cliente paga**: marcar uma parcela como recebida em Contas a Receber gera a comissão dela, se o pedido tiver vendedor — `valor da parcela × % congelada no pedido`, arredondado a 2 casas (meio para cima). A % é a do pedido (etapa 10), não a atual do vendedor
- Uma comissão por parcela: receber de novo não duplica; pedido sem vendedor (anterior à etapa 10) ou com 0% não gera nada
- Parcelas que já estavam recebidas antes deste módulo, de pedidos com vendedor, ganharam a comissão na migration
- Tela **Comissões** (menu Financeiro): filtros por vendedor, período (data do recebimento) e status; cards **Gerado / A pagar / Pago** (somas do vendedor/período, calculadas no servidor); tabela com pedido e parcela, cliente, valor recebido, %, comissão, datas e status
- **Pagar ao vendedor**: na etapa 11 era por linha ou em lote ("Marcar como pagas"); **desde a etapa 12 é pelo Contas a Pagar** (ver abaixo)
- Cancelar um pedido não mexe nas comissões (as parcelas já recebidas continuam recebidas)
- Fora do escopo por enquanto: estorno de comissão, exportação da tela para Excel/PDF, metas/faixas de comissão (a conta a pagar da comissão veio na etapa 12)

---

## Funcionalidades (etapa 12 — Comissão vira conta a pagar + contas avulsas)

- O **Contas a Pagar** passa a ter três **origens**: **Compra** (como antes), **Comissão** e **Avulsa**. A lista mostra o **favorecido** (fornecedor, vendedor ou o nome digitado) e a **descrição** (Compra #N, "Comissões — Vendedor (N)" ou o texto), com filtro por origem e busca por favorecido/descrição
- **Nova conta** (avulsa): descrição, favorecido opcional, valor, 1º vencimento, 1 a 12 parcelas e intervalo em dias — para aluguel, luz, salários etc.
- **Cancelar** uma parcela pendente avulsa ou de comissão (a de compra continua sendo cancelada pelo pedido de compra; parcela paga não cancela)
- **Comissões → conta a pagar**: em Comissões, selecione as comissões **A pagar de um vendedor** e clique **Gerar conta a pagar** (escolhendo o vencimento): nasce **uma** conta com a soma, e as comissões ficam **Em pagamento**
- Pagar essa conta em Contas a Pagar deixa as comissões **Pagas** (mesma data); cancelar a conta devolve as comissões para **A pagar**, para gerar de novo
- O "Marcar como paga" direto em Comissões saiu: todo pagamento de comissão passa pelo Contas a Pagar. Comissões já pagas antes continuam pagas (sem conta ligada)
- Fora do escopo por enquanto: contas recorrentes automáticas, categorias/plano de contas, editar conta lançada (cancele e lance de novo), anexos, juros e pagamento parcial

## Funcionalidades (etapa 13 — Orçamentos)

- Tela **Orçamentos** no menu (Ordem Vendas/Compras, antes de Pedidos de Venda): a proposta que vai para o cliente **antes** da venda, com a mesma montagem do pedido (cliente cadastrado, vendedor e forma de pagamento opcionais, itens com desconto, desconto geral) mais **validade** (sugerida hoje + 15 dias) e **observações** (até 500 caracteres, saem no PDF)
- O preço de cada item é copiado do produto ao adicionar e fica **congelado** no orçamento
- Situações: **Aberto**, **Aprovado** (automático ao gerar o pedido), **Perdido** (marcado à mão, com motivo opcional) e **Vencido** — calculado, não gravado: é o aberto com a validade já passada. Filtro por situação (inclui Vencido) e busca por número ou cliente
- Só o aberto é editável; salvar um **vencido** com uma validade nova o **prorroga**
- **Gerar pedido**: cria o **pedido de venda em rascunho** com os **preços e descontos do orçamento** (mesmo que o produto tenha mudado de preço), cliente, vendedor e forma de pagamento; o orçamento vira Aprovado e mostra o link do pedido. A confirmação do pedido segue o fluxo normal (estoque, parcelas, vendedor). Vencido não gera (prorrogue antes); cliente ou produto inativo → erro; vendedor inativo fica em branco no pedido
- **Baixar PDF** (qualquer situação): A4 com número, data, validade, dados do cliente (CPF/CNPJ formatado, contato, cidade/UF), vendedor, itens com SKU, subtotal, desconto, total, forma de pagamento, observações e "válido até" no rodapé
- Fora do escopo por enquanto: reabrir ou duplicar orçamento, orçamento para quem não é cliente, envio por e-mail, orçamentos no Dashboard/Relatórios (taxa de conversão), desfazer a aprovação se o pedido for cancelado

## Funcionalidades (etapa 14 — Devolução de venda)

- No **pedido confirmado**, o botão **Registrar devolução** abre um modal com os itens ainda não devolvidos: quantidade a devolver (até o disponível; inteira em UN/CX), **volta ao estoque** (marcado por padrão; desmarque se for perda, ex.: defeito), motivo e vencimento do reembolso, com a **prévia do valor**
- Devolução **parcial ou total**, em quantas vezes quiser até devolver tudo. O valor de cada item leva os descontos do item e do pedido; na devolução que zera o pedido vale **o que falta**, para os centavos fecharem com o total
- **Dinheiro:** o valor devolvido abate primeiro as **parcelas pendentes**, da última para a primeira (parcela zerada fica cancelada); o que o cliente já tinha pago vira uma conta a pagar **"Reembolso"** (origem Devolução) em Contas a Pagar, que não pode ser cancelada
- **Comissão:** as parcelas reduzidas já geram comissão menor quando forem recebidas; sobre o valor reembolsado nasce um **estorno** (comissão negativa), descontado automaticamente no próximo "Gerar conta a pagar" do vendedor (se os estornos superarem as comissões, a conta não é gerada)
- Tudo acontece numa **transação só**; a devolução é **definitiva** e o pedido com devolução **não pode mais ser cancelado** (devolva o restante)
- O pedido mostra o **histórico de devoluções** (itens, perda, motivo, valor, abatido, reembolso, estorno); o **Dashboard** ganhou o card **Devoluções do mês** ao lado do faturamento, que continua bruto
- Fora do escopo por enquanto: desfazer/editar devolução, crédito do cliente, troca num passo só, devolução de compra ao fornecedor, faturamento líquido nos relatórios

## Funcionalidades (etapa 15 — Fluxo de caixa)

- Tela **Financeiro → Fluxo de Caixa**: entradas (parcelas a receber) e saídas (parcelas a pagar de todas as origens: compra, comissão, avulsa, reembolso)
- **Realizado** pela data em que a parcela foi recebida/paga; **previsto** pelo vencimento das pendentes, de hoje em diante. Pendente já vencida (atrasada) conta como prevista **hoje**, com um aviso "Em atraso"
- **Saldo acumulado desde o início do ERP**: o saldo inicial do período é tudo o que entrou menos tudo o que saiu antes dele (num período futuro, soma também o previsto até lá); não há saldo para cadastrar
- Visão **Diária** (abre no mês atual; até 93 dias) ou **Mensal** (abre no ano atual; até 366 dias), com cards de saldo inicial, entradas, saídas, saldo final e **menor saldo** (com a data); alerta quando o saldo fica negativo
- **Gráfico** em dois painéis com o mesmo eixo: a linha do saldo e as colunas de entradas (azul, para cima) e saídas (laranja, para baixo), com o previsto mais claro e a marca de hoje; **tabela** com realizado × previsto, resultado e saldo; exportação **Excel/PDF**
- Dias em horário de Brasília (recebimento às 22h conta no mesmo dia); só leitura, sem tabela nova
- Fora do escopo por enquanto: contas bancárias/caixas e saldo inicial digitado, categorias de despesa, abrir os lançamentos de um dia, card no Dashboard, cenários

## Funcionalidades (etapa 16 — NF-e simulada)

> A nota é **de demonstração**: XML em ambiente de homologação (`tpAmb=2`), sem assinatura digital e sem envio à SEFAZ; o DANFE sai com a marca **"SEM VALOR FISCAL"**.

- **Fiscal → Empresa**: dados do emitente (razão social, CNPJ — inclusive o alfanumérico —, Inscrição Estadual, endereço, código IBGE do município conferido com a UF, série da NF-e)
- **Dados fiscais** no cadastro: endereço completo, CEP, código IBGE e Inscrição Estadual (ou ISENTO) no **cliente**; **NCM** no **produto**. Opcionais no cadastro, exigidos só na hora de emitir
- **Emitir NF-e** no pedido de venda confirmado (uma por pedido). Faltando dado, a tela lista tudo de uma vez (empresa, endereço do cliente, NCM de cada produto)
- **Impostos por item (regime normal)**: ICMS CST 00 pela alíquota interna da UF do emitente (tabela das 27 UFs) ou interestadual de **12%** / **7%** (Sul/Sudeste, menos ES, para as demais UFs); PIS 1,65% e COFINS 7,6%. O **desconto do pedido é rateado** entre os itens (sobra de centavos no último) e a nota fecha com o total do pedido
- **CFOP** 5102/6102 na venda e 1202/2202 na devolução; destinatário contribuinte (CNPJ com IE) ou não contribuinte
- **Chave de acesso** de 44 posições com dígito verificador módulo 11 (conferido com o exemplo do manual e com CNPJ alfanumérico), numeração sequencial por série e **protocolo de autorização simulado**
- **XML** `nfeProc` no layout 4.00 (ide, emit, dest, det, total, transp, pag, infAdic + protNFe) e **DANFE** em PDF gerado **a partir do XML gravado**
- **NF-e de devolução** (entrada, finalidade 4) pela devolução de venda, referenciando a chave da nota original, com os produtos e as alíquotas da nota original e os valores da devolução
- A nota é **imutável**: editar cliente, produto ou empresa depois não muda nada nela. Pedido com NF-e **não pode ser cancelado** (não há cancelamento de nota); só devolvido
- **Fiscal → Notas Fiscais**: lista com filtros (período, cliente, tipo, número ou trecho da chave), detalhe com itens e impostos, navegação para a nota devolvida, XML, DANFE e exportação Excel/PDF
- Fora do escopo por enquanto: envio real à SEFAZ e certificado digital, cancelamento e carta de correção, inutilização, NFC-e, IPI/ICMS-ST/DIFAL/FCP, NF-e de compra, busca de endereço pelo CEP, código de barras da chave no DANFE, Simples Nacional

