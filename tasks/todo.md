# Tarefas: Módulo Pedidos

Contexto: [SPEC.md](../SPEC.md) · [plan.md](plan.md). Marque `[x]` ao concluir e verificar. Regras: R1 a R9 e "casos de referência" 1 a 6 estão na spec.

Comandos (raiz): `dotnet build ErpPortfolio.slnx` · `dotnet test backend/ErpPortfolio.Tests` · (front, em `frontend/erp-portfolio-web`) `npx tsc -b` · `npx oxlint src`.
"E2E temporário" = instância da API na porta 5099 + script, com dados de teste apagados no fim (como nos módulos anteriores; scripts ficam fora do repositório).

---

## Fase 1: Regras puras (sem banco)

- [x] **T1: Cálculo do pedido + projeto de testes xUnit** (S) — *concluída em 21/09/2026*
  - Descrição: função pura de subtotal do item e total do pedido, mais o projeto `backend/ErpPortfolio.Tests` ligado à solution.
  - Aceite:
    - `CalculoPedido.Subtotal` e `Total` dão os valores dos **6 casos de referência** da spec.
    - Bordas: desconto 0 e 100, quantidade decimal (`1,5`), arredondamento "metade para cima".
    - O projeto de testes está na `ErpPortfolio.slnx`.
  - Verificar: `dotnet test backend/ErpPortfolio.Tests` verde; `dotnet build ErpPortfolio.slnx` com 0 avisos.
  - Resultado: 16 testes passando, 0 avisos; teste "de mutação" (arredondamento bancário e desconto do pedido removido) faz 3 e 4 testes falharem. O projeto de testes alinha `Microsoft.EntityFrameworkCore.Relational` em 10.0.12 (senão MSB3277). Enquanto a API roda, use `-c Release` para build/test.
  - Dependências: nenhuma
  - Arquivos: `backend/ErpPortfolio.Api/Services/CalculoPedido.cs`, `backend/ErpPortfolio.Tests/ErpPortfolio.Tests.csproj`, `backend/ErpPortfolio.Tests/CalculoPedidoTests.cs`, `ErpPortfolio.slnx`

- [x] **T2: Status, forma de pagamento e transições** (S) — *concluída em 21/09/2026*
  - Descrição: enums `StatusPedido` e `FormaPagamento` (texto no JSON) e regras puras R1/R2 de quem pode editar, confirmar e cancelar.
  - Aceite:
    - Só `Rascunho` pode editar e confirmar; `Rascunho` e `Confirmado` podem cancelar; `Cancelado` é final.
    - Enums serializam como texto (`"Rascunho"`, `"Pix"`).
  - Verificar: `dotnet test` (matriz completa de transições); `dotnet build` com 0 avisos.
  - Resultado: 34 testes passando no total, 0 avisos. **Achado para a T5:** o conversor de enums aceita números (`1` vira `Confirmado`, `99` vira valor inexistente), então o DTO precisa validar `formaPagamento` com `[EnumDataType]` (400 para valor inexistente).
  - Dependências: T1 (projeto de testes)
  - Arquivos: `Models/StatusPedido.cs`, `Models/FormaPagamento.cs`, `Services/TransicoesPedido.cs`, `backend/ErpPortfolio.Tests/TransicoesPedidoTests.cs`

### Checkpoint 1: regras puras
- [x] `dotnet test` verde (34 testes) e `dotnet build` sem avisos
- [x] Os 6 casos de referência conferem com a spec
- [ ] Revisão do Rafael antes de seguir

---

## Fase 2: Banco

- [x] **T3: Entidades e mapeamento EF** (S) — *concluída em 21/09/2026*
  - Descrição: `Pedido` e `PedidoItem` com o mapeamento no `ErpPortfolioDbContext` (snake_case, FKs, índices, `CHECK`, enum como texto).
  - Aceite:
    - FKs `RESTRICT` para cliente e produto; `CASCADE` de itens para o pedido.
    - Índices `ix_pedidos_cliente_id`, `ix_pedidos_data_pedido` e único `ux_pedido_itens_pedido_produto`.
    - `CHECK` de quantidade > 0 e descontos de 0 a 100.
  - Verificar: `dotnet build` com 0 avisos.
  - Resultado: teste de modelo em memória (`ModeloPedidoTests`, sem banco) confere tabelas, colunas, FKs, índices e CHECK; 60 testes passando, 0 avisos; mutação (RESTRICT → CASCADE no cliente) faz 1 teste falhar.
  - Dependências: T2
  - Arquivos: `Models/Pedido.cs`, `Models/PedidoItem.cs`, `Data/ErpPortfolioDbContext.cs`

- [x] **T4: Migration `CriacaoTabelasPedidos`** (S) — *concluída em 21/09/2026*
  - Descrição: gerar a migration, testar num banco descartável (subida e volta) e aplicar no de desenvolvimento **com backup**.
  - Aceite:
    - A migration **só cria** `pedidos` e `pedido_itens`; nada em `clientes`, `produtos` e `categorias` muda (esquema comparado antes e depois).
    - `Down` remove só as duas tabelas.
    - Banco de desenvolvimento com as tabelas vazias; cliente, produto e categoria reais intactos.
  - Verificar: script SQL da migration aplicado num banco descartável e revertido; `dotnet ef database update` no dev; contagens e `\d` conferidos.
  - Resultado: a migration só tem 2 `CreateTable` e 4 `CreateIndex` (Down: 2 `DropTable`). Banco descartável: 19 verificações OK (item repetido, quantidade 0, desconto 101/−1, total negativo e estourado, FKs de cliente e produto, cascade, Down). Dev: backup feito, dados reais e esquema de `clientes`/`produtos`/`categorias` **idênticos** antes e depois, `pedidos` e `pedido_itens` vazias. Cuidado ao comparar `pg_dump`: ele escreve linhas `\restrict` aleatórias.
  - Dependências: T3
  - Arquivos: `Data/Migrations/<data>_CriacaoTabelasPedidos.cs`, `.Designer.cs`, `ErpPortfolioDbContextModelSnapshot.cs`

### Checkpoint 2: banco
- [x] Tabelas criadas, esquema das tabelas antigas idêntico, backup feito
- [ ] Revisão do Rafael antes de seguir

---

## Fase 3: API

- [x] **T5: DTOs de entrada e saída com validação** (S) — *concluída em 21/09/2026*
  - Descrição: corpo de POST/PUT (`clienteId`, `formaPagamento?`, `descontoPercentual`, `itens[]`), item de entrada, resposta com itens e totais.
  - Aceite:
    - 400 para: 0 itens, mais de 100, quantidade ≤ 0 ou fora de `0,001–999.999,999` (mais de 3 casas), desconto fora de `0–100` (mais de 2 casas), **produto repetido**.
    - A resposta traz `clienteNome`, `subtotalItens`, `valorTotal` e itens com `produtoNome`, `sku`, `unidade`, `precoUnitario`, `subtotal`.
    - `formaPagamento` com número inexistente (ex.: `99`) ou nome desconhecido retorna 400 (`[EnumDataType]`).
  - Verificar: `dotnet build` com 0 avisos; validação exercitada no E2E da T6.
  - Dependências: T4
  - Arquivos: `DTOs/PedidoCriacaoDto.cs`, `DTOs/PedidoItemEntradaDto.cs`, `DTOs/PedidoRespostaDto.cs`

- [x] **T6: Criar e obter rascunho (POST e GET por id)** (M) — *concluída em 21/09/2026*
  - Descrição: serviço e controller que criam o pedido em `Rascunho`, copiam o preço do produto, calculam e gravam o total.
  - Aceite:
    - 201 com status `Rascunho`, preços copiados de `produtos.preco_venda` e `valorTotal` igual ao esperado (caso 1 = 830,00; caso 2 = 788,50).
    - 400 no campo certo para cliente/produto **inexistente ou inativo** (R5) e **quantidade decimal em produto `UN`/`CX`** (R8).
    - **400 no campo `Itens` quando o total passa de 9.999.999.999,99** (R10), sem erro 500 do banco.
    - Cliente da API nunca consegue definir preço/total.
    - `GET /pedidos/{id}` devolve o pedido com itens; 404 se não existir; enums em texto.
  - Verificar: E2E temporário cobrindo os casos acima; `dotnet build` 0 avisos.
  - Dependências: T5, T1, T2
  - Arquivos: `Services/IPedidoService.cs`, `Services/PedidoService.cs`, `Controllers/PedidosController.cs`, `Program.cs`

- [x] **T7: Listar pedidos** (S) — *concluída em 21/09/2026*
  - Descrição: `GET /pedidos?busca=&status=&pagina=&tamanhoPagina=`, mais recentes primeiro.
  - Aceite:
    - `busca` acha por número do pedido **ou** nome do cliente (sem diferenciar caixa; `%` não vira curinga).
    - Filtro por `status=Confirmado` (por nome); paginação; `tamanhoPagina=101` retorna 400.
    - Resumo traz `clienteNome`, `dataPedido`, `status`, `valorTotal` e quantidade de itens.
  - Verificar: E2E temporário; `dotnet build` 0 avisos.
  - Dependências: T6
  - Arquivos: `DTOs/PedidoFiltroDto.cs`, `DTOs/PedidoResumoDto.cs`, `Services/PedidoService.cs`, `Controllers/PedidosController.cs`

- [x] **T8: Editar rascunho (PUT)** (M) — *concluída em 21/09/2026*
  - Descrição: substitui cliente, itens, desconto e pagamento **só em rascunho**, atualizando os itens no lugar.
  - Aceite:
    - Item cujo produto já estava no pedido **mantém o preço congelado**, mesmo depois de o preço do produto mudar; produto novo copia o preço atual.
    - Itens removidos somem, novos entram, `valorTotal` é recalculado.
    - PUT em pedido confirmado ou cancelado retorna **409** e nada muda.
    - 404 se não existir; mesmas validações do POST.
  - Verificar: E2E temporário (mudar o preço do produto entre POST e PUT); `dotnet build` 0 avisos.
  - Dependências: T6
  - Arquivos: `Services/PedidoService.cs`, `Controllers/PedidosController.cs`

- [x] **T9: Confirmar e cancelar** (S) — *concluída em 21/09/2026*
  - Descrição: `PATCH /pedidos/{id}/confirmar` e `/cancelar` seguindo R1, R6 e R7.
  - Aceite:
    - Confirmar exige forma de pagamento, cliente ativo e produtos ativos (senão 400 no campo); confirmar duas vezes retorna 409.
    - Cancelar funciona em `Rascunho` e `Confirmado`; cancelar de novo retorna 204; cancelado não confirma nem edita (409).
    - Cliente/produto inativados depois continuam aparecendo no pedido antigo.
  - Verificar: E2E temporário; `dotnet test` verde; `dotnet build` 0 avisos.
  - Dependências: T8
  - Arquivos: `Services/PedidoService.cs`, `Controllers/PedidosController.cs`, `Services/IPedidoService.cs`

### Checkpoint 3: API pronta
- [x] Critérios 1 a 9 e 12 da spec verificados por E2E na API real (instância temporária): 117 verificações em 4 scripts, 0 falhas
- [x] `dotnet test` verde (103 testes), `dotnet build` sem avisos
- [x] Dados reais (cliente, produto, categorias) intactos; dados de teste apagados
- [ ] Revisão do Rafael antes de começar a tela

---

## Fase 4: Tela

- [x] **T10: Base do front (tipos, API, hooks, cálculo, tag de status)** (M) — *concluída em 21/09/2026*
  - Descrição: espelho do contrato da API, hooks do TanStack Query, cálculo em **aritmética inteira** com os mesmos 6 casos e a tag de status do pedido.
  - Aceite:
    - `utils/calculoPedido.ts` reproduz exatamente os 6 casos de referência (verificado por script Node com o arquivo real).
    - Tag com Rascunho, Confirmado e Cancelado no tema Ambition.
    - Hooks invalidam o cache `pedidos` nas mutações.
  - Verificar: `npx tsc -b`, `npx oxlint src`, script Node dos 6 casos.
  - Resultado: `calculoPedido.ts` confere 33 casos (os 6 de referência + bordas, `1,005` que o float erra, limite e entradas vazias). **Além do pedido:** teste de **paridade** com o servidor (2.010 pedidos com semente fixa calculados pelo front em `backend/ErpPortfolio.Tests/Dados/paridade-calculo.json`, exigidos idênticos pelo `CalculoPedido.cs`; com o arredondamento do servidor quebrado o teste acusa 274 divergências). Contrato conferido contra a API real (13 verificações: campos de `Pedido`, `PedidoItem`, `PedidoResumo`, uniões de status e forma, corpo aceito com `formaPagamento: null`). 105 testes xUnit, `tsc` e `oxlint` limpos.
  - Dependências: T5 (contrato)
  - Arquivos: `types/pedido.ts`, `api/pedidosApi.ts`, `hooks/usePedidos.ts`, `utils/calculoPedido.ts`, `components/TagStatusPedido.tsx`

- [x] **T11: Lista de pedidos, menu e rotas** (M) — *concluída em 21/09/2026*
  - Descrição: item **Pedidos** no menu, rotas `/pedidos`, `/pedidos/novo` e `/pedidos/:id`, e a lista com filtro Filtrar (busca + status) e tags removíveis.
  - Aceite:
    - Tabela com Nº, Cliente, Data, Itens, Total, Status e ação Abrir; paginação no servidor.
    - Breadcrumb e item do menu acompanham a rota; `/pedidos/novo` e `/pedidos/:id` abrem uma página provisória.
    - Total em reais e data em `pt-BR`.
  - Verificar: `tsc`, `oxlint`; abrir a tela com pedidos criados pela API e conferir busca, status e paginação.
  - Resultado: 28 verificações no navegador (Edge, Playwright) com API e Vite temporários e 3 pedidos de teste (rascunho, confirmado, cancelado): ordem (mais recentes primeiro), conteúdo das linhas, data em pt-BR, tags e contador do Filtrar, filtro por status e por número (com e sem #), mensagem sem resultado, navegação (menu, breadcrumb com link, Novo pedido, Abrir, Voltar, URL direta) e celular (cartões, sem rolagem horizontal, gaveta do menu). 0 falhas, sem erro no console. O menu e o breadcrumb passaram a valer nas subpáginas.
  - Dependências: T10, T7
  - Arquivos: `pages/Pedidos/PedidosListaPage.tsx`, `App.tsx`, `pages/Pedidos/PedidoPage.tsx` (provisória)

- [x] **T12: Seleção de cliente e de produto com busca no servidor** (M) — *concluída em 21/09/2026*
  - Descrição: dois componentes de seleção que consultam a API enquanto o usuário digita (só ativos, 20 resultados, debounce).
  - Aceite:
    - Digitar filtra no servidor sem recarregar tudo; mostra nome (e documento/SKU) e devolve id.
    - A opção de produto carrega `precoVenda` e `unidade` para o cálculo em tela.
    - Sem resultado: mensagem clara.
  - Verificar: `tsc`, `oxlint`; conferido dentro da página na T13.
  - Resultado: 24 verificações no navegador (Edge, Playwright) numa área de teste provisória de `/pedidos/novo`: só ativos (o inativo não aparece), nome + CPF formatado e SKU + preço + unidade nas opções, digitar rápido gera 1 consulta (`nome=`/`busca=`, `ativo=true`, `tamanhoPagina=20`), busca por SKU, `onChange` devolve o cadastro inteiro, o nome escolhido permanece no campo, produto já no pedido fica desabilitado (o do próprio campo não), mensagem sem resultado e celular sem rolagem horizontal. 0 falhas, sem erro no console. O teste achou um defeito: o escolhido aparecia como resultado de qualquer busca; corrigido com `labelInValue`. A área de teste some na T13.
  - Dependências: T10
  - Arquivos: `components/SelecaoCliente.tsx`, `components/SelecaoProduto.tsx`, `hooks/useBuscaCadastros.ts`

- [x] **T13: Página do pedido: novo, itens e total ao vivo** (M) — *concluída em 21/09/2026*
  - Descrição: formulário com cliente, tabela de itens editável (produto, quantidade, preço só leitura, desconto, subtotal), desconto do pedido, forma de pagamento e resumo.
  - Aceite:
    - Quantidade aceita decimais só para `KG`/`L`/`M`; `UN`/`CX` só inteiros.
    - Produto repetido soma a quantidade em vez de criar outra linha.
    - O total muda ao editar e, ao **Salvar rascunho**, passa a ser o do servidor sem diferença; erros 400 aparecem nos campos.
  - Verificar: `tsc`, `oxlint`; Playwright: montar o pedido do caso 1 (830,00) e do caso 2 (788,50).
  - Resultado: 36 verificações no navegador (Edge, Playwright), 0 falhas: caso 1 (R$ 830,00) e caso 2 (R$ 788,50) montados pela tela, produto repetido soma a quantidade, KG aceita 1,5 e UN vira inteiro, remover linha, quantidade vazia e total acima do limite bloqueiam o envio (a API nem é chamada), salvar cria o rascunho e abre `/pedidos/{nº}` com o total IGUAL ao do servidor, preço congelado após o produto mudar de preço (item novo entra com o preço atual, PUT mantém o antigo), erro 400 do servidor no campo Cliente (cliente inativado depois de escolhido), Confirmado/Cancelado abrem somente leitura, "Pedido não encontrado" (número inexistente e inválido) e celular sem rolagem horizontal na página. Dados reais idênticos ao início. **Adiantado da T14:** abrir `/pedidos/:id`, editar o rascunho (PUT), somente leitura e "Pedido não encontrado"; na T14 faltam Confirmar, Cancelar e o fluxo completo. A área de teste da T12 foi removida da página.
  - Dependências: T11, T12
  - Arquivos: `pages/Pedidos/PedidoPage.tsx`, `pages/Pedidos/ItensPedidoTabela.tsx`, `schemas/pedidoSchema.ts`

- [x] **T14: Página do pedido: editar rascunho, confirmar e cancelar** (M) — *concluída em 21/09/2026*
  - Nota: a T13 já entregou abrir `/pedidos/:id`, editar o rascunho, somente leitura e "Pedido não encontrado"; aqui faltam **Confirmar** e **Cancelar** e o teste do fluxo completo.
  - Descrição: abrir `/pedidos/:id`, editar o rascunho, **Confirmar pedido** (salva pendências e confirma) e **Cancelar pedido**, com somente leitura nos demais status.
  - Aceite:
    - Rascunho: salvar, confirmar (com confirmação) e cancelar; erro de confirmação aparece no campo.
    - Confirmado: sem botão de salvar, tudo desabilitado, ainda pode cancelar.
    - Cancelado: somente leitura. Id inexistente mostra mensagem "Pedido não encontrado".
  - Verificar: `tsc`, `oxlint`; Playwright do fluxo completo (critérios 10 e 11).
  - Resultado: 30 verificações no navegador (Edge, Playwright), 0 falhas: pedido novo só tem Salvar rascunho; rascunho salvo tem Cancelar pedido, Salvar rascunho e Confirmar pedido; Confirmar abre janela (Voltar não envia nada); confirmar salva a edição pendente (PUT) e depois confirma (PATCH), servidor Confirmado com total R$ 1.145,00 igual ao da tela; Confirmado fica travado e só tem Cancelar pedido; cancelar Confirmado e cancelar Rascunho; erros do servidor no campo certo (sem forma de pagamento, cliente inativado, produto inativado) com o pedido seguindo Rascunho; pedido cancelado por fora + confirmar na tela = 409, a tela recarrega e mostra Cancelado; lista mostra os novos status; celular com os 3 botões inteiros e sem rolagem horizontal. Dados reais idênticos ao início.
  - Dependências: T13, T9
  - Arquivos: `pages/Pedidos/PedidoPage.tsx`, `pages/Pedidos/ItensPedidoTabela.tsx`

### Checkpoint 4: fluxo na tela
- [x] Fluxo criar, salvar, editar, confirmar e cancelar funciona no navegador (testes T13 e T14)
- [x] Total da tela = total do servidor nos casos 1 e 2
- [x] `tsc`, `oxlint`, `dotnet build` (0 avisos), `dotnet test` (105 aprovados) limpos; dados de teste apagados
- [ ] Revisão do Rafael

---

## Fase 5: Fechamento

- [x] **T15: Celular e polimento** (S) — *concluída em 21/09/2026*
  - Descrição: lista em cartões e página do pedido empilhada no celular; ajustes de espaçamento.
  - Aceite: 390 px sem rolagem horizontal na lista e na página; tabela de itens legível; nenhum erro de console além dos esperados (409).
  - Verificar: Playwright em 390 px e 1920 px, com capturas conferidas.
  - Resultado: 20 verificações no navegador (Edge, Playwright), 0 falhas, sem erro no console. No celular (< 768 px) cada item vira um **cartão** (nome, SKU, quantidade, desconto, preço e subtotal juntos, lixeira, erro dentro do cartão); em tela larga segue a tabela. Conferido em 390, 768, 1024 e 1920 px: lista e página do pedido (novo, rascunho, confirmado e cancelado) sem rolagem horizontal; nenhum elemento fora da largura; valores enormes (R$ 9.999.999.999,99 × 2) não estouram o cartão. Espaço vertical entre campos empilhados corrigido. A lista de pedidos já estava certa desde a T11. Os testes T13 e T14 foram rodados de novo após a refatoração (0 falhas).
  - Dependências: T14
  - Arquivos: `pages/Pedidos/ItensPedidoTabela.tsx`, `pages/Pedidos/PedidoPage.tsx`, `pages/Pedidos/pedido.css`

- [x] **T16: README, graphify e verificação final** (S) — *concluída em 21/09/2026*
  - Descrição: documentar o módulo, regravar o grafo e conferir os 12 critérios da spec.
  - Aceite:
    - README com funcionalidades, API, banco, decisões, estrutura e testes de Pedidos; `SPEC.md` marcada como implementada.
    - Graphify atualizado sem perder conceitos/hiperarestas; comparação com o backup feita.
    - Os **12 critérios de sucesso** conferidos um a um; registros reais intactos.
  - Verificar: `dotnet build`, `dotnet test`, `tsc`, `oxlint`, E2E API e tela completos, `git status` revisado (sem segredos).
  - Resultado: README.md ganhou a seção "etapa 3 — Pedidos" (funcionalidades, endpoints, fórmula do cálculo, tabelas `pedidos`/`pedido_itens`, 10 decisões técnicas e os resultados de T10-T15) e a estrutura de pastas foi atualizada; `SPEC.md` marcada como implementada, com os 12 critérios de sucesso conferidos um a um e a evidência (teste ou script) apontada em cada um — rodada de novo nesta etapa: `e2e-pedidos-t6..t9.ps1` e `e2e-pedidos-contrato.ps1` (0 falhas cada, dados reais idênticos), `check-calculo-front.mts` (6 casos de referência + 13 casos extras, todos conferem). Build final: `dotnet build -c Release` 0 avisos, `dotnet test` 105/105, `tsc -b` e `oxlint` limpos. Grafo atualizado (67 conceitos novos, comunidades renomeadas, sem perder hiperarestas). `git status` revisado antes de cada commit: sem `bin/obj`, sem segredos, `.gitattributes` fora do commit.
  - Dependências: T15
  - Arquivos: `README.md`, `SPEC.md`

### Checkpoint 5: pronto
- [x] Todos os critérios da spec atendidos
- [x] Commitado a pedido do Rafael ("pode commitar e fazer todas tarefas sem precisar ficar pedindo"); pronto para revisão
