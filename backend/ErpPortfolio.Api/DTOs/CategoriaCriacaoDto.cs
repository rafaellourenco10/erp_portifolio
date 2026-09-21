// =====================================================================================
// Arquivo....: CategoriaCriacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de entrada para inclusão de categoria (POST /api/categorias).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.categorias pelo CategoriaService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class CategoriaCriacaoDto
{
    /// <example>Periféricos</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 60 caracteres.")]
    public string Nome
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;
}
