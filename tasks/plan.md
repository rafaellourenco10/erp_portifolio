# Plano de implementação: Módulo Relatórios (etapa 9)

> Origem: [SPEC.md](../SPEC.md). Tarefas detalhadas e checklist em [todo.md](todo.md).
> Status: **aguardando aprovação**.

## Visão geral

7 tarefas. O ponto central é **um exportador genérico** (modelo → .xlsx/.pdf) que os três relatórios usam, e **um componente de tela** compartilhado entre Vendas e Compras. Sem migration: só leitura das tabelas existentes.

## Grafo de dependências

```
T1 RelatorioService + DTOs + Controller (JSON) + xUnit do período/resumo
  └─► T2 ExportadorRelatorio: Excel (ClosedXML)
        └─► T3 ExportadorRelatorio: PDF (QuestPDF)          ── CP1: API pronta (E2E)
              T4 Front: tipos, api, download, menu Relatórios
                ├─► T5 Tela de Vendas/Compras (componente único, 2 rotas)
                └─► T6 Tela de Estoque
                      └─► T7 Fechamento (README, spec, graphify)
```

## Decisões de arquitetura

| Decisão | Motivo |
|---|---|
| Um endpoint por relatório com `formato=json|xlsx|pdf` | Tela e arquivos saem da mesma consulta (R3); menos rotas. |
| Exportador genérico (título, filtros, resumo, colunas, linhas) | Três relatórios × dois formatos viraria seis geradores; assim são dois. |
| Relatório sem paginação, período limitado a 366 dias | É um relatório, não uma lista; o limite segura o volume. |
| Sem xUnit tocando banco | Padrão do projeto; consultas verificadas por E2E. |

## Riscos e mitigações

| Risco | Mitigação |
|---|---|
| Licença do QuestPDF | Community declarada em `Program.cs`; registrada na spec e no README. |
| Números da tela diferentes dos arquivos | Mesma consulta alimenta os três; E2E compara JSON × .xlsx. |
| Download no front (blob) quebrando nome do arquivo | Servidor manda `Content-Disposition`; CORS expõe o cabeçalho; E2E confere o nome. |

## Comandos de verificação

```
dotnet build ErpPortfolio.slnx -c Release
dotnet test backend/ErpPortfolio.Tests -c Release
cd frontend/erp-portfolio-web; npx tsc -b; npx oxlint src; npm run build
```
