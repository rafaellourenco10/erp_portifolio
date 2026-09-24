// =====================================================================================
// Arquivo....: FluxoCaixaDtos.cs
// Versão.....: 1.0.0
// Data.......: 24/09/2026
// Descrição..: Filtro (query string) e resposta do fluxo de caixa (SPEC.md etapa 15).
//              Período obrigatório e validado aqui (FC7) — erro vira 400 no formato
//              padrão do [ApiController]; os períodos da resposta são os do cálculo.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usados pelo FluxoCaixaService para ler public.parcelas_receber e
//              public.parcelas_pagar.
// Fontes.....: GET /api/fluxo-caixa (FluxoCaixaController).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 24/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Api.DTOs;

public class FluxoCaixaFiltroDto : IValidatableObject
{
    [Required(ErrorMessage = "Informe a data inicial.")]
    public DateOnly? DataInicio { get; set; }

    [Required(ErrorMessage = "Informe a data final.")]
    public DateOnly? DataFim { get; set; }

    /// <summary>Dia (até 93 dias) ou Mes.</summary>
    [EnumDataType(typeof(AgrupamentoFluxoCaixa), ErrorMessage = "Agrupamento inválido.")]
    public AgrupamentoFluxoCaixa Agrupamento { get; set; } = AgrupamentoFluxoCaixa.Dia;

    [EnumDataType(typeof(FormatoRelatorio), ErrorMessage = "Formato inválido.")]
    public FormatoRelatorio Formato { get; set; } = FormatoRelatorio.Json;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataInicio is DateOnly inicio && DataFim is DateOnly fim && FluxoCaixaCalculo.ErroPeriodo(inicio, fim, Agrupamento) is string erro)
            yield return new ValidationResult(erro, [nameof(DataFim)]);
    }
}

/// <summary>Resumo e períodos (FC6); atrasados só quando hoje está no período (senão 0).</summary>
public record FluxoCaixaDto(
    decimal SaldoInicial,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoFinal,
    decimal MenorSaldo,
    DateOnly DataMenorSaldo,
    decimal AtrasadoReceber,
    decimal AtrasadoPagar,
    DateOnly Hoje,
    IReadOnlyList<PeriodoFluxoCaixa> Periodos);
