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

- [ ] **T3: Vendedor no pedido de venda** (M)
  - `PedidoCriacaoDto.VendedorId`; salvar valida vendedor ativo (PV1); confirmar exige vendedor ativo e grava a % (PV2/PV3); `PedidoRespostaDto` com vendedor e %.
  - Verificar: E2E dos critérios 1-5 contra instância temporária.

## Fase 2: Frontend

- [ ] **T4: Tela Vendedores** (M)
  - Tipos, api, hooks, schema Zod, lista + drawer (espelho de Fornecedores, sem cidade/UF, com %), item **Vendedores** em Cadastro.
  - Verificar: `tsc -b`, `oxlint`.

- [ ] **T5: Vendedor no pedido de venda** (S)
  - `SelecaoVendedor`; campo no `PedidoPage` + schema; erro da API no campo; confirmado mostra vendedor e % congelada.
  - Verificar: `tsc -b`, `oxlint`, `npm run build`.

## Fase 3: Fechamento

- [ ] **T6: Documentação e verificação final** (S)
  - README (etapa 10), SPEC marcada como implementada, `python -m graphify update .`, memória.
