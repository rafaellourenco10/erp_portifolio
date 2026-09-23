// =====================================================================================
// Arquivo....: FornecedorFiltroDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Parâmetros de consulta (query string) da listagem de fornecedores:
//              filtro por nome, UFs (múltiplas), status e paginação. Espelho de
//              ClienteFiltroDto.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo FornecedorService para filtrar public.fornecedores.
// Fontes.....: Query string de GET /api/fornecedores
//              (?nome=&ufs=SP&ufs=MG&ativo=true&pagina=&tamanhoPagina=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs.Validacoes;

namespace ErpPortfolio.Api.DTOs;

public class FornecedorFiltroDto
{
    /// <summary>Trecho do nome (busca sem diferenciar maiúsculas/minúsculas).</summary>
    [StringLength(150, ErrorMessage = "O filtro de nome deve ter no máximo 150 caracteres.")]
    public string? Nome { get; set; }

    /// <summary>Siglas de UF; repita o parâmetro para várias (ex.: ufs=SP&amp;ufs=MG).</summary>
    [MaxLength(27, ErrorMessage = "Informe no máximo 27 UFs.")]
    [Uf]
    public List<string>? Ufs { get; set; }

    /// <summary>true = só ativos, false = só inativos, ausente = todos.</summary>
    public bool? Ativo { get; set; }

    /// <summary>Número da página, começando em 1.</summary>
    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de registros por página (1 a 100).</summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}
