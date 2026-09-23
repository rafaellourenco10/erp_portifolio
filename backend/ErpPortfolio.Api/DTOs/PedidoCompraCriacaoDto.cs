// =====================================================================================
// Arquivo....: PedidoCompraCriacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: DTO de entrada do pedido de compra: serve para o POST /api/pedidos-compra e
//              também para o PUT /api/pedidos-compra/{id} (que substitui fornecedor, itens
//              e desconto de um rascunho). Regras PC4 da SPEC.md: 1 a 100 itens, sem
//              produto repetido, descontos de 0 a 100 com até 2 casas. Espelho de
//              PedidoCriacaoDto, sem forma de pagamento.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.pedidos_compra e public.pedido_compra_itens
//              pelo PedidoCompraService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class PedidoCompraCriacaoDto : IValidatableObject
{
    /// <example>1</example>
    [Required(ErrorMessage = "O fornecedor é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "O fornecedor é obrigatório.")]
    public int? FornecedorId { get; set; }

    /// <summary>Desconto sobre a soma dos itens, em %, de 0 a 100, com até 2 casas.</summary>
    /// <example>5</example>
    // 0.0 e 100.0 (double) de propósito: o overload Range(int, int) converte o valor para inteiro e deixaria passar 100,01.
    [Range(0.0, 100.0, ErrorMessage = "O desconto do pedido deve estar entre 0 e 100.")]
    public decimal DescontoPercentual { get; set; }

    /// <summary>De 1 a 100 itens; o mesmo produto não pode repetir.</summary>
    [Required(ErrorMessage = "Informe ao menos um item.")]
    [MinLength(1, ErrorMessage = "Informe ao menos um item.")]
    [MaxLength(100, ErrorMessage = "O pedido pode ter no máximo 100 itens.")]
    public List<PedidoCompraItemEntradaDto> Itens { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (decimal.Round(DescontoPercentual, 2) != DescontoPercentual)
            yield return new("O desconto do pedido deve ter no máximo 2 casas decimais.", [nameof(DescontoPercentual)]);

        if (Itens.GroupBy(i => i.ProdutoId).Any(g => g.Count() > 1))
            yield return new("O mesmo produto não pode aparecer mais de uma vez no pedido (repetido).", [nameof(Itens)]);
    }
}
