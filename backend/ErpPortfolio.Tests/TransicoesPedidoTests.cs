// =====================================================================================
// Arquivo....: TransicoesPedidoTests.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Testes das regras de status do pedido (R1/R2 da SPEC.md: quem pode editar,
//              confirmar e cancelar) e da serialização dos enums como texto no JSON.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/TransicoesPedido.cs, Models/StatusPedido.cs, Models/FormaPagamento.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Text.Json;
using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class TransicoesPedidoTests
{
    [Theory]
    [InlineData(StatusPedido.Rascunho, true)]
    [InlineData(StatusPedido.Confirmado, false)]
    [InlineData(StatusPedido.Cancelado, false)]
    public void Somente_rascunho_pode_ser_editado(StatusPedido status, bool esperado) =>
        Assert.Equal(esperado, TransicoesPedido.PodeEditar(status));

    [Theory]
    [InlineData(StatusPedido.Rascunho, true)]
    [InlineData(StatusPedido.Confirmado, false)]
    [InlineData(StatusPedido.Cancelado, false)]
    public void Somente_rascunho_pode_ser_confirmado(StatusPedido status, bool esperado) =>
        Assert.Equal(esperado, TransicoesPedido.PodeConfirmar(status));

    [Theory]
    [InlineData(StatusPedido.Rascunho, true)]
    [InlineData(StatusPedido.Confirmado, true)]
    [InlineData(StatusPedido.Cancelado, false)] // cancelado é final; cancelar de novo é tratado como 204 pelo serviço
    public void Rascunho_e_confirmado_podem_ser_cancelados(StatusPedido status, bool esperado) =>
        Assert.Equal(esperado, TransicoesPedido.PodeCancelar(status));

    [Fact]
    public void Enums_sao_gravados_no_json_como_texto()
    {
        var json = JsonSerializer.Serialize(new { Status = StatusPedido.Confirmado, Forma = FormaPagamento.Pix });

        Assert.Equal("""{"Status":"Confirmado","Forma":"Pix"}""", json);
    }

    [Theory]
    [InlineData("\"Rascunho\"", StatusPedido.Rascunho)]
    [InlineData("\"Cancelado\"", StatusPedido.Cancelado)]
    public void Status_e_lido_do_json_pelo_nome(string json, StatusPedido esperado) =>
        Assert.Equal(esperado, JsonSerializer.Deserialize<StatusPedido>(json));

    [Theory]
    [InlineData("\"Dinheiro\"", FormaPagamento.Dinheiro)]
    [InlineData("\"Pix\"", FormaPagamento.Pix)]
    [InlineData("\"Boleto\"", FormaPagamento.Boleto)]
    [InlineData("\"Cartao\"", FormaPagamento.Cartao)]
    public void Forma_de_pagamento_e_lida_do_json_pelo_nome(string json, FormaPagamento esperado) =>
        Assert.Equal(esperado, JsonSerializer.Deserialize<FormaPagamento>(json));

    [Fact]
    public void Nome_inexistente_no_json_e_recusado() =>
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StatusPedido>("\"Concluido\""));

    // O conversor de enums do System.Text.Json também aceita números (1 vira Confirmado; 99 vira um valor
    // que não existe). Por isso o DTO de entrada valida o enum com [EnumDataType] (tarefa T5).
    [Fact]
    public void Numero_no_json_e_aceito_pelo_conversor_e_precisa_de_validacao_no_dto()
    {
        Assert.Equal(StatusPedido.Confirmado, JsonSerializer.Deserialize<StatusPedido>("1"));
        Assert.False(Enum.IsDefined(JsonSerializer.Deserialize<FormaPagamento>("99")));
    }
}
