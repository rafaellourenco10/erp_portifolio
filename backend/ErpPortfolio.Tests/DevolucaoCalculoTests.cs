using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;
using static ErpPortfolio.Api.Services.DevolucaoCalculo;

namespace ErpPortfolio.Tests;

public class DevolucaoCalculoTests
{
    // ---------------------------------------------------------------- valor (DV3)

    [Theory]
    [InlineData("1", "100", "0", "0", "100.00")]
    [InlineData("1", "100", "10", "0", "90.00")]
    [InlineData("1", "100", "10", "5", "85.50")]      // 100 × 0,9 × 0,95
    [InlineData("1.5", "10", "0", "5", "14.25")]      // KG fracionado
    [InlineData("1", "33.33", "0", "3.33", "32.22")]  // 32,2201... → 32,22
    public void Valor_do_item_aplica_desconto_do_item_e_do_pedido(string qtd, string preco, string descItem, string descPedido, string esperado) =>
        Assert.Equal(D(esperado), ValorItem(D(qtd), D(preco), D(descItem), D(descPedido)));

    [Fact]
    public void Valor_total_e_a_soma_dos_itens_quando_nao_devolve_tudo() =>
        Assert.Equal(90m, ValorTotal([85.50m, 4.50m], devolveTudo: false, valorTotalPedido: 185.25m, jaDevolvido: 0m));

    [Fact]
    public void Devolucao_que_zera_o_pedido_usa_o_que_falta_para_os_centavos_fecharem()
    {
        // Pedido de 100,00 com 3 itens iguais e 3% de desconto: cada terço arredonda para 32,33 (soma 96,99 ≠ 97,00).
        var terco = ValorItem(1, 33.3333m, 0, 3);
        var primeira = ValorTotal([terco], false, 97m, 0m);
        var segunda = ValorTotal([terco], false, 97m, primeira);
        var ultima = ValorTotal([terco], devolveTudo: true, valorTotalPedido: 97m, jaDevolvido: primeira + segunda);

        Assert.Equal(97m, primeira + segunda + ultima);
    }

    [Fact]
    public void Valor_total_nunca_passa_do_que_falta_devolver() =>
        Assert.Equal(10m, ValorTotal([10.01m], devolveTudo: false, valorTotalPedido: 100m, jaDevolvido: 90m));

    // ---------------------------------------------------------------- abatimento e reembolso (DV4/DV5)

    private static readonly ParcelaPendente[] TresParcelas = [new(11, 1, 100m), new(12, 2, 100m), new(13, 3, 100m)];

    [Fact]
    public void Abate_da_ultima_parcela_para_a_primeira()
    {
        var r = Abater(TresParcelas, 150m);

        Assert.Equal([new AjusteParcela(13, 0m), new AjusteParcela(12, 50m)], r.Ajustes);
        Assert.True(r.Ajustes[0].Cancelar);
        Assert.False(r.Ajustes[1].Cancelar);
        Assert.Equal(150m, r.Abatido);
        Assert.Equal(0m, r.Reembolso);
    }

    [Fact]
    public void Excedente_sobre_as_pendentes_vira_reembolso()
    {
        var r = Abater([new(12, 2, 40m)], 100m);

        Assert.Equal([new AjusteParcela(12, 0m)], r.Ajustes);
        Assert.Equal(40m, r.Abatido);
        Assert.Equal(60m, r.Reembolso);
    }

    [Fact]
    public void Sem_pendentes_tudo_e_reembolso()
    {
        var r = Abater([], 85.50m);

        Assert.Empty(r.Ajustes);
        Assert.Equal(0m, r.Abatido);
        Assert.Equal(85.50m, r.Reembolso);
    }

    [Fact]
    public void Abatimento_exato_zera_a_parcela_sem_reembolso()
    {
        var r = Abater(TresParcelas, 100m);

        Assert.Equal([new AjusteParcela(13, 0m)], r.Ajustes);
        Assert.Equal(0m, r.Reembolso);
    }

    // ---------------------------------------------------------------- estorno de comissão (DV6)

    [Theory]
    [InlineData("60", "5", "-3.00")]
    [InlineData("33.33", "7.5", "-2.50")]   // 2,49975 → 2,50
    [InlineData("0", "5", "0")]             // sem reembolso
    [InlineData("60", "0", "0")]            // pedido com 0%
    public void Estorno_e_negativo_sobre_o_reembolso(string reembolso, string percentual, string esperado) =>
        Assert.Equal(D(esperado), Estorno(D(reembolso), D(percentual)));

    [Fact]
    public void Pedido_sem_vendedor_nao_tem_estorno() => Assert.Equal(0m, Estorno(60m, null));

    private static decimal D(string texto) => decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture);

    // ---------------------------------------------------------------- DTO (DV2)

    private static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }

    private static DevolucaoCriacaoDto Dto(params (int Item, decimal Qtd)[] itens) =>
        new() { Itens = [.. itens.Select(i => new DevolucaoItemEntradaDto { PedidoItemId = i.Item, Quantidade = i.Qtd })] };

    [Fact]
    public void Dto_valido_e_volta_ao_estoque_por_padrao()
    {
        var dto = Dto((1, 1m), (2, 0.5m));
        Assert.Empty(Validar(dto));
        Assert.All(dto.Itens, i => Assert.True(i.VoltaEstoque));
    }

    [Fact]
    public void Dto_exige_itens_sem_repetir()
    {
        Assert.Contains(Validar(Dto()), r => r.MemberNames.Contains(nameof(DevolucaoCriacaoDto.Itens)));
        Assert.Contains(Validar(Dto((1, 1m), (1, 2m))), r => r.MemberNames.Contains(nameof(DevolucaoCriacaoDto.Itens)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1.2345")]
    public void Item_recusa_quantidade_fora_da_regra(string qtd)
    {
        var item = new DevolucaoItemEntradaDto { PedidoItemId = 1, Quantidade = D(qtd) };
        Assert.Contains(Validar(item), r => r.MemberNames.Contains(nameof(DevolucaoItemEntradaDto.Quantidade)));
    }

    [Fact]
    public void Dto_recusa_motivo_acima_de_200()
    {
        var dto = Dto((1, 1m));
        dto.Motivo = new string('x', 201);
        Assert.Contains(Validar(dto), r => r.MemberNames.Contains(nameof(DevolucaoCriacaoDto.Motivo)));
    }
}
