// =====================================================================================
// Arquivo....: ComissaoDtos.cs
// Versão.....: 1.1.0
// Data.......: 23/09/2026
// Descrição..: DTOs da tela de comissões: filtro da listagem (vendedor, status e período
//              pela data do recebimento), linha da lista, totais do filtro e o corpo de
//              "gerar conta a pagar" (SPEC.md etapa 11, CM6; etapa 12, CC1/CC2).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (indiretamente).
// Tabelas....: Projeção de public.comissoes com JOIN em vendedores, pedidos, clientes e
//              parcelas_receber (número X de Y).
// Fontes.....: GET /api/comissoes e POST /api/comissoes/gerar-conta (ComissoesController).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
//   1.1.0 - 23/09/2026 - Gerar conta a pagar (substitui o "pagar" direto), total Em pagamento e
//                        vínculo com a conta (etapa 12).
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Api.DTOs;

public class ComissaoFiltroDto : IValidatableObject
{
    public int? VendedorId { get; set; }

    /// <summary>Pendente ou Paga; ausente = todas.</summary>
    [EnumDataType(typeof(StatusComissao), ErrorMessage = "Status inválido.")]
    public StatusComissao? Status { get; set; }

    /// <summary>Data do recebimento da parcela, inclusiva (AAAA-MM-DD). Opcional.</summary>
    public DateOnly? DataInicio { get; set; }

    /// <summary>Data do recebimento da parcela, inclusiva (AAAA-MM-DD). Opcional.</summary>
    public DateOnly? DataFim { get; set; }

    [Range(1, 100_000, ErrorMessage = "A página deve estar entre 1 e 100000.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Mesma regra de período dos relatórios quando as duas datas vêm.
        if (DataInicio is DateOnly inicio && DataFim is DateOnly fim && RelatorioCalculo.ErroPeriodo(inicio, fim) is string erro)
            yield return new ValidationResult(erro, [nameof(DataFim)]);
    }
}

public record ComissaoRespostaDto(
    int Id,
    int VendedorId,
    string VendedorNome,
    int PedidoId,
    string ClienteNome,
    int NumeroParcela,
    int TotalParcelas,
    decimal ValorBase,
    decimal Percentual,
    decimal Valor,
    DateTime DataGeracao,
    StatusComissao Status,
    DateTime? DataPagamento,
    int? ParcelaPagarId);

/// <summary>Somas das comissões que atendem o filtro (todas as páginas).</summary>
public record ComissaoTotaisDto(decimal TotalGerado, decimal TotalPendente, decimal TotalEmPagamento, decimal TotalPago);

public record ComissaoListaDto(ResultadoPaginadoDto<ComissaoRespostaDto> Resultado, ComissaoTotaisDto Totais);

public class ComissaoGerarContaDto
{
    /// <summary>Comissões Pendentes de UM vendedor (SPEC.md etapa 12, CC1).</summary>
    [Required(ErrorMessage = "Informe as comissões.")]
    [MinLength(1, ErrorMessage = "Informe ao menos uma comissão.")]
    [MaxLength(500, ErrorMessage = "Informe no máximo 500 comissões por vez.")]
    public List<int> Ids { get; set; } = [];

    /// <summary>Vencimento da conta a pagar (AAAA-MM-DD).</summary>
    [Required(ErrorMessage = "Informe o vencimento.")]
    public DateOnly? Vencimento { get; set; }
}

/// <summary>Conta a pagar gerada a partir das comissões.</summary>
public record ComissaoContaGeradaDto(int ParcelaPagarId, int QuantidadeComissoes, decimal Valor, DateOnly Vencimento);
