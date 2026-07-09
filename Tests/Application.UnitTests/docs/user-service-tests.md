# Documentação dos Casos de Teste do UsuarioService

Esta documentação descreve os cenários de testes unitários implementados para o [UsuarioService](file:///C:/dev/backend/dotnet/fin-api/Src/Fin.Application/Services/UsuarioService.cs) no arquivo de testes [UserServiceTest.cs](file:///C:/dev/backend/dotnet/fin-api/Tests/Application.UnitTests/UserServiceTest.cs). Todos os testes foram projetados para garantir 100% de cobertura de código, abrangendo fluxos de sucesso, erros de validação e tratamento de exceções do banco de dados.

---

## 1. Método CriarUsuarioAsync

Testes unitários que validam o comportamento da criação de usuários no [UsuarioService](file:///C:/dev/backend/dotnet/fin-api/Src/Fin.Application/Services/UsuarioService.cs).

### Caso de Teste 1.1: `CreateUserAsync_SuccessfulCreation_ShouldReturnTrue`
* **Descrição:** Garante que o usuário seja criado com sucesso quando o repositório realiza a persistência sem erros.
* **Arrange (Preparação):**
  * Mock do método `CreateUsuarioAsync(usuario)` do `IUsuarioRepository` configurado para retornar `true`.
* **Act (Ação):**
  * Chamada ao método `CriarUsuarioAsync(usuario)`.
* **Assert (Verificação):**
  * O retorno do método deve ser `true`.
  * Verificação de que `CreateUsuarioAsync` no repositório foi chamado exatamente uma vez.
  * O `INotificador.Handle` não deve ser chamado para nenhum erro.

### Caso de Teste 1.2: `CreateUserAsync_RepositoryReturnsFalse_ShouldReturnFalseAndNotifyError`
* **Descrição:** Garante que o serviço retorne `false` e emita uma notificação de erro quando a persistência no banco falha (retornando `false`).
* **Arrange (Preparação):**
  * Mock do método `CreateUsuarioAsync(usuario)` do `IUsuarioRepository` configurado para retornar `false`.
* **Act (Ação):**
  * Chamada ao método `CriarUsuarioAsync(usuario)`.
* **Assert (Verificação):**
  * O retorno do método deve ser `false`.
  * Verificação de que `CreateUsuarioAsync` no repositório foi chamado exatamente uma vez.
  * Verificação de que a notificação `INotificador.Handle<bool>("Erro ao salvar usuario!")` foi disparada exatamente uma vez.

### Caso de Teste 1.3: `CreateUserAsync_DatabaseError_ShouldReturnFalseAndNotifyError`
* **Descrição:** Garante que o serviço lide corretamente com exceções lançadas pelo repositório (`DatabaseException`) durante o processo de criação.
* **Arrange (Preparação):**
  * Mock de `CreateUsuarioAsync(usuario)` do `IUsuarioRepository` configurado para lançar uma exceção do tipo `DatabaseException` com a mensagem `"Database connection timed out"`.
* **Act (Ação):**
  * Chamada ao método `CriarUsuarioAsync(usuario)`.
* **Assert (Verificação):**
  * O retorno do método deve ser `false`.
  * Verificação de que duas notificações de erro foram disparadas no `INotificador`:
    1. `INotificador.Handle<bool>("Erro no banco: Database connection timed out")` (capturado pelo bloco try-catch do `ExecuteAsync` na classe base [BaseService](file:///C:/dev/backend/dotnet/fin-api/Src/Fin.Application/Services/BaseService.cs)).
    2. `INotificador.Handle<bool>("Erro ao salvar usuario!")` (disparado pela checagem de falha interna do método).

---

## 2. Método BuscarUsuarioPorIdAsync

Testes unitários que validam a recuperação de usuários por identificador.

### Caso de Teste 2.1: `GetUserByIdAsync_UserExists_ShouldReturnMappedUserDTO`
* **Descrição:** Garante que o usuário seja localizado com sucesso e mapeado para `UsuarioDTO` caso exista no banco.
* **Arrange (Preparação):**
  * Mock do método `GetUsuarioByIdAsync(id)` do `IUsuarioRepository` configurado para retornar um objeto válido do tipo `Usuario`.
  * Mock do `IMapper.Map<UsuarioDTO>(user)` configurado para retornar um objeto `UsuarioDTO` populado.
* **Act (Ação):**
  * Chamada ao método `BuscarUsuarioPorIdAsync(id)`.
* **Assert (Verificação):**
  * O objeto `UsuarioDTO` retornado não deve ser nulo.
  * Os campos mapeados (Id, Nome, Email) devem coincidir com os valores fornecidos.
  * O `INotificador.Handle` não deve ser acionado.
  * Ambos os métodos do repositório e do mapper devem ser chamados exatamente uma vez.

### Caso de Teste 2.2: `GetUserByIdAsync_UserDoesNotExist_ShouldReturnNullAndNotifyError`
* **Descrição:** Garante que o serviço retorne `null` e envie uma notificação de erro no barramento se o ID do usuário não for encontrado no repositório.
* **Arrange (Preparação):**
  * Mock de `GetUsuarioByIdAsync(id)` do `IUsuarioRepository` configurado para retornar `null`.
* **Act (Ação):**
  * Chamada ao método `BuscarUsuarioPorIdAsync(id)`.
* **Assert (Verificação):**
  * O retorno do método deve ser `null`.
  * Verificação de que a notificação `INotificador.Handle<UsuarioDTO>("Usuario não encontrado!")` foi acionada exatamente uma vez.
  * O `IMapper` nunca deve ser chamado para mapear o usuário.

### Caso de Teste 2.3: `GetUserByIdAsync_DatabaseError_ShouldReturnNullAndNotifyError`
* **Descrição:** Garante que falhas e exceções de acesso a dados no momento de buscar um usuário por ID sejam capturadas, notificadas e retornem nulo.
* **Arrange (Preparação):**
  * Mock de `GetUsuarioByIdAsync(id)` configurado para lançar uma `DatabaseException` com a mensagem `"Query failed"`.
* **Act (Ação):**
  * Chamada ao método `BuscarUsuarioPorIdAsync(id)`.
* **Assert (Verificação):**
  * O retorno do método deve ser `null`.
  * Verificação de que duas notificações de erro foram enviadas:
    1. `INotificador.Handle<Usuario>("Erro no banco: Query failed")` (capturado pelo método `ExecuteAsync` com tipo genérico `<Usuario>`).
    2. `INotificador.Handle<UsuarioDTO>("Usuario não encontrado!")` (invocado pela falha de referência nula resultante).
  * O `IMapper` nunca deve ser acionado.
