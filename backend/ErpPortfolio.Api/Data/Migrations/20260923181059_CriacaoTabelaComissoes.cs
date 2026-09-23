using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoTabelaComissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comissoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    parcela_receber_id = table.Column<int>(type: "integer", nullable: false),
                    pedido_id = table.Column<int>(type: "integer", nullable: false),
                    vendedor_id = table.Column<int>(type: "integer", nullable: false),
                    valor_base = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    data_geracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comissoes", x => x.id);
                    table.CheckConstraint("ck_comissoes_percentual", "percentual > 0 AND percentual <= 100");
                    table.CheckConstraint("ck_comissoes_valor", "valor > 0");
                    table.ForeignKey(
                        name: "fk_comissoes_parcelas_receber",
                        column: x => x.parcela_receber_id,
                        principalTable: "parcelas_receber",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comissoes_pedidos",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comissoes_vendedores",
                        column: x => x.vendedor_id,
                        principalTable: "vendedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comissoes_data_geracao",
                table: "comissoes",
                column: "data_geracao");

            migrationBuilder.CreateIndex(
                name: "ix_comissoes_pedido_id",
                table: "comissoes",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_comissoes_vendedor_id",
                table: "comissoes",
                column: "vendedor_id");

            migrationBuilder.CreateIndex(
                name: "ux_comissoes_parcela_receber_id",
                table: "comissoes",
                column: "parcela_receber_id",
                unique: true);

            // CM3: parcelas já recebidas antes deste módulo, de pedidos com vendedor e % > 0, ganham a comissão
            // (mesma conta do ComissaoCalculo: ROUND em numeric arredonda meio para cima, a 2 casas).
            migrationBuilder.Sql("""
                INSERT INTO comissoes (parcela_receber_id, pedido_id, vendedor_id, valor_base, percentual, valor, data_geracao, status)
                SELECT pr.id, p.id, p.vendedor_id, pr.valor, p.percentual_comissao,
                       ROUND(pr.valor * p.percentual_comissao / 100, 2),
                       COALESCE(pr.data_recebimento, now()), 'Pendente'
                FROM parcelas_receber pr
                JOIN pedidos p ON p.id = pr.pedido_id
                WHERE pr.status = 'Recebido'
                  AND p.vendedor_id IS NOT NULL
                  AND p.percentual_comissao > 0
                  AND ROUND(pr.valor * p.percentual_comissao / 100, 2) > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comissoes");
        }
    }
}
