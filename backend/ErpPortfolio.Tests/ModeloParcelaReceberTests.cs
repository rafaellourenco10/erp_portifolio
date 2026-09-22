// =====================================================================================
// Arquivo....: ModeloParcelaReceberTests.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Testes do mapeamento EF da tabela parcelas_receber. O EF monta o modelo
//              em memória (sem conectar ao banco) e o teste confere nomes, tipos, chave
//              estrangeira, índices e restrições CHECK definidos na SPEC.md.
// -------------------------------------------------------------------------------------
// Banco......: Não conecta ao banco (só constrói o modelo do EF Core).
// Tabelas....: public.parcelas_receber (apenas o desenho, não os dados).
// Fontes.....: Data/ErpPortfolioDbContext.cs, Models/ParcelaReceber.cs.
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

public class ModeloParcelaReceberTests
{
    private static readonly IModel Modelo = ConstruirModelo();

    private static IModel ConstruirModelo()
    {
        var opcoes = new DbContextOptionsBuilder<ErpPortfolioDbContext>().UseNpgsql("Host=nao-conecta;Database=x").Options;
        using var contexto = new ErpPortfolioDbContext(opcoes);
        return contexto.GetService<IDesignTimeModel>().Model;
    }

    private static IEntityType Entidade() => Modelo.FindEntityType(typeof(ParcelaReceber))!;

    private static (string? Coluna, string? Tipo) Coluna(string propriedade)
    {
        var entidade = Entidade();
        var tabela = StoreObjectIdentifier.Table(entidade.GetTableName()!, entidade.GetSchema());
        var prop = entidade.FindProperty(propriedade)!;
        return (prop.GetColumnName(tabela), prop.GetColumnType());
    }

    [Fact]
    public void Tabela_tem_o_nome_da_spec() => Assert.Equal("parcelas_receber", Entidade().GetTableName());

    [Theory]
    [InlineData(nameof(ParcelaReceber.Id), "id", "integer")]
    [InlineData(nameof(ParcelaReceber.PedidoId), "pedido_id", "integer")]
    [InlineData(nameof(ParcelaReceber.NumeroParcela), "numero_parcela", "integer")]
    [InlineData(nameof(ParcelaReceber.Valor), "valor", "numeric(12,2)")]
    [InlineData(nameof(ParcelaReceber.Vencimento), "vencimento", "date")]
    [InlineData(nameof(ParcelaReceber.Status), "status", "character varying(20)")]
    [InlineData(nameof(ParcelaReceber.DataRecebimento), "data_recebimento", "timestamp with time zone")]
    public void Colunas_de_parcelas_receber(string propriedade, string coluna, string tipo) =>
        Assert.Equal((coluna, tipo), Coluna(propriedade));

    [Fact]
    public void Status_e_gravado_como_texto() =>
        Assert.Equal(typeof(string), Entidade().FindProperty(nameof(ParcelaReceber.Status))!.GetProviderClrType());

    [Fact]
    public void Data_recebimento_e_opcional_e_status_e_obrigatorio()
    {
        Assert.True(Entidade().FindProperty(nameof(ParcelaReceber.DataRecebimento))!.IsNullable);
        Assert.False(Entidade().FindProperty(nameof(ParcelaReceber.Status))!.IsNullable);
    }

    [Fact]
    public void Chave_estrangeira_para_pedido()
    {
        var fk = Entidade().GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Pedido));

        Assert.Equal("fk_parcelas_receber_pedidos", fk.GetConstraintName());
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void Indices_da_spec()
    {
        var indices = Entidade().GetIndexes().ToDictionary(i => i.GetDatabaseName()!);

        Assert.True(indices.ContainsKey("ix_parcelas_receber_vencimento"));

        var unico = indices["ux_parcelas_receber_pedido_numero"];
        Assert.True(unico.IsUnique);
        Assert.Equal([nameof(ParcelaReceber.PedidoId), nameof(ParcelaReceber.NumeroParcela)], unico.Properties.Select(p => p.Name));
    }

    [Theory]
    [InlineData("ck_parcelas_receber_valor")]
    [InlineData("ck_parcelas_receber_numero_parcela")]
    public void Restricoes_check_da_spec(string nome) =>
        Assert.Contains(Entidade().GetCheckConstraints(), c => c.Name == nome);
}
