// =====================================================================================
// Arquivo....: Categoria.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Entidade de domínio que representa uma categoria de produtos.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.categorias
// Fontes.....: Mapeada em ErpPortfolioDbContext.Categorias (EF Core / Npgsql).
//              Colunas: id, nome, ativo, data_cadastro (o mapeamento de nomes fica no
//              DbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Categoria
{
    public int Id { get; set; }

    /// <summary>Nome único, sem diferenciar maiúsculas/minúsculas (a checagem fica no serviço).</summary>
    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    /// <summary>Data/hora de cadastro em UTC.</summary>
    public DateTime DataCadastro { get; set; }
}
