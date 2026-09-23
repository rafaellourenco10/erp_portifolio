using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContasPagarOrigemEComissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "pedido_compra_id",
                table: "parcelas_pagar",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "parcelas_pagar",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "favorecido",
                table: "parcelas_pagar",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origem",
                table: "parcelas_pagar",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Compra");

            migrationBuilder.AddColumn<int>(
                name: "total_parcelas",
                table: "parcelas_pagar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "vendedor_id",
                table: "parcelas_pagar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parcela_pagar_id",
                table: "comissoes",
                type: "integer",
                nullable: true);

            // CP3: preenche o total de parcelas das contas existentes (todas de compra) ANTES do CHECK
            // ck_parcelas_pagar_total_parcelas, contando as irmãs do mesmo pedido de compra.
            migrationBuilder.Sql("""
                UPDATE parcelas_pagar p
                SET total_parcelas = (SELECT COUNT(*) FROM parcelas_pagar x WHERE x.pedido_compra_id = p.pedido_compra_id);
                """);

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_pagar_vendedor_id",
                table: "parcelas_pagar",
                column: "vendedor_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_parcelas_pagar_origem",
                table: "parcelas_pagar",
                sql: "(origem = 'Compra' AND pedido_compra_id IS NOT NULL) OR (origem = 'Comissao' AND vendedor_id IS NOT NULL) OR (origem = 'Avulsa' AND descricao IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_parcelas_pagar_total_parcelas",
                table: "parcelas_pagar",
                sql: "total_parcelas >= numero_parcela");

            migrationBuilder.CreateIndex(
                name: "ix_comissoes_parcela_pagar_id",
                table: "comissoes",
                column: "parcela_pagar_id");

            migrationBuilder.AddForeignKey(
                name: "fk_comissoes_parcelas_pagar",
                table: "comissoes",
                column: "parcela_pagar_id",
                principalTable: "parcelas_pagar",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_parcelas_pagar_vendedores",
                table: "parcelas_pagar",
                column: "vendedor_id",
                principalTable: "vendedores",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comissoes_parcelas_pagar",
                table: "comissoes");

            migrationBuilder.DropForeignKey(
                name: "fk_parcelas_pagar_vendedores",
                table: "parcelas_pagar");

            migrationBuilder.DropIndex(
                name: "ix_parcelas_pagar_vendedor_id",
                table: "parcelas_pagar");

            migrationBuilder.DropCheckConstraint(
                name: "ck_parcelas_pagar_origem",
                table: "parcelas_pagar");

            migrationBuilder.DropCheckConstraint(
                name: "ck_parcelas_pagar_total_parcelas",
                table: "parcelas_pagar");

            migrationBuilder.DropIndex(
                name: "ix_comissoes_parcela_pagar_id",
                table: "comissoes");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "parcelas_pagar");

            migrationBuilder.DropColumn(
                name: "favorecido",
                table: "parcelas_pagar");

            migrationBuilder.DropColumn(
                name: "origem",
                table: "parcelas_pagar");

            migrationBuilder.DropColumn(
                name: "total_parcelas",
                table: "parcelas_pagar");

            migrationBuilder.DropColumn(
                name: "vendedor_id",
                table: "parcelas_pagar");

            migrationBuilder.DropColumn(
                name: "parcela_pagar_id",
                table: "comissoes");

            migrationBuilder.AlterColumn<int>(
                name: "pedido_compra_id",
                table: "parcelas_pagar",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
