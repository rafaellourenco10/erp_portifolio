// =====================================================================================
// Arquivo....: CategoriaFiltroDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Parâmetros de consulta (query string) da listagem de categorias:
//              busca por nome, status e paginação.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo CategoriaService para filtrar public.categorias.
// Fontes.....: Query string de GET /api/categorias (?busca=&ativo=true&pagina=&tamanhoPagina=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class CategoriaFiltroDto
{
    /// <summary>Trecho do nome (sem diferenciar maiúsculas/minúsculas).</summary>
    [StringLength(60, ErrorMessage = "A busca deve ter no máximo 60 caracteres.")]
    public string? Busca { get; set; }

    /// <summary>true = só ativas, false = só inativas, ausente = todas.</summary>
    public bool? Ativo { get; set; }

    /// <summary>Número da página, começando em 1.</summary>
    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de registros por página (1 a 100).</summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}
