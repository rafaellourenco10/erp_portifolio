// =====================================================================================
// Arquivo....: FornecedorRespostaDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída com os dados de um fornecedor retornados pela API.
//              Espelho de ClienteRespostaDto.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.fornecedores.
// Fontes.....: Entidade Models.Fornecedor, convertida por DeEntidade().
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record FornecedorRespostaDto(
    int Id,
    string Nome,
    string Documento,
    string? Email,
    string? Telefone,
    string Cidade,
    string Uf,
    bool Ativo,
    DateTime DataCadastro)
{
    public static FornecedorRespostaDto DeEntidade(Fornecedor fornecedor) => new(
        fornecedor.Id,
        fornecedor.Nome,
        fornecedor.Documento,
        fornecedor.Email,
        fornecedor.Telefone,
        fornecedor.Cidade,
        fornecedor.Uf,
        fornecedor.Ativo,
        fornecedor.DataCadastro);
}
