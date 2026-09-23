// =====================================================================================
// Arquivo....: ProdutoFiltroDto.cs
// Versão.....: 1.1.0
// Data.......: 22/09/2026
// Descrição..: Parâmetros de consulta (query string) da listagem de produtos:
//              busca por nome ou SKU, status, categoria e paginação.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo ProdutoService para filtrar public.produtos.
// Fontes.....: Query string de GET /api/produtos (?busca=&ativo=true&categoriaId=&pagina=&tamanhoPagina=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 22/09/2026 - Filtro por categoriaId.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class ProdutoFiltroDto
{
    /// <summary>Trecho do nome ou do SKU (sem diferenciar maiúsculas/minúsculas).</summary>
    [StringLength(150, ErrorMessage = "A busca deve ter no máximo 150 caracteres.")]
    public string? Busca { get; set; }

    /// <summary>true = só ativos, false = só inativos, ausente = todos.</summary>
    public bool? Ativo { get; set; }

    /// <summary>Id da categoria; ausente = todas.</summary>
    public int? CategoriaId { get; set; }

    /// <summary>Número da página, começando em 1.</summary>
    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de registros por página (1 a 100).</summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}
