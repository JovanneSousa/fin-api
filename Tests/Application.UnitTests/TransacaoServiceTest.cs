using AutoMapper;
using Fin.Application.DTOs;
using Fin.Application.http.RequestDTO;
using Fin.Application.Interfaces.Repositories;
using Fin.Application.Notificacoes;
using Fin.Application.Services;
using Fin.Domain.Enums;
using Fin.Domain.Exceptions;
using Fin.Domain.Models;
using Moq;

namespace Application.UnitTests
{
    public class TransacaoServiceTest
    {
        private readonly Mock<ITransacaoRepository> _transacaoRepository;
        private readonly Mock<INotificador> _notificador;
        private readonly Mock<IMapper> _mapper;
        private readonly TransactionService _transacaoService;

        public TransacaoServiceTest()
        {
            _notificador = new Mock<INotificador>();
            _transacaoRepository = new Mock<ITransacaoRepository>();
            _mapper = new Mock<IMapper>();
            _transacaoService = new TransactionService(_transacaoRepository.Object, _notificador.Object, _mapper.Object);

            // Configure IMapper dynamic mocks
            _mapper.Setup(m => m.Map<TransacaoDTO>(It.IsAny<Transacao>()))
                .Returns((Transacao s) => s == null ? null! : new TransacaoDTO
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    Type = s.Type,
                    Titulo = s.Titulo,
                    Valor = s.Valor,
                    CategoriaId = s.CategoriaId,
                    DataMovimentacao = s.DataMovimentacao,
                    IsRecurring = s.IsRecurring,
                    RecorrenciaType = s.RecorrenciaType,
                    RecorrenciaEndDate = s.RecorrenciaEndDate,
                    Parcelas = s.Parcelas,
                    ParcelaAtual = s.ParcelaAtual
                });

            _mapper.Setup(m => m.Map<IEnumerable<TransacaoDTO>>(It.IsAny<IEnumerable<Transacao>>()))
                .Returns((IEnumerable<Transacao> s) => s == null ? null! : s.Select(x => new TransacaoDTO
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Type = x.Type,
                    Titulo = x.Titulo,
                    Valor = x.Valor,
                    CategoriaId = x.CategoriaId,
                    DataMovimentacao = x.DataMovimentacao,
                    IsRecurring = x.IsRecurring,
                    RecorrenciaType = x.RecorrenciaType,
                    RecorrenciaEndDate = x.RecorrenciaEndDate,
                    Parcelas = x.Parcelas,
                    ParcelaAtual = x.ParcelaAtual
                }).ToList());

            _notificador.Setup(n => n.Handle<IEnumerable<TransacaoDTO>>(It.IsAny<string>()))
                .Returns((IEnumerable<TransacaoDTO>)null!);

            _notificador.Setup(n => n.Handle<IEnumerable<Transacao>>(It.IsAny<string>()))
                .Returns((IEnumerable<Transacao>)null!);

            _notificador.Setup(n => n.Handle<IEnumerable<SaldoMensalDTO>>(It.IsAny<string>()))
                .Returns((IEnumerable<SaldoMensalDTO>)null!);
        }

        private Transacao CreateTestTransacao(
            string id = "t-123",
            string userId = "user-123",
            TransacaoType type = TransacaoType.Despesa,
            string titulo = "Transacao Teste",
            decimal valor = 100.00m,
            string categoriaId = "cat-123",
            bool isRecurring = false,
            int? parcelas = null,
            int? parcelaAtual = null,
            string? parentTransactionId = null)
        {
            return new Transacao
            {
                Id = id,
                UserId = userId,
                Type = type,
                Titulo = titulo,
                Valor = valor,
                CategoriaId = categoriaId,
                IsRecurring = isRecurring,
                Parcelas = parcelas,
                ParcelaAtual = parcelaAtual,
                ParentTransactionId = parentTransactionId,
                DataMovimentacao = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private TransacaoDTO CreateTestTransacaoDTO(
            string id = "t-123",
            string userId = "user-123",
            TransacaoType type = TransacaoType.Despesa,
            string titulo = "Transacao Teste",
            decimal valor = 100.00m,
            string categoriaId = "cat-123",
            bool isRecurring = false,
            int? parcelas = null,
            int? parcelaAtual = null)
        {
            return new TransacaoDTO
            {
                Id = id,
                UserId = userId,
                Type = type,
                Titulo = titulo,
                Valor = valor,
                CategoriaId = categoriaId,
                IsRecurring = isRecurring,
                Parcelas = parcelas,
                ParcelaAtual = parcelaAtual,
                DataMovimentacao = DateTime.UtcNow
            };
        }

        #region GetSaldoTotalAsync Tests

        [Fact]
        public async Task GetSaldoTotalAsync_ValidUserId_ReturnsSaldo()
        {
            // Arrange
            var userId = "user-123";
            var expectedSaldo = 1500.50m;
            _transacaoRepository.Setup(r => r.GetSaldoTotal(userId, It.IsAny<DateTime>()))
                .ReturnsAsync(expectedSaldo);

            // Act
            var result = await _transacaoService.GetSaldoTotalAsync(userId);

            // Assert
            Assert.Equal(expectedSaldo, result);
            _transacaoRepository.Verify(r => r.GetSaldoTotal(userId, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task GetSaldoTotalAsync_DatabaseException_ReturnsDefaultAndNotifies()
        {
            // Arrange
            var userId = "user-123";
            var exceptionMessage = "DB connection lost";
            _transacaoRepository.Setup(r => r.GetSaldoTotal(userId, It.IsAny<DateTime>()))
                .ThrowsAsync(new DatabaseException(exceptionMessage));

            _notificador.Setup(n => n.Handle<decimal>(It.IsAny<string>()))
                .Returns(0m);

            // Act
            var result = await _transacaoService.GetSaldoTotalAsync(userId);

            // Assert
            Assert.Equal(0m, result);
            _notificador.Verify(n => n.Handle<decimal>($"Erro no banco: {exceptionMessage}"), Times.Once);
        }

        #endregion

        #region ListTransactionsAsync Tests

        [Fact]
        public async Task ListTransactionsAsync_ValidUserId_ReturnsMappedTransactions()
        {
            // Arrange
            var userId = "user-123";
            var transacoes = new List<Transacao>
            {
                CreateTestTransacao(id: "1", userId: userId),
                CreateTestTransacao(id: "2", userId: userId)
            };
            _transacaoRepository.Setup(r => r.GetAllAsync(userId))
                .ReturnsAsync(transacoes);

            // Act
            var result = await _transacaoService.ListTransactionsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _transacaoRepository.Verify(r => r.GetAllAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ListTransactionsAsync_DatabaseException_ReturnsNullAndNotifies()
        {
            // Arrange
            var userId = "user-123";
            var exceptionMessage = "Database error";
            _transacaoRepository.Setup(r => r.GetAllAsync(userId))
                .ThrowsAsync(new DatabaseException(exceptionMessage));

            // Act
            var result = await _transacaoService.ListTransactionsAsync(userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<Transacao>>($"Erro no banco: {exceptionMessage}"), Times.Once);
        }

        #endregion

        #region GetTransactionAsync Tests

        [Fact]
        public async Task GetTransactionAsync_ValidIdAndUser_ReturnsMappedTransaction()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var transacao = CreateTestTransacao(id: id, userId: userId);
            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(transacao);

            // Act
            var result = await _transacaoService.GetTransactionAsync(id, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task GetTransactionAsync_NotFound_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync((Transacao)null!);

            // Act
            var result = await _transacaoService.GetTransactionAsync(id, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Transação não existe!"), Times.Once);
        }

        [Fact]
        public async Task GetTransactionAsync_ForbiddenUser_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var transacao = CreateTestTransacao(id: id, userId: "other-user");
            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(transacao);

            // Act
            var result = await _transacaoService.GetTransactionAsync(id, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Você não tem permissão para ver essa Transação!"), Times.Once);
        }

        [Fact]
        public async Task GetTransactionAsync_DatabaseException_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var exceptionMessage = "Fatal database error";
            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ThrowsAsync(new DatabaseException(exceptionMessage));

            // Act
            var result = await _transacaoService.GetTransactionAsync(id, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<Transacao>($"Erro no banco: {exceptionMessage}"), Times.Once);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Transação não existe!"), Times.Once);
        }

        #endregion

        #region CreateTransactionAsync Tests

        [Fact]
        public async Task CreateTransactionAsync_SimpleTransaction_SavesAndReturnsTransaction()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Despesa,
                Description = "Compra simples",
                Amount = 50.00m,
                CategoryId = "cat-1",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = false
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Compra simples", result.Titulo);
            Assert.Equal(50.00m, result.Valor);
            _transacaoRepository.Verify(r => r.AddAsync(It.Is<Transacao>(t => t.UserId == userId && !t.IsRecurring)), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_SaveFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Despesa,
                Description = "Compra simples",
                Amount = 50.00m,
                CategoryId = "cat-1",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = false
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Falha ao salvar transação!"), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_RecurringRenda_GeneratesRecurrencesAndReturnsTransaction()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Renda,
                Description = "Salário Recorrente",
                Amount = 3000.00m,
                CategoryId = "cat-salary",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = true
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RecorrenciaType.Mensalmente, result.RecorrenciaType);
            Assert.NotNull(result.RecorrenciaEndDate);

            _transacaoRepository.Verify(r => r.AddAsync(It.Is<Transacao>(t => t.IsRecurring && t.Type == TransacaoType.Renda)), Times.Once);
            _transacaoRepository.Verify(r => r.AddRangeAsync(It.Is<List<Transacao>>(list => list.Count == 11)), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_RecurringRendaAddRangeFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Renda,
                Description = "Salário Recorrente",
                Amount = 3000.00m,
                CategoryId = "cat-salary",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = true
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<bool>("Houve um erro ao salvar as recorrências!"), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_RecurringDespesaInvalidParcelas_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Despesa,
                Description = "Celular parcelado",
                Amount = 1500.00m,
                CategoryId = "cat-tech",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = true,
                Parcelas = 1 // Inválido: recorrente despesa precisa de pelo menos 2 parcelas
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<bool>("Parcelas deve ser no mínimo 2"), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_RecurringDespesa_GeneratesParcelsSuccessfully()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Despesa,
                Description = "Notebook",
                Amount = 3000.00m,
                CategoryId = "cat-tech",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = true,
                Parcelas = 3
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            // Notebook (1/3) should have value 1000.00
            Assert.Equal("Notebook (1/3)", result.Titulo);
            Assert.Equal(1000.00m, result.Valor);

            _transacaoRepository.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Once);
            _transacaoRepository.Verify(r => r.UpdateAsync(It.Is<Transacao>(t => t.Valor == 1000.00m && t.Titulo == "Notebook (1/3)")), Times.Once);
            _transacaoRepository.Verify(r => r.AddRangeAsync(It.Is<List<Transacao>>(list => list.Count == 2 && list[0].Valor == 1000.00m)), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_RecurringDespesaUpdateBaseFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Despesa,
                Description = "Notebook",
                Amount = 3000.00m,
                CategoryId = "cat-tech",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = true,
                Parcelas = 3
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<bool>("Houve um erro ao atualizar o a parcela base"), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_RecurringDespesaAddRangeFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Despesa,
                Description = "Notebook",
                Amount = 3000.00m,
                CategoryId = "cat-tech",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = true,
                Parcelas = 3
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<bool>("Houve um erro ao salvar as parcelas"), Times.Once);
        }

        [Fact]
        public async Task CreateTransactionAsync_DatabaseException_ReturnsNullAndNotifies()
        {
            // Arrange
            var userId = "user-123";
            var request = new TransactionRequest
            {
                Type = TransacaoType.Despesa,
                Description = "Compra simples",
                Amount = 50.00m,
                CategoryId = "cat-1",
                TransactionDate = DateTime.UtcNow,
                IsRecurring = false
            };

            _transacaoRepository.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .ThrowsAsync(new DatabaseException("Crash"));

            // Act
            var result = await _transacaoService.CreateTransactionAsync(request, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<bool>("Erro no banco: Crash"), Times.Once);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Falha ao salvar transação!"), Times.Once);
        }

        #endregion

        #region UpdateTransactionAsync Tests

        [Fact]
        public async Task UpdateTransactionAsync_NotFound_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var dto = CreateTestTransacaoDTO(id: id, userId: userId);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync((Transacao)null!);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Transação não encontrada!"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_ForbiddenUser_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var dto = CreateTestTransacaoDTO(id: id, userId: userId);
            var existente = CreateTestTransacao(id: id, userId: "different-user");

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Você não tem permissão para atualizar essa transação!"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_SimpleTransaction_UpdatesAndReturnsMapped()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: false, valor: 100m, titulo: "Original");
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: false, valor: 150m, titulo: "Updated");

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result.Titulo);
            Assert.Equal(150m, result.Valor);
            _transacaoRepository.Verify(r => r.UpdateAsync(It.Is<Transacao>(t => t.Id == id && t.Titulo == "Updated" && t.Valor == 150m)), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_SimpleTransactionUpdateFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: false);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: false);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Falha ao atualizar transação!"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_TransitionRecurringToSingle_RemovesChildrenUpdatesParent()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            // A Despesa was originally recurring with 3 parcels, value 10.00 each
            var existente = CreateTestTransacao(id: id, userId: userId, type: TransacaoType.Despesa, isRecurring: true, valor: 10.00m, parcelas: 3);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: false, valor: 10.00m); // Changing to not recurring

            var children = new List<Transacao> { CreateTestTransacao(id: "child-1"), CreateTestTransacao(id: "child-2") };

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.GetByParentTransactionId(id))
                .ReturnsAsync(children);

            _transacaoRepository.Setup(r => r.RemoveRangeAsync(children))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsRecurring);
            // It should multiply the single value back: 10.00 * 3 = 30.00
            Assert.Equal(30.00m, result.Valor);

            _transacaoRepository.Verify(r => r.RemoveRangeAsync(children), Times.Once);
            _transacaoRepository.Verify(r => r.UpdateAsync(It.Is<Transacao>(t => !t.IsRecurring && t.Valor == 30.00m)), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_TransitionRecurringToSingleRemoverFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: true);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: false);

            var children = new List<Transacao> { CreateTestTransacao(id: "child-1") };

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.GetByParentTransactionId(id))
                .ReturnsAsync(children);

            _transacaoRepository.Setup(r => r.RemoveRangeAsync(children))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Houve um erro excluindo as parcelas/recorrências"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_TransitionRecurringToSingleUpdateFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: true);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: false);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.GetByParentTransactionId(id))
                .ReturnsAsync(new List<Transacao>()); // No children to delete

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Houve um erro ao atualizar a transação!"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_TransitionSingleToRecurringRenda_UpdatesBaseGeneratesRecurrences()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, type: TransacaoType.Renda, isRecurring: false, valor: 1000m);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, type: TransacaoType.Renda, isRecurring: true, valor: 1000m);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsRecurring);
            _transacaoRepository.Verify(r => r.UpdateAsync(It.Is<Transacao>(t => t.IsRecurring && t.RecorrenciaType == RecorrenciaType.Mensalmente)), Times.Once);
            _transacaoRepository.Verify(r => r.AddRangeAsync(It.Is<List<Transacao>>(list => list.Count == 12)), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_TransitionSingleToRecurringDespesa_UpdatesBaseGeneratesParcels()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, type: TransacaoType.Despesa, isRecurring: false, valor: 300m, parcelas: 3);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, type: TransacaoType.Despesa, isRecurring: true, valor: 300m, parcelas: 3);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsRecurring);
            Assert.Equal(100.00m, result.Valor); // Divides valor by 3

            // Verify both updates: TransformaEmRecorrente saves the transition, GerarParcelas updates the parcel description and amount
            _transacaoRepository.Verify(r => r.UpdateAsync(It.IsAny<Transacao>()), Times.Exactly(2));
            _transacaoRepository.Verify(r => r.AddRangeAsync(It.Is<List<Transacao>>(l => l.Count == 2 && l[0].Valor == 100.00m)), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_TransitionSingleToRecurringGerarFails_ReturnsNull()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, type: TransacaoType.Renda, isRecurring: false);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, type: TransacaoType.Renda, isRecurring: true);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(false); // Fails generating recurrence

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTransactionAsync_TransitionSingleToRecurringUpdateFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: false);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: true);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(false); // First update fails

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Houve um erro ao atualizar a transação!"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_UpdateRecurringRenda_UpdatesBaseRemovesOldGeneratesNew()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, type: TransacaoType.Renda, isRecurring: true);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, type: TransacaoType.Renda, isRecurring: true);

            var oldChildren = new List<Transacao> { CreateTestTransacao(id: "old-child") };

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.GetByParentTransactionId(id))
                .ReturnsAsync(oldChildren);

            _transacaoRepository.Setup(r => r.RemoveRangeAsync(oldChildren))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.NotNull(result);
            _transacaoRepository.Verify(r => r.UpdateAsync(It.IsAny<Transacao>()), Times.Once);
            _transacaoRepository.Verify(r => r.RemoveRangeAsync(oldChildren), Times.Once);
            _transacaoRepository.Verify(r => r.AddRangeAsync(It.IsAny<List<Transacao>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_UpdateRecurringUpdateBaseFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: true);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: true);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(false); // Base update fails

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<bool>("Falha ao atualizar base!"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_UpdateRecurringRemoveFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: true);
            var dto = CreateTestTransacaoDTO(id: id, userId: userId, isRecurring: true);

            var oldChildren = new List<Transacao> { CreateTestTransacao(id: "old-child") };

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.GetByParentTransactionId(id))
                .ReturnsAsync(oldChildren);

            _transacaoRepository.Setup(r => r.RemoveRangeAsync(oldChildren))
                .ReturnsAsync(false); // Remove fails

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<bool>("Falha ao remover recorrências!"), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_DatabaseException_ReturnsNullAndNotifies()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var dto = CreateTestTransacaoDTO(id: id, userId: userId);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ThrowsAsync(new DatabaseException("Crash"));

            // Act
            var result = await _transacaoService.UpdateTransactionAsync(id, dto, userId);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<Transacao>("Erro no banco: Crash"), Times.Once);
            _notificador.Verify(n => n.Handle<TransacaoDTO>("Transação não encontrada!"), Times.Once);
        }

        #endregion

        #region DeleteTransactionAsync Tests

        [Fact]
        public async Task DeleteTransactionAsync_NotFound_ReturnsFalseAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync((Transacao)null!);

            // Act
            var result = await _transacaoService.DeleteTransactionAsync(id, userId);

            // Assert
            Assert.False(result);
            _notificador.Verify(n => n.Handle<bool>("Transação não encontrada!"), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_ForbiddenUser_ReturnsFalseAndNotifiesError()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: "other-user");
            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            // Act
            var result = await _transacaoService.DeleteTransactionAsync(id, userId);

            // Assert
            Assert.False(result);
            _notificador.Verify(n => n.Handle<bool>("Você não tem permissão para excluir essa transação!"), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_SimpleTransaction_DeletesParent()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: false);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.DeleteAsync(existente))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.DeleteTransactionAsync(id, userId);

            // Assert
            Assert.True(result);
            _transacaoRepository.Verify(r => r.DeleteAsync(existente), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_SimpleTransactionDeleteFails_ReturnsFalseAndNotifies()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: false);

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.DeleteAsync(existente))
                .ReturnsAsync(false);

            // Act
            var result = await _transacaoService.DeleteTransactionAsync(id, userId);

            // Assert
            Assert.False(result);
            _notificador.Verify(n => n.Handle<bool>("Houve um problema ao excluir a transação!"), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_RecurringTransaction_RemovesChildrenAndParent()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: true);
            var children = new List<Transacao> { CreateTestTransacao(id: "child-1") };

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.GetByParentTransactionId(id))
                .ReturnsAsync(children);

            _transacaoRepository.Setup(r => r.RemoveRangeAsync(children))
                .ReturnsAsync(true);

            _transacaoRepository.Setup(r => r.DeleteAsync(existente))
                .ReturnsAsync(true);

            // Act
            var result = await _transacaoService.DeleteTransactionAsync(id, userId);

            // Assert
            Assert.True(result);
            _transacaoRepository.Verify(r => r.RemoveRangeAsync(children), Times.Once);
            _transacaoRepository.Verify(r => r.DeleteAsync(existente), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_RecurringTransactionRemoveChildrenFails_ReturnsFalseAndNotifies()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";
            var existente = CreateTestTransacao(id: id, userId: userId, isRecurring: true);
            var children = new List<Transacao> { CreateTestTransacao(id: "child-1") };

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ReturnsAsync(existente);

            _transacaoRepository.Setup(r => r.GetByParentTransactionId(id))
                .ReturnsAsync(children);

            _transacaoRepository.Setup(r => r.RemoveRangeAsync(children))
                .ReturnsAsync(false); // Fails removing children

            // Act
            var result = await _transacaoService.DeleteTransactionAsync(id, userId);

            // Assert
            Assert.False(result);
            _notificador.Verify(n => n.Handle<bool>("Houve uma falha ao excluir as recorrências!"), Times.Once);
            _transacaoRepository.Verify(r => r.DeleteAsync(existente), Times.Never);
        }

        [Fact]
        public async Task DeleteTransactionAsync_DatabaseException_ReturnsFalseAndNotifies()
        {
            // Arrange
            var id = "t-123";
            var userId = "user-123";

            _transacaoRepository.Setup(r => r.GetByIdAsync(id, userId))
                .ThrowsAsync(new DatabaseException("DB down"));

            // Act
            var result = await _transacaoService.DeleteTransactionAsync(id, userId);

            // Assert
            Assert.False(result);
            _notificador.Verify(n => n.Handle<Transacao>("Erro no banco: DB down"), Times.Once);
            _notificador.Verify(n => n.Handle<bool>("Transação não encontrada!"), Times.Once);
        }

        #endregion

        #region ListTransactionsByPeriodAsync Tests

        [Fact]
        public async Task ListTransactionsByPeriodAsync_NullDates_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";

            // Act
            var result = await _transacaoService.ListTransactionsByPeriodAsync(userId, null, DateTime.UtcNow);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<TransacaoDTO>>("As datas iniciais e finais são obrigatórias!"), Times.Once);
        }

        [Fact]
        public async Task ListTransactionsByPeriodAsync_StartAfterEnd_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var start = DateTime.UtcNow.AddDays(1);
            var end = DateTime.UtcNow;

            // Act
            var result = await _transacaoService.ListTransactionsByPeriodAsync(userId, start, end);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<TransacaoDTO>>("A data de fim deve ser maior que a data de início!"), Times.Once);
        }

        [Fact]
        public async Task ListTransactionsByPeriodAsync_ValidDates_ReturnsMappedTransactions()
        {
            // Arrange
            var userId = "user-123";
            var start = DateTime.UtcNow.AddDays(-5);
            var end = DateTime.UtcNow;

            var transacoes = new List<Transacao> { CreateTestTransacao(userId: userId) };

            _transacaoRepository.Setup(r => r.GetByPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(transacoes);

            // Act
            var result = await _transacaoService.ListTransactionsByPeriodAsync(userId, start, end);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _transacaoRepository.Verify(r => r.GetByPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task ListTransactionsByPeriodAsync_MappingFails_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var start = DateTime.UtcNow.AddDays(-5);
            var end = DateTime.UtcNow;

            var transacoes = new List<Transacao> { CreateTestTransacao(userId: userId) };

            _transacaoRepository.Setup(r => r.GetByPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(transacoes);

            // Mock mapper to return null
            _mapper.Setup(m => m.Map<IEnumerable<TransacaoDTO>>(It.IsAny<IEnumerable<Transacao>>()))
                .Returns((IEnumerable<TransacaoDTO>)null!);

            // Act
            var result = await _transacaoService.ListTransactionsByPeriodAsync(userId, start, end);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<TransacaoDTO>>("Houve um problema ao buscar as transações!"), Times.Once);
        }

        [Fact]
        public async Task ListTransactionsByPeriodAsync_DatabaseException_ReturnsNullAndNotifies()
        {
            // Arrange
            var userId = "user-123";
            var start = DateTime.UtcNow.AddDays(-5);
            var end = DateTime.UtcNow;

            _transacaoRepository.Setup(r => r.GetByPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new DatabaseException("Failed"));

            // Act
            var result = await _transacaoService.ListTransactionsByPeriodAsync(userId, start, end);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<Transacao>>("Erro no banco: Failed"), Times.Once);
            _notificador.Verify(n => n.Handle<IEnumerable<TransacaoDTO>>("Houve um problema ao buscar as transações!"), Times.Once);
        }

        #endregion

        #region GetValuesByMonth Tests

        [Fact]
        public async Task GetValuesByMonth_NullDates_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";

            // Act
            var result = await _transacaoService.GetValuesByMonth(userId, null, DateTime.UtcNow);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<SaldoMensalDTO>>("As datas iniciais e finais são obrigatórias!"), Times.Once);
        }

        [Fact]
        public async Task GetValuesByMonth_StartAfterEnd_ReturnsNullAndNotifiesError()
        {
            // Arrange
            var userId = "user-123";
            var start = DateTime.UtcNow.AddDays(1);
            var end = DateTime.UtcNow;

            // Act
            var result = await _transacaoService.GetValuesByMonth(userId, start, end);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<SaldoMensalDTO>>("A data de fim deve ser maior que a data de início!"), Times.Once);
        }

        [Fact]
        public async Task GetValuesByMonth_ValidDates_ReturnsSaldoMensal()
        {
            // Arrange
            var userId = "user-123";
            var start = DateTime.UtcNow.AddDays(-30);
            var end = DateTime.UtcNow;
            var expectedList = new List<SaldoMensalDTO> { new SaldoMensalDTO { Mes = DateTime.UtcNow, Receita = 100, Despesa = 50 } };

            _transacaoRepository.Setup(r => r.GetValuesByMonth(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expectedList);

            // Act
            var result = await _transacaoService.GetValuesByMonth(userId, start, end);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedList, result);
            _transacaoRepository.Verify(r => r.GetValuesByMonth(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task GetValuesByMonth_DatabaseException_ReturnsNullAndNotifies()
        {
            // Arrange
            var userId = "user-123";
            var start = DateTime.UtcNow.AddDays(-30);
            var end = DateTime.UtcNow;

            _transacaoRepository.Setup(r => r.GetValuesByMonth(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new DatabaseException("Crash"));

            // Act
            var result = await _transacaoService.GetValuesByMonth(userId, start, end);

            // Assert
            Assert.Null(result);
            _notificador.Verify(n => n.Handle<IEnumerable<SaldoMensalDTO>>("Erro no banco: Crash"), Times.Once);
        }

        #endregion
    }
}
