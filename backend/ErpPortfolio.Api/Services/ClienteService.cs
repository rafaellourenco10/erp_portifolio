// =====================================================================================
// Arquivo....: ClienteService.cs
// Versão.....: 1.1.0
// Data.......: 18/09/2026
// Descrição..: Regras de negócio e persistência de clientes.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.clientes
//                - SELECT : listagem (ILIKE em nome, uf = ANY(@ufs), ativo = @ativo,
//                           ORDER BY nome, id, LIMIT/OFFSET),
//                           consulta por id e checagem de documento duplicado
//                - INSERT : inclusão
//                - UPDATE : edição e inativação (ativo = false)
// Fontes.....: ErpPortfolioDbContext.Clientes (EF Core / Npgsql).
//              Violação do índice único ix_clientes_documento (SQLSTATE 23505)
//              é convertida em ConflitoException.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 18/09/2026 - Criação do arquivo.
//   1.1.0 - 18/09/2026 - Filtros da listagem por UFs e por status.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.DTOs.Validacoes;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ErpPortfolio.Api.Services;

public class ClienteService(ErpPortfolioDbContext contexto) : IClienteService
{
    private const string MensagemDocumentoDuplicado = "Já existe um cliente cadastrado com este CPF/CNPJ.";

    public async Task<ResultadoPaginadoDto<ClienteRespostaDto>> ListarAsync(ClienteFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var padrao = $"%{EscaparCuringasLike(filtro.Nome.Trim())}%";
            consulta = consulta.Where(c => EF.Functions.ILike(c.Nome, padrao));
        }

        if (filtro.Ufs is { Count: > 0 })
        {
            var ufs = filtro.Ufs.Select(uf => uf.Trim().ToUpperInvariant()).Distinct().ToList();
            consulta = consulta.Where(c => ufs.Contains(c.Uf));
        }

        if (filtro.Ativo is bool ativo)
        {
            consulta = consulta.Where(c => c.Ativo == ativo);
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(c => c.Nome)
            .ThenBy(c => c.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(c => ClienteRespostaDto.DeEntidade(c))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<ClienteRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<ClienteRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        var cliente = await contexto.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancelamento);
        return cliente is null ? null : ClienteRespostaDto.DeEntidade(cliente);
    }

    public async Task<ClienteRespostaDto> CriarAsync(ClienteCriacaoDto dados, CancellationToken cancelamento)
    {
        var cliente = new Cliente
        {
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };
        AplicarDados(cliente, dados);

        await GarantirDocumentoUnicoAsync(cliente.Documento, idIgnorado: null, cancelamento);

        contexto.Clientes.Add(cliente);
        await SalvarAsync(cancelamento);

        return ClienteRespostaDto.DeEntidade(cliente);
    }

    public async Task<ClienteRespostaDto?> AtualizarAsync(int id, ClienteAtualizacaoDto dados, CancellationToken cancelamento)
    {
        var cliente = await contexto.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancelamento);
        if (cliente is null)
            return null;

        AplicarDados(cliente, dados);
        cliente.Ativo = dados.Ativo;

        await GarantirDocumentoUnicoAsync(cliente.Documento, idIgnorado: id, cancelamento);
        await SalvarAsync(cancelamento);

        return ClienteRespostaDto.DeEntidade(cliente);
    }

    public async Task<bool> InativarAsync(int id, CancellationToken cancelamento)
    {
        var cliente = await contexto.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancelamento);
        if (cliente is null)
            return false;

        cliente.Ativo = false;
        await SalvarAsync(cancelamento);

        return true;
    }

    private static void AplicarDados(Cliente cliente, ClienteCriacaoDto dados)
    {
        cliente.Nome = dados.Nome.Trim();
        cliente.Documento = DocumentoValidador.Normalizar(dados.Documento);
        cliente.Email = dados.Email;
        cliente.Telefone = dados.Telefone;
        cliente.Cidade = dados.Cidade.Trim();
        cliente.Uf = dados.Uf.Trim().ToUpperInvariant();
    }

    private async Task GarantirDocumentoUnicoAsync(string documento, int? idIgnorado, CancellationToken cancelamento)
    {
        var existe = await contexto.Clientes
            .AnyAsync(c => c.Documento == documento && c.Id != idIgnorado, cancelamento);

        if (existe)
            throw new ConflitoException(MensagemDocumentoDuplicado);
    }

    // O índice único cobre a corrida entre a checagem prévia e o INSERT/UPDATE.
    private async Task SalvarAsync(CancellationToken cancelamento)
    {
        try
        {
            await contexto.SaveChangesAsync(cancelamento);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflitoException(MensagemDocumentoDuplicado);
        }
    }

    // Impede que '%' e '_' digitados pelo usuário funcionem como curingas no ILIKE.
    private static string EscaparCuringasLike(string texto) =>
        texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
