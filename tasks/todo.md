# Checklist: NF-e simulada (etapa 16)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: NfeCalculo** (M) — *concluída em 28/09/2026*
  - CFOP (NF3); alíquota interna/interestadual 12%/7%; rateio do desconto do pedido com sobra no último; ICMS/PIS/COFINS por item e totais (NF2, NF4); chave de 44 com DV módulo 11, inclusive CNPJ alfanumérico (NF5); validação de NCM e dos dados obrigatórios (NF1). xUnit.
  - Verificar: `dotnet test`.
  - Resultado: `Services/NfeCalculo.cs` (códigos IBGE e alíquotas internas das 27 UFs, `Cfop`, `AliquotaIcms`, `Itens` com rateio proporcional e sobra no último, `Somar`, `Chave`, `DigitoVerificador`, `NcmValido`). A lista do que falta para emitir fica no serviço (T3), que é quem conhece as entidades. `NfeCalculoTests` (+25, inclusive o DV do exemplo do manual). `dotnet test` 271/271.

- [x] **T2: Banco e cadastros** (M) — *concluída em 28/09/2026*
  - Migration: `empresa`, campos fiscais em `clientes` e `produtos`, `notas_fiscais`, `nota_fiscal_itens`. `GET/PUT /api/empresa`; DTOs e validações de cliente/produto com os campos novos.
  - Verificar: build 0 avisos (Release), migration aplicada, `dotnet test`.
  - Resultado: modelos `Empresa`, `NotaFiscal` (+ `TipoNotaFiscal`), `NotaFiscalItem`; campos em `Cliente` (endereço, CEP, IBGE, IE) e `Produto` (NCM). Migration `AdicionaNotasFiscais` (só adiciona; aplicada). Índices únicos (série, número), chave, pedido da nota de saída (filtrado) e devolução; CK de tipo. **Mudança no plano:** sem coluna `proximo_numero` — o próximo número será o maior da série + 1, com a linha da empresa travada (`FOR UPDATE`) e o índice único como garantia. `EmpresaController` direto no DbContext (sem serviço: não há regra além da validação). `Validacoes/EnderecoFiscal` compartilhado (tira só a máscara, para letras serem recusadas; CEP 8 dígitos; IBGE 7 dígitos começando pelo código da UF). `DadosFiscaisValidacaoTests` (+12). Build 0 avisos, `dotnet test` 283/283.

- [x] **T3: Emissão de saída** (L) — *concluída em 28/09/2026*
  - `NotaFiscalService.EmitirDoPedidoAsync` (validação, numeração na transação, cálculo, gravação), `NfeXml` (nfeProc 4.00 com protNFe), `POST /api/pedidos/{id}/nfe`, 409 ao cancelar pedido com nota; `notaFiscalId`/`chaveNfe` no DTO do pedido.
  - Verificar: xUnit do XML; build.
  - Resultado: `NotaFiscalService` (concreto, sem interface): transação + `SELECT ... FOR UPDATE` na empresa, 409 se não confirmado ou já com nota, `NfePendenteException` com tudo o que falta (empresa, endereço do cliente, NCM de cada produto), número = maior da série + 1, cNF aleatório ≠ nNF, protocolo no formato real (1 + cUF + ano + 10), itens pelo `NfeCalculo` com total = `valor_total` do pedido. `NfeXml` (ide/emit/dest/det/total/transp/pag/infAdic + protNFe; destinatário com o nome de homologação exigido pela SEFAZ; `indIEDest` 1/9; tPag por forma de pagamento; devolução com NFref e tPag 90). `HorarioBrasilia.ComFuso` para o `dhEmi`. `NotasFiscaisController` com `POST /api/pedidos/{id}/nfe` (400 com `errors.Pendencias`). Pedido: detalhe com `notaFiscalId`/`numeroNfe`/`chaveNfe`; cancelar com nota → 409. `NfeXmlTests` (+8). Build 0 avisos, `dotnet test` 291/291.

- [x] **T4: Consulta, XML e DANFE** (M) — *concluída em 28/09/2026*
  - Lista com filtros e paginação (NF9), detalhe, `/xml`, `/danfe` (QuestPDF, "SEM VALOR FISCAL"), Excel/PDF.
  - Verificar: **CP1** — E2E na 5099 (critérios 1-4 e 6 da SPEC), dados `ZZT…` apagados.
  - Resultado: `NotaFiscalService` com `ListarAsync` (período em dias de Brasília, cliente, tipo, busca por número ou trecho da chave; mais recente primeiro), `ModeloAsync` (Excel/PDF pelo `ExportadorRelatorio`), `ObterAsync` (com a chave referenciada) e `ObterXmlAsync`. `ExportadorDanfe` monta o DANFE **só a partir do XML gravado** (emitente/destinatário congelados): cabeçalho com DANFE/número/série/folha, chave em grupos de 4, protocolo, destinatário, cálculo do imposto, produtos, dados adicionais e a marca diagonal "SEM VALOR FISCAL". Sem código de barras da chave (marcado com `ponytail:`). E2E `.claude/ferramentas-locais/e2e-nfe.ps1` (empresa de teste na série 999, linha real restaurada): **34/34** — pendências (empresa, cliente, NCM) sem gravar nada; empresa com IBGE de outra UF → 400; numeração 1-2-3; chave/DV recalculados no script; ICMS/PIS/COFINS conferidos à mão em SP 18%, RJ 12% com desconto no item e BA 7% com rateio do desconto do pedido; XML (Id, protNFe, homologação, IE, idDest, tPag, ICMSTot); DANFE; 409 ao emitir de novo, ao cancelar pedido com nota e em rascunho; 404; editar cliente/produto/empresa não muda a nota (md5 do XML e dos itens); lista, filtros, paginação, 400 e Excel/PDF. Dados de teste apagados, reais idênticos. `dotnet test` 292/292.

- [x] **T5: NF-e de devolução** (S) — *concluída em 28/09/2026*
  - `POST /api/devolucoes/{id}/nfe` (NF7); `notaFiscalId` no DTO da devolução.
  - Verificar: **CP2** — E2E completo (critério 5).
  - Resultado: `NotaFiscalService.EmitirDaDevolucaoAsync` (mesmo núcleo `MontarAsync`): 409 se o pedido não tem NF-e de saída ou a devolução já tem nota; código, descrição, NCM, unidade e alíquota vêm da **nota original** (o cadastro pode ter mudado); valores da devolução; CFOP 1202/2202 pela UF do destinatário original; `NotaReferenciadaId` + `refNFe`. `DevolucaoRespostaDto` com `notaFiscalId`/`numeroNfe`. E2E com `-Devolucao` (`e2e-nfe-devolucao.ps1`): **47/47** — devolução de 95,00 da venda BA com CFOP 2202, ICMS 6,65, PIS 1,57, COFINS 7,22, nome e NCM de antes da edição do produto, XML com tpNF 0/finNFe 4/refNFe/tPag 90, DANFE, 409 repetida, 404, nota na lista de devoluções e no filtro Entrada, 409 para venda sem NF-e. DANFE conferido visualmente (CEP com máscara, bordas). Dados de teste apagados, reais idênticos.

## Fase 2: Frontend

- [ ] **T6: Telas de cadastro** (M)
  - Menu Fiscal; tela Empresa; grupo "Dados fiscais" em Cliente e Produto.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

- [ ] **T7: Telas de emissão** (L)
  - Botão Emitir NF-e no pedido (lista do que falta no erro; XML/DANFE depois), botão na devolução, tela Notas Fiscais (filtros, detalhe, downloads, exportar).
  - Verificar: critério 7 com Playwright.

## Fase 3: Fechamento

- [ ] **T8: Fechamento** (S)
  - README (etapa 16, login vira 17), SPEC marcada como implementada, `graphify update`, memória.
  - Verificar: critério 8.
