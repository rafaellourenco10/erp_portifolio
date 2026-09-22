// =====================================================================================
// Arquivo....: EstoqueCalculoTests.cs
// Versão.....: 1.0.0
// Data.......: 22/09/2026
// Descrição..: Testes do cálculo de saldo de estoque (soma de entradas menos saídas).
// -------------------------------------------------------------------------------------
// Banco......: Não acessa banco.
// Tabelas....: Não se aplica.
// Fontes.....: Services/EstoqueCalculo.cs.
// -------------------------------------------------------------------------------------
// Histórico de alterações:
//   1.0.0 - 22/09/2026 - Criação do arquivo.
// =====================================================================================

using ErpPortfolio.Api.Models;
using ErpPortfolio.Api.Services;

namespace ErpPortfolio.Tests;

public class EstoqueCalculoTests
{
    [Fact]
    public void Saldo_sem_movimentacoes_e_zero()
    {
        Assert.Equal(0m, EstoqueCalculo.Saldo([]));
    }

    [Fact]
    public void Saldo_so_entradas_e_a_soma()
    {
        (TipoMovimentacao, decimal)[] movimentacoes = [(TipoMovimentacao.Entrada, 10m), (TipoMovimentacao.Entrada, 5m)];

        Assert.Equal(15m, EstoqueCalculo.Saldo(movimentacoes));
    }

    [Fact]
    public void Saldo_so_saidas_e_negativo()
    {
        (TipoMovimentacao, decimal)[] movimentacoes = [(TipoMovimentacao.Saida, 4m), (TipoMovimentacao.Saida, 3m)];

        Assert.Equal(-7m, EstoqueCalculo.Saldo(movimentacoes));
    }

    [Fact]
    public void Saldo_misto_e_a_diferenca()
    {
        // Compra de 10, venda de 3, estorno de cancelamento de 3: 10 - 3 + 3 = 10
        (TipoMovimentacao, decimal)[] movimentacoes =
        [
            (TipoMovimentacao.Entrada, 10m),
            (TipoMovimentacao.Saida, 3m),
            (TipoMovimentacao.Entrada, 3m),
        ];

        Assert.Equal(10m, EstoqueCalculo.Saldo(movimentacoes));
    }

    [Fact]
    public void Saldo_pode_ficar_negativo_se_as_movimentacoes_permitirem()
    {
        // A regra de bloquear confirmação sem saldo (E2) fica no serviço, não no cálculo puro.
        (TipoMovimentacao, decimal)[] movimentacoes = [(TipoMovimentacao.Entrada, 2m), (TipoMovimentacao.Saida, 5m)];

        Assert.Equal(-3m, EstoqueCalculo.Saldo(movimentacoes));
    }
}
