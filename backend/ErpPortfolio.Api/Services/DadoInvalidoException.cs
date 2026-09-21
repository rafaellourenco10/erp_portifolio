// =====================================================================================
// Arquivo....: DadoInvalidoException.cs
// Versão.....: 1.0.0
// Data.......: 21/09/2026
// Descrição..: Exceção de regra de negócio que aponta um campo com valor inválido que só
//              o banco consegue validar (ex.: categoria inexistente ou inativa).
//              Convertida em HTTP 400 (ValidationProblemDetails) no controller, com o
//              erro associado ao campo.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Não se aplica.
// Fontes.....: Lançada pelos serviços (ex.: ProdutoService).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 21/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

/// <param name="campo">Nome do campo do DTO em PascalCase (ex.: "CategoriaId").</param>
/// <param name="mensagem">Mensagem exibida ao usuário no campo.</param>
public class DadoInvalidoException(string campo, string mensagem) : Exception(mensagem)
{
    public string Campo { get; } = campo;
}
