// =====================================================================================
// Arquivo....: ClienteRespostaDto.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: DTO de saída com os dados de um cliente retornados pela API.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.clientes.
// Fontes.....: Entidade Models.Cliente, convertida por DeEntidade().
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record ClienteRespostaDto(
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
    public static ClienteRespostaDto DeEntidade(Cliente cliente) => new(
        cliente.Id,
        cliente.Nome,
        cliente.Documento,
        cliente.Email,
        cliente.Telefone,
        cliente.Cidade,
        cliente.Uf,
        cliente.Ativo,
        cliente.DataCadastro);
}
