using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaVendedores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "percentual_comissao",
                table: "pedidos",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "vendedor_id",
                table: "pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vendedores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cpf = table.Column<string>(type: "char(11)", nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    percentual_comissao = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendedores", x => x.id);
                    table.CheckConstraint("ck_vendedores_percentual_comissao", "percentual_comissao >= 0 AND percentual_comissao <= 100");
                });

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_vendedor_id",
                table: "pedidos",
                column: "vendedor_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_pedidos_percentual_comissao",
                table: "pedidos",
                sql: "percentual_comissao >= 0 AND percentual_comissao <= 100");

            migrationBuilder.CreateIndex(
                name: "ix_vendedores_cpf",
                table: "vendedores",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendedores_nome",
                table: "vendedores",
                column: "nome");

            migrationBuilder.AddForeignKey(
                name: "fk_pedidos_vendedores",
                table: "pedidos",
                column: "vendedor_id",
                principalTable: "vendedores",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pedidos_vendedores",
                table: "pedidos");

            migrationBuilder.DropTable(
                name: "vendedores");

            migrationBuilder.DropIndex(
                name: "ix_pedidos_vendedor_id",
                table: "pedidos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_pedidos_percentual_comissao",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "percentual_comissao",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "vendedor_id",
                table: "pedidos");
        }
    }
}
