using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoTabelaParcelasPagar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parcelas_pagar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    pedido_compra_id = table.Column<int>(type: "integer", nullable: false),
                    numero_parcela = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parcelas_pagar", x => x.id);
                    table.CheckConstraint("ck_parcelas_pagar_numero_parcela", "numero_parcela > 0");
                    table.CheckConstraint("ck_parcelas_pagar_valor", "valor > 0");
                    table.ForeignKey(
                        name: "fk_parcelas_pagar_pedidos_compra",
                        column: x => x.pedido_compra_id,
                        principalTable: "pedidos_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_pagar_vencimento",
                table: "parcelas_pagar",
                column: "vencimento");

            migrationBuilder.CreateIndex(
                name: "ux_parcelas_pagar_pedido_compra_numero",
                table: "parcelas_pagar",
                columns: new[] { "pedido_compra_id", "numero_parcela" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parcelas_pagar");
        }
    }
}
