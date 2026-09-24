using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class OrcamentoTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 23);

    private static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }

    private static bool TemErroEm(List<ValidationResult> resultados, string campo) =>
        resultados.Any(r => r.MemberNames.Contains(campo));

    private static OrcamentoCriacaoDto Dto(params int[] produtos) => new()
    {
        ClienteId = 1,
        Validade = Hoje.AddDays(15),
        Itens = [.. produtos.Select(p => new PedidoItemEntradaDto { ProdutoId = p, Quantidade = 1m })]
    };

    // ---------------------------------------------------------------- vencido (OR4)

    [Theory]
    [InlineData(-1, true)]   // validade ontem
    [InlineData(0, false)]   // vale até o fim de hoje
    [InlineData(1, false)]
    public void Aberto_vence_so_depois_da_validade(int diasValidade, bool vencido) =>
        Assert.Equal(vencido, new Orcamento { Status = StatusOrcamento.Aberto, Validade = Hoje.AddDays(diasValidade) }.EstaVencido(Hoje));

    [Theory]
    [InlineData(StatusOrcamento.Aprovado)]
    [InlineData(StatusOrcamento.Perdido)]
    public void Fechado_nunca_esta_vencido(StatusOrcamento status) =>
        Assert.False(new Orcamento { Status = status, Validade = Hoje.AddDays(-30) }.EstaVencido(Hoje));

    // ---------------------------------------------------------------- DTO (OR1)

    [Fact]
    public void Dto_valido_nao_tem_erros() => Assert.Empty(Validar(Dto(1, 2)));

    [Fact]
    public void Dto_exige_validade()
    {
        var dto = Dto(1);
        dto.Validade = null;
        Assert.True(TemErroEm(Validar(dto), nameof(OrcamentoCriacaoDto.Validade)));
    }

    [Fact]
    public void Dto_exige_cliente()
    {
        var dto = Dto(1);
        dto.ClienteId = null;
        Assert.True(TemErroEm(Validar(dto), nameof(OrcamentoCriacaoDto.ClienteId)));
    }

    [Fact]
    public void Dto_exige_ao_menos_um_item() =>
        Assert.True(TemErroEm(Validar(Dto()), nameof(OrcamentoCriacaoDto.Itens)));

    [Fact]
    public void Dto_recusa_produto_repetido() =>
        Assert.True(TemErroEm(Validar(Dto(1, 1)), nameof(OrcamentoCriacaoDto.Itens)));

    [Fact]
    public void Dto_recusa_observacoes_acima_de_500()
    {
        var dto = Dto(1);
        dto.Observacoes = new string('x', 501);
        Assert.True(TemErroEm(Validar(dto), nameof(OrcamentoCriacaoDto.Observacoes)));

        dto.Observacoes = new string('x', 500);
        Assert.Empty(Validar(dto));
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("100.01")]
    [InlineData("10.005")]
    public void Dto_recusa_desconto_fora_da_regra(string texto)
    {
        var dto = Dto(1);
        dto.DescontoPercentual = decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(TemErroEm(Validar(dto), nameof(OrcamentoCriacaoDto.DescontoPercentual)));
    }

    // ---------------------------------------------------------------- PDF (PD1)

    [Theory]
    [InlineData("52998224725", "529.982.247-25")]
    [InlineData("11222333000181", "11.222.333/0001-81")]
    [InlineData("12ABC34501DE35", "12.ABC.345/01DE-35")]   // CNPJ alfanumérico
    [InlineData("123", "123")]                             // fora do padrão: sai como está
    public void Pdf_formata_cpf_e_cnpj(string documento, string esperado) =>
        Assert.Equal(esperado, ExportadorOrcamento.FormatarDocumento(documento));
}
