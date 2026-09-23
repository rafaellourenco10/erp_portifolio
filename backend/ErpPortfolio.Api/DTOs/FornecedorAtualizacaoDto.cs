// =====================================================================================
// Arquivo....: FornecedorAtualizacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de entrada para edição de fornecedor (PUT /api/fornecedores/{id}).
//              Reaproveita as validações da criação e permite reativar o fornecedor.
//              Espelho de ClienteAtualizacaoDto.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.fornecedores pelo FornecedorService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public class FornecedorAtualizacaoDto : FornecedorCriacaoDto
{
    /// <summary>Opcional. Ausente = mantém o status atual do fornecedor.</summary>
    /// <example>true</example>
    public bool? Ativo { get; set; }
}
