// =====================================================================================
// Arquivo....: CategoriaAtualizacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de entrada para edição de categoria (PUT /api/categorias/{id}).
//              Reaproveita a validação da criação e permite reativar a categoria.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.categorias pelo CategoriaService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public class CategoriaAtualizacaoDto : CategoriaCriacaoDto
{
    /// <summary>Opcional. Ausente = mantém o status atual da categoria.</summary>
    /// <example>true</example>
    public bool? Ativo { get; set; }
}
