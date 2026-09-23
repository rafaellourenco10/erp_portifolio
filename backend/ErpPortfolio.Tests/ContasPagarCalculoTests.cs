// =====================================================================================
// Arquivo....: ContasPagarCalculoTests.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Testes da conta avulsa: parcelas (valor e vencimento) e validação do DTO
//              (SPEC.md etapa 12, AV1/AV2).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/ContasPagarCalculo.cs, DTOs/ContaAvulsaCriacaoDto.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class ContasPagarCalculoTests
{
    [Fact]
    public void Uma_parcela_vence_no_primeiro_vencimento()
    {
        var parcela = ContasPagarCalculo.ParcelasAvulsa(2500m, 1, new DateOnly(2026, 10, 5), 30).Single();
        Assert.Equal((2500m, new DateOnly(2026, 10, 5)), parcela);
    }

    [Fact]
    public void Tres_parcelas_somam_o_total_com_resto_na_ultima_e_vencimentos_pelo_intervalo()
    {
        var parcelas = ContasPagarCalculo.ParcelasAvulsa(1000m, 3, new DateOnly(2026, 10, 5), 30);

        Assert.Equal([333.33m, 333.33m, 333.34m], parcelas.Select(p => p.Valor));
        Assert.Equal(1000m, parcelas.Sum(p => p.Valor));
        Assert.Equal([new DateOnly(2026, 10, 5), new DateOnly(2026, 11, 4), new DateOnly(2026, 12, 4)], parcelas.Select(p => p.Vencimento));
    }

    [Fact]
    public void Conta_valida_nao_tem_erro()
    {
        Assert.Empty(Validar(Conta()));
    }

    [Theory]
    [InlineData(nameof(ContaAvulsaCriacaoDto.Descricao))]
    [InlineData(nameof(ContaAvulsaCriacaoDto.ValorTotal))]
    [InlineData(nameof(ContaAvulsaCriacaoDto.PrimeiroVencimento))]
    [InlineData(nameof(ContaAvulsaCriacaoDto.NumeroParcelas))]
    public void Campo_invalido_tem_erro_no_campo(string campo)
    {
        var conta = Conta();
        switch (campo)
        {
            case nameof(ContaAvulsaCriacaoDto.Descricao): conta.Descricao = "ab"; break;
            case nameof(ContaAvulsaCriacaoDto.ValorTotal): conta.ValorTotal = 10.555m; break;
            case nameof(ContaAvulsaCriacaoDto.PrimeiroVencimento): conta.PrimeiroVencimento = null; break;
            case nameof(ContaAvulsaCriacaoDto.NumeroParcelas): conta.NumeroParcelas = 13; break;
        }

        Assert.Contains(Validar(conta), e => e.MemberNames.Contains(campo));
    }

    [Fact]
    public void Valor_menor_que_um_centavo_por_parcela_e_recusado()
    {
        var conta = Conta();
        conta.ValorTotal = 0.05m;
        conta.NumeroParcelas = 12;
        Assert.Contains(Validar(conta), e => e.MemberNames.Contains(nameof(ContaAvulsaCriacaoDto.ValorTotal)));
    }

    private static ContaAvulsaCriacaoDto Conta() => new()
    {
        Descricao = "Aluguel outubro",
        ValorTotal = 2500m,
        PrimeiroVencimento = new DateOnly(2026, 10, 5),
    };

    private static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }
}
