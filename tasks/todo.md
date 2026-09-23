# Checklist: Orçamentos (etapa 13)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Schema** (M) — *concluída em 23/09/2026*
  - `Orcamento`, `OrcamentoItem`, `StatusOrcamento` (Aberto/Aprovado/Perdido); mapeamento no DbContext (CHECK de status, único em `pedido_id` e em (`orcamento_id`, `produto_id`)); migration `CriacaoTabelasOrcamentos`.
  - Verificar: build 0 avisos; migration aplicada; tabelas e índices no banco.
  - Resultado: migration `20260923235828_CriacaoTabelasOrcamentos` aplicada; `\d` confere PK, FKs (restrict; itens cascade), únicos e CHECKs. `ck_orcamentos_status` amarra Aprovado ⇔ `pedido_id` preenchido. `Orcamento.EstaVencido(hoje)` implementa OR4 no model. Build Release 0 avisos (a API do Rafael estava rodando em Debug, por isso migrations/builds em Release).

- [ ] **T2: API básica** (M)
  - DTOs (criação/edição com validade e observações, resposta com `vencido`, resumo, filtro); `OrcamentoService` listar/obter/criar/editar; validações de item compartilhadas com o `PedidoService`; `OrcamentosController`; xUnit (vencido, DTOs, transições).
  - Verificar: build; `dotnet test` (os testes do pedido continuam passando).

- [ ] **T3: Gerar pedido + perder** (M)
  - `POST /{id}/gerar-pedido` (GP1-GP4), `PATCH /{id}/perder` (PE1).
  - Verificar: E2E dos critérios 1-5 contra instância temporária (dados `ZZT…` apagados), incluindo a regressão do pedido.

- [ ] **T4: PDF** (S)
  - `ExportadorOrcamento.GerarPdf` + `GET /{id}/pdf` (PD1).
  - Verificar: 200 `application/pdf`; abrir o arquivo e conferir o layout.

## Fase 2: Frontend

- [ ] **T5: Tela de Orçamentos** (M)
  - Tipos, `orcamentosApi`, `useOrcamentos`, schema Zod; página com lista, busca, filtro de status (tag Vencido) e formulário em gaveta (reaproveitando `SelecaoCliente`, `SelecaoVendedor`, `ItemFormulario`); item no menu e rota `/orcamentos`.
  - Verificar: `tsc -b`, `oxlint`.

- [ ] **T6: Ações da tela** (S)
  - Gerar pedido (confirmação → link para o pedido), Marcar como perdido (modal com motivo), Baixar PDF, link "Pedido #N" no Aprovado.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T7: Documentação e verificação final** (S)
  - README (etapa 13; Login passa a 14), SPEC marcada como implementada, `python -m graphify update .`, memória.
