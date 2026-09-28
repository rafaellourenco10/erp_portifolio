# Spec: NF-e simulada (etapa 16)

> Status: **aprovada e implementada em 28/09/2026** (T1 a T8, ver `tasks/todo.md`). Critérios 1-8 conferidos: E2E da API com 47 verificações e teste de tela com Playwright com 25 (dados `ZZT…` apagados, dados reais intactos, empresa restaurada).

## Objetivo

Emitir a **Nota Fiscal Eletrônica (modelo 55)** de um pedido de venda confirmado e a **NF-e de devolução** de uma devolução de venda, com impostos calculados, chave de acesso válida, XML no layout 4.00 da SEFAZ e DANFE em PDF, **sem enviar nada à SEFAZ** (autorização simulada).

- **Quem usa:** o dono do ERP de portfólio (sem login).
- **Por que agora:** escolha do Rafael em 28/09/2026. Mostra conhecimento fiscal brasileiro, que pesa num portfólio de ERP.
- **Sucesso:** confirmar uma venda, clicar em "Emitir NF-e" e baixar um XML que passa na estrutura do layout 4.00 e um DANFE com a chave, os itens e os impostos batendo com o pedido.

### Dentro do escopo
Dados do emitente (a empresa); endereço completo e Inscrição Estadual no cliente; NCM no produto; emissão da NF-e de saída pelo pedido confirmado; NF-e de devolução (entrada) pela devolução de venda, referenciando a nota original; cálculo de ICMS, PIS e COFINS por item (regime normal); chave de 44 dígitos com DV; XML 4.00; DANFE em PDF; tela de notas com filtros, download de XML/DANFE e exportação Excel/PDF.

### Fora do escopo (entram depois)
Envio real à SEFAZ, certificado digital e assinatura XML; **cancelamento da nota** e carta de correção; inutilização de numeração; NFC-e (modelo 65); IPI, ICMS-ST, DIFAL e FCP; NF-e de entrada de compra (quem emite é o fornecedor); tabela de NCM/CEST; busca de endereço pelo CEP; Simples Nacional.

## Decisões já tomadas (com o Rafael, 28/09/2026)

| Tema | Decisão |
|---|---|
| Realismo | **Simulada completa**: chave real, XML 4.00 sem assinatura, DANFE, autorização com protocolo simulado. |
| Regime | **Regime normal**: ICMS, PIS e COFINS destacados por item; CFOP conforme a UF do cliente. |
| Emissão | **Botão no pedido confirmado**, uma nota por pedido, com validação dos dados fiscais antes. |
| Extras | **Tela de notas emitidas** e **NF-e de devolução**. Cancelamento da nota ficou **de fora**. |

### Decididas por padrão (confirmadas pelo Rafael em 28/09/2026)

| Tema | Padrão proposto | Motivo |
|---|---|---|
| Dados do emitente | Tabela `empresa` com **uma linha** e tela **Fiscal → Empresa** (razão social, fantasia, CNPJ, IE, endereço, código IBGE do município, UF, série). | O DANFE e o XML precisam deles; tela é melhor que appsettings num portfólio. |
| Cliente e produto | Campos novos **opcionais** no cadastro (endereço, CEP, código IBGE, IE no cliente; NCM no produto); **obrigatórios só na hora de emitir**. | Não quebra os cadastros existentes; a emissão lista o que falta. |
| Ambiente | XML com `tpAmb=2` (homologação) e DANFE com a marca **"SEM VALOR FISCAL"**. | Deixa claro que a nota é de mentira; é o que a SEFAZ exige em testes. |
| Numeração | Sequência única por série (saída e entrada juntas), começando em 1; o número nunca se repete. | É assim na prática: devolução emitida pela empresa usa a mesma série. |
| ICMS | CST 00. Mesma UF: **alíquota interna da UF do emitente** (tabela das 27 UFs). Outra UF: **12%**, ou **7%** quando o emitente está no Sul/Sudeste (menos ES) e o cliente no Norte, Nordeste, Centro-Oeste ou ES. | Regra real da Resolução do Senado 22/89; DIFAL fica de fora. |
| PIS/COFINS | CST 01, **1,65% e 7,6%** (não cumulativo). | Alíquotas padrão do lucro real. |
| Destinatário | CNPJ com IE → contribuinte (`indIEDest=1`); sem IE → não contribuinte (`indIEDest=9`, `indFinal=1`). | Regra do layout; a alíquota não muda (DIFAL fora). |
| Pedido com nota | Pedido com NF-e autorizada **não pode ser cancelado** (409); só devolvido. | Sem cancelamento de nota, cancelar o pedido deixaria a nota órfã. |

## Regras

### NF-e (NF)

| # | Regra |
|---|---|
| NF1 | **Emitir saída:** só pedido `Confirmado` e sem nota. Antes, valida emitente completo, endereço completo do cliente e NCM (8 dígitos) de cada produto; faltando algo → 400 com a lista do que falta. Segunda emissão → 409. |
| NF2 | **Itens:** um por item do pedido, com o **valor líquido do desconto do item**; o **desconto do pedido é rateado** entre os itens (`vDesc`), com a sobra de centavos no último. A soma dos itens da nota é igual ao `valor_total` do pedido. |
| NF3 | **CFOP:** saída 5102 (mesma UF) / 6102 (outra UF); devolução 1202 / 2202. |
| NF4 | **Impostos por item:** base = valor do item − desconto rateado; ICMS pela alíquota da regra acima; PIS 1,65%; COFINS 7,6%; tudo arredondado em 2 casas. Totais da nota = soma dos itens. |
| NF5 | **Chave de acesso (44):** cUF(2) + AAMM(4) + CNPJ(14) + modelo 55(2) + série(3) + número(9) + tpEmis 1(1) + código numérico(8) + DV(1). DV por **módulo 11** (pesos 2 a 9; resto 0 ou 1 → 0), aceitando **CNPJ alfanumérico** (valor do caractere = ASCII − 48). |
| NF6 | **Autorização simulada:** a nota nasce `Autorizada`, com protocolo de 15 dígitos e data/hora; o XML `nfeProc` inclui o `protNFe`. |
| NF7 | **Devolução:** só para devolução cujo pedido tem NF-e autorizada, uma por devolução. Tipo entrada (`tpNF=0`), `finNFe=4`, `refNFe` = chave da nota original; itens = itens devolvidos com o **valor da devolução** e as **mesmas alíquotas** do item na nota original. |
| NF8 | **Imutável:** a nota gravada (itens, totais e XML) não muda depois, mesmo que cliente, produto ou empresa sejam editados. |
| NF9 | **Lista:** filtros por período de emissão, cliente, tipo (Saída/Entrada) e número ou chave; paginada; exporta Excel/PDF pelo `ExportadorRelatorio`. |

## Modelo de dados (1 migration)

- **`empresa`** (nova, 1 linha): razao_social, nome_fantasia, cnpj, inscricao_estadual, logradouro, numero, complemento, bairro, cep, municipio, codigo_municipio (IBGE, 7), uf, telefone, serie_nfe. *(Na implementação, sem `proximo_numero_nfe`: o próximo número é o maior da série + 1, com a linha da empresa travada por `FOR UPDATE` e o índice único (série, número) como garantia.)*
- **`clientes`**: + logradouro, numero, complemento, bairro, cep, codigo_municipio, inscricao_estadual (todos opcionais).
- **`produtos`**: + ncm (opcional, 8 dígitos).
- **`notas_fiscais`** (nova): tipo (Saida/Entrada), numero, serie, chave (única), cliente_id, pedido_id, devolucao_id, nota_referenciada_id, data_emissao, protocolo, destinatário congelado (nome, documento, UF), valor_produtos, valor_desconto, base_icms, valor_icms, valor_pis, valor_cofins, valor_total, xml (texto). Índices únicos: (serie, numero), chave, pedido_id (saída) e devolucao_id.
- **`nota_fiscal_itens`** (nova): produto_id, codigo, descricao, ncm, cfop, unidade, quantidade, valor_unitario, valor_bruto, valor_desconto, base_icms, aliquota_icms, valor_icms, valor_pis, valor_cofins.

## API

| Método | Rota | Descrição |
|---|---|---|
| GET/PUT | `/api/empresa` | Dados do emitente |
| POST | `/api/pedidos/{id}/nfe` | Emite a NF-e de saída (NF1-NF6) |
| POST | `/api/devolucoes/{id}/nfe` | Emite a NF-e de devolução (NF7) |
| GET | `/api/notas-fiscais?dataInicio=&dataFim=&clienteId=&tipo=&busca=&pagina=&tamanhoPagina=&formato=` | Lista (NF9) |
| GET | `/api/notas-fiscais/{id}` | Detalhe com itens |
| GET | `/api/notas-fiscais/{id}/xml` e `/danfe` | Arquivos `NFe{chave}.xml` e `.pdf` |

Os DTOs de pedido e devolução ganham `notaFiscalId`/`chaveNfe` para a tela saber se já há nota.

## Telas

- **Menu Fiscal** (novo grupo): **Notas Fiscais** e **Empresa**.
- **Empresa:** formulário único com validação de CNPJ (reaproveita a do cliente), UF e IBGE.
- **Cliente/Produto:** campos novos no painel de edição (grupo "Dados fiscais").
- **Pedido confirmado:** botão **Emitir NF-e**; erro mostra a lista do que falta. Com nota: número, chave e botões XML e DANFE. "Cancelar" some.
- **Devolução:** botão **Emitir NF-e de devolução** quando o pedido tem nota.
- **Notas Fiscais:** tabela (número, tipo, emissão, destinatário, total, chave) com filtros, detalhe com itens e impostos, XML, DANFE e Excel/PDF.

## Testing strategy

1. **Unitário (xUnit), `NfeCalculo` puro:** CFOP por UF; alíquota interna/interestadual (inclusive 7%); rateio do desconto com sobra; impostos e totais; DV da chave contra chaves reais conhecidas, inclusive com CNPJ alfanumérico; validação de NCM e dos dados obrigatórios.
2. **XML:** gerar para um pedido fixo e conferir os nós principais (`infNFe/@Id`, `ide`, `emit`, `dest`, `det`, `total/ICMSTot`, `protNFe`).
3. **API ponta a ponta (instância temporária na 5099, dados `ZZT…` apagados):** emitir com dados faltando (400 com a lista), emitir (totais = pedido), emitir de novo (409), cancelar o pedido (409), devolução com nota referenciada, lista e filtros, XML/DANFE/Excel/PDF.
4. **Tela:** `tsc -b`, `oxlint`, `npm run build` limpos; teste de tela com Playwright.

## Boundaries

- **Sempre:** reaproveitar `CalculoPedido`, a validação de CNPJ do cliente, `HorarioBrasilia`, `ExportadorRelatorio`, `BotoesExportar` e o QuestPDF; gerar o XML com `System.Xml.Linq` (sem lib nova).
- **Perguntar antes:** qualquer chamada a serviço externo (SEFAZ, ViaCEP); lib nova de NF-e.
- **Nunca:** apresentar a nota como válida (sempre homologação + "SEM VALOR FISCAL"); commitar segredos ou certificados; push sem pedido.

## Success criteria (testáveis)

Conferidos em 28/09/2026; detalhes na seção "NF-e simulada (28/09/2026)" do README.

1. ✅ Emitir sem dados fiscais → 400 listando cada campo que falta; com os dados → nota `Autorizada`, número sequencial, e a soma dos itens = total do pedido.
2. ✅ Chave com 44 dígitos e DV correto (conferido contra o exemplo do manual da NF-e e recalculado de forma independente no E2E); `Id` do XML = `NFe` + chave.
3. ✅ ICMS, PIS e COFINS de cada item e os totais batem com o cálculo manual, para cliente na mesma UF, em outra UF (12%) e em UF de 7%.
4. ✅ Segunda emissão e cancelamento de pedido com nota → 409.
5. ✅ NF-e de devolução com CFOP 1202/2202, `finNFe=4` e `refNFe` da original; valores = devolução.
6. ✅ Editar cliente/produto/empresa depois não altera a nota nem o XML.
7. ✅ Tela: emitir pelo pedido e pela devolução, lista com filtros, baixar XML e DANFE, exportar; testada no navegador.
8. ✅ `dotnet build` 0 avisos, `dotnet test` todo verde, `tsc -b`/`oxlint`/`npm run build` limpos.
