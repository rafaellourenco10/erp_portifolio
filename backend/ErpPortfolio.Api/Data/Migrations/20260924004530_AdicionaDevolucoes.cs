using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaDevolucoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_parcelas_pagar_origem",
                table: "parcelas_pagar");

            migrationBuilder.DropCheckConstraint(
                name: "ck_comissoes_valor",
                table: "comissoes");

            migrationBuilder.AddColumn<int>(
                name: "devolucao_id",
                table: "parcelas_pagar",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "parcela_receber_id",
                table: "comissoes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "devolucao_id",
                table: "comissoes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "devolucoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    pedido_id = table.Column<int>(type: "integer", nullable: false),
                    data_devolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_abatido = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_reembolso = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devolucoes", x => x.id);
                    table.CheckConstraint("ck_devolucoes_valores", "valor_total > 0 AND valor_abatido >= 0 AND valor_reembolso >= 0 AND valor_total = valor_abatido + valor_reembolso");
                    table.ForeignKey(
                        name: "fk_devolucoes_pedidos",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "devolucao_itens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    devolucao_id = table.Column<int>(type: "integer", nullable: false),
                    pedido_item_id = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    volta_estoque = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devolucao_itens", x => x.id);
                    table.CheckConstraint("ck_devolucao_itens_quantidade", "quantidade > 0");
                    table.CheckConstraint("ck_devolucao_itens_valor", "valor >= 0");
                    table.ForeignKey(
                        name: "fk_devolucao_itens_devolucoes",
                        column: x => x.devolucao_id,
                        principalTable: "devolucoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_devolucao_itens_pedido_itens",
                        column: x => x.pedido_item_id,
                        principalTable: "pedido_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_pagar_devolucao_id",
                table: "parcelas_pagar",
                column: "devolucao_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_parcelas_pagar_origem",
                table: "parcelas_pagar",
                sql: "(origem = 'Compra' AND pedido_compra_id IS NOT NULL) OR (origem = 'Comissao' AND vendedor_id IS NOT NULL) OR (origem = 'Avulsa' AND descricao IS NOT NULL) OR (origem = 'Devolucao' AND devolucao_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_comissoes_devolucao_id",
                table: "comissoes",
                column: "devolucao_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_comissoes_valor",
                table: "comissoes",
                sql: "(parcela_receber_id IS NOT NULL AND devolucao_id IS NULL AND valor > 0) OR (parcela_receber_id IS NULL AND devolucao_id IS NOT NULL AND valor < 0)");

            migrationBuilder.CreateIndex(
                name: "ix_devolucao_itens_pedido_item_id",
                table: "devolucao_itens",
                column: "pedido_item_id");

            migrationBuilder.CreateIndex(
                name: "ux_devolucao_itens_devolucao_item",
                table: "devolucao_itens",
                columns: new[] { "devolucao_id", "pedido_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devolucoes_data_devolucao",
                table: "devolucoes",
                column: "data_devolucao");

            migrationBuilder.CreateIndex(
                name: "ix_devolucoes_pedido_id",
                table: "devolucoes",
                column: "pedido_id");

            migrationBuilder.AddForeignKey(
                name: "fk_comissoes_devolucoes",
                table: "comissoes",
                column: "devolucao_id",
                principalTable: "devolucoes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_parcelas_pagar_devolucoes",
                table: "parcelas_pagar",
                column: "devolucao_id",
                principalTable: "devolucoes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comissoes_devolucoes",
                table: "comissoes");

            migrationBuilder.DropForeignKey(
                name: "fk_parcelas_pagar_devolucoes",
                table: "parcelas_pagar");

            migrationBuilder.DropTable(
                name: "devolucao_itens");

            migrationBuilder.DropTable(
                name: "devolucoes");

            migrationBuilder.DropIndex(
                name: "ix_parcelas_pagar_devolucao_id",
                table: "parcelas_pagar");

            migrationBuilder.DropCheckConstraint(
                name: "ck_parcelas_pagar_origem",
                table: "parcelas_pagar");

            migrationBuilder.DropIndex(
                name: "ix_comissoes_devolucao_id",
                table: "comissoes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_comissoes_valor",
                table: "comissoes");

            migrationBuilder.DropColumn(
                name: "devolucao_id",
                table: "parcelas_pagar");

            migrationBuilder.DropColumn(
                name: "devolucao_id",
                table: "comissoes");

            migrationBuilder.AlterColumn<int>(
                name: "parcela_receber_id",
                table: "comissoes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_parcelas_pagar_origem",
                table: "parcelas_pagar",
                sql: "(origem = 'Compra' AND pedido_compra_id IS NOT NULL) OR (origem = 'Comissao' AND vendedor_id IS NOT NULL) OR (origem = 'Avulsa' AND descricao IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_comissoes_valor",
                table: "comissoes",
                sql: "valor > 0");
        }
    }
}
