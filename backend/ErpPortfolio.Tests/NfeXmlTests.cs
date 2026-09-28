// =====================================================================================
// Arquivo....: NfeXmlTests.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Estrutura do XML da NF-e simulada (etapa 16, NF6): nós principais, ordem dos
//              grupos do infNFe, destinatário de homologação, contribuinte × não contribuinte,
//              devolução com refNFe e o protNFe.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/NfeXml.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Xml.Linq;
using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class NfeXmlTests
{
    private static readonly XNamespace Ns = NfeXml.Ns;

    private static Empresa Empresa() => new()
    {
        Id = 1, RazaoSocial = "Ambition Ltda", Cnpj = "11222333000181", InscricaoEstadual = "110042490114",
        Logradouro = "Av. Paulista", Numero = "1000", Bairro = "Bela Vista", Cep = "01310100",
        Municipio = "São Paulo", CodigoMunicipio = "3550308", Uf = "SP", SerieNfe = 1
    };

    private static Cliente Cliente(string documento = "52998224725", string uf = "SP", string? ie = null) => new()
    {
        Id = 7, Nome = "Maria", Documento = documento, Cidade = "Rio de Janeiro", Uf = uf, Logradouro = "Rua A",
        Numero = "10", Bairro = "Centro", Cep = "20000000", CodigoMunicipio = "3304557", InscricaoEstadual = ie
    };

    private static NotaFiscal Nota(TipoNotaFiscal tipo = TipoNotaFiscal.Saida) => new()
    {
        Tipo = tipo, Serie = 1, Numero = 42,
        Chave = NfeCalculo.Chave("SP", new DateTime(2026, 9, 28), "11222333000181", 1, 42, 12345678),
        DataEmissao = new DateTime(2026, 9, 28, 13, 0, 0, DateTimeKind.Utc), Protocolo = "135260000000001",
        ValorProdutos = 100, ValorDesconto = 10, BaseIcms = 90, ValorIcms = 16.20m, ValorPis = 1.49m, ValorCofins = 6.84m, ValorTotal = 90,
        Itens =
        [
            new NotaFiscalItem
            {
                NumeroItem = 1, Codigo = "10012345", Descricao = "Cabo HDMI", Ncm = "85444200", Cfop = "5102", Unidade = "UN",
                Quantidade = 2, ValorUnitario = 50, ValorBruto = 100, ValorDesconto = 10, BaseIcms = 90,
                AliquotaIcms = 18, ValorIcms = 16.20m, ValorPis = 1.49m, ValorCofins = 6.84m
            }
        ]
    };

    private static XElement Gerar(NotaFiscal nota, Cliente cliente, FormaPagamento? pagamento = FormaPagamento.Pix, string? referenciada = null) =>
        XDocument.Parse(NfeXml.Gerar(nota, Empresa(), cliente, pagamento, referenciada)).Root!;

    private static string Valor(XElement raiz, params string[] caminho) =>
        caminho.Aggregate(raiz, (no, nome) => no.Element(Ns + nome)!).Value;

    [Fact]
    public void Xml_tem_nfeProc_com_infNFe_nos_grupos_na_ordem_do_layout()
    {
        var nota = Nota();
        var raiz = Gerar(nota, Cliente());
        var inf = raiz.Element(Ns + "NFe")!.Element(Ns + "infNFe")!;

        Assert.Equal(Ns + "nfeProc", raiz.Name);
        Assert.Equal("NFe" + nota.Chave, inf.Attribute("Id")!.Value);
        Assert.Equal("4.00", inf.Attribute("versao")!.Value);
        Assert.Equal(
            ["ide", "emit", "dest", "det", "total", "transp", "pag", "infAdic"],
            inf.Elements().Select(e => e.Name.LocalName));
    }

    [Fact]
    public void Ide_da_venda_em_homologacao_com_cNF_e_cDV_tirados_da_chave()
    {
        var nota = Nota();
        var raiz = Gerar(nota, Cliente());
        string Ide(string no) => Valor(raiz, "NFe", "infNFe", "ide", no);

        Assert.Equal("35", Ide("cUF"));
        Assert.Equal("12345678", Ide("cNF"));
        Assert.Equal(nota.Chave[43].ToString(), Ide("cDV"));
        Assert.Equal("42", Ide("nNF"));
        Assert.Equal("1", Ide("tpNF"));
        Assert.Equal("2", Ide("tpAmb"));
        Assert.Equal("1", Ide("finNFe"));
        Assert.Equal("2026-09-28T10:00:00-03:00", Ide("dhEmi"));   // 13h UTC = 10h em Brasília
    }

    [Fact]
    public void Destinatario_usa_o_nome_de_homologacao_e_cpf_nao_contribuinte()
    {
        var dest = Gerar(Nota(), Cliente()).Descendants(Ns + "dest").Single();

        Assert.Equal(NfeXml.NomeHomologacao, dest.Element(Ns + "xNome")!.Value);
        Assert.Equal("52998224725", dest.Element(Ns + "CPF")!.Value);
        Assert.Equal("9", dest.Element(Ns + "indIEDest")!.Value);
        Assert.Null(dest.Element(Ns + "IE"));
    }

    [Fact]
    public void Cnpj_com_ie_e_contribuinte_e_outra_uf_e_interestadual()
    {
        var raiz = Gerar(Nota(), Cliente("11444777000161", "RJ", "12345678"));
        var dest = raiz.Descendants(Ns + "dest").Single();

        Assert.Equal("11444777000161", dest.Element(Ns + "CNPJ")!.Value);
        Assert.Equal("1", dest.Element(Ns + "indIEDest")!.Value);
        Assert.Equal("12345678", dest.Element(Ns + "IE")!.Value);
        Assert.Equal("2", raiz.Descendants(Ns + "idDest").Single().Value);
        Assert.Equal("0", raiz.Descendants(Ns + "indFinal").Single().Value);
    }

    [Fact]
    public void Item_traz_produto_e_os_tres_impostos()
    {
        var det = Gerar(Nota(), Cliente()).Descendants(Ns + "det").Single();

        Assert.Equal("1", det.Attribute("nItem")!.Value);
        Assert.Equal("85444200", Valor(det, "prod", "NCM"));
        Assert.Equal("2.0000", Valor(det, "prod", "qCom"));
        Assert.Equal("50.00", Valor(det, "prod", "vUnCom"));
        Assert.Equal("10.00", Valor(det, "prod", "vDesc"));
        Assert.Equal("16.20", Valor(det, "imposto", "ICMS", "ICMS00", "vICMS"));
        Assert.Equal("18.00", Valor(det, "imposto", "ICMS", "ICMS00", "pICMS"));
        Assert.Equal("1.65", Valor(det, "imposto", "PIS", "PISAliq", "pPIS"));
        Assert.Equal("7.60", Valor(det, "imposto", "COFINS", "COFINSAliq", "pCOFINS"));
    }

    [Fact]
    public void Totais_e_pagamento()
    {
        var raiz = Gerar(Nota(), Cliente());

        Assert.Equal("90.00", Valor(raiz, "NFe", "infNFe", "total", "ICMSTot", "vNF"));
        Assert.Equal("16.20", Valor(raiz, "NFe", "infNFe", "total", "ICMSTot", "vICMS"));
        Assert.Equal("17", Valor(raiz, "NFe", "infNFe", "pag", "detPag", "tPag"));
        Assert.Equal("90.00", Valor(raiz, "NFe", "infNFe", "pag", "detPag", "vPag"));
    }

    [Fact]
    public void Protocolo_autoriza_a_mesma_chave()
    {
        var nota = Nota();
        var raiz = Gerar(nota, Cliente());

        Assert.Equal(nota.Chave, Valor(raiz, "protNFe", "infProt", "chNFe"));
        Assert.Equal("100", Valor(raiz, "protNFe", "infProt", "cStat"));
        Assert.Equal(nota.Protocolo, Valor(raiz, "protNFe", "infProt", "nProt"));
    }

    [Fact]
    public void Danfe_sai_do_xml_gravado_em_pdf()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var xml = NfeXml.Gerar(Nota(), Empresa(), Cliente("11444777000161", "RJ", "12345678"), FormaPagamento.Boleto, null);

        var pdf = ExportadorDanfe.GerarPdf(xml);

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void Devolucao_e_entrada_finalidade_4_com_refNFe_e_sem_pagamento()
    {
        var referenciada = new string('3', 44);
        var raiz = Gerar(Nota(TipoNotaFiscal.Entrada), Cliente(), pagamento: null, referenciada);
        string Ide(string no) => Valor(raiz, "NFe", "infNFe", "ide", no);

        Assert.Equal("0", Ide("tpNF"));
        Assert.Equal("4", Ide("finNFe"));
        Assert.Equal(referenciada, Valor(raiz, "NFe", "infNFe", "ide", "NFref", "refNFe"));
        Assert.Equal("90", Valor(raiz, "NFe", "infNFe", "pag", "detPag", "tPag"));
        Assert.Equal("0.00", Valor(raiz, "NFe", "infNFe", "pag", "detPag", "vPag"));
    }
}
