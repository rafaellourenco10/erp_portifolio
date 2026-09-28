// =====================================================================================
// Arquivo....: Cliente.cs
// Versão.....: 1.1.0
// Data.......: 18/09/2026
// Descrição..: Entidade de domínio que representa um cliente (pessoa física ou jurídica).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.clientes
// Fontes.....: Mapeada em ErpPortfolioDbContext.Clientes (EF Core / Npgsql).
//              Colunas: id, nome, documento, email, telefone, cidade, uf, ativo,
//              data_cadastro (o mapeamento de nomes fica no DbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
//   1.1.0 - 28/09/2026 - Endereço completo, código IBGE e Inscrição Estadual (NF-e, etapa 16).
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Cliente
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

    // Endereço e IE: opcionais no cadastro, obrigatórios (menos complemento e IE) para emitir NF-e (etapa 16).
    public string? Logradouro { get; set; }

    /// <summary>Número do endereço; aceita "S/N".</summary>
    public string? Numero { get; set; }

    public string? Complemento { get; set; }

    public string? Bairro { get; set; }

    /// <summary>8 dígitos, sem máscara.</summary>
    public string? Cep { get; set; }

    /// <summary>Código IBGE do município (7 dígitos; os 2 primeiros são o código da UF).</summary>
    public string? CodigoMunicipio { get; set; }

    /// <summary>Inscrição Estadual (dígitos) ou "ISENTO"; com IE o cliente é contribuinte do ICMS na NF-e.</summary>
    public string? InscricaoEstadual { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>Data/hora de cadastro em UTC.</summary>
    public DateTime DataCadastro { get; set; }
}
