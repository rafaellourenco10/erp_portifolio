// =====================================================================================
// Arquivo....: ClienteFiltroDto.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: Parâmetros de consulta (query string) da listagem de clientes:
//              filtro por nome e paginação.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo ClienteService para filtrar public.clientes.
// Fontes.....: Query string de GET /api/clientes (?nome=&pagina=&tamanhoPagina=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class ClienteFiltroDto
{
    /// <summary>Trecho do nome (busca sem diferenciar maiúsculas/minúsculas).</summary>
    [StringLength(150, ErrorMessage = "O filtro de nome deve ter no máximo 150 caracteres.")]
    public string? Nome { get; set; }

    /// <summary>Número da página, começando em 1.</summary>
    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de registros por página (1 a 100).</summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}
