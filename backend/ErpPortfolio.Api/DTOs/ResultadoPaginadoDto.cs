// =====================================================================================
// Arquivo....: ResultadoPaginadoDto.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: DTO genérico de saída para listagens paginadas.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Nenhuma (envelope de qualquer consulta paginada).
// Fontes.....: Preenchido pelos serviços (ex.: ClienteService.ListarAsync).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public record ResultadoPaginadoDto<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens)
{
    public int TotalPaginas => (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
}
