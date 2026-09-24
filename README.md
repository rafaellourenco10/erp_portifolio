# Ambition ERP

ERP comercial desenvolvido como projeto de portfólio, com back-end em **ASP.NET Core** e front-end em **React**, em tema escuro próprio.
O projeto é evoluído por módulos: **Clientes** (etapa 1), **Produtos e Categorias** (etapa 2), **Pedidos de Venda** (etapa 3), que liga cliente e produtos numa venda com itens, desconto e total calculado, **Estoque** (etapa 4), que baixa e devolve saldo automaticamente a partir dos pedidos, **Contas a Receber** (etapa 5), que gera e controla as parcelas de cada venda confirmada, **Fornecedores e Pedidos de Compra** (etapa 7), que fecha o lado "compra" do estoque: confirmar um pedido de compra dá entrada automática e atualiza o custo dos produtos, **Contas a Pagar** (etapa 8), que gera e controla as parcelas de cada compra confirmada, **Relatórios** (etapa 9) de vendas, compras e estoque, com exportação para Excel e PDF, **Vendedores** (etapa 10), com o vendedor e a % de comissão congelada em cada venda confirmada, **Comissões** (etapa 11), geradas quando o cliente paga cada parcela, a **integração Comissões → Contas a Pagar com contas avulsas** (etapa 12): fechar as comissões de um vendedor gera uma conta a pagar, e o Contas a Pagar passa a aceitar também despesas como aluguel e luz, **Orçamentos** (etapa 13): a proposta ao cliente, com validade e PDF, que vira pedido de venda com um clique, e **Devolução de venda** (etapa 14): parcial ou total, que devolve ao estoque, abate as parcelas pendentes, gera o reembolso do que já foi pago e estorna a comissão do vendedor. O **Dashboard** (etapa 6) resume os outros módulos.

O menu lateral agrupa as telas por departamento: **Dashboard** no topo; **Cadastro** (Clientes, Fornecedores, Vendedores, Produtos, Categorias); **Ordem Vendas/Compras** (Orçamentos, Pedidos de Venda, Pedidos de Compra); **Depósito** (Estoque); **Financeiro** (Contas a Receber, Contas a Pagar, Comissões); **Relatórios** (Vendas, Compras, Estoque).

| Etapa | Módulo | Situação |
|---|---|---|
| 1 | Clientes | Concluída e testada de ponta a ponta (18/09/2026) |
| 1.1 | Tema visual Ambition ERP + filtros por UF e status | Concluída e testada (18/09/2026) |
| 2 | Produtos (+ navegação por rotas no menu) | Concluída e testada (21/09/2026) |
| 2.1 | Categorias (cadastro próprio, escolhido por seleção no produto) | Concluída e testada (21/09/2026) |
| 2.2 | Filtro por categoria em Produtos + atalho "Nova categoria" no seletor | Concluída (22/09/2026) |
| 3 | Pedidos (cliente, itens, desconto, total calculado, confirmar/cancelar) | Concluída e testada (21/09/2026) |
| 4 | Estoque (movimentações, entrada manual, baixa/estorno automáticos) | Back-end testado de ponta a ponta; tela confirmada pelo Rafael (22/09/2026) |
| 5 | Contas a Receber (parcelas, vencimento, status de recebimento) | Back-end testado de ponta a ponta; tela não verificada visualmente (22/09/2026) |
| 6 | Dashboard (indicadores do mês) | Back-end testado com dados reais; tela não verificada visualmente (22/09/2026) |
| 7 | Fornecedores e Pedidos de Compra (entrada automática de estoque, custo atualizado) | Back-end testado de ponta a ponta; tela não verificada visualmente (23/09/2026) |
| 8 | Contas a Pagar (parcelas da compra, vencimento, status de pagamento, card no Dashboard) | Back-end testado de ponta a ponta; telas confirmadas pelo Rafael (23/09/2026) |
| 9 | Relatórios (vendas, compras, estoque) com exportação Excel/PDF | Back-end testado de ponta a ponta e PDFs conferidos; telas confirmadas pelo Rafael (23/09/2026) |
| 10 | Vendedores (cadastro + vendedor no pedido de venda, % de comissão congelada) | Back-end testado de ponta a ponta; telas confirmadas pelo Rafael (23/09/2026) |
| 11 | Comissões (geradas no recebimento da parcela, pagamento ao vendedor) | Back-end testado de ponta a ponta; tela confirmada pelo Rafael (23/09/2026) |
| 12 | Comissão vira conta a pagar + contas avulsas (origem Compra/Comissão/Avulsa, cancelar) | Back-end testado de ponta a ponta; telas não verificadas visualmente (23/09/2026) |
| 13 | Orçamentos (validade, situação Vencido calculada, gerar pedido com os preços do orçamento, marcar como perdido, PDF) | Back-end testado de ponta a ponta; tela testada com Playwright (23/09/2026) |
| 14 | Devolução de venda (parcial, estoque por item, abatimento nas parcelas, reembolso em Contas a Pagar, estorno de comissão, card no Dashboard) | Back-end testado de ponta a ponta; tela testada com Playwright (24/09/2026) |
| 15+ | Login | Planejada |

> Os nomes técnicos (solution `ErpPortfolio`, projeto `ErpPortfolio.Api`, banco `erp_portfolio_db`) foram mantidos; "Ambition ERP" é o nome do produto exibido na interface e no Swagger.

---

## Sumário

1. [Funcionalidades](#funcionalidades-etapa-1--clientes)
2. [Stack e versões](#stack-e-versões)
3. [Tema visual](#tema-visual)
4. [Estrutura de pastas](#estrutura-de-pastas)
5. [Como rodar (passo a passo)](#como-rodar-passo-a-passo)
6. [API](#api)
7. [Regras de validação](#regras-de-validação)
8. [Banco de dados](#banco-de-dados)
9. [Dados de teste](#dados-de-teste)
10. [Decisões técnicas](#decisões-técnicas)
11. [Testes realizados](#testes-realizados)
12. [Padrões do projeto](#padrões-do-projeto)
13. [Solução de problemas](#solução-de-problemas)
14. [Próximas etapas](#próximas-etapas)

---

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
- Datas exibidas no horário de Brasília; o filtro de período compara em UTC (mesma convenção do Dashboard)
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

## Stack e versões

| Camada | Tecnologia | Versão |
|---|---|---|
| Runtime / SDK | .NET SDK | 10.0 |
| Back-end | ASP.NET Core Web API (Controllers) | net10.0 |
| ORM | Entity Framework Core + Npgsql | EF Core 10.0.12 / Npgsql EF 10.0.3 |
| Ferramenta de migrations | dotnet-ef (ferramenta local) | 10.0.12 |
| Swagger | Swashbuckle.AspNetCore | 10.2.3 |
| Excel dos relatórios | ClosedXML (licença MIT) | 0.105.1 |
| PDF dos relatórios | QuestPDF (licença Community — gratuita para uso individual e empresas com faturamento < US$ 1 mi; declarada no `Program.cs`) | 2026.9.0 |
| Banco | PostgreSQL (Docker, imagem `postgres:18-alpine`) | 18 |
| Front-end | React + TypeScript | React 19 / TS 6 |
| Build do front | Vite | 8 |
| Componentes | Ant Design + @ant-design/icons | 6 |
| Chamadas à API | TanStack Query + Axios | 5 / 1 |
| Formulários | React Hook Form + Zod + @hookform/resolvers | 7 / 4 / 5 |
| Datas (seletor de período) | dayjs (já vinha com o Ant Design; declarado no `package.json` na etapa 9) | 1.11 |
| Fonte | Inter (auto-hospedada via @fontsource-variable/inter) | 5 |
| Lint do front | oxlint | 1 |

Ambiente usado no desenvolvimento: Windows 11, Node.js 24, Docker Desktop com Docker Compose.

---

## Tema visual

O visual segue o design system **Ambition ERP**, criado no Google Stitch. Os arquivos originais ficam em [`docs/tema/`](docs/tema/):

| Pasta | Conteúdo |
|---|---|
| `stitch_erp_comercial_web_dark/` | `DESIGN.md` (guia de cores, tipografia, espaçamentos e componentes) e o logo em SVG |
| `stitch_erp_comercial_web_dark (1)/` | Mockup do dashboard de vendas (`code.html`) |
| `stitch_erp_comercial_web_dark (2)/` | Mockup da tela de Clientes (`code.html`) |
| `stitch_erp_comercial_web_dark (3)/` | Mockup do formulário "Novo cliente" (`code.html` e `screen.png`) |

> As imagens `screen.png` das pastas (1) e (2) vieram corrompidas na exportação do Stitch; as telas completas estão nos `code.html` (abra no navegador).

Principais definições:

| Elemento | Valor |
|---|---|
| Fundo da aplicação | `#1B1E21` |
| Cards, tabelas, menu e painéis | `#272B30`, borda `#3B4046` |
| Hover / elevação | `#30353A` |
| Campos de entrada | `#1F2225` (mais escuros que o card, para parecerem "afundados") |
| Texto principal / secundário | `#E6E8EA` / `#A3A9AF` |
| Destaque (marca, ações, ativo) | Verde `#22C55E` (hover `#16A34A`) |
| Alerta / erro / informativo | `#F59E0B` / `#EF4444` / `#38BDF8` |
| Fonte | Inter, com algarismos tabulares em CPF/CNPJ, telefones e datas |
| Cantos | 8px (botões e campos), 12px (cards e painéis) |

Onde o tema está no código:
- [`src/tema/temaAmbition.ts`](frontend/erp-portfolio-web/src/tema/temaAmbition.ts): **fonte única das cores**. Alimenta os tokens do Ant Design (`ConfigProvider`) e publica as variáveis CSS `--cor-*` usadas nos arquivos `.css`. Para mudar uma cor, altere só aqui.
- [`src/components/LogoAmbition.tsx`](frontend/erp-portfolio-web/src/components/LogoAmbition.tsx): logo (completo ou só o ícone, com o menu recolhido).
- [`src/components/TagStatus.tsx`](frontend/erp-portfolio-web/src/components/TagStatus.tsx): tag Ativo/Inativo no padrão do `DESIGN.md`.

Os mockups também mostram itens que dependem de dados que o sistema ainda não tem. Eles **não** foram implementados, para não exibir informação falsa:
- Indicadores (ticket médio, inadimplência)
- Usuário logado e notificações
- Integrações SEFAZ/WhatsApp e consulta à Receita
- Filtro por cidades
- Campos PF/PJ, Inscrição Estadual, Nome Fantasia e Observações
- Menus de módulos futuros

---

## Estrutura de pastas

```
erp_portifolio/
├── ErpPortfolio.slnx               # solution (.NET 10 gera o formato .slnx no lugar do .sln)
├── dotnet-tools.json               # manifesto da ferramenta local dotnet-ef
├── docker-compose.yml              # PostgreSQL de desenvolvimento (porta 5433)
├── .env.example                    # modelo do .env (senha do banco)
├── .gitignore                      # regras para .NET e Node
├── README.md
├── docs/
│   └── tema/                       # design system e mockups do Stitch (ver "Tema visual")
│
├── backend/
│   └── ErpPortfolio.Api/
│       ├── Controllers/
│       │   ├── ClientesController.cs        # endpoints REST de clientes
│       │   ├── ProdutosController.cs        # endpoints REST de produtos
│       │   ├── CategoriasController.cs      # endpoints REST de categorias
│       │   ├── PedidosController.cs         # endpoints REST de pedidos
│       │   ├── OrcamentosController.cs      # orçamentos: CRUD do aberto, gerar pedido, perder, PDF
│       │   ├── DevolucoesController.cs      # /pedidos/{id}/devolucoes: registrar e histórico
│       │   ├── EstoqueController.cs         # endpoints REST de estoque
│       │   ├── ContasReceberController.cs   # endpoints REST de contas a receber
│       │   ├── DashboardController.cs       # endpoints REST do Dashboard (so delegam)
│       │   ├── FornecedoresController.cs    # endpoints REST de fornecedores
│       │   ├── VendedoresController.cs      # endpoints REST de vendedores
│       │   ├── ComissoesController.cs       # listar comissões com totais e pagar em lote
│       │   ├── PedidosCompraController.cs   # endpoints REST de pedidos de compra
│       │   ├── ContasPagarController.cs     # endpoints REST de contas a pagar
│       │   └── RelatoriosController.cs      # relatórios: JSON para a tela ou arquivo (?formato=xlsx|pdf)
│       ├── Models/
│       │   ├── Cliente.cs                   # entidade
│       │   ├── Produto.cs                   # entidade (CategoriaId + navegação)
│       │   ├── Categoria.cs                 # entidade
│       │   ├── Pedido.cs / PedidoItem.cs    # entidades (itens ligados por FK, preco congelado)
│       │   ├── StatusPedido.cs              # enum: Rascunho, Confirmado, Cancelado (reaproveitado por PedidoCompra)
│       │   ├── FormaPagamento.cs            # enum: Dinheiro, Pix, Boleto, Cartao
│       │   ├── EstoqueMovimentacao.cs       # entidade (FK produto, pedido e pedido de compra, opcionais)
│       │   ├── TipoMovimentacao.cs          # enum: Entrada, Saida
│       │   ├── ParcelaReceber.cs            # entidade (FK pedido, vencimento como DateOnly)
│       │   ├── StatusParcela.cs             # enum: Pendente, Recebido, Cancelado
│       │   ├── ParcelaPagar.cs              # entidade (FK pedido de compra, vencimento como DateOnly)
│       │   ├── StatusParcelaPagar.cs        # enum: Pendente, Pago, Cancelado
│       │   ├── Fornecedor.cs                # entidade (espelho de Cliente)
│       │   ├── Vendedor.cs                  # entidade (CPF, % de comissão padrão)
│       │   ├── Comissao.cs                  # entidade (uma por parcela recebida) + enum StatusComissao
│       │   └── PedidoCompra.cs / PedidoCompraItem.cs  # entidades (espelho de Pedido/PedidoItem, sem forma de pagamento)
│       ├── DTOs/
│       │   ├── ClienteCriacaoDto.cs         # entrada do POST
│       │   ├── ClienteAtualizacaoDto.cs     # entrada do PUT (+ campo ativo opcional)
│       │   ├── ClienteRespostaDto.cs        # saída
│       │   ├── ClienteFiltroDto.cs          # query string da listagem
│       │   ├── Produto{Criacao,Atualizacao,Resposta,Filtro}Dto.cs   # mesmo desenho, para produtos
│       │   ├── Categoria{Criacao,Atualizacao,Resposta,Filtro}Dto.cs # mesmo desenho, para categorias
│       │   ├── PedidoCriacaoDto.cs          # entrada do POST/PUT (itens sem preço)
│       │   ├── PedidoItemEntradaDto.cs      # item do POST/PUT: produtoId, quantidade, desconto
│       │   ├── PedidoRespostaDto.cs         # saída com itens (preço, subtotal)
│       │   ├── PedidoResumoDto.cs           # linha da listagem (sem itens)
│       │   ├── PedidoFiltroDto.cs           # query string da listagem
│       │   ├── ResultadoPaginadoDto.cs      # envelope genérico de paginação
│       │   ├── EstoqueEntradaDto.cs         # entrada do POST /estoque/entradas
│       │   ├── EstoqueResumoDto.cs          # linha da listagem (produto + saldo)
│       │   ├── EstoqueFiltroDto.cs          # query string da listagem/extrato
│       │   ├── MovimentacaoRespostaDto.cs   # linha do extrato de um produto
│       │   ├── PedidoConfirmarDto.cs        # entrada do PATCH /confirmar: numeroParcelas, intervaloDias
│       │   ├── ParcelaRespostaDto.cs        # linha da listagem de contas a receber
│       │   ├── ParcelaFiltroDto.cs          # query string da listagem
│       │   ├── FiltroStatusParcela.cs       # enum do filtro (inclui "Atrasado", calculado)
│       │   ├── VendasResumoDto.cs           # saída de /dashboard/vendas
│       │   ├── FaturamentoDiaDto.cs         # ponto do gráfico (dia, valor)
│       │   ├── PedidosPorStatusDto.cs       # contagem por status no mês
│       │   ├── ContasReceberResumoDto.cs    # saída de /dashboard/contas-receber
│       │   ├── ParcelaPagar{Resposta,Filtro}Dto.cs, FiltroStatusParcelaPagar.cs  # mesmo desenho, para contas a pagar
│       │   ├── ContasPagarResumoDto.cs      # saída de /dashboard/contas-pagar
│       │   ├── RelatorioFiltroDtos.cs       # filtros dos relatórios (período validado: R1)
│       │   ├── RelatorioRespostaDtos.cs     # linhas + resumo dos 3 relatórios
│       │   ├── FormatoRelatorio.cs          # enum: Json, Xlsx, Pdf
│       │   ├── EstoqueResumoDashboardDto.cs # saída de /dashboard/estoque
│       │   ├── Fornecedor{Criacao,Atualizacao,Resposta,Filtro}Dto.cs  # mesmo desenho, para fornecedores
│       │   ├── VendedorDtos.cs              # criação, atualização, filtro e resposta de vendedores
│       │   ├── ComissaoDtos.cs              # filtro, linha, totais e corpo do "pagar"
│       │   ├── Validacoes/CpfAttribute.cs   # só CPF (vendedor), reaproveitando o DocumentoValidador
│       │   ├── PedidoCompraCriacaoDto.cs    # entrada do POST/PUT (itens sem preço; sem forma de pagamento)
│       │   ├── PedidoCompraItemEntradaDto.cs # item do POST/PUT: produtoId, quantidade, desconto
│       │   ├── PedidoCompraRespostaDto.cs   # saída com itens (preço = custo, subtotal)
│       │   ├── PedidoCompraResumoDto.cs     # linha da listagem (sem itens)
│       │   ├── PedidoCompraFiltroDto.cs     # query string da listagem
│       │   └── Validacoes/
│       │       ├── DocumentoValidador.cs    # regra de CPF/CNPJ
│       │       ├── CpfCnpjAttribute.cs      # atributo [CpfCnpj]
│       │       └── UfAttribute.cs           # atributo [Uf]
│       ├── Services/
│       │   ├── IClienteService.cs
│       │   ├── ClienteService.cs            # regras de negócio + acesso a dados
│       │   ├── IProdutoService.cs
│       │   ├── ProdutoService.cs            # regras de negócio + acesso a dados de produtos
│       │   ├── ICategoriaService.cs
│       │   ├── CategoriaService.cs          # regras de negócio + acesso a dados de categorias
│       │   ├── IPedidoService.cs
│       │   ├── PedidoService.cs             # criar/editar/confirmar/cancelar + recálculo do total
│       │   ├── CalculoPedido.cs             # subtotal/total em decimal, funcao pura (sem banco)
│       │   ├── TransicoesPedido.cs          # transicoes de status validas, funcao pura
│       │   ├── ConflitoException.cs         # vira HTTP 409
│       │   ├── DadoInvalidoException.cs     # campo inválido que só o banco sabe (ex.: categoria inativa) -> HTTP 400
│       │   ├── IEstoqueService.cs
│       │   ├── EstoqueService.cs            # saldo, extrato, entrada manual, baixa/estorno (venda) e receber/estorno (compra)
│       │   ├── EstoqueCalculo.cs            # saldo = Σ Entrada − Σ Saída, funcao pura (sem banco)
│       │   ├── IContasReceberService.cs
│       │   ├── ContasReceberService.cs      # consulta, receber, gerar/cancelar parcelas (usados pelo PedidoService)
│       │   ├── ContasReceberCalculo.cs      # divisao em N parcelas (resto na ultima), funcao pura (sem banco)
│       │   ├── DashboardCalculo.cs          # ticket medio e preenchimento de dias, funcao pura (sem banco)
│       │   ├── IFornecedorService.cs
│       │   ├── FornecedorService.cs         # regras de negócio + acesso a dados (espelho de ClienteService)
│       │   ├── IVendedorService.cs
│       │   ├── VendedorService.cs           # espelho de FornecedorService, com CPF único
│       │   ├── IComissaoService.cs
│       │   ├── ComissaoService.cs           # lista com totais do filtro, pagar em lote (a geração fica no ContasReceberService)
│       │   ├── ComissaoCalculo.cs           # parcela × % com 2 casas, função pura (com xUnit)
│       │   ├── IOrcamentoService.cs
│       │   ├── OrcamentoService.cs          # orçamento (validações de item do PedidoService), gerar pedido, perder
│       │   ├── ExportadorOrcamento.cs       # PDF do orçamento (QuestPDF, layout de documento)
│       │   ├── IDevolucaoService.cs
│       │   ├── DevolucaoService.cs          # devolução numa transação: parcelas, reembolso, estorno, estoque
│       │   ├── DevolucaoCalculo.cs          # valor, "o que falta", abatimento, reembolso e estorno (puro, com xUnit)
│       │   ├── IPedidoCompraService.cs
│       │   ├── PedidoCompraService.cs       # criar/editar/confirmar/cancelar, reaproveitando CalculoPedido/TransicoesPedido
│       │   ├── IContasPagarService.cs
│       │   ├── ContasPagarService.cs        # lista (origem/favorecido), pagar, cancelar, conta avulsa, parcelas da compra; propaga para comissões
│       │   ├── ContasPagarCalculo.cs        # parcelas da conta avulsa (valor e vencimento), função pura
│       │   ├── IRelatorioService.cs
│       │   ├── RelatorioService.cs          # consultas dos relatórios + montagem do modelo de exportação
│       │   ├── RelatorioCalculo.cs          # período válido e resumo (funções puras, com xUnit)
│       │   ├── RelatorioModelo.cs           # modelo genérico (título, filtros, resumo, colunas, linhas) + formatação pt-BR
│       │   └── ExportadorRelatorio.cs       # modelo -> .xlsx (ClosedXML) ou .pdf (QuestPDF), para os 3 relatórios
│       ├── Data/
│       │   ├── ErpPortfolioDbContext.cs     # mapeamento EF Core (snake_case)
│       │   └── Migrations/                  # migrations geradas pelo EF Core
│       ├── Properties/launchSettings.json   # porta 5065, abre o Swagger
│       ├── appsettings.json
│       ├── appsettings.Development.json     # connection string (sem senha real) + CORS
│       └── ErpPortfolio.Api.http            # requisições prontas para testar a API
│
├── backend/ErpPortfolio.Tests/              # testes unitarios (xUnit): calculo, transicoes, DTOs, modelo, paridade com o front
│
└── frontend/
    └── erp-portfolio-web/
        ├── .env.development                 # VITE_API_URL
        ├── vite.config.ts                   # porta fixa 5173
        ├── public/favicon.svg               # ícone do Ambition ERP
        └── src/
            ├── main.tsx                     # providers (TanStack Query, Ant Design pt-BR + tema, React Router) e fonte Inter
            ├── App.tsx / App.css            # layout: menu lateral recolhível, cabeçalho, conteúdo e rotas
            ├── index.css                    # estilos globais (fundo, fonte, barras de rolagem)
            ├── tema/
            │   └── temaAmbition.ts          # cores e tokens do tema (fonte única)
            ├── components/
            │   ├── ItemFormulario.tsx       # item de formulário (rótulo, obrigatório, erro) compartilhado
            │   ├── LogoAmbition.tsx/.css    # logo
            │   ├── TagStatus.tsx/.css       # tag Ativo/Inativo
            │   ├── TagStatusParcela.tsx     # tag Pendente/Atrasado/Recebido/Pago/Cancelado (receber e pagar)
            │   └── ModalParcelas.tsx        # modal de confirmar com nº de parcelas e intervalo (venda e compra)
            ├── api/
            │   ├── axiosClient.ts           # instância do Axios + leitura de ProblemDetails
            │   ├── clientesApi.ts           # chamadas da API de clientes
            │   ├── produtosApi.ts           # chamadas da API de produtos
            │   ├── categoriasApi.ts         # chamadas da API de categorias
            │   ├── estoqueApi.ts            # chamadas da API de estoque
            │   ├── contasReceberApi.ts      # chamadas da API de contas a receber
            │   ├── contasPagarApi.ts        # chamadas da API de contas a pagar (inclui conta avulsa e cancelar)
            │   ├── relatoriosApi.ts         # dados dos relatórios + download do arquivo (nome do Content-Disposition)
            │   ├── dashboardApi.ts          # chamadas da API do Dashboard (4 endpoints)
            │   ├── fornecedoresApi.ts       # chamadas da API de fornecedores
            │   ├── vendedoresApi.ts         # chamadas da API de vendedores
            │   ├── comissoesApi.ts          # chamadas da API de comissões
            │   ├── orcamentosApi.ts         # chamadas da API de orçamentos (+ URL do PDF)
            │   ├── devolucoesApi.ts         # registrar e listar devoluções de um pedido
            │   └── pedidosCompraApi.ts      # chamadas da API de pedidos de compra
            ├── hooks/
            │   ├── useClientes.ts           # useQuery / useMutation
            │   ├── useProdutos.ts
            │   ├── useCategorias.ts         # inclui as categorias ativas do seletor de produtos
            │   ├── useEstoque.ts            # lista com saldo, extrato por produto, entrada manual
            │   ├── useContasReceber.ts      # lista paginada e marcar parcela como recebida
            │   ├── useContasPagar.ts        # lista paginada e marcar parcela como paga
            │   ├── useRelatorios.ts         # consulta só após "Gerar"; download como mutação
            │   ├── useDashboard.ts          # 4 queries independentes (vendas, contas a receber, contas a pagar, estoque)
            │   ├── useFornecedores.ts       # useQuery / useMutation
            │   ├── useVendedores.ts         # useQuery / useMutation
            │   ├── useComissoes.ts          # lista com totais e marcar como pagas
            │   ├── useOrcamentos.ts         # lista, detalhe, salvar, gerar pedido e perder
            │   ├── useDevolucoes.ts         # histórico e registrar (invalida os módulos afetados)
            │   └── usePedidosCompra.ts      # useQuery / useMutation de pedidos de compra
            ├── pages/Clientes/
            │   ├── ClientesListaPage.tsx    # filtros, tabela, paginação, ações
            │   ├── ClienteFormDrawer.tsx    # painel lateral de inclusão/edição
            │   └── clientes.css             # estilos da tela e do painel (Produtos reaproveita)
            ├── pages/Produtos/
            │   ├── ProdutosListaPage.tsx    # filtros, tabela com margem, paginação, ações
            │   └── ProdutoFormDrawer.tsx    # painel lateral de inclusão/edição (seleção de categoria)
            ├── pages/Categorias/
            │   ├── CategoriasListaPage.tsx  # busca, status, tabela, paginação, ações
            │   └── CategoriaFormDrawer.tsx  # painel lateral de inclusão/edição
            ├── pages/Orcamentos/
            │   ├── OrcamentosListaPage.tsx  # filtro (número/cliente + situação com Vencido), tabela, link do pedido
            │   └── OrcamentoPage.tsx        # formulário (tabela de itens do pedido) + gerar pedido, perder, PDF
            ├── pages/Pedidos/
            │   ├── PedidosListaPage.tsx     # filtro (número/cliente + status), tabela, paginação
            │   ├── PedidoPage.tsx           # formulário: cliente, itens, desconto, resumo, ações
            │   ├── ItensPedidoTabela.tsx    # tabela de itens (tela larga) / cartões (celular)
            │   ├── DevolucaoModal.tsx       # registrar devolução: itens, volta ao estoque, prévia
            │   ├── DevolucoesPedido.tsx     # histórico de devoluções na página do pedido
            │   └── pedido.css               # estilos da página do pedido
            ├── pages/Estoque/
            │   ├── EstoqueListaPage.tsx     # busca, tabela com saldo, paginação, ações
            │   ├── EntradaEstoqueDrawer.tsx # painel de nova entrada manual (produto, quantidade, motivo)
            │   └── MovimentacoesDrawer.tsx  # painel de extrato paginado de um produto
            ├── pages/ContasReceber/
            │   └── ContasReceberListaPage.tsx  # busca, filtro de status, tabela, marcar como recebido
            ├── pages/ContasPagar/
            │   ├── ContasPagarListaPage.tsx    # busca, filtros de origem/status, tabela, pagar, cancelar
            │   └── NovaContaModal.tsx          # lançamento de conta avulsa em parcelas
            ├── pages/Relatorios/
            │   ├── RelatorioPedidosPage.tsx    # Vendas e Compras (prop tipo): filtros, resumo, tabela, exportar
            │   └── RelatorioEstoquePage.tsx    # posição de estoque: filtros, resumo, tabela, exportar
            ├── pages/Dashboard/
            │   ├── DashboardPage.tsx        # cards de indicador + gráfico
            │   ├── CardIndicador.tsx        # card com loading/erro próprios (D7)
            │   ├── GraficoFaturamento.tsx   # gráfico de barras em SVG, sem biblioteca
            │   └── dashboard.css
            ├── pages/Fornecedores/
            │   ├── FornecedoresListaPage.tsx  # filtros, tabela, paginação, ações (espelho de Clientes)
            │   └── FornecedorFormDrawer.tsx   # painel lateral de inclusão/edição
            ├── pages/Comissoes/
            │   └── ComissoesListaPage.tsx     # filtros, cards, tabela com seleção, marcar como pagas
            ├── pages/Vendedores/
            │   ├── VendedoresListaPage.tsx    # filtros, tabela com % de comissão, ações (espelho de Fornecedores)
            │   └── VendedorFormDrawer.tsx     # painel lateral de inclusão/edição
            ├── pages/PedidosCompra/
            │   ├── PedidosCompraListaPage.tsx # filtro (número/fornecedor + status), tabela, paginação
            │   ├── PedidoCompraPage.tsx       # formulário: fornecedor, itens, desconto, resumo, ações
            │   └── ItensPedidoCompraTabela.tsx # tabela de itens (tela larga) / cartões (celular)
            ├── schemas/
            │   ├── clienteSchema.ts         # schema Zod do formulário de cliente
            │   ├── produtoSchema.ts         # schema Zod do formulário de produto
            │   ├── categoriaSchema.ts       # schema Zod do formulário de categoria
            │   ├── pedidoSchema.ts          # schema Zod do formulário de pedido + conversões form/API
            │   ├── estoqueEntradaSchema.ts  # schema Zod do formulário de entrada de estoque
            │   ├── fornecedorSchema.ts      # schema Zod do formulário de fornecedor
            │   ├── vendedorSchema.ts        # schema Zod do formulário de vendedor (só CPF, % 0-100)
            │   ├── contaAvulsaSchema.ts     # schema Zod da conta avulsa
            │   └── pedidoCompraSchema.ts    # schema Zod do formulário de pedido de compra + conversões form/API
            ├── types/
            │   ├── cliente.ts               # tipos (espelham os DTOs)
            │   ├── produto.ts
            │   ├── categoria.ts
            │   ├── pedido.ts                # inclui PedidoConfirmarEntrada (numeroParcelas, intervaloDias)
            │   ├── estoque.ts               # inclui pedidoCompraId na Movimentacao
            │   ├── contaReceber.ts
            │   ├── contaPagar.ts
            │   ├── relatorio.ts
            │   ├── dashboard.ts
            │   ├── fornecedor.ts
            │   ├── vendedor.ts
            │   ├── comissao.ts
            │   ├── pedidoCompra.ts
            │   └── paginacao.ts             # ResultadoPaginado compartilhado
            ├── components/SelecaoCliente.tsx / SelecaoProduto.tsx / SelecaoFornecedor.tsx / SelecaoVendedor.tsx  # seleção com busca no servidor (usadas nos pedidos e nos filtros dos relatórios, com `aoLimpar`)
            ├── components/TagStatusPedido.tsx                      # tag Rascunho/Confirmado/Cancelado (reaproveitada pelo Pedido de Compra)
            ├── hooks/useBuscaCadastros.ts    # busca com debounce para os seletores acima (clientes, fornecedores, produtos)
            ├── hooks/usePedidos.ts           # useQuery / useMutation de pedidos
            ├── api/pedidosApi.ts             # chamadas da API de pedidos
            └── utils/
                ├── documento.ts             # validação e máscara de CPF/CNPJ
                ├── moeda.ts                 # formatação em reais/percentual/quantidade e cálculo de margem
                ├── ufs.ts                   # 27 UFs com nome, busca sem acentos e ordenação
                └── calculoPedido.ts         # subtotal/total em aritmética inteira (BigInt), mesma fórmula do back
```

---

## Como rodar (passo a passo)

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) **aberto e rodando** (ou um PostgreSQL local)

Todos os comandos partem da **raiz do repositório**. Os exemplos usam PowerShell; no bash, troque `Copy-Item` por `cp`.

### 1. Criar o banco e rodar as migrations

```powershell
# 1.1 Crie o .env a partir do modelo e troque POSTGRES_PASSWORD por uma senha sua
Copy-Item .env.example .env

# 1.2 Suba o PostgreSQL (porta 5433 no host)
docker compose up -d

# 1.3 Instale a ferramenta local dotnet-ef (lida do dotnet-tools.json)
dotnet tool restore

# 1.4 Guarde a connection string com a senha real no User Secrets (fora do repositório)
dotnet user-secrets set "ConnectionStrings:ErpPortfolio" "Host=localhost;Port=5433;Database=erp_portfolio_db;Username=erp_user;Password=SENHA_DO_ENV" --project backend/ErpPortfolio.Api

# 1.5 Crie a tabela aplicando as migrations
dotnet ef database update --project backend/ErpPortfolio.Api
```

Observações:
- Use no passo 1.4 **a mesma senha** que colocou no `.env`.
- O `appsettings.Development.json` traz só o texto `DEFINA_VIA_USER_SECRETS` no lugar da senha. Como alternativa ao User Secrets, dá para usar a variável de ambiente `ConnectionStrings__ErpPortfolio`.
- Uma linha `Failed executing DbCommand` no **primeiro** `database update` é normal: o EF tenta ler o histórico de migrations antes de a tabela existir. O importante é terminar com `Done.`
- Para conferir a tabela criada:
  ```powershell
  docker exec erp_portfolio_postgres psql -U erp_user -d erp_portfolio_db -c "\d clientes"
  ```

### 2. Subir a API e testar no Swagger

```powershell
dotnet run --project backend/ErpPortfolio.Api --launch-profile http
```

1. O navegador abre o Swagger sozinho. Se não abrir, acesse <http://localhost:5065/swagger>.
2. Abra `POST /api/clientes`, clique em **Try it out** e envie:
   ```json
   {
     "nome": "Maria da Silva",
     "documento": "529.982.247-25",
     "email": "maria@exemplo.com.br",
     "telefone": "(11) 98765-4321",
     "cidade": "São Paulo",
     "uf": "SP"
   }
   ```
   Resposta esperada: **201 Created**.
3. Envie o mesmo JSON de novo. Resposta esperada: **409 Conflict** ("Já existe um cliente cadastrado com este CPF/CNPJ.").
4. Envie um JSON inválido (ex.: `"nome": "A"`, `"uf": "XX"`). Resposta esperada: **400** com as mensagens por campo em `errors`.
5. Teste também:
   - `GET /api/clientes?nome=silva` (listagem paginada com filtro)
   - `GET /api/clientes?ufs=SP&ufs=MG&ativo=true` (várias UFs + só ativos)
   - `GET /api/clientes/{id}` (200, ou 404 se não existir)
   - `PUT /api/clientes/{id}` (edição; mande `"ativo": true` para reativar)
   - `PATCH /api/clientes/{id}/inativar` (204)

Também dá para testar pelo arquivo [ErpPortfolio.Api.http](backend/ErpPortfolio.Api/ErpPortfolio.Api.http), que já tem essas requisições prontas (VS Code com a extensão REST Client, Visual Studio ou Rider).

### 3. Subir o front e ver a tela de clientes

Com a API rodando, abra **outro terminal**:

```powershell
cd frontend/erp-portfolio-web
npm install
npm run dev
```

Acesse <http://localhost:5173>. Na tela:

| Ação | Como fazer |
|---|---|
| Cadastrar | Botão **Novo cliente** → preencher no painel lateral → **Salvar cliente** |
| Filtrar | Botão **Filtrar** (mostra a quantidade de filtros ativos) → preencher **Buscar cadastro**, **Estado (UF)** (uma ou mais, por nome ou sigla) e/ou **Status cadastral** → **Aplicar** (ou Enter na busca) |
| Ver/remover filtros aplicados | Aparecem como tags abaixo do cabeçalho; o **x** de cada tag remove só aquele filtro, e **Limpar tudo** remove todos |
| Paginar | Rodapé da tabela: páginas e quantidade por página |
| Editar | Ícone de **lápis** na linha |
| Inativar | Ícone **vermelho** na linha → confirmar (desabilitado se o cliente já estiver inativo) |
| Reativar | Editar o cliente e ligar a chave em **Status do cadastro** |
| Recolher o menu | Ícone ao lado do breadcrumb, no cabeçalho (no celular, o ☰ abre o menu) |

Dica: o CPF/CNPJ pode ser digitado sem máscara; ao sair do campo ele é formatado automaticamente.

### Parar tudo

```powershell
# Ctrl+C nos terminais da API e do front
docker compose stop          # para o banco e mantém os dados
docker compose down -v       # remove container e volume (APAGA os dados)
```

---

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

Sem parâmetros — o mês é sempre calculado no servidor (`DateTime.UtcNow`), nunca enviado pelo cliente. Cada rota só delega pro service do módulo de origem (`PedidoService`, `ContasReceberService`, `ContasPagarService`, `EstoqueService`); não existe um "DashboardService" com lógica própria.

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

Confirmar um pedido de compra gera as parcelas (mesmas regras de `numeroParcelas`/`intervaloDias` do Pedido de Venda); cancelar um que estava Confirmado cancela as parcelas **Pendentes** depois da checagem de saldo — se o cancelamento for recusado por saldo, nenhuma parcela muda.

Relatórios (só leitura; `formato` = `json` (padrão), `xlsx` ou `pdf` — com arquivo, a resposta é o download com `Content-Disposition`):

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/relatorios/vendas?dataInicio=&dataFim=&status=&clienteId=&formato=` | Pedidos de venda do período (datas `AAAA-MM-DD`, inclusivas, obrigatórias, no máximo 366 dias), um por linha, com quantidade, valor total e ticket médio | 200, 400 |
| GET | `/relatorios/compras?dataInicio=&dataFim=&status=&fornecedorId=&formato=` | Mesmo formato, para pedidos de compra | 200, 400 |
| GET | `/relatorios/estoque?categoriaId=&somenteAbaixoMinimo=&formato=` | Posição atual dos produtos ativos: saldo, custo, valor em estoque, abaixo do mínimo | 200, 400 |

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

Liberado apenas para as origens de `Cors:OrigensPermitidas`. Em desenvolvimento (`appsettings.Development.json`) a única origem é `http://localhost:5173`. Em produção a lista fica vazia até ser configurada.

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

## Banco de dados

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

---

## Dados de teste

Documentos **válidos** (dígitos verificadores corretos) para testar o cadastro:

| Tipo | Com máscara | Sem máscara |
|---|---|---|
| CPF | 529.982.247-25 | 52998224725 |
| CPF | 390.533.447-05 | 39053344705 |
| CPF | 111.444.777-35 | 11144477735 |
| CNPJ numérico | 11.222.333/0001-81 | 11222333000181 |
| CNPJ alfanumérico | 12.ABC.345/01DE-35 | 12ABC34501DE35 |

Documento **inválido** para testar o erro: `123.456.789-00`.

---

## Decisões técnicas

| Decisão | Motivo |
|---|---|
| Solution em `.slnx` | É o formato padrão gerado pelo .NET 10; funciona no Visual Studio, Rider e CLI. |
| PostgreSQL na porta **5433** | Evitar conflito com outro PostgreSQL que já esteja usando a 5432 na máquina. |
| Volume em `/var/lib/postgresql` | A partir do PostgreSQL 18 a imagem oficial usa esse caminho (e não mais `/var/lib/postgresql/data`). |
| Swashbuckle no lugar de `Microsoft.AspNetCore.OpenApi` | O template trazia `Microsoft.OpenApi 2.0.0`, com vulnerabilidade de alta gravidade conhecida; o Swashbuckle também fornece a interface do Swagger. |
| `dotnet-ef` como ferramenta **local** | Versão fixa no repositório (`dotnet-tools.json`), sem depender de instalação global. |
| Senha no **User Secrets** + `.env` fora do Git | Nenhuma senha real é commitada. |
| Tabelas e colunas em **snake_case** | Convenção do PostgreSQL; o mapeamento é explícito no `ErpPortfolioDbContext`. |
| Duplicidade checada no serviço **e** por índice único | A checagem prévia gera a mensagem amigável; o índice único cobre duas gravações simultâneas (SQLSTATE 23505 também vira 409). |
| Busca com `ILIKE` e curingas escapados | Busca sem diferenciar maiúsculas/minúsculas; `%` e `_` digitados pelo usuário são tratados como texto. |
| Inativação em vez de exclusão | Preserva o histórico (exclusão lógica); a reativação é feita pelo `PUT`. |
| E-mail/telefone vazios viram `null` no DTO | Quem testa pelo Swagger costuma mandar `""`; sem isso a API recusaria um e-mail vazio. |
| `noValidate` no formulário | A validação nativa do navegador (`type="email"`) bloqueava o envio antes do Zod e escondia as mensagens do Ant Design. |
| Colunas responsivas + cartões no celular | Todas as colunas aparecem a partir de 1600 px. Abaixo disso some "Cadastro" (que nem existe no mockup), e abaixo de 1200 px somem E-mail e Telefone (visíveis no painel de edição). No celular (< 768 px), cada cliente vira um cartão com nome, documento, cidade e status. A página nunca rola na horizontal. |
| Cores definidas só em `temaAmbition.ts` | Os tokens do Ant Design e as variáveis CSS `--cor-*` saem do mesmo objeto; trocar uma cor em um lugar atualiza a interface toda. |
| Cores do texto do `DESIGN.md`, não do cabeçalho YAML | O cabeçalho do `DESIGN.md` traz uma paleta gerada (`#111416`, `#4be277`) um pouco diferente da descrita no texto e usada nos mockups (`#0F1112`, `#22C55E`); valem as dos mockups. |
| Fundo e cards mais claros que no `DESIGN.md` | Com o fundo original (`#0F1112`) e os cards (`#1A1D1F`) quase iguais, não dava para distinguir os cards. Os tons de cinza foram clareados e afastados entre si (fundo `#1B1E21`, card `#272B30`, borda `#3B4046`); o verde e as cores de status seguem o `DESIGN.md`. |
| Fonte Inter auto-hospedada | Vem do pacote npm (`@fontsource-variable/inter`), sem depender do Google Fonts nem de internet. |
| Logo com texto em HTML | Texto dentro de SVG não acompanha a largura real da fonte; o "ERP" ficava sobreposto. Só o ícone é SVG. |
| Formulário em painel lateral (Drawer) | Segue o mockup e mantém a lista visível ao fundo durante a edição. |
| Filtros num painel por trás do botão **Filtrar** (Popover), não num card fixo | O card sempre visível ficou "feio" e ocupava espaço mesmo sem filtro nenhum; o botão só mostra o painel quando clicado. Os filtros aplicados continuam visíveis como tags removíveis abaixo do cabeçalho, para não esconder o que está filtrado. |
| Painel de filtros só aplica no botão **Aplicar** | O usuário monta a combinação (nome + UFs + status) e aplica de uma vez, sem uma consulta a cada tecla ou clique. |
| Painel de filtros recarrega do estado aplicado ao abrir | Se o usuário mudar algo e fechar sem aplicar (clicando fora), a próxima abertura descarta esse rascunho e mostra de novo o que está realmente filtrado. |
| Busca de UF ordenada por relevância | Ao digitar "RN", "peRNambuco" também combina; a sigla exata vem primeiro para o Enter selecionar a UF certa. |
| Lista na query string sem colchetes | O Axios envia `ufs[]=SP` por padrão; configurado para `ufs=SP&ufs=MG`, o formato que o ASP.NET entende. |
| Vite com `strictPort` na 5173 | É a origem liberada no CORS; se a porta estiver ocupada, o Vite avisa em vez de trocar de porta. |
| Paginação limitada a 100.000 páginas | Evita estouro de inteiro no cálculo do OFFSET. |
| Categoria como **cadastro próprio** (tabela `categorias`), escolhida por seleção | Texto livre deixava "Periférico" e "periferico" virarem categorias diferentes e não dava para renomear em um lugar só. Ficou opcional no produto. |
| Categoria tem tela própria no menu, e não um "criar categoria" dentro do formulário do produto | Segue o padrão dos outros módulos (lista + painel lateral) e mantém o formulário de produto simples; um atalho "Nova categoria" no seletor pode ser acrescentado depois. |
| Categoria só é inativada, e o FK é `RESTRICT` | Inativar não mexe nos produtos; o banco ainda impede excluir por engano uma categoria com produtos. |
| Nome de categoria único **sem diferenciar maiúsculas/minúsculas**, checado no serviço (`ILIKE`) com índice único exato como reforço | O `citext` ou um índice por `lower(nome)` exigiriam extensão/SQL fora do modelo do EF; o serviço cobre o uso normal e o índice cobre a corrida entre duas gravações do mesmo texto exato. |
| Categoria inexistente ou inativa no produto vira **400** no campo `categoriaId` (`DadoInvalidoException`) | Só o banco sabe; o formato é o mesmo dos erros de validação, então a tela mostra a mensagem no campo. A categoria que o produto já tem continua aceita, senão editar qualquer campo de um produto antigo falharia. |
| Uma única projeção (`ProdutoRespostaDto.Projecao`) para SQL e para entidade carregada | O nome da categoria vem por join na listagem sem duplicar o mapeamento. |
| Migration de categorias editada à mão | Ver a nota em "Banco de dados": a versão gerada apagaria os textos existentes. |
| Custo do produto obrigatório | Toda linha da lista tem margem calculada; sem custo a margem ficaria vazia. |
| React Router com a chave do item de menu igual ao caminho da rota | O menu marca o item pela URL, o breadcrumb acompanha a rota e a página aberta sobrevive ao recarregar. |
| `ativo` opcional no `PUT` (`bool?`) | Com padrão `true`, um `PUT` sem o campo reativava o cadastro inativo em silêncio. Vale também para Clientes. |
| Conflito de documento/SKU avisa quando o existente está inativo | O inativo continua ocupando o índice único; a mensagem orienta reativar em vez de recriar. |
| `trim` (e maiúsculas na unidade) aplicados no DTO **antes** de validar tamanho/formato | Evita que `"  ab"` passe no mínimo de 3 caracteres e seja gravado como `"ab"`. |
| SKU somente numérico (2 a 30 dígitos) | No ERP o SKU é o número de série do produto. A API recusa letras e símbolos e o campo da tela descarta o que não for dígito (também ao colar). A coluna continua `varchar(30)`, então liberar letras no futuro não exige migration. |
| Preço e custo com mais de 2 casas são **recusados**, não arredondados | A coluna é `numeric(12,2)`; sem a checagem o banco arredondaria em silêncio. |
| Telas de Produtos e Categorias reaproveitam as classes de `clientes.css` e o `ItemFormulario` | Evita duplicar estilos e o item de formulário; quando houver um terceiro módulo, vale mover o CSS para um arquivo compartilhado. |
| `CalculoPedido` e `TransicoesPedido` são classes estáticas puras (sem banco) | Testáveis por xUnit sem precisar de banco; o `PedidoService` só orquestra e chama essas funções. |
| Preço do item **copiado e congelado**, não uma referência ao produto | Mudar o preço do produto ou inativá-lo depois não pode alterar pedidos já feitos; provado com o preço do produto mudando de 350 para 999 e o pedido salvo continuando em 350. |
| `valor_total` gravado por um único método de recálculo (`PedidoService.Recalcular`) | Criar e editar passam pelo mesmo lugar; evita divergência entre a soma dos itens e o total gravado, e aplica o limite de R$ 9.999.999.999,99 num só ponto. |
| Confirmar salva o rascunho pendente (PUT) antes de confirmar (PATCH) | Editar e confirmar na mesma ação evita perder uma alteração feita na tela só porque o usuário esqueceu de salvar antes. |
| Cálculo do total em aritmética **inteira** no front (BigInt: milésimos, centavos e centésimos de percentual) | `Number` do JavaScript erra em casos como `1.005 * 100`; a tela precisa bater com o `decimal` do servidor até o centavo. |
| Seleção de cliente/produto com busca **no servidor** (debounce de 300 ms, só ativos, 20 por consulta) | Carregar todos os clientes/produtos na tela do pedido não escalaria; o `labelInValue` do Ant Design mantém o nome do escolhido mesmo depois de a busca mudar. |
| Índice único `(pedido_id, produto_id)` além da checagem no serviço | A checagem prévia dá a mensagem amigável; o índice cobre duas gravações simultâneas do mesmo produto no mesmo pedido. |
| Item do pedido em tabela (telas largas) e em cartão empilhado (< 768 px) | A tabela de 6 colunas exigia rolagem interna e escondia preço/desconto/subtotal no celular; o cartão mostra tudo de uma vez. |
| Pedido nunca é excluído, só cancelado (cancelar é idempotente) | Preserva o histórico de vendas; cancelar um pedido já cancelado retorna sucesso (204) porque o estado desejado já é o atual. |
| Estoque em **histórico de movimentações**, não um campo `saldo` no produto | Dá rastreabilidade (o que entrou, o que saiu, por quê); o saldo é sempre a soma, nunca diverge de um campo gravado à parte. |
| Confirmar um pedido **confere o saldo de todos os itens antes de gravar qualquer movimentação** | Se um item não tiver saldo, nenhum outro item do mesmo pedido é baixado (nem os que tinham saldo) — tudo ou nada. |
| Baixa e estorno de estoque **não chamam `SaveChanges` sozinhos**; ficam na mesma transação do `SaveChangesAsync` que o `PedidoService` já fazia | Confirmar/cancelar o pedido e mexer no estoque acontecem atomicamente, sem transação explícita adicional. |
| Cancelar só estorna se o pedido **estava Confirmado** antes de cancelar | Um rascunho nunca baixou estoque, então cancelá-lo não deve gerar uma entrada de estorno indevida. |
| Entrada manual **sem** cadastro de fornecedor | Fora do escopo por enquanto; o campo motivo (texto livre) cobre a rastreabilidade básica de uma compra. |
| Parcelas geradas **só ao confirmar** (não antes, não depois) | Confirmar é o único momento em que a venda "acontece" de verdade; gerar antes seria prematuro, gerar depois exigiria uma tela extra. |
| Resto da divisão em parcelas sempre na **última** | Garante que a soma bate exatamente com `valorTotal`, sem sobra nem falta de centavos. |
| "Atrasado" **calculado no servidor**, nunca no navegador | O relógio/fuso do navegador não é confiável para uma regra de negócio; o servidor já faz isso com `valorTotal` e saldo de estoque, aqui é o mesmo raciocínio. |
| `vencimento` como `date` (não `timestamptz`) | É uma data de calendário; comparar com timestamp exigiria cuidado extra de fuso horário para decidir se "venceu hoje". |
| Geração/cancelamento de parcelas **não chamam `SaveChanges` sozinhos** | Mesmo padrão do Estoque: ficam na mesma transação do `SaveChangesAsync` que `PedidoService` já fazia ao confirmar/cancelar. |
| Cancelar só cancela parcelas **Pendentes**, nunca as **Recebidas** | Preserva o histórico de recebimento real, mesmo que o pedido seja cancelado depois. |
| Modal de confirmar do pedido virou um **formulário controlado** (não mais `Modal.confirm` imperativo) | Precisa capturar 2 campos (parcelas, intervalo) antes de confirmar; em erro, o modal fica aberto para corrigir sem reabrir. |
| Filtro por **categoria** em Produtos reaproveita `useCategoriasAtivas` (já existia para o formulário) | Mesma lista de categorias ativas serve pro filtro e pro seletor do cadastro, sem duplicar a consulta. |
| Atalho "Nova categoria" devolve a categoria criada por uma prop (`aoCriar`) no `CategoriaFormDrawer` | Evita duplicar o formulário de categoria; o mesmo painel serve pra tela de Categorias e pro atalho dentro de Produtos. |
| Dashboard **sem tabela nova** e **sem serviço próprio** — cada resumo entra no service do módulo de origem | Não há regra de negócio do Dashboard em si, só agregação do que os outros services já calculam; um serviço novo só pra orquestrar seria uma camada sem função. |
| Cada card do Dashboard busca seu próprio endpoint, independente dos outros | Se um indicador falhar, os outros continuam aparecendo; também deixa fácil acrescentar um card novo depois sem mexer nos existentes. |
| Gráfico de faturamento diário em **SVG desenhado à mão**, sem biblioteca de gráfico | Uma série de ~30 barras não justifica o peso de uma lib inteira (o bundle já tem aviso de chunk grande); segue a skill de dataviz do projeto (mark specs, tooltip acessível). |
| Saldo baixo de estoque com **limite fixo no código** (`≤ 5`), não um campo por produto | Não existe estoque mínimo cadastrado ainda; trocar por um campo por produto é uma extensão natural quando precisar. |
| Pedido de Compra reaproveita `StatusPedido`, `TransicoesPedido` e `CalculoPedido` do Pedido de Venda, em vez de duplicar | São funções/enum puros, independentes de qual pedido é — reescrever um paralelo só pra ter um nome diferente seria código a mais sem função. |
| Documento do Fornecedor único **num índice próprio**, separado do de Clientes | Uma mesma empresa pode ser cliente e fornecedora ao mesmo tempo (CNPJ repetido entre as duas tabelas é esperado). |
| `EstoqueMovimentacao` ganha `pedido_compra_id` **ao lado** de `pedido_id` (não reaproveita a mesma coluna) | São FKs para tabelas diferentes (`pedidos` vs. `pedidos_compra`); uma movimentação preenche no máximo uma das duas. |
| Confirmar o Pedido de Compra **não checa saldo** ao dar entrada, mas **cancelar checa saldo antes de estornar** | Uma compra sempre pode entrar no estoque; o risco é o oposto — estornar uma entrada cujo saldo já foi consumido por uma venda deixaria o saldo errado. |
| Preço do item do Pedido de Compra = `Custo` do produto (não `PrecoVenda`), e confirmar **sobrescreve** o `Custo` do produto com o preço congelado no item | Mantém o custo do produto sempre no valor da última compra, mesmo que ele tenha sido editado manualmente entre montar o rascunho e confirmar. |
| `Produto.Custo` **não volta** ao valor anterior quando um Pedido de Compra é cancelado | Reverter exigiria guardar o custo anterior por item, e o valor "correto" fica ambíguo se houve outra compra no meio; aceito como limite conhecido. |
| `SelecaoProduto` ganhou a prop `campoPreco` (`'precoVenda' \| 'custo'`) em vez de um componente novo | Mostrar o preço de venda ao montar uma compra seria enganoso; era uma diferença pequena o bastante pra estender o componente existente. |

---

## Testes realizados

Testes manuais de ponta a ponta executados em 18/09/2026, com o banco em Docker, a API rodando e o front no Microsoft Edge (automatizado com Playwright).

### API

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
| Preflight CORS vindo de `http://localhost:5173` | Cabeçalhos `Access-Control-Allow-*` presentes | ✅ |
| Requisição de origem não permitida | Sem cabeçalhos de CORS | ✅ |
| Swagger (`/swagger`) | Página no ar com os 3 caminhos de `/api/clientes` | ✅ |
| GET `ufs=SP` / `ufs=SP&ufs=mg` | Só clientes dessas UFs (minúscula aceita) | ✅ |
| GET `ativo=false` / `ativo=true&ufs=ES` | Só inativos / combinação sem resultado | ✅ |
| GET `nome=silva&ufs=SP` | Filtros combinados | ✅ |
| GET `ufs=XX` | 400 "UF inválida." | ✅ |

### Tela

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

### Categorias (21/09/2026)

Também numa API temporária (porta 5099) e num Vite temporário (5174); os dados de teste (`ZZT…` e SKUs `77…`/`88…`) foram apagados ao final e o produto e a categoria reais ficaram intactos.

| Verificação | Resultado |
|---|---|
| Migration num banco descartável: "PERIFÉRICO"/"periférico" e "Cabos"/"  Cabos  " viram uma categoria; nulos e vazios ficam sem categoria; o banco recusa excluir categoria em uso; o `Down` devolve os nomes | ✅ |
| Migration no banco de desenvolvimento (com backup antes): o produto existente continuou ligado à categoria "PERIFÉRICO" e os clientes ficaram intactos | ✅ |
| API de Categorias e da integração com Produtos: 34 verificações (criar com `trim`, 4 casos de 400, nome duplicado inclusive com outra caixa, aviso para categoria inativa, `PUT` sem `ativo`, trocar só a caixa do próprio nome, busca, `%` sem virar curinga, status, paginação, 404; produto com/sem categoria, categoria inexistente → 400, trocar/remover categoria, categoria inativa, FK) | ✅ |
| Tela: cadastro com validação e nome com `trim`, nome duplicado com outra caixa no campo, categoria aparecendo na seleção do produto e na lista, limpar e trocar a categoria, inativar (produto mantém o nome; seleção deixa de oferecer; edição mostra "(inativa)"), renomear, filtro Inativas, celular sem rolagem horizontal | ✅ |
| Regressão: testes de Produtos (API e tela) continuam passando | ✅ |

### Produtos e correções de Clientes (21/09/2026)

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

### Pedidos (21/09/2026)

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

### Estoque (22/09/2026)

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

### Contas a Receber (22/09/2026)

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

### Filtro por categoria em Produtos (22/09/2026)

Testado na API real (instância temporária, porta 5099): `GET /api/produtos?categoriaId=` achou só o produto da categoria de teste; `categoriaId` inexistente devolveu lista vazia. Produto e categoria de teste apagados ao final; dados reais intactos.

O atalho "Nova categoria" no seletor do formulário (parte visual, sem chamada de API própria pra verificar por fora do navegador) não foi testado nesta sessão.

### Dashboard (22/09/2026)

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

### Fornecedores e Pedidos de Compra (23/09/2026)

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

### Contas a Pagar (23/09/2026)

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

### Relatórios (23/09/2026)

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

### Vendedores (23/09/2026)

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

### Comissões (23/09/2026)

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

### Comissão vira conta a pagar + contas avulsas (23/09/2026)

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

### Orçamentos (23/09/2026)

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

### Devolução de venda (24/09/2026)

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

---

## Padrões do projeto

- **Cabeçalho obrigatório** no topo de todo arquivo C#, TypeScript e SQL, com: nome do arquivo, versão (começando em 1.0.0), data e histórico de alterações.
- Nos arquivos **C#**, o cabeçalho também documenta **banco, tabelas e fontes de dados** usados, para facilitar o troubleshooting.
- Código, comentários e mensagens em **português**; nomes de classes e métodos em português quando faz sentido.
- A API usa DTOs separados para entrada e saída; a entidade nunca é exposta diretamente.
- Ao alterar um arquivo, incrementar a versão e registrar a mudança no histórico do cabeçalho.

Modelo de cabeçalho C#:

```csharp
// =====================================================================================
// Arquivo....: NomeDoArquivo.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: O que o arquivo faz.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.clientes
// Fontes.....: De onde vêm os dados (DbContext, serviço, requisição...).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================
```

Modelo de cabeçalho TypeScript:

```ts
/**
 * =====================================================================
 * Arquivo....: nomeDoArquivo.ts
 * Versão.....: 1.0.0
 * Data.......: 18/09/2026
 * Descrição..: O que o arquivo faz.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 * =====================================================================
 */
```

### Criando novas migrations

```powershell
dotnet ef migrations add NomeDaMigration --project backend/ErpPortfolio.Api -o Data/Migrations
dotnet ef database update --project backend/ErpPortfolio.Api
```

Depois de gerar, adicione o cabeçalho padrão nos arquivos novos. O `ErpPortfolioDbContextModelSnapshot.cs` é **reescrito** pelo EF Core a cada migration, então o cabeçalho dele precisa ser recolocado (com uma nova linha no histórico).

### Comandos úteis

```powershell
dotnet build ErpPortfolio.slnx                       # compila o back-end
cd frontend/erp-portfolio-web; npm run build          # checa tipos e gera o build do front
cd frontend/erp-portfolio-web; npm run lint           # lint do front (oxlint)
```

---

## Solução de problemas

| Sintoma | Causa provável e solução |
|---|---|
| `failed to connect to the docker API` | O Docker Desktop não está aberto. Abra-o e espere ficar pronto. |
| `Defina POSTGRES_PASSWORD no arquivo .env` | Falta o `.env` na raiz. Copie o `.env.example` (passo 1.1). |
| `password authentication failed for user "erp_user"` | A senha no User Secrets é diferente da do `.env`, ou o volume foi criado com outra senha. Corrija o User Secrets, ou recrie o banco com `docker compose down -v` e `docker compose up -d` (**apaga os dados**). |
| `Bind for 0.0.0.0:5433 failed: port is already allocated` | Outro serviço usa a porta 5433. Troque a porta no `docker-compose.yml` e na connection string. |
| `Failed executing DbCommand` no primeiro `database update` | Normal (ver passo 1). |
| Tela mostra "Não foi possível conectar à API" | A API não está rodando em `http://localhost:5065`, ou o `VITE_API_URL` em `.env.development` está diferente. |
| Erro de CORS no console do navegador | O front não está em `http://localhost:5173`, ou a origem não está em `Cors:OrigensPermitidas`. |
| `Port 5173 is already in use` | Outro processo usa a 5173 (o Vite usa `strictPort` e não troca de porta sozinho). Encerre o outro processo. |
| JSON com acentos recusado ao testar com `curl` no Windows | O terminal enviou o texto fora de UTF-8. Salve o JSON em arquivo UTF-8 e envie com `--data-binary @arquivo.json`, ou use o Swagger. |
| Aviso "Some chunks are larger than 500 kB" no build do front | Apenas informativo (tamanho do Ant Design); não impede o build. |

---

## Próximas etapas

- Campos do mockup de cliente ainda não implementados: **PF/PJ**, **Inscrição Estadual**, **Nome Fantasia** e **Observações** (exige migration)
- Filtro por **cidades** e busca também por CPF/CNPJ (padrão do projeto: filtros de seleção múltipla usam dropdown multi-select com espaçamento normal entre as opções)
- Filtro por **cliente** e por **faixa de data** na lista de Pedidos
- Editar a **forma de pagamento** de um pedido já confirmado (hoje só dá para cancelar e criar outro)
- Verificação visual/Playwright das telas de Contas a Receber, do Dashboard, de Fornecedores/Pedidos de Compra e de Relatórios (cards, gráfico, modal de confirmar com parcelas, formulários, celular) — não feita nas sessões que construíram os módulos
- Saída manual de estoque (perda/quebra/ajuste)
- Recebimento/pagamento parcial de parcela, juros/multa por atraso, edição de parcela já gerada
- Forma de pagamento no Pedido de Compra; pagamento parcial e contas a pagar avulsas (sem pedido de compra, ex.: aluguel)
- Gerar parcelas para pedidos (venda ou compra) confirmados antes dos módulos de Contas a Receber/Pagar
- Recebimento parcial de mercadoria no Pedido de Compra (hoje é recebido inteiro ao confirmar)
- Seletor de período no Dashboard (hoje é sempre o mês atual)
- Comissões: exportação Excel/PDF da tela, estorno de comissão
- Contas a pagar: recorrência automática (todo mês), categorias/plano de contas, editar conta lançada, anexos
- Mais relatórios: contas a receber/pagar vencidas, vendas agrupadas por produto, pedidos com os itens; considerar o fuso de Brasília no filtro de período (hoje em UTC, igual ao Dashboard)
- Mover `clientes.css` (classes usadas também por Produtos, Categorias, Pedidos, Estoque e Contas a Receber/Pagar) para um arquivo compartilhado
- Centralizar o tratamento de `ConflitoException` (hoje repetido nos controllers)
- **Autenticação/login** (etapa 15)
- Devolução: desfazer/editar, crédito do cliente, troca num passo só, devolução de compra ao fornecedor, faturamento líquido nos Relatórios
- Usar o fuso de Brasília como "hoje" no servidor (hoje é UTC: entre 21h e 24h o servidor já está no dia seguinte — afeta vencidos, validade e atrasados)
- Orçamentos: duplicar, reabrir, orçamento para não cliente, envio por e-mail, taxa de conversão no Dashboard/Relatórios; trava de concorrência ao gerar pedido se o ERP virar multiusuário
- Testes automatizados de integração para a API de Clientes/Produtos/Categorias/Fornecedores (Pedidos, Estoque, Contas a Receber, Dashboard e Pedidos de Compra já têm testes unitários e/ou scripts de ponta a ponta)
