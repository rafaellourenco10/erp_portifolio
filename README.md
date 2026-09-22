# Ambition ERP

ERP comercial desenvolvido como projeto de portfólio, com back-end em **ASP.NET Core** e front-end em **React**, em tema escuro próprio.
O projeto é evoluído por módulos, todos dentro da área **Gestão Comercial** do menu: **Clientes** (etapa 1), **Produtos e Categorias** (etapa 2), **Pedidos** (etapa 3), que liga cliente e produtos numa venda com itens, desconto e total calculado, e **Estoque** (etapa 4), que baixa e devolve saldo automaticamente a partir dos pedidos.

| Etapa | Módulo | Situação |
|---|---|---|
| 1 | Clientes | Concluída e testada de ponta a ponta (18/09/2026) |
| 1.1 | Tema visual Ambition ERP + filtros por UF e status | Concluída e testada (18/09/2026) |
| 2 | Produtos (+ navegação por rotas no menu) | Concluída e testada (21/09/2026) |
| 2.1 | Categorias (cadastro próprio, escolhido por seleção no produto) | Concluída e testada (21/09/2026) |
| 3 | Pedidos (cliente, itens, desconto, total calculado, confirmar/cancelar) | Concluída e testada (21/09/2026) |
| 4 | Estoque (movimentações, entrada manual, baixa/estorno automáticos) | Back-end testado de ponta a ponta; tela não verificada visualmente (22/09/2026) |
| 5+ | Login | Planejada |

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

- **Navegação por rotas** (React Router): o menu lateral troca a tela por URL (`/clientes`, `/produtos`); a página aberta sobrevive ao recarregar e uma rota desconhecida cai em `/clientes`
- Cadastro com **nome**, **SKU** (número de série do produto: único e somente dígitos; o campo da tela nem deixa digitar letras), **categoria** (opcional, escolhida numa seleção com as categorias cadastradas), **unidade** (UN, KG, L, M ou CX), **preço de venda** e **custo** (obrigatórios, em reais, com 2 casas decimais)
- Listagem com paginação no servidor, **busca por nome ou SKU** e filtro de status (mesmo painel **Filtrar** e tags removíveis de Clientes)
- Coluna **Margem** calculada na tela: (preço − custo) ÷ preço; margem negativa aparece em vermelho
- **Inativação** (exclusão lógica) e reativação pela edição, como em Clientes
- **SKU único**: repetir um SKU retorna **409**; se o produto existente estiver **inativo**, a mensagem orienta a reativá-lo em vez de criar outro
- Um `PUT` sem o campo `ativo` **mantém** o status atual do produto

## Funcionalidades (etapa 2.1 — Categorias)

- Tela **Categorias** no menu (Gestão Comercial): cadastro só com o **nome**, busca por nome, filtro Todas / Ativas / Inativas, edição e inativação
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
- Tela **Estoque** (Gestão Comercial): tabela com produto, SKU, unidade e **saldo**, busca por nome ou SKU
- **Nova entrada** (compra/ajuste): produto (seleção com busca no servidor), quantidade e motivo em texto livre, opcional
- **Ver movimentações**: extrato paginado de um produto, mais recente primeiro, com tipo, quantidade, motivo/origem e data
- **Confirmar um pedido baixa o estoque** automaticamente: uma saída por item, com a quantidade do item
- Confirmar um pedido **sem saldo suficiente é recusado** (400 no campo, como cliente/produto inativo); nenhuma movimentação é gravada, nem a dos itens que tinham saldo
- **Cancelar um pedido que estava Confirmado devolve o estoque** automaticamente (uma entrada de estorno por item); cancelar um rascunho que nunca foi confirmado não mexe em estoque
- Saldo negativo (não deveria acontecer, dado o bloqueio acima) aparece em vermelho na lista, mesmo padrão da margem negativa em Produtos
- Fora do escopo por enquanto: fornecedores, pedido de compra, saída manual (perda/ajuste), múltiplos depósitos, estoque mínimo/alerta

## Stack e versões

| Camada | Tecnologia | Versão |
|---|---|---|
| Runtime / SDK | .NET SDK | 10.0 |
| Back-end | ASP.NET Core Web API (Controllers) | net10.0 |
| ORM | Entity Framework Core + Npgsql | EF Core 10.0.12 / Npgsql EF 10.0.3 |
| Ferramenta de migrations | dotnet-ef (ferramenta local) | 10.0.12 |
| Swagger | Swashbuckle.AspNetCore | 10.2.3 |
| Banco | PostgreSQL (Docker, imagem `postgres:18-alpine`) | 18 |
| Front-end | React + TypeScript | React 19 / TS 6 |
| Build do front | Vite | 8 |
| Componentes | Ant Design + @ant-design/icons | 6 |
| Chamadas à API | TanStack Query + Axios | 5 / 1 |
| Formulários | React Hook Form + Zod + @hookform/resolvers | 7 / 4 / 5 |
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
│       │   └── EstoqueController.cs         # endpoints REST de estoque
│       ├── Models/
│       │   ├── Cliente.cs                   # entidade
│       │   ├── Produto.cs                   # entidade (CategoriaId + navegação)
│       │   ├── Categoria.cs                 # entidade
│       │   ├── Pedido.cs / PedidoItem.cs    # entidades (itens ligados por FK, preco congelado)
│       │   ├── StatusPedido.cs              # enum: Rascunho, Confirmado, Cancelado
│       │   ├── FormaPagamento.cs            # enum: Dinheiro, Pix, Boleto, Cartao
│       │   ├── EstoqueMovimentacao.cs       # entidade (FK produto e pedido opcional)
│       │   └── TipoMovimentacao.cs          # enum: Entrada, Saida
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
│       │   ├── EstoqueService.cs            # saldo, extrato, entrada manual, baixa/estorno (usados pelo PedidoService)
│       │   └── EstoqueCalculo.cs            # saldo = Σ Entrada − Σ Saída, funcao pura (sem banco)
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
            │   └── TagStatus.tsx/.css       # tag Ativo/Inativo
            ├── api/
            │   ├── axiosClient.ts           # instância do Axios + leitura de ProblemDetails
            │   ├── clientesApi.ts           # chamadas da API de clientes
            │   ├── produtosApi.ts           # chamadas da API de produtos
            │   ├── categoriasApi.ts         # chamadas da API de categorias
            │   └── estoqueApi.ts            # chamadas da API de estoque
            ├── hooks/
            │   ├── useClientes.ts           # useQuery / useMutation
            │   ├── useProdutos.ts
            │   ├── useCategorias.ts         # inclui as categorias ativas do seletor de produtos
            │   └── useEstoque.ts            # lista com saldo, extrato por produto, entrada manual
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
            ├── pages/Pedidos/
            │   ├── PedidosListaPage.tsx     # filtro (número/cliente + status), tabela, paginação
            │   ├── PedidoPage.tsx           # formulário: cliente, itens, desconto, resumo, ações
            │   ├── ItensPedidoTabela.tsx    # tabela de itens (tela larga) / cartões (celular)
            │   └── pedido.css               # estilos da página do pedido
            ├── pages/Estoque/
            │   ├── EstoqueListaPage.tsx     # busca, tabela com saldo, paginação, ações
            │   ├── EntradaEstoqueDrawer.tsx # painel de nova entrada manual (produto, quantidade, motivo)
            │   └── MovimentacoesDrawer.tsx  # painel de extrato paginado de um produto
            ├── schemas/
            │   ├── clienteSchema.ts         # schema Zod do formulário de cliente
            │   ├── produtoSchema.ts         # schema Zod do formulário de produto
            │   ├── categoriaSchema.ts       # schema Zod do formulário de categoria
            │   ├── pedidoSchema.ts          # schema Zod do formulário de pedido + conversões form/API
            │   └── estoqueEntradaSchema.ts  # schema Zod do formulário de entrada de estoque
            ├── types/
            │   ├── cliente.ts               # tipos (espelham os DTOs)
            │   ├── produto.ts
            │   ├── categoria.ts
            │   ├── pedido.ts
            │   ├── estoque.ts
            │   └── paginacao.ts             # ResultadoPaginado compartilhado
            ├── components/SelecaoCliente.tsx / SelecaoProduto.tsx  # seleção com busca no servidor (usadas no pedido)
            ├── components/TagStatusPedido.tsx                      # tag Rascunho/Confirmado/Cancelado
            ├── hooks/useBuscaCadastros.ts    # busca com debounce para os seletores acima
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
| GET | `/produtos?busca=&ativo=&pagina=1&tamanhoPagina=10` | Lista paginada, ordenada por nome; `busca` procura no nome **ou** no SKU | 200, 400 |
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

**Limitação desta rodada:** sem ferramenta de navegador/Playwright disponível na sessão em que o módulo foi construído, a tela (lista, drawer de entrada, drawer de extrato, layout no celular) **não foi verificada visualmente** — só o back-end foi testado de ponta a ponta contra a API real. O código da tela segue os mesmos padrões (componentes, responsividade) já comprovados nos módulos anteriores, mas recomenda-se um teste manual antes de considerar a etapa 4 totalmente fechada.

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
- Dashboard com indicadores, agora que Pedidos existe
- Filtro por **categoria** na lista de Produtos e atalho "Nova categoria" dentro do seletor do formulário
- Filtro por **cliente** e por **faixa de data** na lista de Pedidos
- Editar a **forma de pagamento** de um pedido já confirmado (hoje só dá para cancelar e criar outro)
- Contas a receber, a partir dos pedidos confirmados
- Verificação visual/Playwright da tela de Estoque (lista, drawers, celular) — não feita na sessão que construiu o módulo
- Saída manual de estoque (perda/quebra/ajuste); fornecedores e pedido de compra; estoque mínimo/alerta de ruptura
- Mover `clientes.css` (classes usadas também por Produtos, Categorias, Pedidos e Estoque) para um arquivo compartilhado
- Centralizar o tratamento de `ConflitoException` (hoje repetido nos controllers)
- **Autenticação/login**
- Testes automatizados de integração para a API de Clientes/Produtos/Categorias (Pedidos e Estoque já têm testes unitários e scripts de ponta a ponta)
