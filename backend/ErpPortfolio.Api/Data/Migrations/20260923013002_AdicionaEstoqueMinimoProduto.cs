using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEstoqueMinimoProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "estoque_minimo",
                table: "produtos",
                type: "numeric(12,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "ck_produtos_estoque_minimo",
                table: "produtos",
                sql: "estoque_minimo >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_produtos_estoque_minimo",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "estoque_minimo",
                table: "produtos");
        }
    }
}
