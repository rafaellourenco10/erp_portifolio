// =====================================================================================
// Arquivo....: Program.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: Ponto de entrada da API. Configura injeção de dependência, EF Core,
//              Swagger, CORS, tratamento de erros (ProblemDetails) e controllers.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
//              Connection string "ErpPortfolio" (appsettings.Development.json; a senha
//              real deve vir de User Secrets ou da variável ConnectionStrings__ErpPortfolio).
// Tabelas....: public.clientes (via ErpPortfolioDbContext)
// Fontes.....: Configuração: appsettings*.json, User Secrets, variáveis de ambiente.
//              CORS: seção "Cors:OrigensPermitidas".
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Reflection;
using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

const string PoliticaCorsFrontEnd = "FrontEnd";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ErpPortfolioDbContext>(opcoes =>
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("ErpPortfolio")));

builder.Services.AddScoped<IClienteService, ClienteService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP Portfolio API",
        Version = "v1",
        Description = "API do ERP de portfólio - módulo de Clientes."
    });

    var arquivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    opcoes.IncludeXmlComments(arquivoXml);
});

var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy(PoliticaCorsFrontEnd, politica =>
        politica.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opcoes => opcoes.DocumentTitle = "ERP Portfolio API");
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(PoliticaCorsFrontEnd);

app.UseAuthorization();

app.MapControllers();

app.Run();
