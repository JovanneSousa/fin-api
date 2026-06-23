# Documentação dos Casos de Teste do CategoriaService

Esta documentação descreve os cenários de testes unitários implementados para o [CategoriaService](file:///C:/dev/backend/dotnet/fin-api/Src/Fin.Application/Services/CategoriaService.cs) no arquivo de testes [CategoriesServiceTest.cs](file:///C:/dev/backend/dotnet/fin-api/Tests/Application.UnitTests/CategoriesServiceTest.cs). Todos os testes foram projetados para garantir 100% de cobertura de código, abrangendo fluxos de sucesso, erros de validação, limites de permissão e tratamento de exceções.

---

## 1. Método ListCategoriasAsync

Testes unitários que validam a recuperação da lista de categorias associadas a um usuário.

### Caso de Teste 1.1: `ListCategoriasAsync_CategoriesExist_ShouldReturnListOfCategories`
* **Descrição:** Garante que a lista de categorias seja retornada corretamente quando existem registros cadastrados para o usuário.
* **Arrange:** Mock de `GetAllAsync(userId)` do `ICategoriaRepository` configurado para retornar uma lista com objetos `CategoriaDTO`.
* **Act:** Chamada ao método `ListCategoriasAsync(userId)`.
* **Assert:** Retorno da lista populada, conferência da quantidade de itens e verificação de chamada única ao repositório.

### Caso de Teste 1.2: `ListCategoriasAsync_NoCategoriesExist_ShouldReturnEmptyList`
* **Descrição:** Garante que uma lista vazia seja retornada quando não houver categorias cadastradas (repositório retorna `null`).
* **Arrange:** Mock de `GetAllAsync(userId)` configurado para retornar `null`.
* **Act:** Chamada ao método `ListCategoriasAsync(userId)`.
* **Assert:** O resultado não deve ser nulo, mas sim uma lista vazia (`IEnumerable<CategoriaDTO>`).

### Caso de Teste 1.3: `ListCategoriasAsync_DatabaseError_ShouldReturnEmptyListAndNotifyError`
* **Descrição:** Garante que se uma exceção do banco (`DatabaseException`) for lançada, o fluxo capture o erro no `INotificador` e retorne uma lista vazia.
* **Arrange:** Mock de `GetAllAsync(userId)` configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `ListCategoriasAsync(userId)`.
* **Assert:** Retorno de lista vazia e validação de que `INotificador.Handle` registrou a mensagem correspondente ao erro do banco.

---

## 2. Método CreateCategoriaAsync

Testes unitários que validam o fluxo de criação de categorias de transações.

### Caso de Teste 2.1: `CreateCategoriaAsync_CategoryDoesNotExist_ShouldCreateSuccessfully`
* **Descrição:** Criação bem-sucedida de categoria quando o nome fornecido não existe previamente para o usuário.
* **Arrange:** 
  * Mock de `GetCategoryByNameAndUserIdAsync` retornando `null`.
  * Mock de `AddAsync(categoria)` retornando a entidade persistida `Categoria`.
* **Act:** Chamada ao método `CreateCategoriaAsync(userId, inputDto)`.
* **Assert:** Retorno do DTO correspondente à categoria criada e validação de chamada ao `AddAsync`.

### Caso de Teste 2.2: `CreateCategoriaAsync_ErrorAddingCategory_ShouldReturnNullAndNotifyError`
* **Descrição:** Trata a falha na inserção física da categoria onde a chamada do repositório retorna `null`.
* **Arrange:** Mock de `GetCategoryByNameAndUserIdAsync` retornando `null` e `AddAsync` retornando `null`.
* **Act:** Chamada ao método `CreateCategoriaAsync(userId, inputDto)`.
* **Assert:** Retorno `null` e emissão de notificação no `INotificador` com `"Ocorreu um erro ao criar categoria"`.

### Caso de Teste 2.3: `CreateCategoriaAsync_CategoryAlreadyExistsAndIsNotHidden_ShouldReturnNullAndNotifyError`
* **Descrição:** Bloqueio de inserção caso a categoria já exista e não esteja em estado oculto.
* **Arrange:**
  * Mock de `GetCategoryByNameAndUserIdAsync` retornando uma categoria existente.
  * Mock de `IsCategoryHiddenAsync` retornando `false`.
* **Act:** Chamada ao método `CreateCategoriaAsync(userId, inputDto)`.
* **Assert:** Retorno `null` e envio da notificação `"Categoria já existe para este usuário."`.

### Caso de Teste 2.4: `CreateCategoriaAsync_CategoryAlreadyExistsAndIsHidden_ShouldShowHiddenCategoryAndReturnSuccess`
* **Descrição:** Se uma categoria já existir no banco mas estiver oculta, o sistema reativa a mesma chamando `ShowHiddenCategory`.
* **Arrange:**
  * Mock de `GetCategoryByNameAndUserIdAsync` retornando a categoria existente.
  * Mock de `IsCategoryHiddenAsync` retornando `true`.
* **Act:** Chamada ao método `CreateCategoriaAsync(userId, inputDto)`.
* **Assert:** Retorno do DTO da categoria reativada e confirmação da execução do método `ShowHiddenCategory` no repositório.

---

## 3. Método DeleteCategoriaAsync

Testes unitários que validam as regras de negócio para deleção física ou lógica de categorias.

### Caso de Teste 3.1: `DeleteCategoriaAsync_CategoryDoesNotExist_ShouldReturnFalseAndNotifyError`
* **Descrição:** Valida a tentativa de deleção de uma categoria inexistente.
* **Arrange:** Mock de `GetByIdAsync` retornando `null`.
* **Act:** Chamada ao método `DeleteCategoriaAsync(userId, categoryId)`.
* **Assert:** Retorno `false` e notificação `"Categoria não encontrada!"`.

### Caso de Teste 3.2: `DeleteCategoriaAsync_NoPermission_ShouldReturnFalseAndNotifyError`
* **Descrição:** Impede que um usuário delete uma categoria que pertença a outro usuário do sistema.
* **Arrange:** Mock de `GetByIdAsync` retornando categoria com `UserId` diferente do `userId` ativo.
* **Act:** Chamada ao método `DeleteCategoriaAsync(userId, categoryId)`.
* **Assert:** Retorno `false` e notificação `"Você não tem permissão para deletar esta categoria."`.

### Caso de Teste 3.3: `DeleteCategoriaAsync_HasTransactions_ShouldReturnFalseAndNotifyError`
* **Descrição:** Impede a exclusão de qualquer categoria que já esteja associada a transações financeiras.
* **Arrange:** Mock de `TransactionsExistsByCategoryAsync` retornando `true`.
* **Act:** Chamada ao método `DeleteCategoriaAsync(userId, categoryId)`.
* **Assert:** Retorno `false` e notificação `"Não é possível deletar uma categoria associada a transações."`. O método de deleção nunca deve ser executado no banco.

### Caso de Teste 3.4: `DeleteCategoriaAsync_DefaultCategoryWithoutTransactions_ShouldHideCategoryAndReturnSuccess`
* **Descrição:** Deleção de categoria padrão do sistema (`IsDefault == true`) sem transações vinculadas. Em vez de excluir fisicamente do banco, o sistema apenas oculta logicamente.
* **Arrange:** 
  * Mock de categoria indicando `IsDefault = true`.
  * Mock de `TransactionsExistsByCategoryAsync` retornando `false`.
  * Mock de `HiddenCategory` retornando `true`.
* **Act:** Chamada ao método `DeleteCategoriaAsync(userId, categoryId)`.
* **Assert:** Retorno `true` e verificação de chamada apenas ao método `HiddenCategory` no repositório (o método `DeleteAsync` nunca deve ser executado).

### Caso de Teste 3.5: `DeleteCategoriaAsync_CustomCategoryWithoutTransactions_ShouldDeleteCategoryAndReturnSuccess`
* **Descrição:** Deleção física de categoria personalizada criada pelo próprio usuário (`IsDefault == false`).
* **Arrange:**
  * Mock de categoria com `IsDefault = false`.
  * Mock de `TransactionsExistsByCategoryAsync` retornando `false`.
  * Mock de `DeleteAsync` retornando `true`.
* **Act:** Chamada ao método `DeleteCategoriaAsync(userId, categoryId)`.
* **Assert:** Retorno `true` e execução confirmada de `DeleteAsync` no repositório.

---

## 4. Métodos Auxiliares de Listagem (Ícones e Cores)

### Caso de Teste 4.1: `ListarIconesAsync_IconsExist_ShouldReturnListOfIcons` e `ListarIconesAsync_NoIconsExist_ShouldReturnEmptyList`
* **Descrição:** Valida a obtenção da lista completa de ícones cadastrados no sistema, verificando os fluxos com registros válidos e com retorno vazio (`null`).

### Caso de Teste 4.2: `ListarCoresAsync_ColorsExist_ShouldReturnListOfColors` e `ListarCoresAsync_NoColorsExist_ShouldReturnEmptyList`
* **Descrição:** Valida a listagem de cores cadastradas no sistema, testando tanto o retorno de dados íntegros quanto o fallback para lista vazia se o repositório retornar `null`.

---

## 5. Método ObterCategoriaId

### Caso de Teste 5.1: `ObterCategoriaId_CategoryDoesNotExist_ShouldReturnNullAndNotifyError`
* **Descrição:** Retorna erro `"Categoria não encontrada!"` se o ID pesquisado não existir.

### Caso de Teste 5.2: `ObterCategoriaId_DefaultCategory_ShouldReturnCategorySuccessfully`
* **Descrição:** Permite visualizar a categoria se ela for padrão (`IsDefault == true`), mesmo que o `UserId` não coincida com o usuário logado (compartilhamento de defaults).

### Caso de Teste 5.3: `ObterCategoriaId_CustomCategoryOwnedByUser_ShouldReturnCategorySuccessfully`
* **Descrição:** Permite acesso à categoria personalizada pertencente ao próprio usuário solicitante.

### Caso de Teste 5.4: `ObterCategoriaId_CustomCategoryOwnedByAnotherUser_ShouldReturnNullAndNotifyError`
* **Descrição:** Bloqueia o acesso e dispara notificação `"Você não tem acesso a essa categoria!"` caso a categoria personalizada pertença a terceiros.

---

## 6. Método AtualizarCategoria

Testa a atualização de propriedades de categorias e o mecanismo complexo de sobrescrever valores padrão com perfis personalizados.

### Caso de Teste 6.1: `AtualizarCategoria_CategoryDoesNotExist_ShouldReturnNullAndNotifyError`
* **Descrição:** Retorna nulo se a categoria a ser atualizada não for localizada.

### Caso de Teste 6.2: `AtualizarCategoria_CustomCategorySuccess_ShouldUpdateAndReturnCategory`
* **Descrição:** Atualização direta bem-sucedida de nome e tipo de categoria personalizada (`IsDefault == false`).
* **Arrange:** Mock mapeando o AutoMapper para fundir as propriedades e mock de `UpdateAsync` retornando `true`.
* **Act:** Chamada ao método `AtualizarCategoria`.
* **Assert:** DTO contendo os dados modificados é retornado e método `UpdateAsync` é executado.

### Caso de Teste 6.3: `AtualizarCategoria_CustomCategoryUpdateFailure_ShouldReturnNullAndNotifyError`
* **Descrição:** Falha física no banco ao rodar o comando de atualização em categoria personalizada.
* **Assert:** Emissão da notificação `"Houve um problema ao atualizar a categoria!"`.

### Caso de Teste 6.4: `AtualizarCategoria_DefaultCategoryAttemptToUpdateNameOrType_ShouldReturnNullAndNotifyError`
* **Descrição:** Protege a integridade das categorias padrões do sistema, impedindo alterações em Nome ou Tipo.
* **Assert:** Emissão da notificação `"Não é possivel atualizar o nome ou tipo de uma categoria padrão!"`.

### Caso de Teste 6.5: Personalização de Ícones em Categorias Padrão
* **Cenários testados:**
  * `AtualizarCategoria_DefaultCategoryUpdateIconWithoutExistingCustomIconSuccess_ShouldSaveIconAndReturnCategory`: Usuário define um ícone customizado pela primeira vez. Dispara inserção no repositório.
  * `AtualizarCategoria_DefaultCategoryUpdateIconWithExistingCustomIconSuccess_ShouldDeleteOldSaveNewAndReturnCategory`: Usuário substitui um ícone customizado já existente. Dispara remoção do antigo e inserção do novo.
  * Falhas operacionais de Ícone (`DeleteFailure`, `SaveFailure`): Disparam erro `"Houve um problema ao atualizar a categoria!"`.

### Caso de Teste 6.6: Personalização de Cores em Categorias Padrão
* **Cenários testados:**
  * `AtualizarCategoria_DefaultCategoryUpdateColorWithoutExistingCustomColorSuccess_ShouldSaveColorAndReturnCategory`: Usuário define uma cor customizada pela primeira vez.
  * `AtualizarCategoria_DefaultCategoryUpdateColorWithExistingCustomColorSuccess_ShouldDeleteOldSaveNewAndReturnCategory`: Usuário substitui uma cor customizada antiga.
  * Falhas operacionais de Cor (`DeleteFailure`, `SaveFailure`): Tratam falhas de persistência e notificam erro de atualização.
