// =====================================================================================
// Arquivo....: RelatorioCalculoTests.cs
// Versão.....: 1.0.0
// Data.......: 23/09/2026
// Descrição..: Testes das regras puras dos relatórios: validação do período (R1, também
//              pelo DTO, como o [ApiController] faz) e resumo de pedidos.
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/RelatorioCalculo.cs, DTOs/RelatorioFiltroDtos.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 23/09/2026 - Criação do arquivo.
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using ErpPortfolio.Api.DTOs;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class RelatorioCalculoTests
{
    [Fact]
    public void Periodo_de_um_dia_so_e_valido()
    {
        var dia = new DateOnly(2026, 9, 23);
        Assert.Null(RelatorioCalculo.ErroPeriodo(dia, dia));
    }

    [Fact]
    public void Periodo_com_fim_antes_do_inicio_e_invalido()
    {
        Assert.NotNull(RelatorioCalculo.ErroPeriodo(new DateOnly(2026, 9, 23), new DateOnly(2026, 9, 22)));
    }

    [Fact]
    public void Periodo_de_366_dias_e_valido_e_de_367_nao()
    {
        var inicio = new DateOnly(2026, 1, 1);
        Assert.Null(RelatorioCalculo.ErroPeriodo(inicio, inicio.AddDays(365)));
        Assert.NotNull(RelatorioCalculo.ErroPeriodo(inicio, inicio.AddDays(366)));
    }

    [Fact]
    public void Resumo_sem_pedidos_tem_ticket_zero()
    {
        Assert.Equal((0, 0m, 0m), RelatorioCalculo.ResumoPedidos([]));
    }

    [Fact]
    public void Resumo_soma_e_calcula_o_ticket_medio()
    {
        Assert.Equal((3, 600m, 200m), RelatorioCalculo.ResumoPedidos([100m, 200m, 300m]));
    }

    [Fact]
    public void Filtro_sem_datas_tem_erro_nas_duas()
    {
        var erros = Validar(new RelatorioVendasFiltroDto());
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(RelatorioPedidosFiltroDto.DataInicio)));
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(RelatorioPedidosFiltroDto.DataFim)));
    }

    [Fact]
    public void Filtro_com_periodo_invertido_tem_erro_na_data_final()
    {
        var erros = Validar(new RelatorioComprasFiltroDto { DataInicio = new DateOnly(2026, 9, 23), DataFim = new DateOnly(2026, 9, 1) });
        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(RelatorioPedidosFiltroDto.DataFim)));
    }

    [Fact]
    public void Filtro_com_periodo_valido_nao_tem_erro()
    {
        Assert.Empty(Validar(new RelatorioVendasFiltroDto { DataInicio = new DateOnly(2026, 9, 1), DataFim = new DateOnly(2026, 9, 30) }));
    }

    private static List<ValidationResult> Validar(object objeto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(objeto, new ValidationContext(objeto), resultados, validateAllProperties: true);
        return resultados;
    }
}
