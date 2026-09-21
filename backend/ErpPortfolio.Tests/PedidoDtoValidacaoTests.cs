// =====================================================================================
// Arquivo....: PedidoDtoValidacaoTests.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Testes da validação dos DTOs de entrada do pedido (R4 e R8 da SPEC.md):
//              quantidade, descontos, itens (1 a 100, sem produto repetido) e forma de
//              pagamento. Usa o mesmo mecanismo do [ApiController] (DataAnnotations).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: DTOs/PedidoCriacaoDto.cs e DTOs/PedidoItemEntradaDto.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Tests;

public class PedidoDtoValidacaoTests
{
    private static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }

    private static bool TemErroEm(List<ValidationResult> resultados, string campo) =>
        resultados.Any(r => r.MemberNames.Contains(campo));

    private static PedidoItemEntradaDto Item(int produtoId = 1, decimal? quantidade = 2m, decimal desconto = 0m) =>
        new() { ProdutoId = produtoId, Quantidade = quantidade, DescontoPercentual = desconto };

    private static PedidoCriacaoDto Pedido(params PedidoItemEntradaDto[] itens) =>
        new() { ClienteId = 1, Itens = [.. itens] };

    // ---------------------------------------------------------------- item

    [Fact]
    public void Item_valido_nao_tem_erros() => Assert.Empty(Validar(Item(produtoId: 3, quantidade: 1.5m, desconto: 10m)));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Item_exige_produto(int produtoId) =>
        Assert.True(TemErroEm(Validar(Item(produtoId: produtoId)), nameof(PedidoItemEntradaDto.ProdutoId)));

    [Fact]
    public void Item_exige_quantidade() =>
        Assert.True(TemErroEm(Validar(Item(quantidade: null)), nameof(PedidoItemEntradaDto.Quantidade)));

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("0.0001")]        // menor que o mínimo (0,001)
    [InlineData("1000000")]       // maior que o máximo (999.999,999)
    [InlineData("1.2345")]        // 4 casas decimais
    public void Item_recusa_quantidade_fora_da_regra(string texto) =>
        Assert.True(TemErroEm(Validar(Item(quantidade: decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture))), nameof(PedidoItemEntradaDto.Quantidade)));

    [Theory]
    [InlineData("0.001")]
    [InlineData("1")]
    [InlineData("1.234")]
    [InlineData("999999.999")]
    public void Item_aceita_quantidade_dentro_da_regra(string texto) =>
        Assert.Empty(Validar(Item(quantidade: decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture))));

    [Theory]
    [InlineData("-0.01")]
    [InlineData("100.01")]
    [InlineData("10.005")]        // 3 casas decimais
    public void Item_recusa_desconto_fora_da_regra(string texto) =>
        Assert.True(TemErroEm(Validar(Item(desconto: decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture))), nameof(PedidoItemEntradaDto.DescontoPercentual)));

    [Theory]
    [InlineData("0")]
    [InlineData("33.33")]
    [InlineData("100")]
    public void Item_aceita_desconto_dentro_da_regra(string texto) =>
        Assert.Empty(Validar(Item(desconto: decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture))));

    // -------------------------------------------------------------- pedido

    [Fact]
    public void Pedido_valido_nao_tem_erros() => Assert.Empty(Validar(Pedido(Item(1), Item(2))));

    [Fact]
    public void Pedido_exige_cliente()
    {
        var pedido = Pedido(Item());
        pedido.ClienteId = null;

        Assert.True(TemErroEm(Validar(pedido), nameof(PedidoCriacaoDto.ClienteId)));
    }

    [Fact]
    public void Pedido_recusa_cliente_zero()
    {
        var pedido = Pedido(Item());
        pedido.ClienteId = 0;

        Assert.True(TemErroEm(Validar(pedido), nameof(PedidoCriacaoDto.ClienteId)));
    }

    [Fact]
    public void Pedido_exige_ao_menos_um_item() =>
        Assert.True(TemErroEm(Validar(Pedido()), nameof(PedidoCriacaoDto.Itens)));

    [Fact]
    public void Pedido_aceita_ate_100_itens_e_recusa_101()
    {
        var cem = Enumerable.Range(1, 100).Select(i => Item(produtoId: i)).ToArray();
        var cento_e_um = Enumerable.Range(1, 101).Select(i => Item(produtoId: i)).ToArray();

        Assert.Empty(Validar(Pedido(cem)));
        Assert.True(TemErroEm(Validar(Pedido(cento_e_um)), nameof(PedidoCriacaoDto.Itens)));
    }

    [Fact]
    public void Pedido_recusa_produto_repetido()
    {
        var erros = Validar(Pedido(Item(produtoId: 5), Item(produtoId: 7), Item(produtoId: 5)));

        Assert.True(TemErroEm(erros, nameof(PedidoCriacaoDto.Itens)));
        Assert.Contains(erros, e => e.ErrorMessage!.Contains("repetido", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("5.555")]         // 3 casas decimais
    public void Pedido_recusa_desconto_fora_da_regra(string texto)
    {
        var pedido = Pedido(Item());
        pedido.DescontoPercentual = decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture);

        Assert.True(TemErroEm(Validar(pedido), nameof(PedidoCriacaoDto.DescontoPercentual)));
    }

    [Fact]
    public void Pedido_aceita_desconto_5_por_cento()
    {
        var pedido = Pedido(Item());
        pedido.DescontoPercentual = 5m;

        Assert.Empty(Validar(pedido));
    }

    [Theory]
    [InlineData(FormaPagamento.Dinheiro)]
    [InlineData(FormaPagamento.Pix)]
    [InlineData(FormaPagamento.Boleto)]
    [InlineData(FormaPagamento.Cartao)]
    public void Pedido_aceita_forma_de_pagamento_valida(FormaPagamento forma)
    {
        var pedido = Pedido(Item());
        pedido.FormaPagamento = forma;

        Assert.Empty(Validar(pedido));
    }

    [Fact]
    public void Pedido_aceita_forma_de_pagamento_ausente_no_rascunho()
    {
        var pedido = Pedido(Item());
        pedido.FormaPagamento = null;

        Assert.Empty(Validar(pedido));
    }

    [Fact]
    public void Pedido_recusa_forma_de_pagamento_com_numero_inexistente()
    {
        var pedido = Pedido(Item());
        pedido.FormaPagamento = (FormaPagamento)99; // o conversor de JSON deixa passar; o DTO precisa barrar

        Assert.True(TemErroEm(Validar(pedido), nameof(PedidoCriacaoDto.FormaPagamento)));
    }
}
