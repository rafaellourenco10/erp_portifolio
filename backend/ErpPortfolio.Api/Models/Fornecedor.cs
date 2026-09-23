// =====================================================================================
// Arquivo....: Fornecedor.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Entidade de domínio que representa um fornecedor (pessoa física ou jurídica).
//              Mesmo formato do Cliente (SPEC.md, F1); documento único num índice próprio,
//              independente do de clientes.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.fornecedores
// Fontes.....: Mapeada em ErpPortfolioDbContext.Fornecedores (EF Core / Npgsql).
//              Colunas: id, nome, documento, email, telefone, cidade, uf, ativo,
//              data_cadastro (o mapeamento de nomes fica no DbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Fornecedor
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF (11 dígitos) ou CNPJ (14 caracteres), armazenado sem máscara.</summary>
    public string Documento { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public string Cidade { get; set; } = string.Empty;

    /// <summary>Sigla da unidade federativa, sempre em maiúsculas (ex.: SP).</summary>
    public string Uf { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    /// <summary>Data/hora de cadastro em UTC.</summary>
    public DateTime DataCadastro { get; set; }
}
