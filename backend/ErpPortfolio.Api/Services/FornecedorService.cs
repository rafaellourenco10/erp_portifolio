// =====================================================================================
// Arquivo....: FornecedorService.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Regras de negócio e persistência de fornecedores. Espelho de
//              ClienteService (SPEC.md, F1-F3); documento único num índice próprio,
//              independente do de clientes.
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.fornecedores
//                - SELECT : listagem (ILIKE em nome, uf = ANY(@ufs), ativo = @ativo,
//                           ORDER BY nome, id, LIMIT/OFFSET),
//                           consulta por id e checagem de documento duplicado
//                - INSERT : inclusão
//                - UPDATE : edição e inativação (ativo = false)
// Fontes.....: ErpPortfolioDbContext.Fornecedores (EF Core / Npgsql).
//              Violação do índice único ix_fornecedores_documento (SQLSTATE 23505)
//              é convertida em ConflitoException.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.DTOs.Validacoes;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ErpPortfolio.Api.Services;

public class FornecedorService(ErpPortfolioDbContext contexto) : IFornecedorService
{
    private const string MensagemDocumentoDuplicado = "Já existe um fornecedor cadastrado com este CPF/CNPJ.";
    private const string MensagemDocumentoDuplicadoInativo =
        "Já existe um fornecedor INATIVO com este CPF/CNPJ. Reative o cadastro existente em vez de criar outro.";

    public async Task<ResultadoPaginadoDto<FornecedorRespostaDto>> ListarAsync(FornecedorFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Fornecedores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var padrao = $"%{ClienteService.EscaparCuringasLike(filtro.Nome.Trim())}%";
            consulta = consulta.Where(f => EF.Functions.ILike(f.Nome, padrao));
        }

        if (filtro.Ufs is { Count: > 0 })
        {
            var ufs = filtro.Ufs.Select(uf => uf.Trim().ToUpperInvariant()).Distinct().ToList();
            consulta = consulta.Where(f => ufs.Contains(f.Uf));
        }

        if (filtro.Ativo is bool ativo)
        {
            consulta = consulta.Where(f => f.Ativo == ativo);
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(f => f.Nome)
            .ThenBy(f => f.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(f => FornecedorRespostaDto.DeEntidade(f))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<FornecedorRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<FornecedorRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        var fornecedor = await contexto.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancelamento);
        return fornecedor is null ? null : FornecedorRespostaDto.DeEntidade(fornecedor);
    }

    public async Task<FornecedorRespostaDto> CriarAsync(FornecedorCriacaoDto dados, CancellationToken cancelamento)
    {
        var fornecedor = new Fornecedor
        {
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };
        AplicarDados(fornecedor, dados);

        await GarantirDocumentoUnicoAsync(fornecedor.Documento, idIgnorado: null, cancelamento);

        contexto.Fornecedores.Add(fornecedor);
        await SalvarAsync(cancelamento);

        return FornecedorRespostaDto.DeEntidade(fornecedor);
    }

    public async Task<FornecedorRespostaDto?> AtualizarAsync(int id, FornecedorAtualizacaoDto dados, CancellationToken cancelamento)
    {
        var fornecedor = await contexto.Fornecedores.FirstOrDefaultAsync(f => f.Id == id, cancelamento);
        if (fornecedor is null)
            return null;

        AplicarDados(fornecedor, dados);
        if (dados.Ativo is bool ativo)
            fornecedor.Ativo = ativo;

        await GarantirDocumentoUnicoAsync(fornecedor.Documento, idIgnorado: id, cancelamento);
        await SalvarAsync(cancelamento);

        return FornecedorRespostaDto.DeEntidade(fornecedor);
    }

    public async Task<bool> InativarAsync(int id, CancellationToken cancelamento)
    {
        var fornecedor = await contexto.Fornecedores.FirstOrDefaultAsync(f => f.Id == id, cancelamento);
        if (fornecedor is null)
            return false;

        fornecedor.Ativo = false;
        await SalvarAsync(cancelamento);

        return true;
    }

    private static void AplicarDados(Fornecedor fornecedor, FornecedorCriacaoDto dados)
    {
        fornecedor.Nome = dados.Nome.Trim();
        fornecedor.Documento = DocumentoValidador.Normalizar(dados.Documento);
        fornecedor.Email = dados.Email;
        fornecedor.Telefone = dados.Telefone;
        fornecedor.Cidade = dados.Cidade.Trim();
        fornecedor.Uf = dados.Uf.Trim().ToUpperInvariant();
    }

    private async Task GarantirDocumentoUnicoAsync(string documento, int? idIgnorado, CancellationToken cancelamento)
    {
        var existente = await contexto.Fornecedores
            .Where(f => f.Documento == documento && f.Id != idIgnorado)
            .Select(f => new { f.Ativo })
            .FirstOrDefaultAsync(cancelamento);

        if (existente is not null)
            throw new ConflitoException(existente.Ativo ? MensagemDocumentoDuplicado : MensagemDocumentoDuplicadoInativo);
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
}
