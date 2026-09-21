# Ambition ERP

ERP comercial desenvolvido como projeto de portfólio, com back-end em **ASP.NET Core** e front-end em **React**, em tema escuro próprio.
O projeto é evoluído por módulos. A **etapa 1** entrega o **módulo de Clientes** completo (API + banco + tela).

| Etapa | Módulo | Situação |
|---|---|---|
| 1 | Clientes | Concluída e testada de ponta a ponta (18/09/2026) |
| 1.1 | Tema visual Ambition ERP + filtros por UF e status | Concluída e testada (18/09/2026) |
| 2+ | Produtos, Pedidos, Login | Planejadas |

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

---

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
│       │   └── ClientesController.cs        # endpoints REST
│       ├── Models/
│       │   └── Cliente.cs                   # entidade
│       ├── DTOs/
│       │   ├── ClienteCriacaoDto.cs         # entrada do POST
│       │   ├── ClienteAtualizacaoDto.cs     # entrada do PUT (+ campo ativo)
│       │   ├── ClienteRespostaDto.cs        # saída
│       │   ├── ClienteFiltroDto.cs          # query string da listagem
│       │   ├── ResultadoPaginadoDto.cs      # envelope genérico de paginação
│       │   └── Validacoes/
│       │       ├── DocumentoValidador.cs    # regra de CPF/CNPJ
│       │       ├── CpfCnpjAttribute.cs      # atributo [CpfCnpj]
│       │       └── UfAttribute.cs           # atributo [Uf]
│       ├── Services/
│       │   ├── IClienteService.cs
│       │   ├── ClienteService.cs            # regras de negócio + acesso a dados
│       │   └── ConflitoException.cs         # vira HTTP 409
│       ├── Data/
│       │   ├── ErpPortfolioDbContext.cs     # mapeamento EF Core (snake_case)
│       │   └── Migrations/                  # migrations geradas pelo EF Core
│       ├── Properties/launchSettings.json   # porta 5065, abre o Swagger
│       ├── appsettings.json
│       ├── appsettings.Development.json     # connection string (sem senha real) + CORS
│       └── ErpPortfolio.Api.http            # requisições prontas para testar a API
│
└── frontend/
    └── erp-portfolio-web/
        ├── .env.development                 # VITE_API_URL
        ├── vite.config.ts                   # porta fixa 5173
        ├── public/favicon.svg               # ícone do Ambition ERP
        └── src/
            ├── main.tsx                     # providers (TanStack Query, Ant Design pt-BR + tema) e fonte Inter
            ├── App.tsx / App.css            # layout: menu lateral recolhível, cabeçalho, conteúdo
            ├── index.css                    # estilos globais (fundo, fonte, barras de rolagem)
            ├── tema/
            │   └── temaAmbition.ts          # cores e tokens do tema (fonte única)
            ├── components/
            │   ├── LogoAmbition.tsx/.css    # logo
            │   └── TagStatus.tsx/.css       # tag Ativo/Inativo
            ├── api/
            │   ├── axiosClient.ts           # instância do Axios + leitura de ProblemDetails
            │   └── clientesApi.ts           # chamadas da API de clientes
            ├── hooks/
            │   └── useClientes.ts           # useQuery / useMutation
            ├── pages/Clientes/
            │   ├── ClientesListaPage.tsx    # filtros, tabela, paginação, ações
            │   ├── ClienteFormDrawer.tsx    # painel lateral de inclusão/edição
            │   └── clientes.css             # estilos da tela e do painel
            ├── schemas/
            │   └── clienteSchema.ts         # schema Zod do formulário
            ├── types/
            │   └── cliente.ts               # tipos (espelham os DTOs)
            └── utils/
                ├── documento.ts             # validação e máscara de CPF/CNPJ
                └── ufs.ts                   # 27 UFs com nome, busca sem acentos e ordenação
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
- Dashboard com indicadores (depende de Pedidos)
- Módulo de **Produtos**
- Módulo de **Pedidos**
- **Autenticação/login**
- Testes automatizados (unitários para validação de CPF/CNPJ e de integração para a API)
