using ErpPortfolio.Api.Services;
using static ErpPortfolio.Api.Services.NfeCalculo;

namespace ErpPortfolio.Tests;

public class NfeCalculoTests
{
    // ---------------------------------------------------------------- CFOP e alíquota (NF3)

    [Theory]
    [InlineData(false, "SP", "SP", "5102")]
    [InlineData(false, "SP", "RJ", "6102")]
    [InlineData(true, "SP", "SP", "1202")]
    [InlineData(true, "SP", "BA", "2202")]
    public void Cfop_depende_da_operacao_e_da_uf(bool devolucao, string origem, string destino, string esperado) =>
        Assert.Equal(esperado, Cfop(devolucao, origem, destino));

    [Theory]
    [InlineData("SP", "SP", "18")]   // interna do emitente
    [InlineData("MA", "MA", "23")]
    [InlineData("SP", "RJ", "12")]   // Sul/Sudeste → Sul/Sudeste
    [InlineData("SP", "BA", "7")]    // Sul/Sudeste → Nordeste
    [InlineData("SP", "ES", "7")]    // ES recebe 7%
    [InlineData("ES", "BA", "12")]   // ES não dá 7%
    [InlineData("BA", "SP", "12")]
    public void Aliquota_de_icms_interna_ou_interestadual(string origem, string destino, string esperado) =>
        Assert.Equal(decimal.Parse(esperado), AliquotaIcms(origem, destino));

    [Fact]
    public void Todas_as_27_ufs_tem_codigo_e_aliquota_interna()
    {
        Assert.Equal(27, CodigoUf.Count);
        foreach (var uf in CodigoUf.Keys)
            Assert.InRange(AliquotaIcms(uf, uf), 17m, 23m);
    }

    // ---------------------------------------------------------------- itens e impostos (NF2, NF4)

    [Fact]
    public void Item_sem_desconto_calcula_os_tres_impostos()
    {
        var item = Assert.Single(Itens([new ItemEntrada(2, 50, 100)], 100, 18));

        Assert.Equal(new ItemCalculado(100, 0, 100, 18, 18, 1.65m, 7.60m), item);
    }

    [Fact]
    public void Desconto_do_item_e_do_pedido_viram_vDesc_e_a_base_fecha_com_o_total()
    {
        // Item 1: 100 com 10% → 90. Item 2: 50 sem desconto. Pedido com 5%: 140 × 0,95 = 133.
        var total = CalculoPedido.Total([90, 50], 5);
        var itens = Itens([new ItemEntrada(1, 100, 90), new ItemEntrada(1, 50, 50)], total, 12);

        Assert.Equal(133m, total);
        Assert.Equal(85.50m, itens[0].BaseIcms);     // 90 × 133/140
        Assert.Equal(14.50m, itens[0].ValorDesconto);
        Assert.Equal(47.50m, itens[1].BaseIcms);
        Assert.Equal(total, Somar(itens).ValorTotal);
    }

    [Fact]
    public void Sobra_de_centavos_do_rateio_fica_no_ultimo_item()
    {
        // 3 itens de 33,33 com 3,33% no pedido: cada parte arredonda e o último fecha o total.
        var total = CalculoPedido.Total([33.33m, 33.33m, 33.33m], 3.33m);
        var itens = Itens([new(1, 33.33m, 33.33m), new(1, 33.33m, 33.33m), new(1, 33.33m, 33.33m)], total, 18);

        Assert.Equal(total, itens.Sum(i => i.BaseIcms));
        Assert.Equal(total, Somar(itens).ValorTotal);
        Assert.All(itens, i => Assert.Equal(i.ValorBruto - i.ValorDesconto, i.BaseIcms));
    }

    [Fact]
    public void Quantidade_fracionada_arredonda_o_valor_bruto()
    {
        var item = Assert.Single(Itens([new ItemEntrada(1.235m, 10.01m, 12.36m)], 12.36m, 18));

        Assert.Equal(12.36m, item.ValorBruto);   // 12,36235
        Assert.Equal(0m, item.ValorDesconto);
        Assert.Equal(2.22m, item.ValorIcms);     // 2,2248
    }

    [Fact]
    public void Pedido_com_100_por_cento_de_desconto_zera_tudo_sem_dividir_por_zero()
    {
        var itens = Itens([new ItemEntrada(1, 10, 0), new ItemEntrada(1, 20, 0)], 0, 18);

        Assert.All(itens, i => Assert.Equal(0m, i.BaseIcms));
        Assert.Equal(30m, Somar(itens).ValorDesconto);
    }

    [Fact]
    public void Totais_somam_os_itens()
    {
        var totais = Somar([
            new ItemCalculado(100, 10, 90, 18, 16.20m, 1.49m, 6.84m),
            new ItemCalculado(50, 0, 50, 18, 9.00m, 0.83m, 3.80m)]);

        Assert.Equal(new Totais(150, 10, 140, 25.20m, 2.32m, 10.64m, 140), totais);
    }

    // ---------------------------------------------------------------- chave (NF5)

    [Fact]
    public void Dv_confere_com_o_exemplo_do_manual_da_nfe() =>
        Assert.Equal(5, DigitoVerificador("5206043300991100250655012000000780026730161"));

    [Fact]
    public void Chave_tem_44_posicoes_na_ordem_do_layout()
    {
        var chave = Chave("SP", new DateTime(2026, 9, 28), "11222333000181", 1, 42, 12345678);

        Assert.Equal(44, chave.Length);
        Assert.Equal("35" + "2609" + "11222333000181" + "55" + "001" + "000000042" + "1" + "12345678", chave[..43]);
        Assert.Equal(DigitoVerificador(chave[..43]), chave[43] - '0');
    }

    [Fact]
    public void Dv_aceita_cnpj_alfanumerico_com_letra_valendo_ascii_menos_48()
    {
        // "A" vale 17: trocar um "0" por "A" na mesma posição muda a soma em 17 × peso.
        var comNumero = "3526091234567800019555001000000042112345678";
        var comLetra = "35260912ABC67800019555001000000042112345678";

        Assert.InRange(DigitoVerificador(comLetra), 0, 9);
        Assert.NotEqual(DigitoVerificador(comNumero), DigitoVerificador(comLetra));
    }

    [Theory]
    [InlineData("12345678", true)]
    [InlineData("1234567", false)]
    [InlineData("1234567A", false)]
    [InlineData(null, false)]
    public void Ncm_tem_8_digitos(string? ncm, bool esperado) => Assert.Equal(esperado, NcmValido(ncm));
}
