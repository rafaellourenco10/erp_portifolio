// =====================================================================================
// Arquivo....: FiltroStatusParcela.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Status para filtrar a listagem de contas a receber. Espelha
//              Models/StatusParcela.cs e acrescenta "Atrasado", que não é um status
//              gravado: é uma parcela Pendente com vencimento no passado (SPEC.md, C5).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo ContasReceberService para filtrar public.parcelas_receber.
// Fontes.....: Query string de GET /api/contas-receber (?status=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public enum FiltroStatusParcela
{
    Pendente,
    Recebido,
    Cancelado,

    /// <summary>Não gravado: calculado como Pendente com vencimento antes de hoje.</summary>
    Atrasado
}
