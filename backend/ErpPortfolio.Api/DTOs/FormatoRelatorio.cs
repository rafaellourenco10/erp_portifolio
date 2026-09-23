// =====================================================================================
// Arquivo....: FormatoRelatorio.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Formato de saída de um relatório: JSON (dados para a tela), Excel (.xlsx)
//              ou PDF. Tela e arquivos saem da mesma consulta (SPEC.md, R3).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Não se aplica.
// Fontes.....: Query string ?formato= de GET /api/relatorios/* (sem diferenciar maiúsculas).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public enum FormatoRelatorio
{
    Json,
    Xlsx,
    Pdf
}
