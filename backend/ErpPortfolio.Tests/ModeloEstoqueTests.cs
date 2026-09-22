// =====================================================================================
// Arquivo....: ModeloEstoqueTests.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Testes do mapeamento EF da tabela estoque_movimentacoes. O EF monta o
//              modelo em memória (sem conectar ao banco) e o teste confere nomes, tipos,
//              chaves estrangeiras, índices e restrição CHECK definidos na SPEC.md.
// -------------------------------------------------------------------------------------
// Banco......: Não conecta ao banco (só constrói o modelo do EF Core).
// Tabelas....: public.estoque_movimentacoes (apenas o desenho, não os dados).
// Fontes.....: Data/ErpPortfolioDbContext.cs, Models/EstoqueMovimentacao.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ErpPortfolio.Tests;

public class ModeloEstoqueTests
{
    private static readonly IModel Modelo = ConstruirModelo();

    private static IModel ConstruirModelo()
    {
        var opcoes = new DbContextOptionsBuilder<ErpPortfolioDbContext>().UseNpgsql("Host=nao-conecta;Database=x").Options;
        using var contexto = new ErpPortfolioDbContext(opcoes);
        return contexto.GetService<IDesignTimeModel>().Model;
    }

    private static IEntityType Entidade() => Modelo.FindEntityType(typeof(EstoqueMovimentacao))!;

    private static (string? Coluna, string? Tipo) Coluna(string propriedade)
    {
        var entidade = Entidade();
        var tabela = StoreObjectIdentifier.Table(entidade.GetTableName()!, entidade.GetSchema());
        var prop = entidade.FindProperty(propriedade)!;
        return (prop.GetColumnName(tabela), prop.GetColumnType());
    }

    [Fact]
    public void Tabela_tem_o_nome_da_spec() => Assert.Equal("estoque_movimentacoes", Entidade().GetTableName());

    [Theory]
    [InlineData(nameof(EstoqueMovimentacao.Id), "id", "integer")]
    [InlineData(nameof(EstoqueMovimentacao.ProdutoId), "produto_id", "integer")]
    [InlineData(nameof(EstoqueMovimentacao.Tipo), "tipo", "character varying(20)")]
    [InlineData(nameof(EstoqueMovimentacao.Quantidade), "quantidade", "numeric(12,3)")]
    [InlineData(nameof(EstoqueMovimentacao.Motivo), "motivo", "character varying(200)")]
    [InlineData(nameof(EstoqueMovimentacao.PedidoId), "pedido_id", "integer")]
    [InlineData(nameof(EstoqueMovimentacao.DataMovimentacao), "data_movimentacao", "timestamp with time zone")]
    public void Colunas_de_estoque_movimentacoes(string propriedade, string coluna, string tipo) =>
        Assert.Equal((coluna, tipo), Coluna(propriedade));

    [Fact]
    public void Tipo_e_gravado_como_texto() =>
        Assert.Equal(typeof(string), Entidade().FindProperty(nameof(EstoqueMovimentacao.Tipo))!.GetProviderClrType());

    [Fact]
    public void Pedido_id_e_motivo_sao_opcionais_e_produto_e_tipo_sao_obrigatorios()
    {
        Assert.True(Entidade().FindProperty(nameof(EstoqueMovimentacao.PedidoId))!.IsNullable);
        Assert.True(Entidade().FindProperty(nameof(EstoqueMovimentacao.Motivo))!.IsNullable);
        Assert.False(Entidade().FindProperty(nameof(EstoqueMovimentacao.ProdutoId))!.IsNullable);
        Assert.False(Entidade().FindProperty(nameof(EstoqueMovimentacao.Tipo))!.IsNullable);
    }

    [Theory]
    [InlineData(typeof(Produto), "fk_estoque_movimentacoes_produtos", DeleteBehavior.Restrict)]
    [InlineData(typeof(Pedido), "fk_estoque_movimentacoes_pedidos", DeleteBehavior.Restrict)]
    public void Chaves_estrangeiras(Type principal, string nome, DeleteBehavior comportamento)
    {
        var fk = Entidade().GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == principal);

        Assert.Equal(nome, fk.GetConstraintName());
        Assert.Equal(comportamento, fk.DeleteBehavior);
    }

    [Fact]
    public void Indices_da_spec()
    {
        var indices = Entidade().GetIndexes().ToDictionary(i => i.GetDatabaseName()!);

        Assert.True(indices.ContainsKey("ix_estoque_movimentacoes_produto_id"));
        Assert.True(indices.ContainsKey("ix_estoque_movimentacoes_pedido_id"));
        Assert.True(indices.ContainsKey("ix_estoque_movimentacoes_data_movimentacao"));
    }

    [Fact]
    public void Restricao_check_de_quantidade()
    {
        var constraint = Entidade().GetCheckConstraints().Single(c => c.Name == "ck_estoque_movimentacoes_quantidade");

        Assert.Equal("quantidade > 0", constraint.Sql);
    }
}
