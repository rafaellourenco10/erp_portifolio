// =====================================================================================
// Arquivo....: ErpPortfolioDbContext.cs
// Versão.....: 1.2.0
// Data.......: 21/09/2026
// Descrição..: DbContext do EF Core. Define os DbSets e o mapeamento das entidades
//              para as tabelas do PostgreSQL (nomes em snake_case).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.clientes
//                - PK  : pk_clientes (id, identity)
//                - UK  : ix_clientes_documento (documento)
//                - IDX : ix_clientes_nome (nome)
//              public.categorias
//                - PK  : pk_categorias (id, identity)
//                - UK  : ix_categorias_nome (nome)
//              public.produtos
//                - PK  : pk_produtos (id, identity)
//                - UK  : ix_produtos_sku (sku)
//                - IDX : ix_produtos_nome (nome), ix_produtos_categoria_id (categoria_id)
//                - FK  : fk_produtos_categorias (categoria_id -> categorias.id, restrict)
//              public.__EFMigrationsHistory (controle de migrations do EF Core)
// Fontes.....: Npgsql.EntityFrameworkCore.PostgreSQL. Migrations em Data/Migrations.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo com o mapeamento de Cliente.
//   1.1.0 - 21/09/2026 - Mapeamento de Produto (tabela produtos).
//   1.2.0 - 21/09/2026 - Mapeamento de Categoria e FK produtos.categoria_id.
// =====================================================================================

using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Data;

public class ErpPortfolioDbContext(DbContextOptions<ErpPortfolioDbContext> opcoes) : DbContext(opcoes)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entidade =>
        {
            entidade.ToTable("categorias");

            entidade.HasKey(c => c.Id).HasName("pk_categorias");

            entidade.Property(c => c.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(c => c.Nome)
                .HasColumnName("nome")
                .HasMaxLength(60)
                .IsRequired();

            entidade.Property(c => c.Ativo)
                .HasColumnName("ativo")
                .IsRequired();

            entidade.Property(c => c.DataCadastro)
                .HasColumnName("data_cadastro")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // Índice exato: o "sem diferenciar maiúsculas" é garantido pelo CategoriaService (ILIKE);
            // este índice só cobre duas gravações simultâneas do mesmo texto.
            entidade.HasIndex(c => c.Nome)
                .IsUnique()
                .HasDatabaseName("ix_categorias_nome");
        });

        modelBuilder.Entity<Produto>(entidade =>
        {
            entidade.ToTable("produtos");

            entidade.HasKey(p => p.Id).HasName("pk_produtos");

            entidade.Property(p => p.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(p => p.Nome)
                .HasColumnName("nome")
                .HasMaxLength(150)
                .IsRequired();

            entidade.Property(p => p.Sku)
                .HasColumnName("sku")
                .HasMaxLength(30)
                .IsRequired();

            entidade.Property(p => p.CategoriaId)
                .HasColumnName("categoria_id");

            // Restrict: categoria só é inativada, nunca excluída; o banco impede apagar uma que ainda tenha produtos.
            entidade.HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .HasConstraintName("fk_produtos_categorias")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(p => p.Unidade)
                .HasColumnName("unidade")
                .HasMaxLength(2)
                .IsRequired();

            entidade.Property(p => p.PrecoVenda)
                .HasColumnName("preco_venda")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.Property(p => p.Custo)
                .HasColumnName("custo")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.Property(p => p.Ativo)
                .HasColumnName("ativo")
                .IsRequired();

            entidade.Property(p => p.DataCadastro)
                .HasColumnName("data_cadastro")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entidade.HasIndex(p => p.Sku)
                .IsUnique()
                .HasDatabaseName("ix_produtos_sku");

            entidade.HasIndex(p => p.Nome)
                .HasDatabaseName("ix_produtos_nome");

            entidade.HasIndex(p => p.CategoriaId)
                .HasDatabaseName("ix_produtos_categoria_id");
        });

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
