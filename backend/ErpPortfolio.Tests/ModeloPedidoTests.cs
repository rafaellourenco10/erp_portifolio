// =====================================================================================
// Arquivo....: ModeloPedidoTests.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Testes do mapeamento EF das tabelas pedidos e pedido_itens. O EF monta o
//              modelo em memória (sem conectar ao banco) e o teste confere nomes, tipos,
//              chaves estrangeiras, índices e restrições CHECK definidos na SPEC.md.
// -------------------------------------------------------------------------------------
// Banco......: Não conecta ao banco (só constrói o modelo do EF Core).
// Tabelas....: public.pedidos e public.pedido_itens (apenas o desenho, não os dados).
// Fontes.....: Data/ErpPortfolioDbContext.cs, Models/Pedido.cs, Models/PedidoItem.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ErpPortfolio.Tests;

public class ModeloPedidoTests
{
    private static readonly IModel Modelo = ConstruirModelo();

    private static IModel ConstruirModelo()
    {
        var opcoes = new DbContextOptionsBuilder<ErpPortfolioDbContext>().UseNpgsql("Host=nao-conecta;Database=x").Options;
        using var contexto = new ErpPortfolioDbContext(opcoes);
        return contexto.GetService<IDesignTimeModel>().Model;
    }

    private static IEntityType Entidade<T>() => Modelo.FindEntityType(typeof(T))!;

    private static (string? Coluna, string? Tipo) Coluna<T>(string propriedade)
    {
        var entidade = Entidade<T>();
        var tabela = StoreObjectIdentifier.Table(entidade.GetTableName()!, entidade.GetSchema());
        var prop = entidade.FindProperty(propriedade)!;
        return (prop.GetColumnName(tabela), prop.GetColumnType());
    }

    [Fact]
    public void Tabelas_tem_os_nomes_da_spec()
    {
        Assert.Equal("pedidos", Entidade<Pedido>().GetTableName());
        Assert.Equal("pedido_itens", Entidade<PedidoItem>().GetTableName());
    }

    [Theory]
    [InlineData(nameof(Pedido.Id), "id", "integer")]
    [InlineData(nameof(Pedido.ClienteId), "cliente_id", "integer")]
    [InlineData(nameof(Pedido.DataPedido), "data_pedido", "timestamp with time zone")]
    [InlineData(nameof(Pedido.Status), "status", "character varying(20)")]
    [InlineData(nameof(Pedido.FormaPagamento), "forma_pagamento", "character varying(20)")]
    [InlineData(nameof(Pedido.DescontoPercentual), "desconto_percentual", "numeric(5,2)")]
    [InlineData(nameof(Pedido.ValorTotal), "valor_total", "numeric(12,2)")]
    public void Colunas_de_pedidos(string propriedade, string coluna, string tipo) =>
        Assert.Equal((coluna, tipo), Coluna<Pedido>(propriedade));

    [Theory]
    [InlineData(nameof(PedidoItem.Id), "id", "integer")]
    [InlineData(nameof(PedidoItem.PedidoId), "pedido_id", "integer")]
    [InlineData(nameof(PedidoItem.ProdutoId), "produto_id", "integer")]
    [InlineData(nameof(PedidoItem.Quantidade), "quantidade", "numeric(12,3)")]
    [InlineData(nameof(PedidoItem.PrecoUnitario), "preco_unitario", "numeric(12,2)")]
    [InlineData(nameof(PedidoItem.DescontoPercentual), "desconto_percentual", "numeric(5,2)")]
    public void Colunas_de_pedido_itens(string propriedade, string coluna, string tipo) =>
        Assert.Equal((coluna, tipo), Coluna<PedidoItem>(propriedade));

    [Fact]
    public void Status_e_forma_de_pagamento_sao_gravados_como_texto()
    {
        Assert.Equal(typeof(string), Entidade<Pedido>().FindProperty(nameof(Pedido.Status))!.GetProviderClrType());
        Assert.Equal(typeof(string), Entidade<Pedido>().FindProperty(nameof(Pedido.FormaPagamento))!.GetProviderClrType());
    }

    [Fact]
    public void Forma_de_pagamento_e_opcional_e_status_e_obrigatorio()
    {
        Assert.True(Entidade<Pedido>().FindProperty(nameof(Pedido.FormaPagamento))!.IsNullable);
        Assert.False(Entidade<Pedido>().FindProperty(nameof(Pedido.Status))!.IsNullable);
    }

    [Theory]
    [InlineData(typeof(Pedido), typeof(Cliente), "fk_pedidos_clientes", DeleteBehavior.Restrict)]
    [InlineData(typeof(PedidoItem), typeof(Pedido), "fk_pedido_itens_pedidos", DeleteBehavior.Cascade)]
    [InlineData(typeof(PedidoItem), typeof(Produto), "fk_pedido_itens_produtos", DeleteBehavior.Restrict)]
    public void Chaves_estrangeiras(Type dependente, Type principal, string nome, DeleteBehavior comportamento)
    {
        var fk = Modelo.FindEntityType(dependente)!.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == principal);

        Assert.Equal(nome, fk.GetConstraintName());
        Assert.Equal(comportamento, fk.DeleteBehavior);
    }

    [Fact]
    public void Indices_da_spec()
    {
        var pedidos = Entidade<Pedido>().GetIndexes().ToDictionary(i => i.GetDatabaseName()!);
        Assert.True(pedidos.ContainsKey("ix_pedidos_cliente_id"));
        Assert.True(pedidos.ContainsKey("ix_pedidos_data_pedido"));

        var itens = Entidade<PedidoItem>().GetIndexes().ToDictionary(i => i.GetDatabaseName()!);
        Assert.True(itens.ContainsKey("ix_pedido_itens_produto_id"));

        var unico = itens["ux_pedido_itens_pedido_produto"];
        Assert.True(unico.IsUnique);
        Assert.Equal([nameof(PedidoItem.PedidoId), nameof(PedidoItem.ProdutoId)], unico.Properties.Select(p => p.Name));
    }

    [Theory]
    [InlineData(typeof(Pedido), "ck_pedidos_desconto_percentual")]
    [InlineData(typeof(Pedido), "ck_pedidos_valor_total")]
    [InlineData(typeof(PedidoItem), "ck_pedido_itens_quantidade")]
    [InlineData(typeof(PedidoItem), "ck_pedido_itens_preco_unitario")]
    [InlineData(typeof(PedidoItem), "ck_pedido_itens_desconto_percentual")]
    public void Restricoes_check_da_spec(Type entidade, string nome) =>
        Assert.Contains(Modelo.FindEntityType(entidade)!.GetCheckConstraints(), c => c.Name == nome);

    [Fact]
    public void Restricao_de_quantidade_exige_maior_que_zero_e_a_de_desconto_vai_de_0_a_100()
    {
        var quantidade = Entidade<PedidoItem>().GetCheckConstraints().Single(c => c.Name == "ck_pedido_itens_quantidade").Sql;
        var desconto = Entidade<PedidoItem>().GetCheckConstraints().Single(c => c.Name == "ck_pedido_itens_desconto_percentual").Sql;

        Assert.Equal("quantidade > 0", quantidade);
        Assert.Equal("desconto_percentual >= 0 AND desconto_percentual <= 100", desconto);
    }
}
