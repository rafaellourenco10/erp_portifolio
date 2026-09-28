// =====================================================================================
// Arquivo....: Empresa.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Dados da empresa emitente da NF-e (SPEC.md etapa 16). Tabela de uma linha só
//              (id = 1), editada em Fiscal → Empresa.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.empresa
// Fontes.....: Mapeada em ErpPortfolioDbContext.Empresa. Lida e gravada pelo EmpresaService;
//              travada (FOR UPDATE) pelo NotaFiscalService ao numerar uma nota.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Empresa
{
    /// <summary>Sempre 1: só existe uma empresa.</summary>
    public int Id { get; set; }

    public string RazaoSocial { get; set; } = string.Empty;

    public string? NomeFantasia { get; set; }

    /// <summary>CNPJ sem máscara (aceita o alfanumérico).</summary>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>Somente dígitos.</summary>
    public string InscricaoEstadual { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    public string Bairro { get; set; } = string.Empty;

    /// <summary>8 dígitos.</summary>
    public string Cep { get; set; } = string.Empty;

    public string Municipio { get; set; } = string.Empty;

    /// <summary>Código IBGE (7 dígitos).</summary>
    public string CodigoMunicipio { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;

    public string? Telefone { get; set; }

    /// <summary>Série das notas (0 a 999); a numeração recomeça em 1 em cada série.</summary>
    public int SerieNfe { get; set; } = 1;
}
