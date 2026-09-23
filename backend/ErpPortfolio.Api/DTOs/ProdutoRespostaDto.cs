// =====================================================================================
// Arquivo....: ProdutoRespostaDto.cs
// Versão.....: 1.2.0
// Data.......: 22/09/2026
// Descrição..: DTO de saída com os dados de um produto retornados pela API, incluindo
//              a categoria (id e nome).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.produtos (LEFT JOIN public.categorias).
// Fontes.....: Entidade Models.Produto. A mesma projeção (Projecao) vira SQL nas
//              consultas e é compilada em DeEntidade() para entidades já carregadas.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 21/09/2026 - Categoria (CategoriaId e CategoriaNome) no lugar do texto livre.
//   1.2.0 - 22/09/2026 - EstoqueMinimo.
// =====================================================================================

using System.Linq.Expressions;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record ProdutoRespostaDto(
    int Id,
    string Nome,
    string Sku,
    int? CategoriaId,
    string? CategoriaNome,
    string Unidade,
    decimal PrecoVenda,
    decimal Custo,
    decimal EstoqueMinimo,
    bool Ativo,
    DateTime DataCadastro)
{
    /// <summary>Mapeamento único: traduzido para SQL nas consultas (com o nome da categoria via join).</summary>
    public static readonly Expression<Func<Produto, ProdutoRespostaDto>> Projecao = produto => new ProdutoRespostaDto(
        produto.Id,
        produto.Nome,
        produto.Sku,
        produto.CategoriaId,
        produto.Categoria != null ? produto.Categoria.Nome : null,
        produto.Unidade,
        produto.PrecoVenda,
        produto.Custo,
        produto.EstoqueMinimo,
        produto.Ativo,
        produto.DataCadastro);

    private static readonly Func<Produto, ProdutoRespostaDto> ProjecaoCompilada = Projecao.Compile();

    /// <summary>Para uma entidade já carregada (a navegação Categoria precisa estar preenchida).</summary>
    public static ProdutoRespostaDto DeEntidade(Produto produto) => ProjecaoCompilada(produto);
}
