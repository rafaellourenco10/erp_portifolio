# Checklist: Orçamentos (etapa 13)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Schema** (M) — *concluída em 23/09/2026*
  - `Orcamento`, `OrcamentoItem`, `StatusOrcamento` (Aberto/Aprovado/Perdido); mapeamento no DbContext (CHECK de status, único em `pedido_id` e em (`orcamento_id`, `produto_id`)); migration `CriacaoTabelasOrcamentos`.
  - Verificar: build 0 avisos; migration aplicada; tabelas e índices no banco.
  - Resultado: migration `20260923235828_CriacaoTabelasOrcamentos` aplicada; `\d` confere PK, FKs (restrict; itens cascade), únicos e CHECKs. `ck_orcamentos_status` amarra Aprovado ⇔ `pedido_id` preenchido. `Orcamento.EstaVencido(hoje)` implementa OR4 no model. Build Release 0 avisos (a API do Rafael estava rodando em Debug, por isso migrations/builds em Release).

- [x] **T2: API básica** (M) — *concluída em 23/09/2026*
  - DTOs (criação/edição com validade e observações, resposta com `vencido`, resumo, filtro); `OrcamentoService` listar/obter/criar/editar; validações de item compartilhadas com o `PedidoService`; `OrcamentosController`; xUnit (vencido, DTOs, transições).
  - Verificar: build; `dotnet test` (os testes do pedido continuam passando).
  - Resultado: `DTOs/OrcamentoDtos.cs` (itens de entrada/resposta reaproveitam `PedidoItemEntradaDto`/`PedidoItemRespostaDto`); `OrcamentoService` com filtro Aberto/Vencido/Aprovado/Perdido e `vencido` calculado na projeção; validade ≥ hoje no serviço (400 no campo `Validade`); edição só do Aberto (409), itens no lugar como no pedido. `PedidoService` 1.9: helpers de cliente/vendedor/produto/quantidade viraram `internal static` (sem mudar comportamento). Transições por status ficam para o E2E da T3 (dependem do banco). `OrcamentoTests` (+18): `dotnet test` 203/203, build 0 avisos.

- [x] **T3: Gerar pedido + perder** (M) — *concluída em 23/09/2026*
  - `POST /{id}/gerar-pedido` (GP1-GP4), `PATCH /{id}/perder` (PE1).
  - Verificar: E2E dos critérios 1-5 contra instância temporária (dados `ZZT…` apagados), incluindo a regressão do pedido.
  - Resultado: `GerarPedidoAsync` monta o `Pedido` Rascunho com os preços/descontos do orçamento e aprova o orçamento num `SaveChanges` só (201 + `Location: /api/pedidos/N`); `PerderAsync` idempotente sem trocar o motivo. Sem trava de concorrência (anotado com `ponytail:`, como o resto do ERP). E2E (API temporária 5099, `.claude/ferramentas-locais/e2e-orcamentos.ps1`): **51/51** — criar/validar (validade passada/hoje, itens, UN fracionada, observações, cliente inativo), editar com preço congelado e item novo com preço atual, vencido/prorrogar, filtros Aberto/Vencido/Aprovado/Perdido e busca `#N`, gerar pedido (produto inativo → 400 sem gravar; pedido com preço 100 com o produto a 130; total igual; Aprovado + `pedidoId`; de novo 409; vendedor inativo em branco; cliente inativado 400), pedido gerado confirma (2 saídas de estoque, 2 parcelas somando o total), perder (motivo aparado, idempotente, sem corpo, 201 caracteres 400, vencido ok, Aprovado 409). Dados de teste apagados; dados reais e contagens idênticos.

- [x] **T4: PDF** (S) — *concluída em 23/09/2026*
  - `ExportadorOrcamento.GerarPdf` + `GET /{id}/pdf` (PD1).
  - Verificar: 200 `application/pdf`; abrir o arquivo e conferir o layout.
  - Resultado: `ExportadorOrcamento` (QuestPDF, A4 retrato): cabeçalho com nº, data (Brasília), "Válido até" e a situação quando não é um aberto válido (Aprovado com nº do pedido / Perdido / Vencido); quadro do cliente (CPF/CNPJ formatado, e-mail · telefone, cidade/UF, vendedor); tabela com SKU sob o nome; subtotal, desconto (só se houver) e total; forma de pagamento; observações; rodapé "Orçamento válido até … · Página X de Y". `GET /api/orcamentos/{id}/pdf` → `orcamento-N.pdf`. E2E com `-pdf`: **54/54** (200, `%PDF`, nome do arquivo, 404); PDF aberto e conferido (totais batem com a tela). +4 xUnit de máscara CPF/CNPJ (inclusive alfanumérico): 207/207, build 0 avisos.

## Fase 2: Frontend

- [x] **T5: Tela de Orçamentos** (M) — *concluída em 23/09/2026*
  - Tipos, `orcamentosApi`, `useOrcamentos`, schema Zod; página com lista, busca, filtro de status (tag Vencido) e formulário em gaveta (reaproveitando `SelecaoCliente`, `SelecaoVendedor`, `ItemFormulario`); item no menu e rota `/orcamentos`.
  - Verificar: `tsc -b`, `oxlint`.
  - Resultado: o formulário virou **página** (`/orcamentos/novo`, `/orcamentos/:id`) em vez de gaveta, igual ao pedido, que é o padrão da casa para cabeçalho + itens. `orcamentoSchema` = `pedidoSchema.safeExtend({ validade, observacoes })` (mantém o limite do total); conversões reaproveitam as do pedido (`paraFormulario` do pedido passou a aceitar um `Pick`). A tabela de itens é a `ItensPedidoTabela` (cast comentado do `control`). Lista com colunas Validade e Status (tag + link "Pedido #N" no aprovado), filtro com Vencido; `TagStatusOrcamento` (Aberto azul, Vencido laranja `--cor-alerta`, Aprovado verde, Perdido cinza). Menu "Orçamentos" (ícone `FileTextOutlined`) antes de Pedidos de Venda, rotas e breadcrumb. `tsc -b` e `oxlint` limpos.

- [ ] **T6: Ações da tela** (S)
  - Gerar pedido (confirmação → link para o pedido), Marcar como perdido (modal com motivo), Baixar PDF, link "Pedido #N" no Aprovado.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T7: Documentação e verificação final** (S)
  - README (etapa 13; Login passa a 14), SPEC marcada como implementada, `python -m graphify update .`, memória.
