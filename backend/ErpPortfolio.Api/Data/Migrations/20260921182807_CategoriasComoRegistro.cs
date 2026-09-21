using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Editada à mão: o EF gerou DropColumn("categoria") antes de criar a tabela nova, o que apagaria a
    /// categoria (texto livre) dos produtos já cadastrados. Aqui cada texto distinto (sem diferenciar
    /// maiúsculas/minúsculas) vira um registro em categorias, os produtos são ligados a ele e só então a
    /// coluna antiga é removida. O Down faz o caminho inverso.
    /// </remarks>
    public partial class CategoriasComoRegistro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categorias_nome",
                table: "categorias",
                column: "nome",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "categoria_id",
                table: "produtos",
                type: "integer",
                nullable: true);

            // Converte o texto livre existente em registros e liga os produtos a eles.
            migrationBuilder.Sql(@"
                INSERT INTO categorias (nome, ativo, data_cadastro)
                SELECT DISTINCT ON (lower(btrim(categoria))) btrim(categoria), true, now()
                FROM produtos
                WHERE categoria IS NOT NULL AND btrim(categoria) <> ''
                ORDER BY lower(btrim(categoria)), btrim(categoria);

                UPDATE produtos p
                SET categoria_id = c.id
                FROM categorias c
                WHERE p.categoria IS NOT NULL AND lower(btrim(p.categoria)) = lower(c.nome);");

            migrationBuilder.DropColumn(
                name: "categoria",
                table: "produtos");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_categoria_id",
                table: "produtos",
                column: "categoria_id");

            migrationBuilder.AddForeignKey(
                name: "fk_produtos_categorias",
                table: "produtos",
                column: "categoria_id",
                principalTable: "categorias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "categoria",
                table: "produtos",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            // Devolve o nome da categoria como texto livre.
            migrationBuilder.Sql(@"
                UPDATE produtos p
                SET categoria = c.nome
                FROM categorias c
                WHERE p.categoria_id = c.id;");

            migrationBuilder.DropForeignKey(
                name: "fk_produtos_categorias",
                table: "produtos");

            migrationBuilder.DropIndex(
                name: "ix_produtos_categoria_id",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                table: "produtos");

            migrationBuilder.DropTable(
                name: "categorias");
        }
    }
}
