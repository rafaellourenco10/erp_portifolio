// =====================================================================================
// Arquivo....: ClienteAtualizacaoDto.cs
// Versão.....: 1.1.0
// Data.......: 21/09/2026
// Descrição..: DTO de entrada para edição de cliente (PUT /api/clientes/{id}).
//              Reaproveita as validações da criação e permite reativar o cliente.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco diretamente.
// Tabelas....: Os dados são gravados em public.clientes pelo ClienteService.
// Fontes.....: Corpo (JSON) da requisição HTTP.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
//   1.1.0 - 21/09/2026 - Ativo passa a ser opcional: um PUT sem o campo mantém o status
//                        atual (antes assumia true e reativava o cliente inativo).
// =====================================================================================

namespace ErpPortfolio.Api.DTOs;

public class ClienteAtualizacaoDto : ClienteCriacaoDto
{
    /// <summary>Opcional. Ausente = mantém o status atual do cliente.</summary>
    /// <example>true</example>
    public bool? Ativo { get; set; }
}
