// =====================================================================================
// Arquivo....: ErpPortfolioDbContext.cs
// Versão.....: 1.8.0
// Data.......: 23/09/2026
// Descrição..: DbContext do EF Core. Define os DbSets e o mapeamento das entidades
//              para as tabelas do PostgreSQL (nomes em snake_case).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.fornecedores
//                - PK  : pk_fornecedores (id, identity)
//                - UK  : ix_fornecedores_documento (documento)
//                - IDX : ix_fornecedores_nome (nome)
//              public.pedidos_compra
//                - PK  : pk_pedidos_compra (id, identity)
//                - IDX : ix_pedidos_compra_fornecedor_id (fornecedor_id),
//                        ix_pedidos_compra_data_pedido (data_pedido)
//                - FK  : fk_pedidos_compra_fornecedores (fornecedor_id -> fornecedores.id, restrict)
//                - CK  : ck_pedidos_compra_desconto_percentual, ck_pedidos_compra_valor_total
//              public.pedido_compra_itens
//                - PK  : pk_pedido_compra_itens (id, identity)
//                - UK  : ux_pedido_compra_itens_pedido_produto (pedido_compra_id, produto_id)
//                - IDX : ix_pedido_compra_itens_produto_id (produto_id)
//                - FK  : fk_pedido_compra_itens_pedidos_compra (pedido_compra_id -> pedidos_compra.id, cascade),
//                        fk_pedido_compra_itens_produtos (produto_id -> produtos.id, restrict)
//                - CK  : ck_pedido_compra_itens_quantidade, ck_pedido_compra_itens_preco_unitario,
//                        ck_pedido_compra_itens_desconto_percentual
//              public.clientes
//                - PK  : pk_clientes (id, identity)
//                - UK  : ix_clientes_documento (documento)
//                - IDX : ix_clientes_nome (nome)
//              public.pedidos
//                - PK  : pk_pedidos (id, identity)
//                - IDX : ix_pedidos_cliente_id (cliente_id), ix_pedidos_data_pedido (data_pedido)
//                - FK  : fk_pedidos_clientes (cliente_id -> clientes.id, restrict)
//                - CK  : ck_pedidos_desconto_percentual, ck_pedidos_valor_total
//              public.pedido_itens
//                - PK  : pk_pedido_itens (id, identity)
//                - UK  : ux_pedido_itens_pedido_produto (pedido_id, produto_id)
//                - IDX : ix_pedido_itens_produto_id (produto_id)
//                - FK  : fk_pedido_itens_pedidos (pedido_id -> pedidos.id, cascade),
//                        fk_pedido_itens_produtos (produto_id -> produtos.id, restrict)
//                - CK  : ck_pedido_itens_quantidade, ck_pedido_itens_preco_unitario,
//                        ck_pedido_itens_desconto_percentual
//              public.categorias
//                - PK  : pk_categorias (id, identity)
//                - UK  : ix_categorias_nome (nome)
//              public.produtos
//                - PK  : pk_produtos (id, identity)
//                - UK  : ix_produtos_sku (sku)
//                - IDX : ix_produtos_nome (nome), ix_produtos_categoria_id (categoria_id)
//                - FK  : fk_produtos_categorias (categoria_id -> categorias.id, restrict)
//                - CK  : ck_produtos_estoque_minimo
//              public.estoque_movimentacoes
//                - PK  : pk_estoque_movimentacoes (id, identity)
//                - IDX : ix_estoque_movimentacoes_produto_id (produto_id),
//                        ix_estoque_movimentacoes_pedido_id (pedido_id),
//                        ix_estoque_movimentacoes_pedido_compra_id (pedido_compra_id),
//                        ix_estoque_movimentacoes_data_movimentacao (data_movimentacao)
//                - FK  : fk_estoque_movimentacoes_produtos (produto_id -> produtos.id, restrict),
//                        fk_estoque_movimentacoes_pedidos (pedido_id -> pedidos.id, restrict),
//                        fk_estoque_movimentacoes_pedidos_compra (pedido_compra_id -> pedidos_compra.id, restrict)
//                - CK  : ck_estoque_movimentacoes_quantidade
//              public.parcelas_receber
//                - PK  : pk_parcelas_receber (id, identity)
//                - UK  : ux_parcelas_receber_pedido_numero (pedido_id, numero_parcela)
//                - IDX : ix_parcelas_receber_vencimento (vencimento)
//                - FK  : fk_parcelas_receber_pedidos (pedido_id -> pedidos.id, restrict)
//                - CK  : ck_parcelas_receber_valor, ck_parcelas_receber_numero_parcela
//              public.parcelas_pagar
//                - PK  : pk_parcelas_pagar (id, identity)
//                - UK  : ux_parcelas_pagar_pedido_compra_numero (pedido_compra_id, numero_parcela)
//                - IDX : ix_parcelas_pagar_vencimento (vencimento)
//                - FK  : fk_parcelas_pagar_pedidos_compra (pedido_compra_id -> pedidos_compra.id, restrict)
//                - CK  : ck_parcelas_pagar_valor, ck_parcelas_pagar_numero_parcela
//              public.__EFMigrationsHistory (controle de migrations do EF Core)
// Fontes.....: Npgsql.EntityFrameworkCore.PostgreSQL. Migrations em Data/Migrations.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo com o mapeamento de Cliente.
//   1.1.0 - 21/09/2026 - Mapeamento de Produto (tabela produtos).
//   1.2.0 - 21/09/2026 - Mapeamento de Categoria e FK produtos.categoria_id.
//   1.3.0 - 21/09/2026 - Mapeamento de Pedido e PedidoItem (tabelas pedidos e pedido_itens).
//   1.4.0 - 22/09/2026 - Mapeamento de EstoqueMovimentacao (tabela estoque_movimentacoes).
//   1.5.0 - 22/09/2026 - Mapeamento de ParcelaReceber (tabela parcelas_receber).
//   1.6.0 - 22/09/2026 - produtos.estoque_minimo (usado pelo card "saldo baixo" do Dashboard).
//   1.7.0 - 22/09/2026 - Mapeamento de Fornecedor, PedidoCompra e PedidoCompraItem;
//                        estoque_movimentacoes.pedido_compra_id (etapa 7).
//   1.8.0 - 23/09/2026 - Mapeamento de ParcelaPagar (tabela parcelas_pagar, etapa 8).
// =====================================================================================

using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Data;

public class ErpPortfolioDbContext(DbContextOptions<ErpPortfolioDbContext> opcoes) : DbContext(opcoes)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

    public DbSet<EstoqueMovimentacao> EstoqueMovimentacoes => Set<EstoqueMovimentacao>();

    public DbSet<ParcelaReceber> ParcelasReceber => Set<ParcelaReceber>();

    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();

    public DbSet<PedidoCompra> PedidosCompra => Set<PedidoCompra>();

    public DbSet<PedidoCompraItem> PedidoCompraItens => Set<PedidoCompraItem>();

    public DbSet<ParcelaPagar> ParcelasPagar => Set<ParcelaPagar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParcelaReceber>(entidade =>
        {
            entidade.ToTable("parcelas_receber", tabela =>
            {
                tabela.HasCheckConstraint("ck_parcelas_receber_valor", "valor > 0");
                tabela.HasCheckConstraint("ck_parcelas_receber_numero_parcela", "numero_parcela > 0");
            });

            entidade.HasKey(p => p.Id).HasName("pk_parcelas_receber");

            entidade.Property(p => p.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(p => p.PedidoId)
                .HasColumnName("pedido_id");

            // Restrict: o histórico de parcelas nunca é apagado, então o pedido também não pode ser.
            entidade.HasOne(p => p.Pedido)
                .WithMany()
                .HasForeignKey(p => p.PedidoId)
                .HasConstraintName("fk_parcelas_receber_pedidos")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(p => p.NumeroParcela)
                .HasColumnName("numero_parcela")
                .IsRequired();

            entidade.Property(p => p.Valor)
                .HasColumnName("valor")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.Property(p => p.Vencimento)
                .HasColumnName("vencimento")
                .HasColumnType("date")
                .IsRequired();

            // Enum gravado como texto ("Pendente"/"Recebido"/"Cancelado"), legível no banco.
            entidade.Property(p => p.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entidade.Property(p => p.DataRecebimento)
                .HasColumnName("data_recebimento")
                .HasColumnType("timestamp with time zone");

            entidade.HasIndex(p => new { p.PedidoId, p.NumeroParcela })
                .IsUnique()
                .HasDatabaseName("ux_parcelas_receber_pedido_numero");

            entidade.HasIndex(p => p.Vencimento)
                .HasDatabaseName("ix_parcelas_receber_vencimento");
        });

        modelBuilder.Entity<ParcelaPagar>(entidade =>
        {
            entidade.ToTable("parcelas_pagar", tabela =>
            {
                tabela.HasCheckConstraint("ck_parcelas_pagar_valor", "valor > 0");
                tabela.HasCheckConstraint("ck_parcelas_pagar_numero_parcela", "numero_parcela > 0");
            });

            entidade.HasKey(p => p.Id).HasName("pk_parcelas_pagar");

            entidade.Property(p => p.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(p => p.PedidoCompraId)
                .HasColumnName("pedido_compra_id");

            // Restrict: o histórico de parcelas nunca é apagado, então o pedido de compra também não pode ser.
            entidade.HasOne(p => p.PedidoCompra)
                .WithMany()
                .HasForeignKey(p => p.PedidoCompraId)
                .HasConstraintName("fk_parcelas_pagar_pedidos_compra")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(p => p.NumeroParcela)
                .HasColumnName("numero_parcela")
                .IsRequired();

            entidade.Property(p => p.Valor)
                .HasColumnName("valor")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.Property(p => p.Vencimento)
                .HasColumnName("vencimento")
                .HasColumnType("date")
                .IsRequired();

            // Enum gravado como texto ("Pendente"/"Pago"/"Cancelado"), legível no banco.
            entidade.Property(p => p.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entidade.Property(p => p.DataPagamento)
                .HasColumnName("data_pagamento")
                .HasColumnType("timestamp with time zone");

            entidade.HasIndex(p => new { p.PedidoCompraId, p.NumeroParcela })
                .IsUnique()
                .HasDatabaseName("ux_parcelas_pagar_pedido_compra_numero");

            entidade.HasIndex(p => p.Vencimento)
                .HasDatabaseName("ix_parcelas_pagar_vencimento");
        });

        modelBuilder.Entity<EstoqueMovimentacao>(entidade =>
        {
            entidade.ToTable("estoque_movimentacoes", tabela =>
            {
                tabela.HasCheckConstraint("ck_estoque_movimentacoes_quantidade", "quantidade > 0");
            });

            entidade.HasKey(m => m.Id).HasName("pk_estoque_movimentacoes");

            entidade.Property(m => m.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(m => m.ProdutoId)
                .HasColumnName("produto_id");

            // Restrict: o histórico de movimentações nunca é apagado, então o produto também não pode ser.
            entidade.HasOne(m => m.Produto)
                .WithMany()
                .HasForeignKey(m => m.ProdutoId)
                .HasConstraintName("fk_estoque_movimentacoes_produtos")
                .OnDelete(DeleteBehavior.Restrict);

            // Enum gravado como texto ("Entrada"/"Saida"), legível no banco.
            entidade.Property(m => m.Tipo)
                .HasColumnName("tipo")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entidade.Property(m => m.Quantidade)
                .HasColumnName("quantidade")
                .HasColumnType("numeric(12,3)")
                .IsRequired();

            entidade.Property(m => m.Motivo)
                .HasColumnName("motivo")
                .HasMaxLength(200);

            entidade.Property(m => m.PedidoId)
                .HasColumnName("pedido_id");

            // Restrict: pedido nunca é excluído, então não há necessidade de cascade aqui.
            entidade.HasOne(m => m.Pedido)
                .WithMany()
                .HasForeignKey(m => m.PedidoId)
                .HasConstraintName("fk_estoque_movimentacoes_pedidos")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(m => m.PedidoCompraId)
                .HasColumnName("pedido_compra_id");

            // Restrict: pedido de compra nunca é excluído, então não há necessidade de cascade aqui.
            entidade.HasOne(m => m.PedidoCompra)
                .WithMany()
                .HasForeignKey(m => m.PedidoCompraId)
                .HasConstraintName("fk_estoque_movimentacoes_pedidos_compra")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(m => m.DataMovimentacao)
                .HasColumnName("data_movimentacao")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entidade.HasIndex(m => m.ProdutoId)
                .HasDatabaseName("ix_estoque_movimentacoes_produto_id");

            entidade.HasIndex(m => m.PedidoId)
                .HasDatabaseName("ix_estoque_movimentacoes_pedido_id");

            entidade.HasIndex(m => m.PedidoCompraId)
                .HasDatabaseName("ix_estoque_movimentacoes_pedido_compra_id");

            entidade.HasIndex(m => m.DataMovimentacao)
                .HasDatabaseName("ix_estoque_movimentacoes_data_movimentacao");
        });

        modelBuilder.Entity<Pedido>(entidade =>
        {
            entidade.ToTable("pedidos", tabela =>
            {
                tabela.HasCheckConstraint("ck_pedidos_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
                tabela.HasCheckConstraint("ck_pedidos_valor_total", "valor_total >= 0");
            });

            entidade.HasKey(p => p.Id).HasName("pk_pedidos");

            entidade.Property(p => p.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(p => p.ClienteId)
                .HasColumnName("cliente_id");

            // Restrict: pedido nunca é excluído, então o cliente também não pode ser apagado enquanto tiver pedidos.
            entidade.HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .HasConstraintName("fk_pedidos_clientes")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(p => p.DataPedido)
                .HasColumnName("data_pedido")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // Enums gravados como texto ("Rascunho"...), legíveis no banco.
            entidade.Property(p => p.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entidade.Property(p => p.FormaPagamento)
                .HasColumnName("forma_pagamento")
                .HasConversion<string>()
                .HasMaxLength(20);

            entidade.Property(p => p.DescontoPercentual)
                .HasColumnName("desconto_percentual")
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entidade.Property(p => p.ValorTotal)
                .HasColumnName("valor_total")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.HasIndex(p => p.ClienteId)
                .HasDatabaseName("ix_pedidos_cliente_id");

            entidade.HasIndex(p => p.DataPedido)
                .HasDatabaseName("ix_pedidos_data_pedido");
        });

        modelBuilder.Entity<PedidoItem>(entidade =>
        {
            entidade.ToTable("pedido_itens", tabela =>
            {
                tabela.HasCheckConstraint("ck_pedido_itens_quantidade", "quantidade > 0");
                tabela.HasCheckConstraint("ck_pedido_itens_preco_unitario", "preco_unitario >= 0");
                tabela.HasCheckConstraint("ck_pedido_itens_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
            });

            entidade.HasKey(i => i.Id).HasName("pk_pedido_itens");

            entidade.Property(i => i.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(i => i.PedidoId)
                .HasColumnName("pedido_id");

            // Cascade: os itens só existem dentro do pedido (o rascunho atualiza os itens no lugar).
            entidade.HasOne(i => i.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(i => i.PedidoId)
                .HasConstraintName("fk_pedido_itens_pedidos")
                .OnDelete(DeleteBehavior.Cascade);

            entidade.Property(i => i.ProdutoId)
                .HasColumnName("produto_id");

            entidade.HasOne(i => i.Produto)
                .WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .HasConstraintName("fk_pedido_itens_produtos")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(i => i.Quantidade)
                .HasColumnName("quantidade")
                .HasColumnType("numeric(12,3)")
                .IsRequired();

            entidade.Property(i => i.PrecoUnitario)
                .HasColumnName("preco_unitario")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.Property(i => i.DescontoPercentual)
                .HasColumnName("desconto_percentual")
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entidade.HasIndex(i => new { i.PedidoId, i.ProdutoId })
                .IsUnique()
                .HasDatabaseName("ux_pedido_itens_pedido_produto");

            entidade.HasIndex(i => i.ProdutoId)
                .HasDatabaseName("ix_pedido_itens_produto_id");
        });

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
            entidade.ToTable("produtos", tabela =>
            {
                tabela.HasCheckConstraint("ck_produtos_estoque_minimo", "estoque_minimo >= 0");
            });

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

            entidade.Property(p => p.EstoqueMinimo)
                .HasColumnName("estoque_minimo")
                .HasColumnType("numeric(12,3)")
                .HasDefaultValue(0m)
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

        modelBuilder.Entity<Fornecedor>(entidade =>
        {
            entidade.ToTable("fornecedores");

            entidade.HasKey(f => f.Id).HasName("pk_fornecedores");

            entidade.Property(f => f.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(f => f.Nome)
                .HasColumnName("nome")
                .HasMaxLength(150)
                .IsRequired();

            entidade.Property(f => f.Documento)
                .HasColumnName("documento")
                .HasMaxLength(14)
                .IsRequired();

            entidade.Property(f => f.Email)
                .HasColumnName("email")
                .HasMaxLength(150);

            entidade.Property(f => f.Telefone)
                .HasColumnName("telefone")
                .HasMaxLength(20);

            entidade.Property(f => f.Cidade)
                .HasColumnName("cidade")
                .HasMaxLength(100)
                .IsRequired();

            entidade.Property(f => f.Uf)
                .HasColumnName("uf")
                .HasColumnType("char(2)")
                .IsRequired();

            entidade.Property(f => f.Ativo)
                .HasColumnName("ativo")
                .IsRequired();

            entidade.Property(f => f.DataCadastro)
                .HasColumnName("data_cadastro")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entidade.HasIndex(f => f.Documento)
                .IsUnique()
                .HasDatabaseName("ix_fornecedores_documento");

            entidade.HasIndex(f => f.Nome)
                .HasDatabaseName("ix_fornecedores_nome");
        });

        modelBuilder.Entity<PedidoCompra>(entidade =>
        {
            entidade.ToTable("pedidos_compra", tabela =>
            {
                tabela.HasCheckConstraint("ck_pedidos_compra_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
                tabela.HasCheckConstraint("ck_pedidos_compra_valor_total", "valor_total >= 0");
            });

            entidade.HasKey(p => p.Id).HasName("pk_pedidos_compra");

            entidade.Property(p => p.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(p => p.FornecedorId)
                .HasColumnName("fornecedor_id");

            // Restrict: pedido de compra nunca é excluído, então o fornecedor também não pode ser apagado enquanto tiver pedidos.
            entidade.HasOne(p => p.Fornecedor)
                .WithMany()
                .HasForeignKey(p => p.FornecedorId)
                .HasConstraintName("fk_pedidos_compra_fornecedores")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(p => p.DataPedido)
                .HasColumnName("data_pedido")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            // Enum gravado como texto ("Rascunho"...), legível no banco; reaproveita StatusPedido do Pedido de Venda.
            entidade.Property(p => p.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entidade.Property(p => p.DescontoPercentual)
                .HasColumnName("desconto_percentual")
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entidade.Property(p => p.ValorTotal)
                .HasColumnName("valor_total")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.HasIndex(p => p.FornecedorId)
                .HasDatabaseName("ix_pedidos_compra_fornecedor_id");

            entidade.HasIndex(p => p.DataPedido)
                .HasDatabaseName("ix_pedidos_compra_data_pedido");
        });

        modelBuilder.Entity<PedidoCompraItem>(entidade =>
        {
            entidade.ToTable("pedido_compra_itens", tabela =>
            {
                tabela.HasCheckConstraint("ck_pedido_compra_itens_quantidade", "quantidade > 0");
                tabela.HasCheckConstraint("ck_pedido_compra_itens_preco_unitario", "preco_unitario >= 0");
                tabela.HasCheckConstraint("ck_pedido_compra_itens_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
            });

            entidade.HasKey(i => i.Id).HasName("pk_pedido_compra_itens");

            entidade.Property(i => i.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entidade.Property(i => i.PedidoCompraId)
                .HasColumnName("pedido_compra_id");

            // Cascade: os itens só existem dentro do pedido (o rascunho atualiza os itens no lugar).
            entidade.HasOne(i => i.PedidoCompra)
                .WithMany(p => p.Itens)
                .HasForeignKey(i => i.PedidoCompraId)
                .HasConstraintName("fk_pedido_compra_itens_pedidos_compra")
                .OnDelete(DeleteBehavior.Cascade);

            entidade.Property(i => i.ProdutoId)
                .HasColumnName("produto_id");

            entidade.HasOne(i => i.Produto)
                .WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .HasConstraintName("fk_pedido_compra_itens_produtos")
                .OnDelete(DeleteBehavior.Restrict);

            entidade.Property(i => i.Quantidade)
                .HasColumnName("quantidade")
                .HasColumnType("numeric(12,3)")
                .IsRequired();

            entidade.Property(i => i.PrecoUnitario)
                .HasColumnName("preco_unitario")
                .HasColumnType("numeric(12,2)")
                .IsRequired();

            entidade.Property(i => i.DescontoPercentual)
                .HasColumnName("desconto_percentual")
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entidade.HasIndex(i => new { i.PedidoCompraId, i.ProdutoId })
                .IsUnique()
                .HasDatabaseName("ux_pedido_compra_itens_pedido_produto");

            entidade.HasIndex(i => i.ProdutoId)
                .HasDatabaseName("ix_pedido_compra_itens_produto_id");
        });
    }
}
