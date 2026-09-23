# Checklist: Módulo Vendedores (etapa 10)

> Origem: [plan.md](plan.md). Marcar `[x]` ao concluir cada tarefa e registrar o resultado.

## Fase 1: Backend

- [x] **T1: Model + migration + validação de CPF** (S) — *concluída em 23/09/2026*
  - `Models/Vendedor.cs`; `Pedido` ganha `VendedorId`/`Vendedor`/`PercentualComissao`; mapeamento; migration `AdicionaVendedores`; `DTOs/Validacoes/CpfAttribute.cs` com xUnit.
  - Verificar: build 0 avisos; migration aplicada; `dotnet test` verde.
  - Resultado: migration `20260923163031_AdicionaVendedores` aplicada no banco de dev (tabela `vendedores`, `pedidos.vendedor_id` e `pedidos.percentual_comissao`, ambas nullable, com FK/índice/CHECKs); os 3 pedidos existentes continuam, sem vendedor. `[Cpf]` recusa CNPJ; `dotnet test` 165/165 (7 novos em `VendedorValidacaoTests`). Build 0 avisos.

- [x] **T2: Vendedor backend** (M) — *concluída em 23/09/2026*
  - DTOs (criação, atualização, filtro, resposta), `IVendedorService`/`VendedorService` (espelho de `FornecedorService`, CPF único), `VendedoresController`, DI.
  - Verificar: build; E2E na T3.
  - Resultado: DTOs em `DTOs/VendedorDtos.cs`, `VendedorService` (CPF normalizado e único, mensagem de reativar se o duplicado estiver inativo), `VendedoresController` gerado do de Fornecedores, DI e descrição do Swagger. 7 testes a mais (% de comissão 0-100 com 2 casas; CNPJ recusado no DTO): `dotnet test` 172/172. Build 0 avisos. CRUD verificado por E2E na T3.

- [x] **T3: Vendedor no pedido de venda** (M) — *concluída em 23/09/2026*
  - `PedidoCriacaoDto.VendedorId`; salvar valida vendedor ativo (PV1); confirmar exige vendedor ativo e grava a % (PV2/PV3); `PedidoRespostaDto` com vendedor e %.
  - Verificar: E2E dos critérios 1-5 contra instância temporária.
  - Resultado: E2E (instância temporária na 5099, dados `ZZT…` apagados ao final; 3 pedidos reais intactos): 19 verificações OK — CRUD do vendedor (CPF com máscara gravado só com dígitos, CPF repetido 409, CNPJ 400, 100,01% 400, filtro, edição, reativação pelo PUT); rascunho com e sem vendedor; vendedor inexistente/inativo → 400 em `VendedorId`; confirmar sem vendedor ou com vendedor inativado → 400 e continua Rascunho; confirmar grava 5,5% e mudar a % do vendedor para 9 não altera o pedido (o próximo pega 9); pedido antigo abre sem vendedor.

## Fase 2: Frontend

- [x] **T4: Tela Vendedores** (M) — *concluída em 23/09/2026*
  - Tipos, api, hooks, schema Zod, lista + drawer (espelho de Fornecedores, sem cidade/UF, com %), item **Vendedores** em Cadastro.
  - Verificar: `tsc -b`, `oxlint`.
  - Resultado: `types/vendedor.ts`, `api/vendedoresApi.ts`, `hooks/useVendedores.ts`, `schemas/vendedorSchema.ts` (só CPF; % 0-100 com 2 casas), `pages/Vendedores/` (lista e drawer gerados a partir dos de Fornecedores: sem cidade/UF, com coluna e campo de comissão `InputNumber` com `%`). Item **Vendedores** em Cadastro (`SolutionOutlined`), rota `/vendedores`. `tsc -b` e `oxlint` limpos.

- [x] **T5: Vendedor no pedido de venda** (S) — *concluída em 23/09/2026*
  - `SelecaoVendedor`; campo no `PedidoPage` + schema; erro da API no campo; confirmado mostra vendedor e % congelada.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.
  - Resultado: `SelecaoVendedor` (gerado do `SelecaoFornecedor`, com CPF na opção e `aoLimpar`), `useBuscaVendedores`; campo **Vendedor** no `PedidoPage` (placeholder "Obrigatório só para confirmar"; erro `vendedorId` da API vai para o campo); no pedido confirmado aparece "Comissão do vendedor: X% (congelada na confirmação)". Tipos e schema do pedido com `vendedorId`. `tsc -b`, `oxlint` e `npm run build` limpos.

## Fase 3: Fechamento

- [ ] **T6: Documentação e verificação final** (S)
  - README (etapa 10), SPEC marcada como implementada, `python -m graphify update .`, memória.
