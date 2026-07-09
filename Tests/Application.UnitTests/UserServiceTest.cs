using AutoMapper;
using Fin.Application.DTOs;
using Fin.Application.Interfaces.Repositories;
using Fin.Application.Notificacoes;
using Fin.Application.Services;
using Fin.Domain.Exceptions;
using Fin.Domain.Models;
using Moq;

namespace Application.UnitTests
{
    public class UserServiceTest
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<INotificador> _notificadorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UsuarioService _service;

        public UserServiceTest()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _notificadorMock = new Mock<INotificador>();
            _mapperMock = new Mock<IMapper>();

            _service = new UsuarioService(
                _usuarioRepositoryMock.Object,
                _notificadorMock.Object,
                _mapperMock.Object
            );
        }

        private Usuario CreateTestUsuario(string id = "user-123", string nome = "John Doe", string email = "john@example.com")
        {
            return new Usuario
            {
                Id = id,
                Nome = nome,
                Email = email
            };
        }

        private UsuarioDTO CreateTestUsuarioDTO(string id = "user-123", string nome = "John Doe", string email = "john@example.com")
        {
            return new UsuarioDTO
            {
                Id = id,
                Nome = nome,
                Email = email
            };
        }

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_SuccessfulCreation_ShouldReturnTrue()
        {
            // Arrange
            var usuario = CreateTestUsuario();
            _usuarioRepositoryMock.Setup(repo => repo.CreateUsuarioAsync(usuario))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CriarUsuarioAsync(usuario);

            // Assert
            Assert.True(result);
            _usuarioRepositoryMock.Verify(repo => repo.CreateUsuarioAsync(usuario), Times.Once);
            _notificadorMock.Verify(n => n.Handle<bool>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_RepositoryReturnsFalse_ShouldReturnFalseAndNotifyError()
        {
            // Arrange
            var usuario = CreateTestUsuario();
            _usuarioRepositoryMock.Setup(repo => repo.CreateUsuarioAsync(usuario))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CriarUsuarioAsync(usuario);

            // Assert
            Assert.False(result);
            _usuarioRepositoryMock.Verify(repo => repo.CreateUsuarioAsync(usuario), Times.Once);
            _notificadorMock.Verify(n => n.Handle<bool>("Erro ao salvar usuario!"), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_DatabaseError_ShouldReturnFalseAndNotifyError()
        {
            // Arrange
            var usuario = CreateTestUsuario();
            var exceptionMessage = "Database connection timed out";
            _usuarioRepositoryMock.Setup(repo => repo.CreateUsuarioAsync(usuario))
                .ThrowsAsync(new DatabaseException(exceptionMessage));

            // Act
            var result = await _service.CriarUsuarioAsync(usuario);

            // Assert
            Assert.False(result);
            _notificadorMock.Verify(n => n.Handle<bool>($"Erro no banco: {exceptionMessage}"), Times.Once);
            _notificadorMock.Verify(n => n.Handle<bool>("Erro ao salvar usuario!"), Times.Once);
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_UserExists_ShouldReturnMappedUserDTO()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUsuario(id: userId);
            var userDto = CreateTestUsuarioDTO(id: userId);

            _usuarioRepositoryMock.Setup(repo => repo.GetUsuarioByIdAsync(userId))
                .ReturnsAsync(user);

            _mapperMock.Setup(m => m.Map<UsuarioDTO>(user))
                .Returns(userDto);

            // Act
            var result = await _service.BuscarUsuarioPorIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(user.Nome, result.Nome);
            Assert.Equal(user.Email, result.Email);
            _usuarioRepositoryMock.Verify(repo => repo.GetUsuarioByIdAsync(userId), Times.Once);
            _mapperMock.Verify(m => m.Map<UsuarioDTO>(user), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserDoesNotExist_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var userId = "user-999";
            _usuarioRepositoryMock.Setup(repo => repo.GetUsuarioByIdAsync(userId))
                .ReturnsAsync((Usuario)null!);

            // Act
            var result = await _service.BuscarUsuarioPorIdAsync(userId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<UsuarioDTO>("Usuario não encontrado!"), Times.Once);
            _mapperMock.Verify(m => m.Map<UsuarioDTO>(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdAsync_DatabaseError_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var userId = "user-123";
            var exceptionMessage = "Query failed";
            _usuarioRepositoryMock.Setup(repo => repo.GetUsuarioByIdAsync(userId))
                .ThrowsAsync(new DatabaseException(exceptionMessage));

            // Act
            var result = await _service.BuscarUsuarioPorIdAsync(userId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<Usuario>($"Erro no banco: {exceptionMessage}"), Times.Once);
            _notificadorMock.Verify(n => n.Handle<UsuarioDTO>("Usuario não encontrado!"), Times.Once);
            _mapperMock.Verify(m => m.Map<UsuarioDTO>(It.IsAny<Usuario>()), Times.Never);
        }

        #endregion
    }
}
