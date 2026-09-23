// =====================================================================================
// Arquivo....: Vendedor.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Entidade de domínio que representa um vendedor (pessoa física), com a %
//              de comissão padrão. A % é copiada para o pedido ao confirmar (SPEC.md, PV3).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.vendedores
// Fontes.....: Mapeada em ErpPortfolioDbContext.Vendedores (EF Core / Npgsql).
//              Colunas: id, nome, cpf, email, telefone, percentual_comissao, ativo,
//              data_cadastro (o mapeamento de nomes fica no DbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Vendedor
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF com 11 dígitos, armazenado sem máscara.</summary>
    public string Cpf { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    /// <summary>% de comissão padrão (0 a 100), copiada para o pedido ao confirmar.</summary>
    public decimal PercentualComissao { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>Data/hora de cadastro em UTC.</summary>
    public DateTime DataCadastro { get; set; }
}
