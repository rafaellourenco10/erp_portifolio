// =====================================================================================
// Arquivo....: PedidoItemEntradaDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de entrada de um item do pedido (dentro do corpo de POST/PUT
//              /api/pedidos): produto, quantidade e desconto do item. O preço unitário
//              NÃO é enviado: o servidor o copia do produto (R3 da SPEC.md).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.pedido_itens pelo PedidoService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;

namespace ErpPortfolio.Api.DTOs;

public class PedidoItemEntradaDto : IValidatableObject
{
    /// <example>5</example>
    [Range(1, int.MaxValue, ErrorMessage = "O produto é obrigatório.")]
    public int ProdutoId { get; set; }

    /// <summary>De 0,001 a 999.999,999, com até 3 casas; inteira para produtos em UN e CX.</summary>
    /// <example>2</example>
    [Required(ErrorMessage = "A quantidade é obrigatória.")]
    [Range(0.001, 999_999.999, ErrorMessage = "A quantidade deve estar entre 0,001 e 999.999,999.")]
    public decimal? Quantidade { get; set; }

    /// <summary>Desconto do item em %, de 0 a 100, com até 2 casas.</summary>
    /// <example>10</example>
    // 0.0 e 100.0 (double) de propósito: o overload Range(int, int) converte o valor para inteiro e deixaria passar 100,01.
    [Range(0.0, 100.0, ErrorMessage = "O desconto deve estar entre 0 e 100.")]
    public decimal DescontoPercentual { get; set; }

    // As colunas são numeric(12,3) e numeric(5,2): mais casas seriam arredondadas em silêncio pelo banco.
    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Quantidade is { } quantidade && decimal.Round(quantidade, 3) != quantidade)
            yield return new("A quantidade deve ter no máximo 3 casas decimais.", [nameof(Quantidade)]);

        if (decimal.Round(DescontoPercentual, 2) != DescontoPercentual)
            yield return new("O desconto deve ter no máximo 2 casas decimais.", [nameof(DescontoPercentual)]);
    }
}
