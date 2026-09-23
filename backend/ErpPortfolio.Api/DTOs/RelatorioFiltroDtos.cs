// =====================================================================================
// Arquivo....: RelatorioFiltroDtos.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Parâmetros (query string) dos relatórios. Vendas e Compras compartilham
//              período, status e formato (RelatorioPedidosFiltroDto); cada um acrescenta o
//              seu parceiro (cliente / fornecedor). O período é obrigatório e validado
//              aqui (SPEC.md, R1) — erro vira 400 no formato padrão do [ApiController].
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usados pelo RelatorioService para filtrar public.pedidos, public.pedidos_compra
//              e public.produtos.
// Fontes.....: GET /api/relatorios/vendas | compras | estoque.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Api.DTOs;

public abstract class RelatorioPedidosFiltroDto : IValidatableObject
{
    [Required(ErrorMessage = "Informe a data inicial.")]
    public DateOnly? DataInicio { get; set; }

    [Required(ErrorMessage = "Informe a data final.")]
    public DateOnly? DataFim { get; set; }

    /// <summary>Rascunho, Confirmado ou Cancelado; ausente = todos.</summary>
    [EnumDataType(typeof(StatusPedido), ErrorMessage = "Status inválido.")]
    public StatusPedido? Status { get; set; }

    [EnumDataType(typeof(FormatoRelatorio), ErrorMessage = "Formato inválido.")]
    public FormatoRelatorio Formato { get; set; } = FormatoRelatorio.Json;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataInicio is null || DataFim is null)
            yield break; // [Required] já reporta.

        var erro = RelatorioCalculo.ErroPeriodo(DataInicio.Value, DataFim.Value);
        if (erro is not null)
            yield return new ValidationResult(erro, [nameof(DataFim)]);
    }
}

public class RelatorioVendasFiltroDto : RelatorioPedidosFiltroDto
{
    public int? ClienteId { get; set; }
}

public class RelatorioComprasFiltroDto : RelatorioPedidosFiltroDto
{
    public int? FornecedorId { get; set; }
}

public class RelatorioEstoqueFiltroDto
{
    public int? CategoriaId { get; set; }

    /// <summary>Só produtos com saldo menor ou igual ao estoque mínimo (mesma regra do Dashboard).</summary>
    public bool SomenteAbaixoMinimo { get; set; }

    [EnumDataType(typeof(FormatoRelatorio), ErrorMessage = "Formato inválido.")]
    public FormatoRelatorio Formato { get; set; } = FormatoRelatorio.Json;
}
