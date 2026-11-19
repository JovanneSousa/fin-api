# CONTEXT.md — Fin-API (.NET 10)

> **Propósito:** Fonte única de verdade (AI-First) para agentes de IA e desenvolvedores entenderem rapidamente a arquitetura, padrões de código, fluxos existentes, armadilhas conhecidas, diferenças entre branches e regras de contribuição da **Fin-API**.

---

## 1. Visão Geral do Projeto

API RESTful de gestão de finanças pessoais em **.NET 10** (`net10.0`), responsável por gerenciar usuários financeiros, categorias (padrão e personalizadas), ícones, cores, transações (únicas, recorrentes de receita e parceladas de despesa), saldos e agregações mensais.

### Integrações no Ecossistema
- **`Auth.Api` (.NET 10):** Serviço central de Identidade (IdP). Emite tokens JWT assinados assimetricamente validados pela `Fin-API` via **JWKS Discovery** (`/jwks`) e publica eventos de registro de usuário.
- **`RabbitMQ` (`Jovane.MessageBus` v1.0.7):** Comunicação assíncrona via padrão **Request/Response** (`UsuarioRegistradoIntegrationEvent` -> `ResponseMessage`) consumida pelo background worker `RegistroUsuarioIntegrationHandler`.
- **`FinControl` (`fin` - React 19 SPA):** Frontend web que consome os endpoints de transações, categorias e perfil do usuário.
- **`SmartApi` (Java 25 + Spring Boot 4 + Spring AI):** Assistente financeiro via voz/texto e Bot do Telegram que consome `GET /api/categories` e `POST /api/transacoes/novo` via OpenFeign.

---

## 2. Estrutura da Solution (`fin-api.sln`)

> **Atenção ao estado das branches (`main` vs `preview-prod`):**
> A branch `main` contém a evolução mais recente da arquitetura limpa/DDD (com `Tests/Application.UnitTests`, `TransactionRequest` em inglês, `Jovanne.Jwks.Client`, `GlobalExceptionHandler` e inversão de dependência completa onde `Fin.Application` referencia apenas `Fin.Domain`). Já a branch `preview-prod` contém um snapshot anterior de deploy onde `DTOs`, `INotificador` e interfaces de repositórios ainda residiam em `Fin.Infra`. **Sempre verifique em qual branch você está trabalhando (`git branch --show-current`) antes de editar namespaces.**

| Projeto | Responsabilidade | Observações |
| :--- | :--- | :--- |
| `Src/Fin.API` | Entry point (`Program.cs`), Controllers (`MainController`, `TransacoesController`, `CategoriesController`, `UsuarioController`), DI, CORS, Auth/JWKS, Swagger, Serilog e Exception Handling | `net10.0`. Valida configurações obrigatórias no startup (`DefaultConnection`, `MEU_APP`, `JwtSettings:AutenticacaoJwksUrl`). |
| `Src/Fin.Application` | Regras de negócio (`TransactionService`, `CategoriaService`, `UsuarioService`), Background Consumer (`RegistroUsuarioIntegrationHandler`), AutoMapper Profiles, DTOs e Contratos | Herda de `BaseService` com Notification Pattern (`INotificador`) e tratamento de `DatabaseException`. |
| `Src/Fin.Domain` | Entidades (`Transacao`, `Categoria`, `Usuario`, `Icon`, `Cor`, `UserHiddenCategory`, `IconeCategoriaUsuario`, `CorCategoriaUsuario`), Enums (`TransacaoType`, `RecorrenciaType`), `UsuarioValidation` (FluentValidation) e `DatabaseException` | Sem dependências de infraestrutura. |
| `Src/Fin.Infra` | `ApiDbContext` (EF Core + PostgreSQL/Npgsql), Repositórios (`TransacaoRepository`, `CategoriaRepository`, `UsuarioRepository`), `Notificador` e Migrations | Repositórios herdam de `BaseRepository`, que encapsula chamadas ao EF Core e converte falhas em `DatabaseException`. |
| `Tests/Application.UnitTests` | Testes unitários (`xUnit` + `Moq` + `Bogus`) cobrindo `TransactionService`, `CategoriaService` e `UsuarioService` + documentação de casos em `Tests/Application.UnitTests/docs/` | Presente nas branches `main` e `feat/implementar-testes`. |

---

## 3. Padrões Arquiteturais Implementados (A Serem Seguidos)

Ao criar ou alterar funcionalidades na `Fin-API`, siga rigorosamente os padrões já estabelecidos:

1. **Envelope HTTP Padronizado (`MainController.CustomResponse`):**
   - Todas as controllers herdam de `MainController` e retornam `CustomResponse(result)`.
   - **Sucesso (`200 OK`):** `ResponseSuccessDTO<T>` -> `{ "success": true, "data": <payload> }`.
   - **Erro de Negócio / Validação (`400 Bad Request`):** `ResponseErrorDTO` -> `{ "success": false, "errors": ["mensagem 1", ...] }`.
2. **Notification Pattern (`INotificador` + `BaseService`):**
   - **Nunca** lance exceções para controle de fluxo de negócio nas camadas de `Application`.
   - Registre erros via `_notificador.Handle(...)` (ou `RetornaErroProcessamento<T>("mensagem")` em `BaseService`) e retorne `null` / `default` / `false`.
   - A `MainController` verifica `OperacaoValida()` (`!_notificador.TemNotificacao()`) automaticamente para decidir entre `200 OK` e `400 Bad Request`.
3. **Resiliência de Dados (`BaseRepository.ExecuteAsync` -> `DatabaseException` -> `BaseService.ExecuteAsync`):**
   - Todo método de repositório em `Fin.Infra` deve envolver consultas/comandos do EF Core em `await ExecuteAsync(async () => ...)`.
   - `BaseRepository` captura `PostgresException`, `NpgsqlException`, `DbUpdateException` e `Exception`, relançando como `DatabaseException`.
   - Na camada de serviço, `BaseService.ExecuteAsync` captura `DatabaseException`, notifica `"Erro no banco: {ex.Message}"` no `INotificador` e evita que exceções cruas vazem sem envelope padronizado.
4. **Identidade Contextual e Isolamento Multi-Tenant por Usuário (`IUser`):**
   - O ID do usuário autenticado vem sempre do token JWT (`ClaimTypes.NameIdentifier` / `sub`) injetado via `IUser` (`UsuarioId` na `MainController`).
   - **Nunca** confie em `UserId` vindo do body da requisição para autorizar leitura, edição ou exclusão; valide sempre `entidade.UserId == userId`.
5. **Autorização Baseada em Claims (`[ClaimsAuthorize]`):**
   - Endpoints protegidos validam claims do tipo `"permission"` com valores granulares do sistema `FIN`:
     - Categorias: `FIN:CTG_LER`, `FIN:CTG_CRIAR`, `FIN:CTG_EXCLUIR`
     - Transações e Usuário: `FIN:TRN_LER`, `FIN:TRN_CRIAR`, `FIN:TRN_EDITAR`, `FIN:TRN_EXCLUIR`
6. **Categorias Padrão (`IsDefault`) vs Personalizadas por Usuário:**
   - Categorias globais (`IsDefault = true`) não são deletadas nem alteradas diretamente na tabela `Categories`.
   - Quando um usuário "deleta" uma categoria padrão, cria-se um registro em `UserHiddenCategories`. Se ele criar uma categoria com o mesmo nome de uma padrão oculta, o registro em `UserHiddenCategories` é removido (`ShowHiddenCategory`).
   - Quando um usuário altera ícone ou cor de uma categoria padrão, salva-se um override em `IconeCategoriaUsuarios` ou `CorCategoriaUsuarios`. A entidade `Categoria` resolve a propriedade `[NotMapped] Icone` e `Cor` priorizando a customização do usuário sobre `IconePadrao` / `CorPadrao`.

---

## 4. Arquivos Essenciais para Leitura (Antes de Alterar Código)

1. **Bootstrap & Configurações:**
   - `Src/Fin.API/Program.cs` — Pipeline HTTP, JWKS, RabbitMQ (`AddRabbitConfiguration`), AutoMapper e CORS.
   - `Src/Fin.API/Configuration/DiConfig.cs`, `DbContextConfig.cs`, `CorsConfig.cs`, `IdentityConfig.cs` (ou `ApiConfig.cs` na `main`).
2. **Controllers:**
   - `Src/Fin.API/Controllers/MainController.cs` — Envelope `CustomResponse` e resolução de `UsuarioId`.
   - `Src/Fin.API/Controllers/TransacoesController.cs` — Endpoints `/api/transacoes`.
   - `Src/Fin.API/Controllers/CategoriesController.cs` — Endpoints `/api/categories`.
   - `Src/Fin.API/Controllers/UsuarioController.cs` — Endpoint `/api/usuarios`.
3. **Serviços & Mensageria:**
   - `Src/Fin.Application/Services/TransacaoService.cs` — Regras de saldo, período, recorrência mensal de receita (12 meses) e parcelamento de despesa (`1/N .. N/N`).
   - `Src/Fin.Application/Services/CategoriaService.cs` — CRUD de categorias, ocultação de categorias padrão e personalização de ícones/cores.
   - `Src/Fin.Application/Services/RegistroUsuarioIntegrationHandler.cs` — Consumer RabbitMQ para `UsuarioRegistradoIntegrationEvent`.
4. **Persistência:**
   - `Src/Fin.Infra/Data/ApiDbContext.cs` — Mapeamento Fluent API e relacionamentos.
   - `Src/Fin.Infra/Repositories/TransacaoRepository.cs`, `CategoriaRepository.cs`, `UsuarioRepository.cs`.

---

## 5. Mapa Resumido dos Fluxos Existentes

> Detalhamento completo com diagramas e regras de negócio em **[docs/flows.md](docs/flows.md)**.

- **Sincronização de Registro de Usuário (RabbitMQ):** `RegistroUsuarioIntegrationHandler` escuta `UsuarioRegistradoIntegrationEvent` enviado pela `Auth.Api`, valida `Usuario` via `UsuarioValidation` e persiste em `Usuarios`, respondendo `ResponseMessage` com `ValidationResult`.
- **Consulta de Perfil (`GET /api/usuarios`):** Retorna `UsuarioDTO` do `UsuarioId` extraído do JWT.
- **Gestão de Categorias (`/api/categories`):** Listagem (filtrando `UserHiddenCategories`), busca por ID, listagem de ícones (`/icones`) e cores (`/cores`), criação (com reativação automática de categoria padrão oculta), atualização (direta para categorias do usuário; override de ícone/cor em tabelas associativas para categorias padrão) e exclusão (bloqueada se houver transações vinculadas; soft-hide para `IsDefault` e hard-delete para categorias do usuário).
- **Gestão de Transações (`/api/transacoes`):**
  - `GET /saldo`: Soma receitas (`+Valor`) e subtrai despesas (`-Valor`) até o último instante do mês corrente (`inicioProximoMes`).
  - `GET /periodo` e `GET /saldo-mes`: Filtro por `startDate` e `endDate` (obrigatórios, `start <= end`, convertidos para UTC) retornando lista detalhada ou agrupamento mensal (`Mes`, `Receita`, `Despesa`).
  - `POST /novo`: Cria transação em UTC. Se `IsRecurring == true` e `Type == Renda`, gera 11 transações filhas mensais (`ParentTransactionId = origem.Id`). Se `IsRecurring == true` e `Type == Despesa`, divide `Valor / Parcelas` (mínimo 2), renomeia a base para `"Titulo (1/N)"` e cria as parcelas `2..N` nos meses subsequentes.
  - `PUT /{id}` e `DELETE /{id}`: Suportam transição entre transação única e recorrente/parcelada, recálculo/remoção em cascata das transações filhas (`ParentTransactionId`).

---

## 6. Pontos de Atenção e Armadilhas Conhecidas (Tech Debt)

- **Divergência entre `main` e `preview-prod` no contrato de `POST /api/transacoes/novo`:**
  - Na `main` (e esperado pelo frontend `fin` e pela `smartApi`), o endpoint `POST /api/transacoes/novo` recebe `TransactionRequest` com propriedades em inglês (`type`, `description`, `amount`, `categoryId`, `transactionDate`, `isRecurring`, `parcelas`) e converte para domínio via `ToDomain()`.
  - Na `preview-prod`, a action recebe diretamente a entidade `Transacao` (propriedades em português: `titulo`, `valor`, `categoriaId`, `dataMovimentacao`).
- **Inversão de Booleano no `DELETE /api/categories/{id}`:**
  - Em `CategoriesController.Deletar`, o retorno está escrito como `CustomResponse(!await _categoriaService.DeleteCategoriaAsync(UsuarioId, id))`. Quando `DeleteCategoriaAsync` tem sucesso (`true`), o campo `data` do envelope `ResponseSuccessDTO<bool>` retorna `false` (com `success: true` e HTTP `200`). Cuidado ao alterar esse comportamento para não quebrar consumidores que dependam apenas de `success: true`.
- **Verificação de Transações na Exclusão de Categoria (`preview-prod` vs `main`):**
  - Na `preview-prod`, `DeleteCategoriaAsync` carrega todas as transações do usuário em memória (`_transacaoRepository.GetAllAsync(userId)`) para checar `Any(t => t.CategoriaId == categoria.Id)`. Na `main`, isso já foi otimizado para `_transacaoRepository.PossuiTransacoesAssociadasAsync(userId, categoriaId)`.
- **Hard-fail no Startup:**
  - A aplicação lança `InvalidOperationException` ao iniciar caso não encontre:
    - `ConnectionStrings:DefaultConnection`
    - `MEU_APP` (array/seção com origens permitidas no CORS)
    - `JwtSettings:AutenticacaoJwksUrl`
    - Configuração do RabbitMQ exigida por `AddRabbitConfiguration`.

---

## 7. Comandos Úteis

```powershell
# Restaurar e compilar a solution
dotnet restore fin-api.sln
dotnet build fin-api.sln

# Executar a API (Swagger em /swagger no ambiente Development - porta padrão HTTP 5290 / HTTPS 7176)
dotnet run --project Src/Fin.API/Fin.API.csproj

# Executar testes unitários (quando na branch main / feat/implementar-testes)
dotnet test fin-api.sln

# Aplicar migrations do EF Core no PostgreSQL
dotnet ef database update --project Src/Fin.Infra --startup-project Src/Fin.API

# Build da imagem Docker (porta 8080)
docker build -f DockerFile -t fin-api .
```

---

## 8. Fluxo Git, Branches, Commits e PRs

Este repositório segue a padronização de branches intermediárias, Conventional Commits e Pull Requests estruturados do ecossistema. Consulte o guia completo em **[docs/git-workflow.md](docs/git-workflow.md)** e o template de PR em **[.github/pull_request_template.md](.github/pull_request_template.md)**.
