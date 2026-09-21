// =====================================================================================
// Arquivo....: CategoriaRespostaDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de saída com os dados de uma categoria retornados pela API.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.categorias.
// Fontes.....: Entidade Models.Categoria, convertida por DeEntidade().
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record CategoriaRespostaDto(int Id, string Nome, bool Ativo, DateTime DataCadastro)
{
    public static CategoriaRespostaDto DeEntidade(Categoria categoria) => new(
        categoria.Id,
        categoria.Nome,
        categoria.Ativo,
        categoria.DataCadastro);
}
