// =====================================================================================
// Arquivo....: NotaFiscalService.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Emissão da NF-e simulada (SPEC.md etapa 16): confere os dados fiscais (NF1),
//              numera na série com a linha da empresa travada, calcula itens e impostos pelo
//              NfeCalculo (NF2-NF5), grava a nota já autorizada com protocolo simulado e o
//              XML do NfeXml (NF6). A nota não muda depois (NF8). Também lista (NF9), detalha
//              e entrega o XML (o DANFE sai dele, pelo ExportadorDanfe).
// -------------------------------------------------------------------------------------
// Banco......: PostgreSQL - erp_portfolio_db
// Tabelas....: public.notas_fiscais, public.nota_fiscal_itens (grava); public.empresa
//              (SELECT ... FOR UPDATE), public.pedidos, public.clientes, public.produtos (lê).
// Fontes.....: NotasFiscaisController, PedidosController.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Data;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErpPortfolio.Api.Services;

/// <summary>Dados que faltam para emitir (NF1); vira 400 com a lista.</summary>
public class NfePendenteException(IReadOnlyList<string> pendencias)
    : Exception("Faltam dados para emitir a NF-e: " + string.Join("; ", pendencias))
{
    public IReadOnlyList<string> Pendencias { get; } = pendencias;
}

public class NotaFiscalService(ErpPortfolioDbContext contexto)
{
    /// <summary>NF1-NF6. Nulo = pedido inexistente.</summary>
    public async Task<NotaFiscalDetalheDto?> EmitirDoPedidoAsync(int pedidoId, CancellationToken cancelamento)
    {
        await using var transacao = await contexto.Database.BeginTransactionAsync(cancelamento);
        var empresa = await TravarEmpresaAsync(cancelamento);

        var pedido = await contexto.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == pedidoId, cancelamento);
        if (pedido is null)
            return null;

        if (pedido.Status != StatusPedido.Confirmado)
            throw new ConflitoException($"O pedido {pedidoId} está {pedido.Status}; só pedido confirmado emite NF-e.");

        if (await contexto.NotasFiscais.AnyAsync(n => n.PedidoId == pedidoId && n.Tipo == TipoNotaFiscal.Saida, cancelamento))
            throw new ConflitoException($"O pedido {pedidoId} já tem NF-e emitida.");

        var itens = pedido.Itens.OrderBy(i => i.Id).ToList();
        Conferir(empresa, pedido.Cliente!, itens.Select(i => i.Produto!));

        var entradas = itens
            .Select(i => new NfeCalculo.ItemEntrada(i.Quantidade, i.PrecoUnitario, CalculoPedido.Subtotal(i.Quantidade, i.PrecoUnitario, i.DescontoPercentual)))
            .ToList();
        var aliquota = NfeCalculo.AliquotaIcms(empresa!.Uf, pedido.Cliente!.Uf);
        var cfop = NfeCalculo.Cfop(devolucao: false, empresa.Uf, pedido.Cliente.Uf);

        var nota = await MontarAsync(TipoNotaFiscal.Saida, empresa, pedido.Cliente, pedidoId,
            itens.Select(i => i.Produto!).ToList(), entradas, pedido.ValorTotal, aliquota, cfop, cancelamento);
        nota.Xml = NfeXml.Gerar(nota, empresa, pedido.Cliente, pedido.FormaPagamento, chaveReferenciada: null);

        contexto.NotasFiscais.Add(nota);
        await contexto.SaveChangesAsync(cancelamento);
        await transacao.CommitAsync(cancelamento);

        return NotaFiscalDetalheDto.DeEntidade(nota, chaveReferenciada: null);
    }

    public async Task<ResultadoPaginadoDto<NotaFiscalResumoDto>> ListarAsync(NotaFiscalFiltroDto filtro, CancellationToken cancelamento)
    {
        var consulta = Filtrar(filtro);
        var total = await consulta.CountAsync(cancelamento);
        var itens = await Resumir(consulta
                .OrderByDescending(n => n.DataEmissao).ThenByDescending(n => n.Id)
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina))
            .ToListAsync(cancelamento);

        return new ResultadoPaginadoDto<NotaFiscalResumoDto>(itens, filtro.Pagina, filtro.TamanhoPagina, total);
    }

    /// <summary>Todas as notas do filtro (sem paginar) para o Excel/PDF.</summary>
    public async Task<RelatorioModelo> ModeloAsync(NotaFiscalFiltroDto filtro, CancellationToken cancelamento)
    {
        var notas = await Resumir(Filtrar(filtro).OrderBy(n => n.DataEmissao).ThenBy(n => n.Id)).ToListAsync(cancelamento);
        var descricaoFiltros = new List<string>
        {
            filtro.DataInicio is null && filtro.DataFim is null
                ? "Período: todas"
                : $"Período: {filtro.DataInicio?.ToString("dd/MM/yyyy") ?? "início"} a {filtro.DataFim?.ToString("dd/MM/yyyy") ?? "hoje"}"
        };
        if (filtro.Tipo is { } tipo) descricaoFiltros.Add($"Tipo: {(tipo == TipoNotaFiscal.Saida ? "Saída" : "Entrada")}");
        if (filtro.ClienteId is not null && notas.Count > 0) descricaoFiltros.Add($"Cliente: {notas[0].DestinatarioNome}");
        if (!string.IsNullOrWhiteSpace(filtro.Busca)) descricaoFiltros.Add($"Busca: {filtro.Busca.Trim()}");

        return new RelatorioModelo(
            "Notas Fiscais (NF-e simulada)",
            $"notas-fiscais-{HorarioBrasilia.Hoje():yyyy-MM-dd}",
            HorarioBrasilia.ParaBrasilia(DateTime.UtcNow),
            descricaoFiltros,
            [
                new("Notas", notas.Count, TipoValor.Inteiro),
                new("Total de saídas", notas.Where(n => n.Tipo == TipoNotaFiscal.Saida).Sum(n => n.ValorTotal), TipoValor.Moeda),
                new("Total de entradas (devoluções)", notas.Where(n => n.Tipo == TipoNotaFiscal.Entrada).Sum(n => n.ValorTotal), TipoValor.Moeda),
            ],
            [
                new("Número", TipoValor.Inteiro), new("Série", TipoValor.Inteiro), new("Tipo", TipoValor.Texto),
                new("Emissão", TipoValor.DataHora), new("Destinatário", TipoValor.Texto), new("UF", TipoValor.Texto),
                new("Pedido", TipoValor.Inteiro), new("Total", TipoValor.Moeda), new("Chave de acesso", TipoValor.Texto),
            ],
            notas.Select(n => new object?[]
            {
                n.Numero, n.Serie, n.Tipo == TipoNotaFiscal.Saida ? "Saída" : "Entrada",
                HorarioBrasilia.ParaBrasilia(n.DataEmissao), n.DestinatarioNome, n.DestinatarioUf,
                n.PedidoId, n.ValorTotal, n.Chave
            }).ToList());
    }

    public async Task<NotaFiscalDetalheDto?> ObterAsync(int id, CancellationToken cancelamento)
    {
        var nota = await contexto.NotasFiscais.AsNoTracking()
            .Include(n => n.Itens)
            .Include(n => n.NotaReferenciada)
            .FirstOrDefaultAsync(n => n.Id == id, cancelamento);
        return nota is null ? null : NotaFiscalDetalheDto.DeEntidade(nota, nota.NotaReferenciada?.Chave);
    }

    /// <summary>Chave e XML da nota; nulo = inexistente.</summary>
    public async Task<(string Chave, string Xml)?> ObterXmlAsync(int id, CancellationToken cancelamento)
    {
        var nota = await contexto.NotasFiscais.AsNoTracking()
            .Where(n => n.Id == id)
            .Select(n => new { n.Chave, n.Xml })
            .FirstOrDefaultAsync(cancelamento);
        return nota is null ? null : (nota.Chave, nota.Xml);
    }

    private IQueryable<NotaFiscal> Filtrar(NotaFiscalFiltroDto filtro)
    {
        var consulta = contexto.NotasFiscais.AsNoTracking();
        if (filtro.DataInicio is { } inicio)
        {
            var desde = HorarioBrasilia.InicioDoDiaUtc(inicio);
            consulta = consulta.Where(n => n.DataEmissao >= desde);
        }
        if (filtro.DataFim is { } fim)
        {
            var ate = HorarioBrasilia.InicioDoDiaUtc(fim.AddDays(1));
            consulta = consulta.Where(n => n.DataEmissao < ate);
        }
        if (filtro.ClienteId is { } clienteId)
            consulta = consulta.Where(n => n.ClienteId == clienteId);
        if (filtro.Tipo is { } tipo)
            consulta = consulta.Where(n => n.Tipo == tipo);
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            // Número exato da nota ou trecho da chave (com ou sem os espaços do DANFE).
            var busca = new string(filtro.Busca.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            consulta = int.TryParse(busca, out var numero)
                ? consulta.Where(n => n.Numero == numero || n.Chave.Contains(busca))
                : consulta.Where(n => n.Chave.Contains(busca));
        }
        return consulta;
    }

    private static IQueryable<NotaFiscalResumoDto> Resumir(IQueryable<NotaFiscal> consulta) =>
        consulta.Select(n => new NotaFiscalResumoDto(
            n.Id, n.Tipo, n.Serie, n.Numero, n.Chave, n.DataEmissao, n.ClienteId, n.DestinatarioNome,
            n.DestinatarioDocumento, n.DestinatarioUf, n.PedidoId, n.DevolucaoId, n.ValorTotal));

    // Trava a linha da empresa até o fim da transação: duas emissões ao mesmo tempo não pegam o mesmo número.
    private async Task<Empresa?> TravarEmpresaAsync(CancellationToken cancelamento) =>
        await contexto.Empresa.FromSql($"SELECT * FROM empresa WHERE id = 1 FOR UPDATE").FirstOrDefaultAsync(cancelamento);

    /// <summary>NF1: tudo o que falta de uma vez, para o usuário corrigir numa passada só.</summary>
    private static void Conferir(Empresa? empresa, Cliente cliente, IEnumerable<Produto> produtos)
    {
        var pendencias = new List<string>();
        if (empresa is null)
            pendencias.Add("Cadastre os dados da empresa em Fiscal → Empresa.");

        var faltaCliente = new List<string>();
        if (cliente.Logradouro is null) faltaCliente.Add("logradouro");
        if (cliente.Numero is null) faltaCliente.Add("número");
        if (cliente.Bairro is null) faltaCliente.Add("bairro");
        if (cliente.Cep is null) faltaCliente.Add("CEP");
        if (cliente.CodigoMunicipio is null) faltaCliente.Add("código IBGE do município");
        if (faltaCliente.Count > 0)
            pendencias.Add($"Cliente \"{cliente.Nome}\" sem {string.Join(", ", faltaCliente)}.");

        foreach (var produto in produtos.Where(p => !NfeCalculo.NcmValido(p.Ncm)).DistinctBy(p => p.Id))
            pendencias.Add($"Produto \"{produto.Nome}\" sem NCM.");

        if (pendencias.Count > 0)
            throw new NfePendenteException(pendencias);
    }

    private async Task<NotaFiscal> MontarAsync(
        TipoNotaFiscal tipo, Empresa empresa, Cliente cliente, int pedidoId, IReadOnlyList<Produto> produtos,
        IReadOnlyList<NfeCalculo.ItemEntrada> entradas, decimal valorTotal, decimal aliquota, string cfop,
        CancellationToken cancelamento)
    {
        var numero = (await contexto.NotasFiscais.Where(n => n.Serie == empresa.SerieNfe).MaxAsync(n => (int?)n.Numero, cancelamento) ?? 0) + 1;
        var emissao = DateTime.UtcNow;
        emissao = emissao.AddTicks(-(emissao.Ticks % TimeSpan.TicksPerSecond));

        // cNF: aleatório de 8 dígitos e diferente do número da nota (regra do layout).
        var codigoNumerico = Random.Shared.Next(10_000_000, 100_000_000);
        if (codigoNumerico == numero)
            codigoNumerico++;

        var calculados = NfeCalculo.Itens(entradas, valorTotal, aliquota);
        var totais = NfeCalculo.Somar(calculados);

        return new NotaFiscal
        {
            Tipo = tipo,
            Serie = empresa.SerieNfe,
            Numero = numero,
            Chave = NfeCalculo.Chave(empresa.Uf, HorarioBrasilia.ParaBrasilia(emissao), empresa.Cnpj, empresa.SerieNfe, numero, codigoNumerico),
            DataEmissao = emissao,
            // Formato do protocolo real: 1 + cUF + ano (2) + sequencial (10).
            Protocolo = $"1{NfeCalculo.CodigoUf[empresa.Uf]}{emissao:yy}{Random.Shared.NextInt64(0, 10_000_000_000):D10}",
            ClienteId = cliente.Id,
            PedidoId = pedidoId,
            DestinatarioNome = cliente.Nome,
            DestinatarioDocumento = cliente.Documento,
            DestinatarioUf = cliente.Uf,
            ValorProdutos = totais.ValorProdutos,
            ValorDesconto = totais.ValorDesconto,
            BaseIcms = totais.BaseIcms,
            ValorIcms = totais.ValorIcms,
            ValorPis = totais.ValorPis,
            ValorCofins = totais.ValorCofins,
            ValorTotal = totais.ValorTotal,
            Itens = calculados.Select((c, i) => new NotaFiscalItem
            {
                NumeroItem = i + 1,
                ProdutoId = produtos[i].Id,
                Codigo = produtos[i].Sku,
                Descricao = produtos[i].Nome,
                Ncm = produtos[i].Ncm!,
                Cfop = cfop,
                Unidade = produtos[i].Unidade,
                Quantidade = entradas[i].Quantidade,
                ValorUnitario = entradas[i].ValorUnitario,
                ValorBruto = c.ValorBruto,
                ValorDesconto = c.ValorDesconto,
                BaseIcms = c.BaseIcms,
                AliquotaIcms = c.AliquotaIcms,
                ValorIcms = c.ValorIcms,
                ValorPis = c.ValorPis,
                ValorCofins = c.ValorCofins
            }).ToList()
        };
    }
}
