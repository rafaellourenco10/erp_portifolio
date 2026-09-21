// =====================================================================================
// Arquivo....: PedidoFiltroDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Parâmetros de consulta (query string) da listagem de pedidos: busca por
//              número do pedido ou nome do cliente, status e paginação.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo PedidoService para filtrar public.pedidos.
// Fontes.....: Query string de GET /api/pedidos (?busca=&status=&pagina=&tamanhoPagina=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public class PedidoFiltroDto
{
    /// <summary>Número do pedido (ex.: 12 ou #12) ou trecho do nome do cliente (sem diferenciar maiúsculas/minúsculas).</summary>
    [StringLength(150, ErrorMessage = "A busca deve ter no máximo 150 caracteres.")]
    public string? Busca { get; set; }

    /// <summary>Rascunho, Confirmado ou Cancelado; ausente = todos.</summary>
    [EnumDataType(typeof(StatusPedido), ErrorMessage = "Status inválido.")]
    public StatusPedido? Status { get; set; }

    /// <summary>Número da página, começando em 1.</summary>
    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de registros por página (1 a 100).</summary>
    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}
