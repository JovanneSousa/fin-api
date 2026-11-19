## 📌 Resumo
<!-- Descreva de forma objetiva o que foi alterado e qual problema resolve. -->

## 🎯 Tipo de Alteração
- [ ] `feat` (Nova funcionalidade)
- [ ] `fix` / `hotfix` (Correção de bug)
- [ ] `refactor` (Refatoração sem mudança de comportamento)
- [ ] `docs` (Documentação / `CONTEXT.md` / `docs/`)
- [ ] `test` (Testes unitários ou de integração)
- [ ] `perf` (Otimização de consulta / performance)
- [ ] `chore` (Dependências, build, Docker, configs)
- [ ] `release` (Consolidação para `main`)

## 🔄 Camadas e Fluxos Impactados
<!-- Ex: Fin.API (TransacoesController), Fin.Application (TransactionService), Fin.Infra (TransacaoRepository), RabbitMQ -->
- 

## 🧪 Como foi testado
- [ ] Build da solution (`dotnet build fin-api.sln`) sem erros
- [ ] Testes unitários (`dotnet test`) executados com sucesso *(quando aplicável)*
- [ ] Testado manualmente via Swagger / `Fin.API.http` / Integração com `fin` ou `smartApi`

## ⚠️ Impactos em Consumidores e Deploy
<!-- Informe se houve mudança de contrato que afete o frontend (fin) ou o assistente (smartApi), novas variáveis em appsettings ou migrations pendentes. -->
- [ ] Sem quebra de contrato com `fin` (Frontend) e `smartApi` (Java/Telegram)
- [ ] Possui nova Migration do EF Core? (`dotnet ef database update`)
- [ ] Requer nova variável de ambiente / configuração?

## ✅ Checklist
- [ ] Branch base correta e sincronizada
- [ ] `CONTEXT.md` e `docs/flows.md` atualizados caso algum fluxo, endpoint ou contrato tenha mudado
- [ ] Sem segredos ou credenciais hardcoded nos arquivos alterados
