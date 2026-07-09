# Documentação dos Casos de Teste do TransacaoService

Esta documentação descreve os cenários de testes unitários implementados para o [TransactionService](file:///C:/dev/backend/dotnet/fin-api/Src/Fin.Application/Services/TransacaoService.cs) no arquivo de testes [TransacaoServiceTest.cs](file:///C:/dev/backend/dotnet/fin-api/Tests/Application.UnitTests/TransacaoServiceTest.cs). Todos os testes foram projetados para validar fluxos de sucesso, regras de permissão, criação e atualização de recorrências/parcelas, filtros por período, consolidação mensal e tratamento de exceções do banco de dados.

---

## 1. Método GetSaldoTotalAsync

Testes unitários que validam a consulta do saldo total do usuário até o início do próximo mês.

### Caso de Teste 1.1: `GetSaldoTotalAsync_ValidUserId_ReturnsSaldo`
* **Descrição:** Garante que o saldo total calculado pelo repositório seja retornado corretamente para um usuário válido.
* **Arrange:** Mock de `GetSaldoTotal(userId, dataLimite)` do `ITransacaoRepository` configurado para retornar um valor decimal esperado.
* **Act:** Chamada ao método `GetSaldoTotalAsync(userId)`.
* **Assert:** O retorno deve ser igual ao saldo esperado e o repositório deve ser chamado exatamente uma vez.

### Caso de Teste 1.2: `GetSaldoTotalAsync_DatabaseException_ReturnsDefaultAndNotifies`
* **Descrição:** Garante que uma falha de banco durante a consulta do saldo seja capturada e notificada.
* **Arrange:** Mock de `GetSaldoTotal` configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `GetSaldoTotalAsync(userId)`.
* **Assert:** O retorno deve ser `0m` e o `INotificador.Handle<decimal>("Erro no banco: ...")` deve ser acionado.

---

## 2. Método ListTransactionsAsync

Testes unitários que validam a listagem geral de transações de um usuário.

### Caso de Teste 2.1: `ListTransactionsAsync_ValidUserId_ReturnsMappedTransactions`
* **Descrição:** Garante que as transações retornadas pelo repositório sejam mapeadas para `TransacaoDTO`.
* **Arrange:** Mock de `GetAllAsync(userId)` retornando uma lista de transações.
* **Act:** Chamada ao método `ListTransactionsAsync(userId)`.
* **Assert:** O resultado não deve ser nulo, deve conter a quantidade esperada de itens e o repositório deve ser chamado uma vez.

### Caso de Teste 2.2: `ListTransactionsAsync_DatabaseException_ReturnsNullAndNotifies`
* **Descrição:** Garante que erros de banco na listagem sejam tratados pelo fluxo padrão de notificação.
* **Arrange:** Mock de `GetAllAsync(userId)` configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `ListTransactionsAsync(userId)`.
* **Assert:** O retorno deve ser nulo e o `INotificador.Handle<IEnumerable<Transacao>>("Erro no banco: ...")` deve ser chamado.

---

## 3. Método GetTransactionAsync

Testes unitários que validam a recuperação de uma transação por identificador e usuário.

### Caso de Teste 3.1: `GetTransactionAsync_ValidIdAndUser_ReturnsMappedTransaction`
* **Descrição:** Garante que uma transação existente e pertencente ao usuário seja retornada corretamente.
* **Arrange:** Mock de `GetByIdAsync(id, userId)` retornando uma transação válida.
* **Act:** Chamada ao método `GetTransactionAsync(id, userId)`.
* **Assert:** O DTO retornado não deve ser nulo e deve manter os valores de `Id` e `UserId`.

### Caso de Teste 3.2: `GetTransactionAsync_NotFound_ReturnsNullAndNotifiesError`
* **Descrição:** Valida o retorno nulo quando a transação não existe.
* **Arrange:** Mock de `GetByIdAsync(id, userId)` retornando `null`.
* **Act:** Chamada ao método `GetTransactionAsync(id, userId)`.
* **Assert:** O retorno deve ser nulo e a notificação `"Transação não existe!"` deve ser emitida.

### Caso de Teste 3.3: `GetTransactionAsync_ForbiddenUser_ReturnsNullAndNotifiesError`
* **Descrição:** Bloqueia o acesso quando a transação encontrada pertence a outro usuário.
* **Arrange:** Mock de `GetByIdAsync(id, userId)` retornando transação com `UserId` diferente do usuário solicitante.
* **Act:** Chamada ao método `GetTransactionAsync(id, userId)`.
* **Assert:** O retorno deve ser nulo e a notificação `"Você não tem permissão para ver essa Transação!"` deve ser disparada.

### Caso de Teste 3.4: `GetTransactionAsync_DatabaseException_ReturnsNullAndNotifiesError`
* **Descrição:** Garante que exceções de banco na busca por ID sejam capturadas e resultem em notificação de erro.
* **Arrange:** Mock de `GetByIdAsync(id, userId)` configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `GetTransactionAsync(id, userId)`.
* **Assert:** O retorno deve ser nulo, com notificação do erro de banco e da ausência da transação.

---

## 4. Método CreateTransactionAsync

Testes unitários que validam a criação de transações simples, recorrências de renda e parcelamentos de despesa.

### Caso de Teste 4.1: `CreateTransactionAsync_SimpleTransaction_SavesAndReturnsTransaction`
* **Descrição:** Criação bem-sucedida de uma transação simples sem recorrência.
* **Arrange:** Mock de `AddAsync(transacao)` retornando `true`.
* **Act:** Chamada ao método `CreateTransactionAsync(request, userId)`.
* **Assert:** O DTO retornado deve conter título e valor esperados, e `AddAsync` deve ser chamado para uma transação do usuário sem recorrência.

### Caso de Teste 4.2: `CreateTransactionAsync_SaveFails_ReturnsNullAndNotifiesError`
* **Descrição:** Trata falha ao persistir a transação base.
* **Arrange:** Mock de `AddAsync(transacao)` retornando `false`.
* **Act:** Chamada ao método `CreateTransactionAsync(request, userId)`.
* **Assert:** O retorno deve ser nulo e a notificação `"Falha ao salvar transação!"` deve ser emitida.

### Caso de Teste 4.3: `CreateTransactionAsync_RecurringRenda_GeneratesRecurrencesAndReturnsTransaction`
* **Descrição:** Garante que uma renda recorrente mensal gere as transações futuras esperadas.
* **Arrange:** Mock de `AddAsync` e `AddRangeAsync` retornando `true`.
* **Act:** Chamada ao método `CreateTransactionAsync` com `Type = Renda` e `IsRecurring = true`.
* **Assert:** O retorno deve conter `RecorrenciaType.Mensalmente`, data final de recorrência preenchida e geração de 11 recorrências futuras.

### Caso de Teste 4.4: `CreateTransactionAsync_RecurringRendaAddRangeFails_ReturnsNullAndNotifiesError`
* **Descrição:** Valida o erro quando a transação base é salva, mas a persistência das recorrências falha.
* **Arrange:** Mock de `AddAsync` retornando `true` e `AddRangeAsync` retornando `false`.
* **Act:** Criação de renda recorrente.
* **Assert:** O retorno deve ser nulo e a notificação `"Houve um erro ao salvar as recorrências!"` deve ser disparada.

### Caso de Teste 4.5: Criação de despesa recorrente parcelada
* **Cenários testados:**
  * `CreateTransactionAsync_RecurringDespesaInvalidParcelas_ReturnsNullAndNotifiesError`: Retorna nulo e notifica `"Parcelas deve ser no mínimo 2"` quando a quantidade de parcelas é inválida.
  * `CreateTransactionAsync_RecurringDespesa_GeneratesParcelsSuccessfully`: Divide o valor total, atualiza a parcela base como `"Notebook (1/3)"` e cria as parcelas futuras.
  * `CreateTransactionAsync_RecurringDespesaUpdateBaseFails_ReturnsNullAndNotifiesError`: Retorna nulo se a atualização da parcela base falhar.
  * `CreateTransactionAsync_RecurringDespesaAddRangeFails_ReturnsNullAndNotifiesError`: Retorna nulo se a gravação das parcelas futuras falhar.

### Caso de Teste 4.6: `CreateTransactionAsync_DatabaseException_ReturnsNullAndNotifies`
* **Descrição:** Garante que exceções de banco durante a criação sejam notificadas.
* **Arrange:** Mock de persistência configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `CreateTransactionAsync(request, userId)`.
* **Assert:** O retorno deve ser nulo e o erro de banco deve ser enviado ao `INotificador`.

---

## 5. Método UpdateTransactionAsync

Testes unitários que validam atualização de transações simples, mudança de estado recorrente e reprocessamento de parcelas/recorrências.

### Caso de Teste 5.1: Validações iniciais de atualização
* **Cenários testados:**
  * `UpdateTransactionAsync_NotFound_ReturnsNullAndNotifiesError`: Retorna nulo e notifica `"Transação não encontrada!"` quando o registro não existe.
  * `UpdateTransactionAsync_ForbiddenUser_ReturnsNullAndNotifiesError`: Retorna nulo e notifica falta de permissão quando a transação pertence a outro usuário.

### Caso de Teste 5.2: Atualização de transação simples
* **Cenários testados:**
  * `UpdateTransactionAsync_SimpleTransaction_UpdatesAndReturnsMapped`: Atualiza título, valor, categoria e data de uma transação não recorrente.
  * `UpdateTransactionAsync_SimpleTransactionUpdateFails_ReturnsNullAndNotifiesError`: Retorna nulo e notifica `"Falha ao atualizar transação!"` quando a persistência falha.

### Caso de Teste 5.3: Transição de recorrente para única
* **Cenários testados:**
  * `UpdateTransactionAsync_TransitionRecurringToSingle_RemovesChildrenUpdatesParent`: Remove transações filhas, limpa dados de recorrência e atualiza a transação principal.
  * `UpdateTransactionAsync_TransitionRecurringToSingleRemoverFails_ReturnsNullAndNotifiesError`: Retorna nulo quando a remoção das recorrências/parcelas falha.
  * `UpdateTransactionAsync_TransitionRecurringToSingleUpdateFails_ReturnsNullAndNotifiesError`: Retorna nulo e notifica erro quando a atualização final da transação principal falha.

### Caso de Teste 5.4: Transição de única para recorrente
* **Cenários testados:**
  * `UpdateTransactionAsync_TransitionSingleToRecurringRenda_UpdatesBaseGeneratesRecurrences`: Atualiza a base e gera recorrências mensais para transação de renda.
  * `UpdateTransactionAsync_TransitionSingleToRecurringDespesa_UpdatesBaseGeneratesParcels`: Atualiza a base e gera parcelas futuras para despesa.
  * `UpdateTransactionAsync_TransitionSingleToRecurringGerarFails_ReturnsNull`: Retorna nulo se a geração das recorrências ou parcelas falhar.
  * `UpdateTransactionAsync_TransitionSingleToRecurringUpdateFails_ReturnsNullAndNotifiesError`: Retorna nulo se a primeira atualização da transação base falhar.

### Caso de Teste 5.5: Atualização de transação que já é recorrente
* **Cenários testados:**
  * `UpdateTransactionAsync_UpdateRecurringRenda_UpdatesBaseRemovesOldGeneratesNew`: Atualiza a base, remove recorrências antigas e gera novas recorrências.
  * `UpdateTransactionAsync_UpdateRecurringUpdateBaseFails_ReturnsNullAndNotifiesError`: Retorna nulo e notifica `"Falha ao atualizar base!"`.
  * `UpdateTransactionAsync_UpdateRecurringRemoveFails_ReturnsNullAndNotifiesError`: Retorna nulo e notifica `"Falha ao remover recorrências!"`.

### Caso de Teste 5.6: `UpdateTransactionAsync_DatabaseException_ReturnsNullAndNotifies`
* **Descrição:** Garante que exceções de banco durante a busca inicial sejam capturadas e notificadas.
* **Arrange:** Mock de `GetByIdAsync(id, userId)` configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `UpdateTransactionAsync(id, dto, userId)`.
* **Assert:** O retorno deve ser nulo, com notificação do erro de banco e da transação não encontrada.

---

## 6. Método DeleteTransactionAsync

Testes unitários que validam exclusão de transações simples e recorrentes.

### Caso de Teste 6.1: Validações iniciais de exclusão
* **Cenários testados:**
  * `DeleteTransactionAsync_NotFound_ReturnsFalseAndNotifiesError`: Retorna `false` e notifica `"Transação não encontrada!"`.
  * `DeleteTransactionAsync_ForbiddenUser_ReturnsFalseAndNotifiesError`: Retorna `false` e notifica falta de permissão quando a transação pertence a outro usuário.

### Caso de Teste 6.2: Exclusão de transação simples
* **Cenários testados:**
  * `DeleteTransactionAsync_SimpleTransaction_DeletesParent`: Exclui diretamente a transação não recorrente.
  * `DeleteTransactionAsync_SimpleTransactionDeleteFails_ReturnsFalseAndNotifies`: Retorna `false` e notifica `"Houve um problema ao excluir a transação!"` se a exclusão falhar.

### Caso de Teste 6.3: Exclusão de transação recorrente
* **Cenários testados:**
  * `DeleteTransactionAsync_RecurringTransaction_RemovesChildrenAndParent`: Remove as transações filhas antes de excluir a transação principal.
  * `DeleteTransactionAsync_RecurringTransactionRemoveChildrenFails_ReturnsFalseAndNotifies`: Retorna `false`, notifica falha ao excluir recorrências e não executa a exclusão da principal.

### Caso de Teste 6.4: `DeleteTransactionAsync_DatabaseException_ReturnsFalseAndNotifies`
* **Descrição:** Garante que falhas de banco na busca da transação sejam capturadas.
* **Arrange:** Mock de `GetByIdAsync(id, userId)` configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `DeleteTransactionAsync(id, userId)`.
* **Assert:** O retorno deve ser `false`, com notificação do erro de banco e da transação não encontrada.

---

## 7. Método ListTransactionsByPeriodAsync

Testes unitários que validam filtros de transações por período.

### Caso de Teste 7.1: `ListTransactionsByPeriodAsync_NullDates_ReturnsNullAndNotifiesError`
* **Descrição:** Valida a obrigatoriedade das datas inicial e final.
* **Assert:** Retorno nulo e notificação `"As datas iniciais e finais são obrigatórias!"`.

### Caso de Teste 7.2: `ListTransactionsByPeriodAsync_StartAfterEnd_ReturnsNullAndNotifiesError`
* **Descrição:** Impede consulta quando a data inicial é maior que a data final.
* **Assert:** Retorno nulo e notificação `"A data de fim deve ser maior que a data de início!"`.

### Caso de Teste 7.3: `ListTransactionsByPeriodAsync_ValidDates_ReturnsMappedTransactions`
* **Descrição:** Garante que o período válido seja convertido para intervalo diário completo e enviado ao repositório.
* **Arrange:** Mock de `GetByPeriodAsync(userId, inicioUtc, fimUtc)` retornando lista de transações.
* **Act:** Chamada ao método `ListTransactionsByPeriodAsync(userId, start, end)`.
* **Assert:** Resultado populado, com mapeamento para DTO e chamada única ao repositório.

### Caso de Teste 7.4: `ListTransactionsByPeriodAsync_MappingFails_ReturnsNullAndNotifiesError`
* **Descrição:** Trata falha no mapeamento das transações para DTO.
* **Arrange:** Mock do mapper retornando `null`.
* **Assert:** Retorno nulo e notificação `"Houve um problema ao buscar as transações!"`.

### Caso de Teste 7.5: `ListTransactionsByPeriodAsync_DatabaseException_ReturnsNullAndNotifies`
* **Descrição:** Garante que erros de banco na consulta por período sejam capturados.
* **Assert:** Retorno nulo, notificação do erro de banco e notificação de problema ao buscar transações.

---

## 8. Método GetValuesByMonth

Testes unitários que validam a consolidação mensal de receitas e despesas.

### Caso de Teste 8.1: `GetValuesByMonth_NullDates_ReturnsNullAndNotifiesError`
* **Descrição:** Valida que as datas inicial e final são obrigatórias para a consulta mensal.
* **Assert:** Retorno nulo e notificação `"As datas iniciais e finais são obrigatórias!"`.

### Caso de Teste 8.2: `GetValuesByMonth_StartAfterEnd_ReturnsNullAndNotifiesError`
* **Descrição:** Impede a consulta mensal quando a data inicial é maior que a data final.
* **Assert:** Retorno nulo e notificação `"A data de fim deve ser maior que a data de início!"`.

### Caso de Teste 8.3: `GetValuesByMonth_ValidDates_ReturnsSaldoMensal`
* **Descrição:** Garante que o repositório retorne a lista de `SaldoMensalDTO` para o período informado.
* **Arrange:** Mock de `GetValuesByMonth(userId, inicioUtc, fimUtc)` retornando lista com valores de receita e despesa.
* **Act:** Chamada ao método `GetValuesByMonth(userId, start, end)`.
* **Assert:** O resultado deve ser igual à lista esperada e o repositório deve ser chamado uma vez.

### Caso de Teste 8.4: `GetValuesByMonth_DatabaseException_ReturnsNullAndNotifies`
* **Descrição:** Garante tratamento de exceção de banco durante a consolidação mensal.
* **Arrange:** Mock de `GetValuesByMonth` configurado para lançar `DatabaseException`.
* **Act:** Chamada ao método `GetValuesByMonth(userId, start, end)`.
* **Assert:** O retorno deve ser nulo e o `INotificador.Handle<IEnumerable<SaldoMensalDTO>>("Erro no banco: ...")` deve ser acionado.
