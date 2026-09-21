/**
 * =====================================================================
 * Arquivo....: paginacao.ts
 * Versão.....: 1.0.0
 * Data.......: 21/09/2026
 * Descrição..: Tipo do resultado paginado das listagens da API
 *              (ResultadoPaginadoDto), compartilhado entre os módulos.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 21/09/2026 - Criação do arquivo (movido de types/cliente.ts).
 * =====================================================================
 */

export interface ResultadoPaginado<T> {
  itens: T[]
  pagina: number
  tamanhoPagina: number
  totalItens: number
  totalPaginas: number
}
