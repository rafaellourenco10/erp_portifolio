// =====================================================================================
// Arquivo....: OrcamentoDtos.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: DTOs de orçamentos (SPEC.md etapa 13): criação/edição (OR1), filtro da
//              listagem (com "Vencido" calculado, OR4), linha da lista, resposta com itens,
//              motivo da perda (PE1) e pedido gerado (GP4).
//              Os itens de entrada e de resposta reaproveitam os do pedido (mesmas regras).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usados pelo OrcamentoService para ler/gravar public.orcamentos e
//              public.orcamento_itens.
// Fontes.....: Corpo de POST/PUT e query string de GET /api/orcamentos.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Api.DTOs;

/// <summary>Entrada de POST e PUT /api/orcamentos. A validade ≥ hoje é conferida pelo serviço (depende da data).</summary>
public class OrcamentoCriacaoDto : IValidatableObject
{
    /// <example>1</example>
    [Required(ErrorMessage = "O cliente é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "O cliente é obrigatório.")]
    public int? ClienteId { get; set; }

    /// <summary>Opcional. Vai para o pedido gerado se ainda estiver ativo.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Vendedor inválido.")]
    public int? VendedorId { get; set; }

    /// <summary>Opcional. Dinheiro, Pix, Boleto ou Cartao.</summary>
    /// <example>Pix</example>
    [EnumDataType(typeof(FormaPagamento), ErrorMessage = "Forma de pagamento inválida.")]
    public FormaPagamento? FormaPagamento { get; set; }

    /// <summary>Último dia em que o orçamento vale (não pode ser antes de hoje).</summary>
    /// <example>2026-10-08</example>
    [Required(ErrorMessage = "A validade é obrigatória.")]
    public DateOnly? Validade { get; set; }

    /// <summary>Desconto sobre a soma dos itens, em %, de 0 a 100, com até 2 casas.</summary>
    /// <example>5</example>
    // 0.0 e 100.0 (double): o overload Range(int, int) deixaria passar 100,01 (ver PedidoCriacaoDto).
    [Range(0.0, 100.0, ErrorMessage = "O desconto do orçamento deve estar entre 0 e 100.")]
    public decimal DescontoPercentual { get; set; }

    /// <summary>Texto livre que sai no PDF (condições, prazo de entrega...).</summary>
    [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres.")]
    public string? Observacoes { get; set; }

    /// <summary>De 1 a 100 itens; o mesmo produto não pode repetir.</summary>
    [Required(ErrorMessage = "Informe ao menos um item.")]
    [MinLength(1, ErrorMessage = "Informe ao menos um item.")]
    [MaxLength(100, ErrorMessage = "O orçamento pode ter no máximo 100 itens.")]
    public List<PedidoItemEntradaDto> Itens { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (decimal.Round(DescontoPercentual, 2) != DescontoPercentual)
            yield return new("O desconto do orçamento deve ter no máximo 2 casas decimais.", [nameof(DescontoPercentual)]);

        if (Itens.GroupBy(i => i.ProdutoId).Any(g => g.Count() > 1))
            yield return new("O mesmo produto não pode aparecer mais de uma vez no orçamento (repetido).", [nameof(Itens)]);
    }
}

public enum FiltroStatusOrcamento
{
    /// <summary>Aberto e dentro da validade.</summary>
    Aberto,

    /// <summary>Não gravado: Aberto com a validade antes de hoje (OR4).</summary>
    Vencido,

    Aprovado,
    Perdido
}

public class OrcamentoFiltroDto
{
    /// <summary>Número do orçamento (ex.: 12 ou #12) ou trecho do nome do cliente.</summary>
    [StringLength(150, ErrorMessage = "A busca deve ter no máximo 150 caracteres.")]
    public string? Busca { get; set; }

    /// <summary>Aberto (não vencidos), Vencido, Aprovado ou Perdido; ausente = todos.</summary>
    [EnumDataType(typeof(FiltroStatusOrcamento), ErrorMessage = "Status inválido.")]
    public FiltroStatusOrcamento? Status { get; set; }

    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;
}

/// <summary>Corpo (opcional) de PATCH /api/orcamentos/{id}/perder (PE1).</summary>
public class OrcamentoPerderDto
{
    /// <example>Cliente achou mais barato no concorrente</example>
    [StringLength(200, ErrorMessage = "O motivo deve ter no máximo 200 caracteres.")]
    public string? Motivo { get; set; }
}

/// <summary>Resposta de POST /api/orcamentos/{id}/gerar-pedido (GP4).</summary>
public record OrcamentoPedidoGeradoDto(int PedidoId);

/// <summary>Linha da listagem (sem itens).</summary>
public record OrcamentoResumoDto(
    int Id,
    int ClienteId,
    string ClienteNome,
    DateTime DataOrcamento,
    DateOnly Validade,
    StatusOrcamento Status,
    bool Vencido,
    decimal ValorTotal,
    int QuantidadeItens,
    int? PedidoId);

public record OrcamentoRespostaDto(
    int Id,
    int ClienteId,
    string ClienteNome,
    string ClienteDocumento,
    DateTime DataOrcamento,
    DateOnly Validade,
    StatusOrcamento Status,
    bool Vencido,
    FormaPagamento? FormaPagamento,
    int? VendedorId,
    string? VendedorNome,
    decimal DescontoPercentual,
    decimal SubtotalItens,
    decimal ValorTotal,
    string? Observacoes,
    string? MotivoPerda,
    int? PedidoId,
    IReadOnlyList<PedidoItemRespostaDto> Itens)
{
    public static OrcamentoRespostaDto DeEntidade(Orcamento orcamento, DateOnly hoje)
    {
        var itens = orcamento.Itens
            .OrderBy(i => i.Id)
            .Select(i => new PedidoItemRespostaDto(
                i.Id, i.ProdutoId, i.Produto!.Nome, i.Produto.Sku, i.Produto.Unidade,
                i.Quantidade, i.PrecoUnitario, i.DescontoPercentual,
                CalculoPedido.Subtotal(i.Quantidade, i.PrecoUnitario, i.DescontoPercentual)))
            .ToList();

        return new OrcamentoRespostaDto(
            orcamento.Id,
            orcamento.ClienteId,
            orcamento.Cliente!.Nome,
            orcamento.Cliente.Documento,
            orcamento.DataOrcamento,
            orcamento.Validade,
            orcamento.Status,
            orcamento.EstaVencido(hoje),
            orcamento.FormaPagamento,
            orcamento.VendedorId,
            orcamento.Vendedor?.Nome,
            orcamento.DescontoPercentual,
            itens.Sum(i => i.Subtotal),
            orcamento.ValorTotal,
            orcamento.Observacoes,
            orcamento.MotivoPerda,
            orcamento.PedidoId,
            itens);
    }
}
