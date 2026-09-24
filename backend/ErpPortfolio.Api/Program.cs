// =====================================================================================
// Arquivo....: Program.cs
// Versão.....: 1.17.0
// Data.......: 23/09/2026
// Descrição..: Ponto de entrada da API. Configura injeção de dependência, EF Core,
//              Swagger, CORS, tratamento de erros (ProblemDetails) e controllers.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
//              Connection string "ErpPortfolio" (appsettings.Development.json; a senha
//              real deve vir de User Secrets ou da variável ConnectionStrings__ErpPortfolio).
// Tabelas....: public.clientes, public.produtos, public.categorias, public.pedidos,
//              public.pedido_itens (via ErpPortfolioDbContext)
// Fontes.....: Configuração: appsettings*.json, User Secrets, variáveis de ambiente.
//              CORS: seção "Cors:OrigensPermitidas".
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
//   1.1.0 - 18/09/2026 - Swagger renomeado para "Ambition ERP API".
//   1.2.0 - 21/09/2026 - Registro do IProdutoService e descrição do Swagger com Produtos.
//   1.3.0 - 21/09/2026 - Registro do ICategoriaService e Categorias na descrição do Swagger.
//   1.4.0 - 21/09/2026 - Registro do IPedidoService e Pedidos na descrição do Swagger.
//   1.5.0 - 22/09/2026 - Registro do IEstoqueService e Estoque na descrição do Swagger.
//   1.6.0 - 22/09/2026 - Registro do IContasReceberService e Contas a Receber no Swagger.
//   1.7.0 - 22/09/2026 - Registro do IFornecedorService (etapa 7).
//   1.8.0 - 22/09/2026 - Registro do IPedidoCompraService (etapa 7).
//   1.9.0 - 23/09/2026 - Registro do IContasPagarService e Contas a Pagar no Swagger (etapa 8).
//   1.10.0 - 23/09/2026 - Registro do IRelatorioService e Relatórios no Swagger (etapa 9).
//   1.11.0 - 23/09/2026 - Licença Community do QuestPDF (PDF dos relatórios).
//   1.12.0 - 23/09/2026 - CORS expõe Content-Disposition (nome do arquivo dos relatórios).
//   1.13.0 - 23/09/2026 - Registro do IVendedorService (etapa 10).
//   1.14.0 - 23/09/2026 - Registro do IComissaoService (etapa 11).
//   1.15.0 - 23/09/2026 - Registro do IOrcamentoService e Orçamentos no Swagger (etapa 13).
//   1.16.0 - 24/09/2026 - Registro do IDevolucaoService e Devoluções no Swagger (etapa 14).
//   1.17.0 - 24/09/2026 - Registro do IFluxoCaixaService e Fluxo de Caixa no Swagger (etapa 15).
// =====================================================================================

using System.Reflection;
using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

const string PoliticaCorsFrontEnd = "FrontEnd";

// QuestPDF exige declarar a licença; a Community é gratuita para uso individual e empresas pequenas (portfólio).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ErpPortfolioDbContext>(opcoes =>
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("ErpPortfolio")));

builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IContasReceberService, ContasReceberService>();
builder.Services.AddScoped<IContasPagarService, ContasPagarService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IVendedorService, VendedorService>();
builder.Services.AddScoped<IComissaoService, ComissaoService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IPedidoCompraService, PedidoCompraService>();
builder.Services.AddScoped<IOrcamentoService, OrcamentoService>();
builder.Services.AddScoped<IDevolucaoService, DevolucaoService>();
builder.Services.AddScoped<IFluxoCaixaService, FluxoCaixaService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ambition ERP API",
        Version = "v1",
        Description = "API do Ambition ERP (projeto de portfólio) - módulos de Clientes, Produtos, Categorias, Fornecedores, Vendedores, Orçamentos, Pedidos (venda e compra), Devoluções, Estoque, Contas a Receber, Contas a Pagar, Comissões, Fluxo de Caixa e Relatórios."
    });

    var arquivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    opcoes.IncludeXmlComments(arquivoXml);
});

var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy(PoliticaCorsFrontEnd, politica =>
        politica.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod()
            // O front lê o nome do arquivo exportado (relatórios) deste cabeçalho.
            .WithExposedHeaders("Content-Disposition")));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opcoes => opcoes.DocumentTitle = "Ambition ERP API");
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(PoliticaCorsFrontEnd);

app.UseAuthorization();

app.MapControllers();

app.Run();
