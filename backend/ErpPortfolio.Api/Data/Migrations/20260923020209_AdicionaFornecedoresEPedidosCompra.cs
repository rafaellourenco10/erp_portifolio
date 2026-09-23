using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaFornecedoresEPedidosCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "pedido_compra_id",
                table: "estoque_movimentacoes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fornecedores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    uf = table.Column<string>(type: "char(2)", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fornecedores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pedidos_compra",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fornecedor_id = table.Column<int>(type: "integer", nullable: false),
                    data_pedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    desconto_percentual = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    valor_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedidos_compra", x => x.id);
                    table.CheckConstraint("ck_pedidos_compra_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
                    table.CheckConstraint("ck_pedidos_compra_valor_total", "valor_total >= 0");
                    table.ForeignKey(
                        name: "fk_pedidos_compra_fornecedores",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedido_compra_itens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    pedido_compra_id = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    desconto_percentual = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedido_compra_itens", x => x.id);
                    table.CheckConstraint("ck_pedido_compra_itens_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
                    table.CheckConstraint("ck_pedido_compra_itens_preco_unitario", "preco_unitario >= 0");
                    table.CheckConstraint("ck_pedido_compra_itens_quantidade", "quantidade > 0");
                    table.ForeignKey(
                        name: "fk_pedido_compra_itens_pedidos_compra",
                        column: x => x.pedido_compra_id,
                        principalTable: "pedidos_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pedido_compra_itens_produtos",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_estoque_movimentacoes_pedido_compra_id",
                table: "estoque_movimentacoes",
                column: "pedido_compra_id");

            migrationBuilder.CreateIndex(
                name: "ix_fornecedores_documento",
                table: "fornecedores",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedores_nome",
                table: "fornecedores",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_compra_itens_produto_id",
                table: "pedido_compra_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ux_pedido_compra_itens_pedido_produto",
                table: "pedido_compra_itens",
                columns: new[] { "pedido_compra_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_compra_data_pedido",
                table: "pedidos_compra",
                column: "data_pedido");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_compra_fornecedor_id",
                table: "pedidos_compra",
                column: "fornecedor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_estoque_movimentacoes_pedidos_compra",
                table: "estoque_movimentacoes",
                column: "pedido_compra_id",
                principalTable: "pedidos_compra",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_estoque_movimentacoes_pedidos_compra",
                table: "estoque_movimentacoes");

            migrationBuilder.DropTable(
                name: "pedido_compra_itens");

            migrationBuilder.DropTable(
                name: "pedidos_compra");

            migrationBuilder.DropTable(
                name: "fornecedores");

            migrationBuilder.DropIndex(
                name: "ix_estoque_movimentacoes_pedido_compra_id",
                table: "estoque_movimentacoes");

            migrationBuilder.DropColumn(
                name: "pedido_compra_id",
                table: "estoque_movimentacoes");
        }
    }
}
