// =====================================================================================
// Arquivo....: PedidoCriacaoDto.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: DTO de entrada do pedido: serve para o POST /api/pedidos e também para o
//              PUT /api/pedidos/{id} (que substitui cliente, itens, desconto e pagamento
//              de um rascunho). Regras R4 e R8 da SPEC.md: 1 a 100 itens, sem produto
//              repetido, descontos de 0 a 100 com até 2 casas.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.pedidos e public.pedido_itens pelo
//              PedidoService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.DTOs;

public class PedidoCriacaoDto : IValidatableObject
{
    /// <example>1</example>
    [Required(ErrorMessage = "O cliente é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "O cliente é obrigatório.")]
    public int? ClienteId { get; set; }

    /// <summary>Opcional no rascunho; obrigatória para confirmar. Dinheiro, Pix, Boleto ou Cartao.</summary>
    /// <example>Pix</example>
    // O conversor de JSON aceita números (99 viraria um valor inexistente), por isso a validação do enum.
    [EnumDataType(typeof(FormaPagamento), ErrorMessage = "Forma de pagamento inválida.")]
    public FormaPagamento? FormaPagamento { get; set; }

    /// <summary>Desconto sobre a soma dos itens, em %, de 0 a 100, com até 2 casas.</summary>
    /// <example>5</example>
    // 0.0 e 100.0 (double) de propósito: o overload Range(int, int) converte o valor para inteiro e deixaria passar 100,01.
    [Range(0.0, 100.0, ErrorMessage = "O desconto do pedido deve estar entre 0 e 100.")]
    public decimal DescontoPercentual { get; set; }

    /// <summary>De 1 a 100 itens; o mesmo produto não pode repetir.</summary>
    [Required(ErrorMessage = "Informe ao menos um item.")]
    [MinLength(1, ErrorMessage = "Informe ao menos um item.")]
    [MaxLength(100, ErrorMessage = "O pedido pode ter no máximo 100 itens.")]
    public List<PedidoItemEntradaDto> Itens { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (decimal.Round(DescontoPercentual, 2) != DescontoPercentual)
            yield return new("O desconto do pedido deve ter no máximo 2 casas decimais.", [nameof(DescontoPercentual)]);

        if (Itens.GroupBy(i => i.ProdutoId).Any(g => g.Count() > 1))
            yield return new("O mesmo produto não pode aparecer mais de uma vez no pedido (repetido).", [nameof(Itens)]);
    }
}
