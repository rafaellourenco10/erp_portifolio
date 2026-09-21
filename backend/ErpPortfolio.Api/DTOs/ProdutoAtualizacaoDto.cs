// =====================================================================================
// Arquivo....: ProdutoAtualizacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de entrada para edição de produto (PUT /api/produtos/{id}).
//              Reaproveita as validações da criação e permite reativar o produto.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.produtos pelo ProdutoService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public class ProdutoAtualizacaoDto : ProdutoCriacaoDto
{
    /// <summary>Opcional. Ausente = mantém o status atual do produto.</summary>
    /// <example>true</example>
    public bool? Ativo { get; set; }
}
