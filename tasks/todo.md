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

- [ ] **T3: Emissão de saída** (L)
  - `NotaFiscalService.EmitirDoPedidoAsync` (validação, numeração na transação, cálculo, gravação), `NfeXml` (nfeProc 4.00 com protNFe), `POST /api/pedidos/{id}/nfe`, 409 ao cancelar pedido com nota; `notaFiscalId`/`chaveNfe` no DTO do pedido.
  - Verificar: xUnit do XML; build.

- [ ] **T4: Consulta, XML e DANFE** (M)
  - Lista com filtros e paginação (NF9), detalhe, `/xml`, `/danfe` (QuestPDF, "SEM VALOR FISCAL"), Excel/PDF.
  - Verificar: **CP1** — E2E na 5099 (critérios 1-4 e 6 da SPEC), dados `ZZT…` apagados.

- [ ] **T5: NF-e de devolução** (S)
  - `POST /api/devolucoes/{id}/nfe` (NF7); `notaFiscalId` no DTO da devolução.
  - Verificar: **CP2** — E2E completo (critério 5).

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
