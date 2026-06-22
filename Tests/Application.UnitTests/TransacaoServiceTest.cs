using AutoMapper;
using Fin.Application.DTOs;
using Fin.Application.http.RequestDTO;
using Fin.Application.Interfaces.Repositories;
using Fin.Application.Notificacoes;
using Fin.Application.Services;
using Fin.Domain.Enums;
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
        }

        [Fact]
        public async Task Shoud_SaveAnSimpleTransaction()
        {
            // Arrange

            var transacao = new TransactionRequest
            {
                Amount = 1,
                CategoryId = "123456789",
                Description = "Transacao Teste",
                IsRecurring = false,
                Type = TransacaoType.Despesa,
                TransactionDate = DateTime.UtcNow
            };

            _transacaoRepository.Setup(
                x => x.AddAsync(It.IsAny<Transacao>()))
                .ReturnsAsync(true);

            _mapper.Setup(
                x => x.Map<TransacaoDTO>(It.IsAny<Transacao>()))
                .Returns(new TransacaoDTO());

            // Act

            var result = await _transacaoService.CreateTransactionAsync(transacao, "123");

            // Assert
            Assert.NotNull(result);

            _transacaoRepository.Verify(
                x => x.AddAsync(It.IsAny<Transacao>()),
                Times.Once
                );
        }
    }
}
