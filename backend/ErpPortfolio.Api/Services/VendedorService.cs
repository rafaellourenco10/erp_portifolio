// =====================================================================================
// Arquivo....: VendedorService.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Regras de negócio e persistência de vendedores (espelho de
//              FornecedorService): CPF normalizado e único entre vendedores (V2),
//              inativação lógica (V3).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db (connection string "ErpPortfolio")
// Tabelas....: public.vendedores
//                - SELECT : listagem (ILIKE no nome, ativo, ORDER BY nome, id, LIMIT/OFFSET),
//                           consulta por id e checagem de CPF duplicado
//                - INSERT : inclusão
//                - UPDATE : edição, reativação e inativação
// Fontes.....: ErpPortfolioDbContext.Vendedores (EF Core / Npgsql). O índice único
//              ix_vendedores_cpf cobre a corrida entre a checagem e o INSERT/UPDATE.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.DTOs.Validacoes;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ErpPortfolio.Api.Services;

public class VendedorService(ErpPortfolioDbContext contexto) : IVendedorService
{
    private const string MensagemCpfDuplicado = "Já existe um vendedor cadastrado com este CPF.";
    private const string MensagemCpfDuplicadoInativo =
        "Já existe um vendedor INATIVO com este CPF. Reative o cadastro existente em vez de criar outro.";

    public async Task<ResultadoPaginadoDto<VendedorRespostaDto>> ListarAsync(VendedorFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = contexto.Vendedores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var padrao = $"%{ClienteService.EscaparCuringasLike(filtro.Nome.Trim())}%";
            consulta = consulta.Where(v => EF.Functions.ILike(v.Nome, padrao));
        }

        if (filtro.Ativo is bool ativo)
        {
            consulta = consulta.Where(v => v.Ativo == ativo);
        }

        var totalItens = await consulta.CountAsync(cancelamento);

        var itens = await consulta
            .OrderBy(v => v.Nome)
            .ThenBy(v => v.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(v => VendedorRespostaDto.DeEntidade(v))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<VendedorRespostaDto>(itens, filtro.Pagina, filtro.TamanhoPagina, totalItens);
    }

    public async Task<VendedorRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancelamento)
    {
        var vendedor = await contexto.Vendedores.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, cancelamento);
        return vendedor is null ? null : VendedorRespostaDto.DeEntidade(vendedor);
    }

    public async Task<VendedorRespostaDto> CriarAsync(VendedorCriacaoDto dados, CancellationToken cancelamento)
    {
        var vendedor = new Vendedor
        {
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };
        AplicarDados(vendedor, dados);

        await GarantirCpfUnicoAsync(vendedor.Cpf, idIgnorado: null, cancelamento);

        contexto.Vendedores.Add(vendedor);
        await SalvarAsync(cancelamento);

        return VendedorRespostaDto.DeEntidade(vendedor);
    }

    public async Task<VendedorRespostaDto?> AtualizarAsync(int id, VendedorAtualizacaoDto dados, CancellationToken cancelamento)
    {
        var vendedor = await contexto.Vendedores.FirstOrDefaultAsync(v => v.Id == id, cancelamento);
        if (vendedor is null)
            return null;

        AplicarDados(vendedor, dados);
        if (dados.Ativo is bool ativo)
            vendedor.Ativo = ativo;

        await GarantirCpfUnicoAsync(vendedor.Cpf, idIgnorado: id, cancelamento);
        await SalvarAsync(cancelamento);

        return VendedorRespostaDto.DeEntidade(vendedor);
    }

    public async Task<bool> InativarAsync(int id, CancellationToken cancelamento)
    {
        var vendedor = await contexto.Vendedores.FirstOrDefaultAsync(v => v.Id == id, cancelamento);
        if (vendedor is null)
            return false;

        vendedor.Ativo = false;
        await SalvarAsync(cancelamento);

        return true;
    }

    private static void AplicarDados(Vendedor vendedor, VendedorCriacaoDto dados)
    {
        vendedor.Nome = dados.Nome.Trim();
        vendedor.Cpf = DocumentoValidador.Normalizar(dados.Cpf);
        vendedor.Email = dados.Email;
        vendedor.Telefone = dados.Telefone;
        vendedor.PercentualComissao = dados.PercentualComissao;
    }

    private async Task GarantirCpfUnicoAsync(string cpf, int? idIgnorado, CancellationToken cancelamento)
    {
        var existente = await contexto.Vendedores
            .Where(v => v.Cpf == cpf && v.Id != idIgnorado)
            .Select(v => new { v.Ativo })
            .FirstOrDefaultAsync(cancelamento);

        if (existente is not null)
            throw new ConflitoException(existente.Ativo ? MensagemCpfDuplicado : MensagemCpfDuplicadoInativo);
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
            throw new ConflitoException(MensagemCpfDuplicado);
        }
    }
}
