// =====================================================================================
// Arquivo....: FiltroStatusParcelaPagar.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Status para filtrar a listagem de contas a pagar. Espelha
//              Models/StatusParcelaPagar.cs e acrescenta "Atrasado", que não é um status
//              gravado: é uma parcela Pendente com vencimento no passado (SPEC.md, P3).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Usado pelo ContasPagarService para filtrar public.parcelas_pagar.
// Fontes.....: Query string de GET /api/contas-pagar (?status=).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public enum FiltroStatusParcelaPagar
{
    Pendente,
    Pago,
    Cancelado,

    /// <summary>Não gravado: calculado como Pendente com vencimento antes de hoje.</summary>
    Atrasado
}
