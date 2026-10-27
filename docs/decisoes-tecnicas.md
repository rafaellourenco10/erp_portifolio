# Decisões técnicas

> Parte da documentação do [Ambition ERP](../README.md).


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
| Vite com `strictPort` na 5180 | É a origem liberada no CORS; se a porta estiver ocupada, o Vite avisa em vez de trocar de porta. |
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

