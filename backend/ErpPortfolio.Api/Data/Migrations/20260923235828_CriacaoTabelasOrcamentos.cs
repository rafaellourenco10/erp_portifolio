using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoTabelasOrcamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orcamentos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    vendedor_id = table.Column<int>(type: "integer", nullable: true),
                    data_orcamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    validade = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    desconto_percentual = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    valor_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    motivo_perda = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pedido_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orcamentos", x => x.id);
                    table.CheckConstraint("ck_orcamentos_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
                    table.CheckConstraint("ck_orcamentos_status", "(status = 'Aprovado' AND pedido_id IS NOT NULL) OR (status IN ('Aberto', 'Perdido') AND pedido_id IS NULL)");
                    table.CheckConstraint("ck_orcamentos_valor_total", "valor_total >= 0");
                    table.ForeignKey(
                        name: "fk_orcamentos_clientes",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orcamentos_pedidos",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orcamentos_vendedores",
                        column: x => x.vendedor_id,
                        principalTable: "vendedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orcamento_itens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    orcamento_id = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    desconto_percentual = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orcamento_itens", x => x.id);
                    table.CheckConstraint("ck_orcamento_itens_desconto_percentual", "desconto_percentual >= 0 AND desconto_percentual <= 100");
                    table.CheckConstraint("ck_orcamento_itens_preco_unitario", "preco_unitario >= 0");
                    table.CheckConstraint("ck_orcamento_itens_quantidade", "quantidade > 0");
                    table.ForeignKey(
                        name: "fk_orcamento_itens_orcamentos",
                        column: x => x.orcamento_id,
                        principalTable: "orcamentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_orcamento_itens_produtos",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_orcamento_itens_produto_id",
                table: "orcamento_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ux_orcamento_itens_orcamento_produto",
                table: "orcamento_itens",
                columns: new[] { "orcamento_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_cliente_id",
                table: "orcamentos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_data_orcamento",
                table: "orcamentos",
                column: "data_orcamento");

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_vendedor_id",
                table: "orcamentos",
                column: "vendedor_id");

            migrationBuilder.CreateIndex(
                name: "ux_orcamentos_pedido_id",
                table: "orcamentos",
                column: "pedido_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orcamento_itens");

            migrationBuilder.DropTable(
                name: "orcamentos");
        }
    }
}
