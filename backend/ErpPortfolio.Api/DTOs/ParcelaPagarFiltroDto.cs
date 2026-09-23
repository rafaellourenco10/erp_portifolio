// =====================================================================================
// Arquivo....: ParcelaPagarFiltroDto.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: Parâmetros de consulta (query string) da listagem de contas a pagar:
//              busca por nº da compra ou favorecido/descrição, origem, status (incluindo
//              "Atrasado", calculado) e paginação (SPEC.md, P6).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo ContasPagarService para filtrar public.parcelas_pagar.
// Fontes.....: Query string de GET /api/contas-pagar (?busca=&status=&pagina=&tamanhoPagina=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Filtro de origem; busca também por favorecido/descrição (etapa 12).
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public class ParcelaPagarFiltroDto
{
    [StringLength(150, ErrorMessage = "A busca deve ter no máximo 150 caracteres.")]
    public string? Busca { get; set; }

    [EnumDataType(typeof(FiltroStatusParcelaPagar), ErrorMessage = "Status inválido.")]
    public FiltroStatusParcelaPagar? Status { get; set; }

    /// <summary>Compra, Comissao ou Avulsa; ausente = todas.</summary>
    [EnumDataType(typeof(OrigemContaPagar), ErrorMessage = "Origem inválida.")]
    public OrigemContaPagar? Origem { get; set; }

    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}
