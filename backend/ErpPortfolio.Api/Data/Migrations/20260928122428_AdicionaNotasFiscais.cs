using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpPortfolio.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaNotasFiscais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ncm",
                table: "produtos",
                type: "char(8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bairro",
                table: "clientes",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cep",
                table: "clientes",
                type: "char(8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_municipio",
                table: "clientes",
                type: "char(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complemento",
                table: "clientes",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inscricao_estadual",
                table: "clientes",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logradouro",
                table: "clientes",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero",
                table: "clientes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    razao_social = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cnpj = table.Column<string>(type: "char(14)", nullable: false),
                    inscricao_estadual = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    logradouro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    numero = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    cep = table.Column<string>(type: "char(8)", nullable: false),
                    municipio = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    codigo_municipio = table.Column<string>(type: "char(7)", nullable: false),
                    uf = table.Column<string>(type: "char(2)", nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    serie_nfe = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empresa", x => x.id);
                    table.CheckConstraint("ck_empresa_id", "id = 1");
                    table.CheckConstraint("ck_empresa_serie_nfe", "serie_nfe BETWEEN 0 AND 999");
                });

            migrationBuilder.CreateTable(
                name: "notas_fiscais",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    chave = table.Column<string>(type: "char(44)", nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    protocolo = table.Column<string>(type: "char(15)", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    pedido_id = table.Column<int>(type: "integer", nullable: false),
                    devolucao_id = table.Column<int>(type: "integer", nullable: true),
                    nota_referenciada_id = table.Column<int>(type: "integer", nullable: true),
                    destinatario_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    destinatario_documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    destinatario_uf = table.Column<string>(type: "char(2)", nullable: false),
                    valor_produtos = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    base_icms = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_icms = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_pis = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_cofins = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    xml = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notas_fiscais", x => x.id);
                    table.CheckConstraint("ck_notas_fiscais_tipo", "(tipo = 'Saida' AND devolucao_id IS NULL AND nota_referenciada_id IS NULL) OR (tipo = 'Entrada' AND devolucao_id IS NOT NULL AND nota_referenciada_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_notas_fiscais_clientes",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notas_fiscais_devolucoes",
                        column: x => x.devolucao_id,
                        principalTable: "devolucoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notas_fiscais_nota_referenciada",
                        column: x => x.nota_referenciada_id,
                        principalTable: "notas_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notas_fiscais_pedidos",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "nota_fiscal_itens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nota_fiscal_id = table.Column<int>(type: "integer", nullable: false),
                    numero_item = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ncm = table.Column<string>(type: "char(8)", nullable: false),
                    cfop = table.Column<string>(type: "char(4)", nullable: false),
                    unidade = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_bruto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_desconto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    base_icms = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    aliquota_icms = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    valor_icms = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_pis = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_cofins = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nota_fiscal_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_nota_fiscal_itens_notas_fiscais",
                        column: x => x.nota_fiscal_id,
                        principalTable: "notas_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_nota_fiscal_itens_produtos",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_nota_fiscal_itens_produto_id",
                table: "nota_fiscal_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ux_nota_fiscal_itens_nota_numero",
                table: "nota_fiscal_itens",
                columns: new[] { "nota_fiscal_id", "numero_item" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notas_fiscais_cliente_id",
                table: "notas_fiscais",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_notas_fiscais_data_emissao",
                table: "notas_fiscais",
                column: "data_emissao");

            migrationBuilder.CreateIndex(
                name: "ix_notas_fiscais_nota_referenciada_id",
                table: "notas_fiscais",
                column: "nota_referenciada_id");

            migrationBuilder.CreateIndex(
                name: "ux_notas_fiscais_chave",
                table: "notas_fiscais",
                column: "chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_notas_fiscais_devolucao_id",
                table: "notas_fiscais",
                column: "devolucao_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_notas_fiscais_pedido_saida",
                table: "notas_fiscais",
                column: "pedido_id",
                unique: true,
                filter: "tipo = 'Saida'");

            migrationBuilder.CreateIndex(
                name: "ux_notas_fiscais_serie_numero",
                table: "notas_fiscais",
                columns: new[] { "serie", "numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "empresa");

            migrationBuilder.DropTable(
                name: "nota_fiscal_itens");

            migrationBuilder.DropTable(
                name: "notas_fiscais");

            migrationBuilder.DropColumn(
                name: "ncm",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "bairro",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cep",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "codigo_municipio",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "complemento",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "inscricao_estadual",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "logradouro",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "numero",
                table: "clientes");
        }
    }
}
