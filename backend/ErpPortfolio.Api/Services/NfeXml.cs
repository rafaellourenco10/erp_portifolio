// =====================================================================================
// Arquivo....: NfeXml.cs
// Versão.....: 1.0.0
// Data.......: 28/09/2026
// Descrição..: Monta o XML da NF-e simulada (SPEC.md etapa 16, NF6): nfeProc com a NFe no
//              layout 4.00 (ide, emit, dest, det, total, transp, pag, infAdic) e o protNFe
//              da autorização simulada. Ambiente de homologação (tpAmb 2) e SEM assinatura
//              digital: é um XML de demonstração, não é aceito pela SEFAZ.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Manual de Orientação do Contribuinte (MOC) 7.0, leiaute 4.00. Usado pelo
//              NotaFiscalService; testado em NfeXmlTests.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 28/09/2026 - Criação do arquivo.
// =====================================================================================

using System.Globalization;
using System.Xml.Linq;
using ErpPortfolio.Api.Models;

namespace ErpPortfolio.Api.Services;

public static class NfeXml
{
    public static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

    /// <summary>Em homologação a SEFAZ exige este texto no lugar do nome do destinatário.</summary>
    public const string NomeHomologacao = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";

    /// <param name="nota">Com os itens, a chave, o protocolo e os totais já preenchidos.</param>
    /// <param name="empresa">Emitente.</param>
    /// <param name="cliente">Destinatário (endereço completo validado antes).</param>
    /// <param name="formaPagamento">Nula na devolução (sem pagamento, tPag 90).</param>
    /// <param name="chaveReferenciada">Só na devolução: chave da NF-e de saída devolvida.</param>
    public static string Gerar(NotaFiscal nota, Empresa empresa, Cliente cliente, FormaPagamento? formaPagamento, string? chaveReferenciada)
    {
        var devolucao = nota.Tipo == TipoNotaFiscal.Entrada;
        var interestadual = empresa.Uf != cliente.Uf;
        var contribuinte = cliente.Documento.Length == 14 && cliente.InscricaoEstadual is { } ie && ie != "ISENTO";
        var dhEmi = HorarioBrasilia.ComFuso(nota.DataEmissao).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        var ide = new XElement(Ns + "ide",
            E("cUF", NfeCalculo.CodigoUf[empresa.Uf]),
            E("cNF", nota.Chave.Substring(35, 8)),
            E("natOp", devolucao ? "Devolucao de venda" : "Venda de mercadoria"),
            E("mod", "55"),
            E("serie", nota.Serie),
            E("nNF", nota.Numero),
            E("dhEmi", dhEmi),
            E("tpNF", devolucao ? "0" : "1"),
            E("idDest", interestadual ? "2" : "1"),
            E("cMunFG", empresa.CodigoMunicipio),
            E("tpImp", "1"),
            E("tpEmis", "1"),
            E("cDV", nota.Chave[43]),
            E("tpAmb", "2"),
            E("finNFe", devolucao ? "4" : "1"),
            E("indFinal", contribuinte ? "0" : "1"),
            // 9 = não presencial (venda pelo ERP); 0 = não se aplica (devolução).
            E("indPres", devolucao ? "0" : "9"),
            devolucao ? null : E("indIntermed", "0"),
            E("procEmi", "0"),
            E("verProc", "Ambition ERP 1.0"),
            chaveReferenciada is null ? null : new XElement(Ns + "NFref", E("refNFe", chaveReferenciada)));

        var emit = new XElement(Ns + "emit",
            E("CNPJ", empresa.Cnpj),
            E("xNome", empresa.RazaoSocial),
            empresa.NomeFantasia is null ? null : E("xFant", empresa.NomeFantasia),
            new XElement(Ns + "enderEmit",
                E("xLgr", empresa.Logradouro),
                E("nro", empresa.Numero),
                empresa.Complemento is null ? null : E("xCpl", empresa.Complemento),
                E("xBairro", empresa.Bairro),
                E("cMun", empresa.CodigoMunicipio),
                E("xMun", empresa.Municipio),
                E("UF", empresa.Uf),
                E("CEP", empresa.Cep),
                E("cPais", "1058"),
                E("xPais", "Brasil"),
                empresa.Telefone is null ? null : E("fone", new string(empresa.Telefone.Where(char.IsAsciiDigit).ToArray()))),
            E("IE", empresa.InscricaoEstadual),
            // 3 = regime normal.
            E("CRT", "3"));

        var dest = new XElement(Ns + "dest",
            E(cliente.Documento.Length == 14 ? "CNPJ" : "CPF", cliente.Documento),
            E("xNome", NomeHomologacao),
            new XElement(Ns + "enderDest",
                E("xLgr", cliente.Logradouro),
                E("nro", cliente.Numero),
                cliente.Complemento is null ? null : E("xCpl", cliente.Complemento),
                E("xBairro", cliente.Bairro),
                E("cMun", cliente.CodigoMunicipio),
                E("xMun", cliente.Cidade),
                E("UF", cliente.Uf),
                E("CEP", cliente.Cep),
                E("cPais", "1058"),
                E("xPais", "Brasil")),
            // 1 = contribuinte com IE; 9 = não contribuinte (CPF, CNPJ sem IE ou isento).
            E("indIEDest", contribuinte ? "1" : "9"),
            contribuinte ? E("IE", cliente.InscricaoEstadual) : null,
            cliente.Email is null ? null : E("email", cliente.Email));

        var dets = nota.Itens.OrderBy(i => i.NumeroItem).Select(i => new XElement(Ns + "det",
            new XAttribute("nItem", i.NumeroItem),
            new XElement(Ns + "prod",
                E("cProd", i.Codigo),
                E("cEAN", "SEM GTIN"),
                E("xProd", i.Descricao),
                E("NCM", i.Ncm),
                E("CFOP", i.Cfop),
                E("uCom", i.Unidade),
                E("qCom", Quantidade(i.Quantidade)),
                E("vUnCom", Unitario(i.ValorUnitario)),
                E("vProd", Valor(i.ValorBruto)),
                E("cEANTrib", "SEM GTIN"),
                E("uTrib", i.Unidade),
                E("qTrib", Quantidade(i.Quantidade)),
                E("vUnTrib", Unitario(i.ValorUnitario)),
                i.ValorDesconto > 0 ? E("vDesc", Valor(i.ValorDesconto)) : null,
                E("indTot", "1")),
            new XElement(Ns + "imposto",
                new XElement(Ns + "ICMS", new XElement(Ns + "ICMS00",
                    E("orig", "0"), E("CST", "00"), E("modBC", "3"),
                    E("vBC", Valor(i.BaseIcms)), E("pICMS", Aliquota(i.AliquotaIcms)), E("vICMS", Valor(i.ValorIcms)))),
                new XElement(Ns + "PIS", new XElement(Ns + "PISAliq",
                    E("CST", "01"), E("vBC", Valor(i.BaseIcms)), E("pPIS", Aliquota(NfeCalculo.AliquotaPis)), E("vPIS", Valor(i.ValorPis)))),
                new XElement(Ns + "COFINS", new XElement(Ns + "COFINSAliq",
                    E("CST", "01"), E("vBC", Valor(i.BaseIcms)), E("pCOFINS", Aliquota(NfeCalculo.AliquotaCofins)), E("vCOFINS", Valor(i.ValorCofins)))))));

        var total = new XElement(Ns + "total", new XElement(Ns + "ICMSTot",
            E("vBC", Valor(nota.BaseIcms)), E("vICMS", Valor(nota.ValorIcms)), E("vICMSDeson", "0.00"),
            E("vFCP", "0.00"), E("vBCST", "0.00"), E("vST", "0.00"), E("vFCPST", "0.00"), E("vFCPSTRet", "0.00"),
            E("vProd", Valor(nota.ValorProdutos)), E("vFrete", "0.00"), E("vSeg", "0.00"), E("vDesc", Valor(nota.ValorDesconto)),
            E("vII", "0.00"), E("vIPI", "0.00"), E("vIPIDevol", "0.00"), E("vPIS", Valor(nota.ValorPis)),
            E("vCOFINS", Valor(nota.ValorCofins)), E("vOutro", "0.00"), E("vNF", Valor(nota.ValorTotal))));

        var pag = new XElement(Ns + "pag", new XElement(Ns + "detPag",
            E("tPag", CodigoPagamento(formaPagamento)),
            E("vPag", Valor(formaPagamento is null ? 0 : nota.ValorTotal))));

        var infCpl = devolucao
            ? $"Devolucao referente a NF-e {chaveReferenciada}. Documento simulado sem valor fiscal."
            : "Documento simulado sem valor fiscal, emitido pelo Ambition ERP (portfolio).";

        var nfe = new XElement(Ns + "NFe",
            new XElement(Ns + "infNFe", new XAttribute("Id", "NFe" + nota.Chave), new XAttribute("versao", "4.00"),
                ide, emit, dest, dets, total,
                new XElement(Ns + "transp", E("modFrete", "9")),
                pag,
                new XElement(Ns + "infAdic", E("infCpl", infCpl))));

        var protocolo = new XElement(Ns + "protNFe", new XAttribute("versao", "4.00"),
            new XElement(Ns + "infProt",
                E("tpAmb", "2"),
                E("verAplic", "SIMULADO"),
                E("chNFe", nota.Chave),
                E("dhRecbto", dhEmi),
                E("nProt", nota.Protocolo),
                E("cStat", "100"),
                E("xMotivo", "Autorizado o uso da NF-e")));

        var documento = new XDocument(new XDeclaration("1.0", "UTF-8", null),
            new XElement(Ns + "nfeProc", new XAttribute("versao", "4.00"), nfe, protocolo));

        return documento.Declaration + documento.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>tPag: 01 dinheiro, 17 PIX, 15 boleto, 03 cartão de crédito, 90 sem pagamento.</summary>
    private static string CodigoPagamento(FormaPagamento? forma) => forma switch
    {
        FormaPagamento.Dinheiro => "01",
        FormaPagamento.Pix => "17",
        FormaPagamento.Boleto => "15",
        FormaPagamento.Cartao => "03",
        _ => "90"
    };

    private static XElement E(string nome, object? valor) => new(Ns + nome, valor);

    private static string Valor(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Aliquota(decimal v) => v.ToString("0.00##", CultureInfo.InvariantCulture);

    private static string Quantidade(decimal v) => v.ToString("0.0000", CultureInfo.InvariantCulture);

    private static string Unitario(decimal v) => v.ToString("0.00########", CultureInfo.InvariantCulture);
}
