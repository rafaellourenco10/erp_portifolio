# Guia de desenvolvimento

> Parte da documentação do [Ambition ERP](../README.md).

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

Também dá para testar pelo arquivo [ErpPortfolio.Api.http](../backend/ErpPortfolio.Api/ErpPortfolio.Api.http), que já tem essas requisições prontas (VS Code com a extensão REST Client, Visual Studio ou Rider).

### 3. Subir o front e ver a tela de clientes

Com a API rodando, abra **outro terminal**:

```powershell
cd frontend/erp-portfolio-web
npm install
npm run dev
```

Acesse <http://localhost:5180>. Na tela:

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


## Tema visual

O visual segue o design system **Ambition ERP**, criado no Google Stitch. Os arquivos originais ficam em [`docs/tema/`](tema/):

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
| Texto principal / secundário | `#F5F6F7` / `#CDD2D7` |
| Destaque (marca, ações, ativo) | Verde `#22C55E` (hover `#16A34A`) |
| Alerta / erro / informativo | `#F59E0B` / `#EF4444` / `#38BDF8` |
| Fonte | Inter, com algarismos tabulares em CPF/CNPJ, telefones e datas |
| Cantos | 8px (botões e campos), 12px (cards e painéis) |

Onde o tema está no código:
- [`src/tema/temaAmbition.ts`](../frontend/erp-portfolio-web/src/tema/temaAmbition.ts): **fonte única das cores**. Alimenta os tokens do Ant Design (`ConfigProvider`) e publica as variáveis CSS `--cor-*` usadas nos arquivos `.css`. Para mudar uma cor, altere só aqui.
- [`src/components/LogoAmbition.tsx`](../frontend/erp-portfolio-web/src/components/LogoAmbition.tsx): logo (completo ou só o ícone, com o menu recolhido).
- [`src/components/TagStatus.tsx`](../frontend/erp-portfolio-web/src/components/TagStatus.tsx): tag Ativo/Inativo no padrão do `DESIGN.md`.

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
│       │   ├── OrcamentosController.cs      # orçamentos: CRUD do aberto, gerar pedido, perder, PDF
│       │   ├── DevolucoesController.cs      # /pedidos/{id}/devolucoes: registrar e histórico
│       │   ├── EstoqueController.cs         # endpoints REST de estoque
│       │   ├── ContasReceberController.cs   # endpoints REST de contas a receber
│       │   ├── DashboardController.cs       # endpoints REST do Dashboard (so delegam)
│       │   ├── FornecedoresController.cs    # endpoints REST de fornecedores
│       │   ├── VendedoresController.cs      # endpoints REST de vendedores
│       │   ├── ComissoesController.cs       # listar comissões com totais e pagar em lote
│       │   ├── PedidosCompraController.cs   # endpoints REST de pedidos de compra
│       │   ├── ContasPagarController.cs     # endpoints REST de contas a pagar
│       │   ├── FluxoCaixaController.cs      # fluxo de caixa: JSON ou arquivo (?formato=xlsx|pdf)
│       │   ├── EmpresaController.cs         # GET/PUT /empresa (emitente da NF-e)
│       │   ├── NotasFiscaisController.cs    # emitir (pedido/devolução), lista, detalhe, XML e DANFE
│       │   └── RelatoriosController.cs      # relatórios: JSON para a tela ou arquivo (?formato=xlsx|pdf)
│       ├── Models/
│       │   ├── Cliente.cs                   # entidade
│       │   ├── Produto.cs                   # entidade (CategoriaId + navegação)
│       │   ├── Categoria.cs                 # entidade
│       │   ├── Pedido.cs / PedidoItem.cs    # entidades (itens ligados por FK, preco congelado)
│       │   ├── StatusPedido.cs              # enum: Rascunho, Confirmado, Cancelado (reaproveitado por PedidoCompra)
│       │   ├── FormaPagamento.cs            # enum: Dinheiro, Pix, Boleto, Cartao
│       │   ├── EstoqueMovimentacao.cs       # entidade (FK produto, pedido e pedido de compra, opcionais)
│       │   ├── TipoMovimentacao.cs          # enum: Entrada, Saida
│       │   ├── ParcelaReceber.cs            # entidade (FK pedido, vencimento como DateOnly)
│       │   ├── StatusParcela.cs             # enum: Pendente, Recebido, Cancelado
│       │   ├── ParcelaPagar.cs              # entidade (FK pedido de compra, vencimento como DateOnly)
│       │   ├── StatusParcelaPagar.cs        # enum: Pendente, Pago, Cancelado
│       │   ├── Fornecedor.cs                # entidade (espelho de Cliente)
│       │   ├── Vendedor.cs                  # entidade (CPF, % de comissão padrão)
│       │   ├── Comissao.cs                  # entidade (uma por parcela recebida) + enum StatusComissao
│       │   └── PedidoCompra.cs / PedidoCompraItem.cs  # entidades (espelho de Pedido/PedidoItem, sem forma de pagamento)
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
│       │   ├── PedidoConfirmarDto.cs        # entrada do PATCH /confirmar: numeroParcelas, intervaloDias
│       │   ├── ParcelaRespostaDto.cs        # linha da listagem de contas a receber
│       │   ├── ParcelaFiltroDto.cs          # query string da listagem
│       │   ├── FiltroStatusParcela.cs       # enum do filtro (inclui "Atrasado", calculado)
│       │   ├── VendasResumoDto.cs           # saída de /dashboard/vendas
│       │   ├── FaturamentoDiaDto.cs         # ponto do gráfico (dia, valor)
│       │   ├── PedidosPorStatusDto.cs       # contagem por status no mês
│       │   ├── ContasReceberResumoDto.cs    # saída de /dashboard/contas-receber
│       │   ├── ParcelaPagar{Resposta,Filtro}Dto.cs, FiltroStatusParcelaPagar.cs  # mesmo desenho, para contas a pagar
│       │   ├── ContasPagarResumoDto.cs      # saída de /dashboard/contas-pagar
│       │   ├── RelatorioFiltroDtos.cs       # filtros dos relatórios (período validado: R1)
│       │   ├── RelatorioRespostaDtos.cs     # linhas + resumo dos 3 relatórios
│       │   ├── FormatoRelatorio.cs          # enum: Json, Xlsx, Pdf
│       │   ├── FluxoCaixaDtos.cs            # filtro (período validado: FC7) e resposta do fluxo de caixa
│       │   ├── EstoqueResumoDashboardDto.cs # saída de /dashboard/estoque
│       │   ├── Fornecedor{Criacao,Atualizacao,Resposta,Filtro}Dto.cs  # mesmo desenho, para fornecedores
│       │   ├── VendedorDtos.cs              # criação, atualização, filtro e resposta de vendedores
│       │   ├── ComissaoDtos.cs              # filtro, linha, totais e corpo do "pagar"
│       │   ├── Validacoes/CpfAttribute.cs   # só CPF (vendedor), reaproveitando o DocumentoValidador
│       │   ├── PedidoCompraCriacaoDto.cs    # entrada do POST/PUT (itens sem preço; sem forma de pagamento)
│       │   ├── PedidoCompraItemEntradaDto.cs # item do POST/PUT: produtoId, quantidade, desconto
│       │   ├── PedidoCompraRespostaDto.cs   # saída com itens (preço = custo, subtotal)
│       │   ├── PedidoCompraResumoDto.cs     # linha da listagem (sem itens)
│       │   ├── PedidoCompraFiltroDto.cs     # query string da listagem
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
│       │   ├── EstoqueService.cs            # saldo, extrato, entrada manual, baixa/estorno (venda) e receber/estorno (compra)
│       │   ├── EstoqueCalculo.cs            # saldo = Σ Entrada − Σ Saída, funcao pura (sem banco)
│       │   ├── IContasReceberService.cs
│       │   ├── ContasReceberService.cs      # consulta, receber, gerar/cancelar parcelas (usados pelo PedidoService)
│       │   ├── ContasReceberCalculo.cs      # divisao em N parcelas (resto na ultima), funcao pura (sem banco)
│       │   ├── DashboardCalculo.cs          # ticket medio e preenchimento de dias, funcao pura (sem banco)
│       │   ├── IFornecedorService.cs
│       │   ├── FornecedorService.cs         # regras de negócio + acesso a dados (espelho de ClienteService)
│       │   ├── IVendedorService.cs
│       │   ├── VendedorService.cs           # espelho de FornecedorService, com CPF único
│       │   ├── IComissaoService.cs
│       │   ├── ComissaoService.cs           # lista com totais do filtro, pagar em lote (a geração fica no ContasReceberService)
│       │   ├── ComissaoCalculo.cs           # parcela × % com 2 casas, função pura (com xUnit)
│       │   ├── IOrcamentoService.cs
│       │   ├── OrcamentoService.cs          # orçamento (validações de item do PedidoService), gerar pedido, perder
│       │   ├── ExportadorOrcamento.cs       # PDF do orçamento (QuestPDF, layout de documento)
│       │   ├── IDevolucaoService.cs
│       │   ├── DevolucaoService.cs          # devolução numa transação: parcelas, reembolso, estorno, estoque
│       │   ├── DevolucaoCalculo.cs          # valor, "o que falta", abatimento, reembolso e estorno (puro, com xUnit)
│       │   ├── IPedidoCompraService.cs
│       │   ├── PedidoCompraService.cs       # criar/editar/confirmar/cancelar, reaproveitando CalculoPedido/TransicoesPedido
│       │   ├── IContasPagarService.cs
│       │   ├── ContasPagarService.cs        # lista (origem/favorecido), pagar, cancelar, conta avulsa, parcelas da compra; propaga para comissões
│       │   ├── ContasPagarCalculo.cs        # parcelas da conta avulsa (valor e vencimento), função pura
│       │   ├── IFluxoCaixaService.cs
│       │   ├── FluxoCaixaService.cs         # lê as parcelas (realizado no dia de Brasília, pendentes) e monta o arquivo
│       │   ├── FluxoCaixaCalculo.cs         # períodos, realizado × previsto, saldos e menor saldo (puro, com xUnit)
│       │   ├── NfeCalculo.cs                # CFOP, alíquotas por UF, rateio, impostos, chave + DV (puro, com xUnit)
│       │   ├── NfeXml.cs                    # XML nfeProc no layout 4.00 (System.Xml.Linq)
│       │   ├── NotaFiscalService.cs         # emissão (saída e devolução) numa transação, lista, detalhe
│       │   ├── ExportadorDanfe.cs           # DANFE em PDF (QuestPDF) a partir do XML gravado
│       │   ├── HorarioBrasilia.cs           # "hoje" e limites de dia/mês em horário de Brasília
│       │   ├── IRelatorioService.cs
│       │   ├── RelatorioService.cs          # consultas dos relatórios + montagem do modelo de exportação
│       │   ├── RelatorioCalculo.cs          # período válido e resumo (funções puras, com xUnit)
│       │   ├── RelatorioModelo.cs           # modelo genérico (título, filtros, resumo, colunas, linhas) + formatação pt-BR
│       │   └── ExportadorRelatorio.cs       # modelo -> .xlsx (ClosedXML) ou .pdf (QuestPDF), para os 3 relatórios
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
        ├── vite.config.ts                   # porta fixa 5180
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
            │   ├── TagStatus.tsx/.css       # tag Ativo/Inativo
            │   ├── TagStatusParcela.tsx     # tag Pendente/Atrasado/Recebido/Pago/Cancelado (receber e pagar)
            │   └── ModalParcelas.tsx        # modal de confirmar com nº de parcelas e intervalo (venda e compra)
            ├── api/
            │   ├── axiosClient.ts           # instância do Axios + leitura de ProblemDetails
            │   ├── clientesApi.ts           # chamadas da API de clientes
            │   ├── produtosApi.ts           # chamadas da API de produtos
            │   ├── categoriasApi.ts         # chamadas da API de categorias
            │   ├── estoqueApi.ts            # chamadas da API de estoque
            │   ├── contasReceberApi.ts      # chamadas da API de contas a receber
            │   ├── contasPagarApi.ts        # chamadas da API de contas a pagar (inclui conta avulsa e cancelar)
            │   ├── relatoriosApi.ts         # dados dos relatórios + download do arquivo (nome do Content-Disposition)
            │   ├── dashboardApi.ts          # chamadas da API do Dashboard (4 endpoints)
            │   ├── fornecedoresApi.ts       # chamadas da API de fornecedores
            │   ├── vendedoresApi.ts         # chamadas da API de vendedores
            │   ├── comissoesApi.ts          # chamadas da API de comissões
            │   ├── orcamentosApi.ts         # chamadas da API de orçamentos (+ URL do PDF)
            │   ├── devolucoesApi.ts         # registrar e listar devoluções de um pedido
            │   └── pedidosCompraApi.ts      # chamadas da API de pedidos de compra
            ├── hooks/
            │   ├── useClientes.ts           # useQuery / useMutation
            │   ├── useProdutos.ts
            │   ├── useCategorias.ts         # inclui as categorias ativas do seletor de produtos
            │   ├── useEstoque.ts            # lista com saldo, extrato por produto, entrada manual
            │   ├── useContasReceber.ts      # lista paginada e marcar parcela como recebida
            │   ├── useContasPagar.ts        # lista paginada e marcar parcela como paga
            │   ├── useRelatorios.ts         # consulta só após "Gerar"; download como mutação
            │   ├── useDashboard.ts          # 4 queries independentes (vendas, contas a receber, contas a pagar, estoque)
            │   ├── useFornecedores.ts       # useQuery / useMutation
            │   ├── useVendedores.ts         # useQuery / useMutation
            │   ├── useComissoes.ts          # lista com totais e marcar como pagas
            │   ├── useOrcamentos.ts         # lista, detalhe, salvar, gerar pedido e perder
            │   ├── useDevolucoes.ts         # histórico e registrar (invalida os módulos afetados)
            │   └── usePedidosCompra.ts      # useQuery / useMutation de pedidos de compra
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
            ├── pages/Orcamentos/
            │   ├── OrcamentosListaPage.tsx  # filtro (número/cliente + situação com Vencido), tabela, link do pedido
            │   └── OrcamentoPage.tsx        # formulário (tabela de itens do pedido) + gerar pedido, perder, PDF
            ├── pages/Pedidos/
            │   ├── PedidosListaPage.tsx     # filtro (número/cliente + status), tabela, paginação
            │   ├── PedidoPage.tsx           # formulário: cliente, itens, desconto, resumo, ações
            │   ├── ItensPedidoTabela.tsx    # tabela de itens (tela larga) / cartões (celular)
            │   ├── DevolucaoModal.tsx       # registrar devolução: itens, volta ao estoque, prévia
            │   ├── DevolucoesPedido.tsx     # histórico de devoluções na página do pedido (+ coluna NF-e)
            │   ├── NotaFiscalPedido.tsx     # seção "Nota fiscal": emitir NF-e ou número/chave/XML/DANFE
            │   └── pedido.css               # estilos da página do pedido
            ├── pages/Estoque/
            │   ├── EstoqueListaPage.tsx     # busca, tabela com saldo, paginação, ações
            │   ├── EntradaEstoqueDrawer.tsx # painel de nova entrada manual (produto, quantidade, motivo)
            │   └── MovimentacoesDrawer.tsx  # painel de extrato paginado de um produto
            ├── pages/ContasReceber/
            │   └── ContasReceberListaPage.tsx  # busca, filtro de status, tabela, marcar como recebido
            ├── pages/ContasPagar/
            │   ├── ContasPagarListaPage.tsx    # busca, filtros de origem/status, tabela, pagar, cancelar
            │   └── NovaContaModal.tsx          # lançamento de conta avulsa em parcelas
            ├── pages/Relatorios/
            │   ├── RelatorioPedidosPage.tsx    # Vendas e Compras (prop tipo): filtros, resumo, tabela, exportar
            │   └── RelatorioEstoquePage.tsx    # posição de estoque: filtros, resumo, tabela, exportar
            ├── pages/Dashboard/
            │   ├── DashboardPage.tsx        # cards de indicador + gráfico
            │   ├── CardIndicador.tsx        # card com loading/erro próprios (D7)
            │   ├── GraficoFaturamento.tsx   # gráfico de barras em SVG, sem biblioteca
            │   └── dashboard.css
            ├── pages/Fornecedores/
            │   ├── FornecedoresListaPage.tsx  # filtros, tabela, paginação, ações (espelho de Clientes)
            │   └── FornecedorFormDrawer.tsx   # painel lateral de inclusão/edição
            ├── pages/FluxoCaixa/
            │   ├── FluxoCaixaPage.tsx         # visão, período, cards, alertas, tabela, exportar
            │   └── GraficoFluxoCaixa.tsx      # SVG: saldo + entradas/saídas (realizado × previsto)
            ├── pages/Empresa/
            │   └── EmpresaPage.tsx            # dados do emitente da NF-e
            ├── pages/NotasFiscais/
            │   └── NotasFiscaisListaPage.tsx  # filtros, tabela, detalhe (drawer), XML/DANFE, exportar
            ├── pages/Comissoes/
            │   └── ComissoesListaPage.tsx     # filtros, cards, tabela com seleção, marcar como pagas
            ├── pages/Vendedores/
            │   ├── VendedoresListaPage.tsx    # filtros, tabela com % de comissão, ações (espelho de Fornecedores)
            │   └── VendedorFormDrawer.tsx     # painel lateral de inclusão/edição
            ├── pages/PedidosCompra/
            │   ├── PedidosCompraListaPage.tsx # filtro (número/fornecedor + status), tabela, paginação
            │   ├── PedidoCompraPage.tsx       # formulário: fornecedor, itens, desconto, resumo, ações
            │   └── ItensPedidoCompraTabela.tsx # tabela de itens (tela larga) / cartões (celular)
            ├── schemas/
            │   ├── clienteSchema.ts         # schema Zod do formulário de cliente
            │   ├── produtoSchema.ts         # schema Zod do formulário de produto
            │   ├── categoriaSchema.ts       # schema Zod do formulário de categoria
            │   ├── pedidoSchema.ts          # schema Zod do formulário de pedido + conversões form/API
            │   ├── estoqueEntradaSchema.ts  # schema Zod do formulário de entrada de estoque
            │   ├── fornecedorSchema.ts      # schema Zod do formulário de fornecedor
            │   ├── vendedorSchema.ts        # schema Zod do formulário de vendedor (só CPF, % 0-100)
            │   ├── contaAvulsaSchema.ts     # schema Zod da conta avulsa
            │   └── pedidoCompraSchema.ts    # schema Zod do formulário de pedido de compra + conversões form/API
            ├── types/
            │   ├── cliente.ts               # tipos (espelham os DTOs)
            │   ├── produto.ts
            │   ├── categoria.ts
            │   ├── pedido.ts                # inclui PedidoConfirmarEntrada (numeroParcelas, intervaloDias)
            │   ├── estoque.ts               # inclui pedidoCompraId na Movimentacao
            │   ├── contaReceber.ts
            │   ├── contaPagar.ts
            │   ├── relatorio.ts
            │   ├── dashboard.ts
            │   ├── fornecedor.ts
            │   ├── vendedor.ts
            │   ├── comissao.ts
            │   ├── pedidoCompra.ts
            │   └── paginacao.ts             # ResultadoPaginado compartilhado
            ├── components/SelecaoCliente.tsx / SelecaoProduto.tsx / SelecaoFornecedor.tsx / SelecaoVendedor.tsx  # seleção com busca no servidor (usadas nos pedidos e nos filtros dos relatórios, com `aoLimpar`)
            ├── components/TagStatusPedido.tsx                      # tag Rascunho/Confirmado/Cancelado (reaproveitada pelo Pedido de Compra)
            ├── hooks/useBuscaCadastros.ts    # busca com debounce para os seletores acima (clientes, fornecedores, produtos)
            ├── hooks/usePedidos.ts           # useQuery / useMutation de pedidos
            ├── api/pedidosApi.ts             # chamadas da API de pedidos
            └── utils/
                ├── documento.ts             # validação e máscara de CPF/CNPJ
                ├── moeda.ts                 # formatação em reais/percentual/quantidade e cálculo de margem
                ├── ufs.ts                   # 27 UFs com nome, busca sem acentos e ordenação
                └── calculoPedido.ts         # subtotal/total em aritmética inteira (BigInt), mesma fórmula do back
```

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

## Solução de problemas

| Sintoma | Causa provável e solução |
|---|---|
| `failed to connect to the docker API` | O Docker Desktop não está aberto. Abra-o e espere ficar pronto. |
| `Defina POSTGRES_PASSWORD no arquivo .env` | Falta o `.env` na raiz. Copie o `.env.example` (passo 1.1). |
| `password authentication failed for user "erp_user"` | A senha no User Secrets é diferente da do `.env`, ou o volume foi criado com outra senha. Corrija o User Secrets, ou recrie o banco com `docker compose down -v` e `docker compose up -d` (**apaga os dados**). |
| `Bind for 0.0.0.0:5433 failed: port is already allocated` | Outro serviço usa a porta 5433. Troque a porta no `docker-compose.yml` e na connection string. |
| `Failed executing DbCommand` no primeiro `database update` | Normal (ver passo 1). |
| Tela mostra "Não foi possível conectar à API" | A API não está rodando em `http://localhost:5065`, ou o `VITE_API_URL` em `.env.development` está diferente. |
| Erro de CORS no console do navegador | O front não está em `http://localhost:5180`, ou a origem não está em `Cors:OrigensPermitidas`. |
| `Port 5180 is already in use` | Outro processo usa a 5180 (o Vite usa `strictPort` e não troca de porta sozinho). Encerre o outro processo. |
| JSON com acentos recusado ao testar com `curl` no Windows | O terminal enviou o texto fora de UTF-8. Salve o JSON em arquivo UTF-8 e envie com `--data-binary @arquivo.json`, ou use o Swagger. |
| Aviso "Some chunks are larger than 500 kB" no build do front | Apenas informativo (tamanho do Ant Design); não impede o build. |


## Próximas etapas

- Campos do mockup de cliente ainda não implementados: **PF/PJ**, **Nome Fantasia** e **Observações** (exige migration; a Inscrição Estadual entrou na etapa 16)
- Filtro por **cidades** e busca também por CPF/CNPJ (padrão do projeto: filtros de seleção múltipla usam dropdown multi-select com espaçamento normal entre as opções)
- Filtro por **cliente** e por **faixa de data** na lista de Pedidos
- Editar a **forma de pagamento** de um pedido já confirmado (hoje só dá para cancelar e criar outro)
- Verificação visual/Playwright das telas de Contas a Receber, do Dashboard, de Fornecedores/Pedidos de Compra e de Relatórios (cards, gráfico, modal de confirmar com parcelas, formulários, celular) — não feita nas sessões que construíram os módulos
- Saída manual de estoque (perda/quebra/ajuste)
- Recebimento/pagamento parcial de parcela, juros/multa por atraso, edição de parcela já gerada
- Forma de pagamento no Pedido de Compra; pagamento parcial e contas a pagar avulsas (sem pedido de compra, ex.: aluguel)
- Gerar parcelas para pedidos (venda ou compra) confirmados antes dos módulos de Contas a Receber/Pagar
- Recebimento parcial de mercadoria no Pedido de Compra (hoje é recebido inteiro ao confirmar)
- Seletor de período no Dashboard (hoje é sempre o mês atual)
- Contas a pagar: recorrência automática (todo mês), categorias/plano de contas, editar conta lançada, anexos
- Mais relatórios: contas a receber/pagar vencidas, vendas agrupadas por produto, pedidos com os itens
- Mover `clientes.css` (classes usadas também por Produtos, Categorias, Pedidos, Estoque e Contas a Receber/Pagar) para um arquivo compartilhado
- Centralizar o tratamento de `ConflitoException` (hoje repetido nos controllers)
- **Autenticação/login** (etapa 17)
- NF-e: cancelamento (24h, justificativa) e carta de correção, inutilização de numeração, código de barras da chave no DANFE, busca de endereço pelo CEP, IPI/ICMS-ST/DIFAL, Simples Nacional, NF-e de compra; envio real à SEFAZ exigiria certificado A1 e assinatura XML
- Categorias de despesa (decidido em 28/09: só despesas, categoria fixa por origem nas contas automáticas, filtro em Contas a Pagar)
- Fluxo de caixa: contas bancárias/caixas e saldo inicial, categorias de despesa, lançamentos do dia ao clicar, card no Dashboard
- Devolução: desfazer/editar, crédito do cliente, troca num passo só, devolução de compra ao fornecedor, faturamento líquido nos Relatórios
- Orçamentos: duplicar, reabrir, orçamento para não cliente, envio por e-mail, taxa de conversão no Dashboard/Relatórios; trava de concorrência ao gerar pedido se o ERP virar multiusuário
- Testes automatizados de integração para a API de Clientes/Produtos/Categorias/Fornecedores (Pedidos, Estoque, Contas a Receber, Dashboard e Pedidos de Compra já têm testes unitários e/ou scripts de ponta a ponta)

