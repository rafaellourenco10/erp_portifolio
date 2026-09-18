// =====================================================================================
// Arquivo....: ErpPortfolioDbContext.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: DbContext do EF Core. Define os DbSets e o mapeamento das entidades
//              para as tabelas do PostgreSQL (nomes em snake_case).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.clientes
//                - PK  : pk_clientes (id, identity)
//                - UK  : ix_clientes_documento (documento)
//                - IDX : ix_clientes_nome (nome)
//              public.__EFMigrationsHistory (controle de migrations do EF Core)
// Fontes.....: Npgsql.EntityFrameworkCore.PostgreSQL. Migrations em Data/Migrations.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo com o mapeamento de Cliente.
// =====================================================================================

using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Data;

public class ErpPortfolioDbContext(DbContextOptions<ErpPortfolioDbContext> opcoes) : DbContext(opcoes)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entidade =>
        {
            entidade.ToTable("clientes");

            entidade.HasKey(c => c.Id).HasName("pk_clientes");

            entidade.Property(c => c.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(c => c.Nome)
                .HasColumnName("nome")
                .HasMaxLength(150)
                .IsRequired();

            entidade.Property(c => c.Documento)
                .HasColumnName("documento")
                .HasMaxLength(14)
                .IsRequired();

            entidade.Property(c => c.Email)
                .HasColumnName("email")
                .HasMaxLength(150);

            entidade.Property(c => c.Telefone)
                .HasColumnName("telefone")
                .HasMaxLength(20);

            entidade.Property(c => c.Cidade)
                .HasColumnName("cidade")
                .HasMaxLength(100)
                .IsRequired();

            entidade.Property(c => c.Uf)
                .HasColumnName("uf")
                .HasColumnType("char(2)")
                .IsRequired();

            entidade.Property(c => c.Ativo)
                .HasColumnName("ativo")
                .IsRequired();

            entidade.Property(c => c.DataCadastro)
                .HasColumnName("data_cadastro")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entidade.HasIndex(c => c.Documento)
                .IsUnique()
                .HasDatabaseName("ix_clientes_documento");

            entidade.HasIndex(c => c.Nome)
                .HasDatabaseName("ix_clientes_nome");
        });
    }
}
