using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoTabelaEstoqueMovimentacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estoque_movimentacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    produto_id = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pedido_id = table.Column<int>(type: "integer", nullable: true),
                    data_movimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estoque_movimentacoes", x => x.id);
                    table.CheckConstraint("ck_estoque_movimentacoes_quantidade", "quantidade > 0");
                    table.ForeignKey(
                        name: "fk_estoque_movimentacoes_pedidos",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_estoque_movimentacoes_produtos",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_estoque_movimentacoes_data_movimentacao",
                table: "estoque_movimentacoes",
                column: "data_movimentacao");

            migrationBuilder.CreateIndex(
                name: "ix_estoque_movimentacoes_pedido_id",
                table: "estoque_movimentacoes",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_estoque_movimentacoes_produto_id",
                table: "estoque_movimentacoes",
                column: "produto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "estoque_movimentacoes");
        }
    }
}
