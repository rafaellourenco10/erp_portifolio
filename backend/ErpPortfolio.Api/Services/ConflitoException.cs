// =====================================================================================
// Arquivo....: ConflitoException.cs
// Versão.....: 1.0.0
// Data.......: 18/09/2026
// Descrição..: Exceção de regra de negócio que indica conflito com um registro já
//              existente (ex.: documento duplicado). Convertida em HTTP 409 no controller.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (origem típica: violação de índice único).
// Tabelas....: public.clientes (ix_clientes_documento).
// Fontes.....: Lançada pelos serviços (ex.: ClienteService).
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
// =====================================================================================

namespace ErpPortfolio.Api.Services;

public class ConflitoException(string mensagem) : Exception(mensagem);
