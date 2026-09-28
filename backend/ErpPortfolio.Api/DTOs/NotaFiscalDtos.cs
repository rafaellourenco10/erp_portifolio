// =====================================================================================
// Arquivo....: NotaFiscalDtos.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: DTOs da NF-e simulada (etapa 16): detalhe com itens e impostos, linha da
//              lista e filtros da lista (NF9).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: NotasFiscaisController, NotaFiscalService.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public record NotaFiscalItemDto(
    int NumeroItem, int ProdutoId, string Codigo, string Descricao, string Ncm, string Cfop, string Unidade,
    decimal Quantidade, decimal ValorUnitario, decimal ValorBruto, decimal ValorDesconto,
    decimal BaseIcms, decimal AliquotaIcms, decimal ValorIcms, decimal ValorPis, decimal ValorCofins)
{
    public static NotaFiscalItemDto DeEntidade(NotaFiscalItem i) => new(
        i.NumeroItem, i.ProdutoId, i.Codigo, i.Descricao, i.Ncm, i.Cfop, i.Unidade,
        i.Quantidade, i.ValorUnitario, i.ValorBruto, i.ValorDesconto,
        i.BaseIcms, i.AliquotaIcms, i.ValorIcms, i.ValorPis, i.ValorCofins);
}

public record NotaFiscalResumoDto(
    int Id, TipoNotaFiscal Tipo, int Serie, int Numero, string Chave, DateTime DataEmissao,
    int ClienteId, string DestinatarioNome, string DestinatarioDocumento, string DestinatarioUf,
    int PedidoId, int? DevolucaoId, decimal ValorTotal);

public record NotaFiscalDetalheDto(
    int Id, TipoNotaFiscal Tipo, int Serie, int Numero, string Chave, DateTime DataEmissao, string Protocolo,
    int ClienteId, string DestinatarioNome, string DestinatarioDocumento, string DestinatarioUf,
    int PedidoId, int? DevolucaoId, int? NotaReferenciadaId, string? ChaveReferenciada,
    decimal ValorProdutos, decimal ValorDesconto, decimal BaseIcms, decimal ValorIcms,
    decimal ValorPis, decimal ValorCofins, decimal ValorTotal,
    IReadOnlyList<NotaFiscalItemDto> Itens)
{
    /// <param name="nota">Com os itens carregados.</param>
    /// <param name="chaveReferenciada">Só na devolução: chave da nota de saída devolvida.</param>
    public static NotaFiscalDetalheDto DeEntidade(NotaFiscal nota, string? chaveReferenciada) => new(
        nota.Id, nota.Tipo, nota.Serie, nota.Numero, nota.Chave, nota.DataEmissao, nota.Protocolo,
        nota.ClienteId, nota.DestinatarioNome, nota.DestinatarioDocumento, nota.DestinatarioUf,
        nota.PedidoId, nota.DevolucaoId, nota.NotaReferenciadaId, chaveReferenciada,
        nota.ValorProdutos, nota.ValorDesconto, nota.BaseIcms, nota.ValorIcms,
        nota.ValorPis, nota.ValorCofins, nota.ValorTotal,
        nota.Itens.OrderBy(i => i.NumeroItem).Select(NotaFiscalItemDto.DeEntidade).ToList());
}

/// <summary>Filtros da lista de notas (NF9). Período pela data de emissão (dia de Brasília).</summary>
public class NotaFiscalFiltroDto : IValidatableObject
{
    public DateOnly? DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    public int? ClienteId { get; set; }

    public TipoNotaFiscal? Tipo { get; set; }

    /// <summary>Número da nota ou parte da chave de acesso.</summary>
    [StringLength(44)]
    public string? Busca { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que zero.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 20;

    public FormatoRelatorio Formato { get; set; } = FormatoRelatorio.Json;

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (DataInicio is { } inicio && DataFim is { } fim && fim < inicio)
            yield return new("A data final deve ser igual ou posterior à inicial.", [nameof(DataFim)]);
    }
}
