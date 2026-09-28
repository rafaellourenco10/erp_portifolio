# Plano de implementação: NF-e simulada (etapa 16)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aprovado e implementado** (28/09/2026).

## Visão geral

8 tarefas, 1 migration. O coração é um cálculo puro (CFOP, alíquotas, rateio, impostos, chave com DV), testado sem banco; o XML e o DANFE só leem o que o cálculo produziu e o que foi gravado. A API vem antes das telas, validada por E2E.

## Grafo de dependências

```
T1 NfeCalculo (puro, xUnit): CFOP, alíquotas por UF, rateio do desconto, impostos, chave + DV, validação
  └─► T2 Banco + cadastros: migration (empresa, campos em clientes/produtos, notas_fiscais, itens), GET/PUT /api/empresa, DTOs de cliente/produto
        └─► T3 Emissão de saída: NotaFiscalService, NfeXml (4.00 + protNFe), POST /api/pedidos/{id}/nfe, bloqueio de cancelar
              └─► T4 Consulta: lista/detalhe, XML, DANFE (QuestPDF), Excel/PDF ── CP1: API de saída pronta (E2E na 5099)
                    └─► T5 NF-e de devolução: POST /api/devolucoes/{id}/nfe (entrada, refNFe) ── CP2: E2E completo
                          └─► T6 Telas de cadastro: Fiscal → Empresa, dados fiscais em Cliente e Produto
                                └─► T7 Telas de emissão: botões no pedido e na devolução, tela Notas Fiscais, Playwright
                                      └─► T8 Fechamento (README, SPEC, grafo, memória)
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| `NfeCalculo` puro, recebendo itens e UFs já carregados | Toda a regra fiscal testável em xUnit. |
| Itens e totais gravados + XML guardado como texto | A nota é imutável (NF8); DANFE e lista não recalculam nada. |
| XML com `System.Xml.Linq` | O layout é só uma árvore de elementos; sem lib nova. |
| Numeração em `empresa.proximo_numero_nfe`, incrementada na mesma transação da nota + índice único (serie, numero) | Duas emissões simultâneas não repetem número. |
| Tabela de alíquotas internas das 27 UFs no código | São 27 números estáveis; tabela no banco seria configuração que ninguém edita. |

## Riscos

| Risco | Mitigação |
|---|---|
| DV da chave errado passa despercebido | Testar contra chaves de notas reais publicadas. |
| Rateio do desconto não fechar com o total do pedido | Sobra no último item e teste com desconto que gera dízima. |
| XML fora do layout | Conferir nós e ordem contra o Manual de Orientação do Contribuinte 7.0 no teste. |
