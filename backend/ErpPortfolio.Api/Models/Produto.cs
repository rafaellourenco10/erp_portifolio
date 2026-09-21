// =====================================================================================
// Arquivo....: Produto.cs
// Versão.....: 1.1.0
// Data.......: 21/09/2026
// Descrição..: Entidade de domínio que representa um produto vendável.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.produtos (FK categoria_id -> public.categorias)
// Fontes.....: Mapeada em ErpPortfolioDbContext.Produtos (EF Core / Npgsql).
//              Colunas: id, nome, sku, categoria_id, unidade, preco_venda, custo, ativo,
//              data_cadastro (o mapeamento de nomes fica no DbContext).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
//   1.1.0 - 21/09/2026 - Categoria deixa de ser texto livre e passa a ser um registro
//                        (CategoriaId + navegação).
// =====================================================================================

namespace ErpPortfolio.Api.Models;

public class Produto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Número de série do produto, único; somente dígitos (ex.: 10012345).</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Categoria do produto; opcional.</summary>
    public int? CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    /// <summary>Unidade de medida: UN, KG, L, M ou CX.</summary>
    public string Unidade { get; set; } = string.Empty;

    /// <summary>Preço de venda em reais, com 2 casas decimais.</summary>
    public decimal PrecoVenda { get; set; }

    /// <summary>Custo em reais, com 2 casas decimais.</summary>
    public decimal Custo { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>Data/hora de cadastro em UTC.</summary>
    public DateTime DataCadastro { get; set; }
}
