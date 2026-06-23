using AutoMapper;
using Fin.Application.DTOs;
using Fin.Application.Interfaces.Repositories;
using Fin.Application.Interfaces.Services;
using Fin.Application.Notificacoes;
using Fin.Application.Services;
using Fin.Domain.Exceptions;
using Fin.Domain.Enums;
using Fin.Domain.Models;
using Moq;

namespace Application.UnitTests
{
    public class CategoriesServiceTest
    {
        private readonly Mock<ICategoriaRepository> _repositoryMock;
        private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
        private readonly Mock<INotificador> _notificadorMock;
        private readonly Mock<ITransacaoService> _transacaoServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CategoriaService _service;

        public CategoriesServiceTest()
        {
            _repositoryMock = new Mock<ICategoriaRepository>();
            _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
            _notificadorMock = new Mock<INotificador>();
            _transacaoServiceMock = new Mock<ITransacaoService>();
            _mapperMock = new Mock<IMapper>();

            _service = new CategoriaService(
                _repositoryMock.Object,
                _transacaoRepositoryMock.Object,
                _notificadorMock.Object,
                _transacaoServiceMock.Object,
                _mapperMock.Object
            );
        }

        private Categoria CreateTestCategoria(
            string id = "cat-123",
            string name = "Alimentação",
            TransacaoType type = TransacaoType.Despesa,
            string userId = "user-123",
            bool isDefault = false,
            string iconId = "icon-123",
            string corId = "cor-123")
        {
            return new Categoria
            {
                Id = id,
                Name = name,
                Type = type,
                UserId = userId,
                IsDefault = isDefault,
                IconId = iconId,
                CorId = corId,
                IconePadrao = new Icon { Id = iconId, Name = "IconePadrao", Url = "icone.png" },
                CorPadrao = new Cor { Id = corId, Url = "#FFFFFF" }
            };
        }

        private CategoriaDTO CreateTestCategoriaDTO(
            string id = "cat-123",
            string name = "Alimentação",
            TransacaoType type = TransacaoType.Despesa,
            string userId = "user-123",
            string iconId = "icon-123",
            string corId = "cor-123")
        {
            return new CategoriaDTO
            {
                Id = id,
                Name = name,
                Type = type,
                UserId = userId,
                IconId = iconId,
                CorId = corId,
                Icone = new IconDTO { Id = iconId, Name = "IconePadrao", Url = "icone.png" },
                Cor = new CorDTO { Id = corId, Url = "#FFFFFF" }
            };
        }

        #region ListCategoriasAsync Tests

        [Fact]
        public async Task ListCategoriasAsync_CategoriesExist_ShouldReturnListOfCategories()
        {
            // Arrange
            var userId = "user-123";
            var categoriesList = new List<CategoriaDTO>
            {
                CreateTestCategoriaDTO(id: "cat-1"),
                CreateTestCategoriaDTO(id: "cat-2")
            };

            _repositoryMock.Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(categoriesList);

            // Act
            var result = await _service.ListCategoriasAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _repositoryMock.Verify(repo => repo.GetAllAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ListCategoriasAsync_NoCategoriesExist_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = "user-123";
            _repositoryMock.Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync((List<CategoriaDTO>)null!);

            // Act
            var result = await _service.ListCategoriasAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _repositoryMock.Verify(repo => repo.GetAllAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ListCategoriasAsync_DatabaseError_ShouldReturnEmptyListAndNotifyError()
        {
            // Arrange
            var userId = "user-123";
            var exceptionMessage = "Connection failed";
            _repositoryMock.Setup(repo => repo.GetAllAsync(userId))
                .ThrowsAsync(new DatabaseException(exceptionMessage));

            // Act
            var result = await _service.ListCategoriasAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _notificadorMock.Verify(n => n.Handle<IEnumerable<CategoriaDTO>>($"Erro no banco: {exceptionMessage}"), Times.Once);
        }

        #endregion

        #region CreateCategoriaAsync Tests

        [Fact]
        public async Task CreateCategoriaAsync_CategoryDoesNotExist_ShouldCreateSuccessfully()
        {
            // Arrange
            var userId = "user-123";
            var inputDto = CreateTestCategoriaDTO(id: "cat-123");
            var createdEntity = CreateTestCategoria(id: "cat-123");

            _repositoryMock.Setup(repo => repo.GetCategoryByNameAndUserIdAsync(userId, inputDto.Name, inputDto.Type))
                .ReturnsAsync((Categoria)null!);

            _repositoryMock.Setup(repo => repo.AddAsync(It.Is<Categoria>(c => c.Name == inputDto.Name && c.UserId == userId)))
                .ReturnsAsync(createdEntity);

            // Act
            var result = await _service.CreateCategoriaAsync(userId, inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdEntity.Id, result.Id);
            Assert.Equal(createdEntity.Name, result.Name);
            _repositoryMock.Verify(repo => repo.GetCategoryByNameAndUserIdAsync(userId, inputDto.Name, inputDto.Type), Times.Once);
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public async Task CreateCategoriaAsync_ErrorAddingCategory_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var userId = "user-123";
            var inputDto = CreateTestCategoriaDTO();

            _repositoryMock.Setup(repo => repo.GetCategoryByNameAndUserIdAsync(userId, inputDto.Name, inputDto.Type))
                .ReturnsAsync((Categoria)null!);

            _repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Categoria>()))
                .ReturnsAsync((Categoria)null!);

            // Act
            var result = await _service.CreateCategoriaAsync(userId, inputDto);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Ocorreu um erro ao criar categoria"), Times.Once);
        }

        [Fact]
        public async Task CreateCategoriaAsync_CategoryAlreadyExistsAndIsNotHidden_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var userId = "user-123";
            var inputDto = CreateTestCategoriaDTO();
            var existingCategory = CreateTestCategoria();

            _repositoryMock.Setup(repo => repo.GetCategoryByNameAndUserIdAsync(userId, inputDto.Name, inputDto.Type))
                .ReturnsAsync(existingCategory);

            _repositoryMock.Setup(repo => repo.IsCategoryHiddenAsync(userId, existingCategory.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateCategoriaAsync(userId, inputDto);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Categoria já existe para este usuário."), Times.Once);
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task CreateCategoriaAsync_CategoryAlreadyExistsAndIsHidden_ShouldShowHiddenCategoryAndReturnSuccess()
        {
            // Arrange
            var userId = "user-123";
            var inputDto = CreateTestCategoriaDTO();
            var existingCategory = CreateTestCategoria();

            _repositoryMock.Setup(repo => repo.GetCategoryByNameAndUserIdAsync(userId, inputDto.Name, inputDto.Type))
                .ReturnsAsync(existingCategory);

            _repositoryMock.Setup(repo => repo.IsCategoryHiddenAsync(userId, existingCategory.Id))
                .ReturnsAsync(true);

            _repositoryMock.Setup(repo => repo.ShowHiddenCategory(userId, existingCategory))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateCategoriaAsync(userId, inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingCategory.Id, result.Id);
            _repositoryMock.Verify(repo => repo.ShowHiddenCategory(userId, existingCategory), Times.Once);
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Categoria>()), Times.Never);
        }

        #endregion

        #region DeleteCategoriaAsync Tests

        [Fact]
        public async Task DeleteCategoriaAsync_CategoryDoesNotExist_ShouldReturnFalseAndNotifyError()
        {
            // Arrange
            var userId = "user-123";
            var categoryId = "cat-999";

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync((Categoria)null!);

            // Act
            var result = await _service.DeleteCategoriaAsync(userId, categoryId);

            // Assert
            Assert.False(result);
            _notificadorMock.Verify(n => n.Handle<bool>("Categoria não encontrada!"), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoriaAsync_NoPermission_ShouldReturnFalseAndNotifyError()
        {
            // Arrange
            var userId = "user-123";
            var categoryId = "cat-123";
            var category = CreateTestCategoria(id: categoryId, userId: "another-user");

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            // Act
            var result = await _service.DeleteCategoriaAsync(userId, categoryId);

            // Assert
            Assert.False(result);
            _notificadorMock.Verify(n => n.Handle<bool>("Você não tem permissão para deletar esta categoria."), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoriaAsync_HasTransactions_ShouldReturnFalseAndNotifyError()
        {
            // Arrange
            var userId = "user-123";
            var categoryId = "cat-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId);

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _transacaoRepositoryMock.Setup(tRepo => tRepo.TransactionsExistsByCategoryAsync(userId, categoryId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteCategoriaAsync(userId, categoryId);

            // Assert
            Assert.False(result);
            _notificadorMock.Verify(n => n.Handle<bool>("Não é possível deletar uma categoria associada a transações."), Times.Once);
            _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Categoria>()), Times.Never);
            _repositoryMock.Verify(repo => repo.HiddenCategory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCategoriaAsync_DefaultCategoryWithoutTransactions_ShouldHideCategoryAndReturnSuccess()
        {
            // Arrange
            var userId = "user-123";
            var categoryId = "cat-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true);

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _transacaoRepositoryMock.Setup(tRepo => tRepo.TransactionsExistsByCategoryAsync(userId, categoryId))
                .ReturnsAsync(false);

            _repositoryMock.Setup(repo => repo.HiddenCategory(userId, categoryId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteCategoriaAsync(userId, categoryId);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(repo => repo.HiddenCategory(userId, categoryId), Times.Once);
            _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCategoriaAsync_CustomCategoryWithoutTransactions_ShouldDeleteCategoryAndReturnSuccess()
        {
            // Arrange
            var userId = "user-123";
            var categoryId = "cat-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: false);

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _transacaoRepositoryMock.Setup(tRepo => tRepo.TransactionsExistsByCategoryAsync(userId, categoryId))
                .ReturnsAsync(false);

            _repositoryMock.Setup(repo => repo.DeleteAsync(category))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteCategoriaAsync(userId, categoryId);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(repo => repo.DeleteAsync(category), Times.Once);
            _repositoryMock.Verify(repo => repo.HiddenCategory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region ListarIconesAsync Tests

        [Fact]
        public async Task ListarIconesAsync_IconsExist_ShouldReturnListOfIcons()
        {
            // Arrange
            var iconsList = new List<IconDTO>
            {
                new IconDTO { Id = "icon-1", Name = "Home", Url = "home.png" },
                new IconDTO { Id = "icon-2", Name = "Car", Url = "car.png" }
            };

            _repositoryMock.Setup(repo => repo.GetAllIconsAsync())
                .ReturnsAsync(iconsList);

            // Act
            var result = await _service.ListarIconesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _repositoryMock.Verify(repo => repo.GetAllIconsAsync(), Times.Once);
        }

        [Fact]
        public async Task ListarIconesAsync_NoIconsExist_ShouldReturnEmptyList()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.GetAllIconsAsync())
                .ReturnsAsync((IList<IconDTO>)null!);

            // Act
            var result = await _service.ListarIconesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _repositoryMock.Verify(repo => repo.GetAllIconsAsync(), Times.Once);
        }

        #endregion

        #region ListarCoresAsync Tests

        [Fact]
        public async Task ListarCoresAsync_ColorsExist_ShouldReturnListOfColors()
        {
            // Arrange
            var coresList = new List<CorDTO>
            {
                new CorDTO { Id = "cor-1", Url = "#FF0000" },
                new CorDTO { Id = "cor-2", Url = "#00FF00" }
            };

            _repositoryMock.Setup(repo => repo.GetAllCorAsync())
                .ReturnsAsync(coresList);

            // Act
            var result = await _service.ListarCoresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _repositoryMock.Verify(repo => repo.GetAllCorAsync(), Times.Once);
        }

        [Fact]
        public async Task ListarCoresAsync_NoColorsExist_ShouldReturnEmptyList()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.GetAllCorAsync())
                .ReturnsAsync((IList<CorDTO>)null!);

            // Act
            var result = await _service.ListarCoresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _repositoryMock.Verify(repo => repo.GetAllCorAsync(), Times.Once);
        }

        #endregion

        #region ObterCategoriaId Tests

        [Fact]
        public async Task ObterCategoriaId_CategoryDoesNotExist_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-999";
            var userId = "user-123";

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync((Categoria)null!);

            // Act
            var result = await _service.ObterCategoriaId(categoryId, userId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Categoria não encontrada!"), Times.Once);
        }

        [Fact]
        public async Task ObterCategoriaId_DefaultCategory_ShouldReturnCategorySuccessfully()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: "another-user", isDefault: true);

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            // Act
            var result = await _service.ObterCategoriaId(categoryId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.Id);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ObterCategoriaId_CustomCategoryOwnedByUser_ShouldReturnCategorySuccessfully()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: false);

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            // Act
            var result = await _service.ObterCategoriaId(categoryId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.Id);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ObterCategoriaId_CustomCategoryOwnedByAnotherUser_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: "another-user", isDefault: false);

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            // Act
            var result = await _service.ObterCategoriaId(categoryId, userId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Você não tem acesso a essa categoria!"), Times.Once);
        }

        #endregion

        #region AtualizarCategoria Tests

        [Fact]
        public async Task AtualizarCategoria_CategoryDoesNotExist_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-999";
            var userId = "user-123";
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync((Categoria)null!);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Categoria não encontrada!"), Times.Once);
        }

        [Fact]
        public async Task AtualizarCategoria_CustomCategorySuccess_ShouldUpdateAndReturnCategory()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: false, name: "Antigo Nome", type: TransacaoType.Despesa);
            var updateDto = new CategoriaUpdateDTO { Name = "Novo Nome", Type = TransacaoType.Renda };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _mapperMock.Setup(m => m.Map(It.IsAny<CategoriaUpdateDTO>(), It.IsAny<Categoria>()))
                .Returns((CategoriaUpdateDTO dto, Categoria cat) =>
                {
                    if (dto.Name != null) cat.Name = dto.Name;
                    if (dto.Type != null) cat.Type = dto.Type.Value;
                    return cat;
                });

            _repositoryMock.Setup(repo => repo.UpdateAsync(It.Is<Categoria>(c => c.Id == categoryId && c.Name == "Novo Nome" && c.Type == TransacaoType.Renda)))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Novo Nome", result.Name);
            Assert.Equal(TransacaoType.Renda, result.Type);
            _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarCategoria_CustomCategoryUpdateFailure_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: false);
            var updateDto = new CategoriaUpdateDTO { Name = "Novo Nome" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _mapperMock.Setup(m => m.Map(It.IsAny<CategoriaUpdateDTO>(), It.IsAny<Categoria>()))
                .Returns((CategoriaUpdateDTO dto, Categoria cat) =>
                {
                    if (dto.Name != null) cat.Name = dto.Name;
                    return cat;
                });

            _repositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Categoria>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Houve um problema ao atualizar a categoria!"), Times.Once);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryAttemptToUpdateNameOrType_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa);
            var updateDtoNameChange = new CategoriaUpdateDTO { Name = "Outro Nome", Type = TransacaoType.Despesa };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            // Act
            var result = await _service.AtualizarCategoria(updateDtoNameChange, userId, categoryId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Não é possivel atualizar o nome ou tipo de uma categoria padrão!"), Times.Once);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateIconWithoutExistingCustomIconSuccess_ShouldSaveIconAndReturnCategory()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, iconId: "old-icon");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, IconId = "new-icon" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetIconsByUsuarioAsync(userId, categoryId))
                .ReturnsAsync((IconeCategoriaUsuario)null!);

            _repositoryMock.Setup(repo => repo.SalvaIconePersonalizado(It.Is<IconeCategoriaUsuario>(i => i.UserId == userId && i.CategoriaId == categoryId && i.IconId == "new-icon")))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(repo => repo.GetIconsByUsuarioAsync(userId, categoryId), Times.Once);
            _repositoryMock.Verify(repo => repo.SalvaIconePersonalizado(It.IsAny<IconeCategoriaUsuario>()), Times.Once);
            _repositoryMock.Verify(repo => repo.DeleteIconCategoriaUsuario(It.IsAny<IconeCategoriaUsuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateIconWithExistingCustomIconSuccess_ShouldDeleteOldSaveNewAndReturnCategory()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, iconId: "old-icon");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, IconId = "new-icon" };
            var existingIconCustom = new IconeCategoriaUsuario { UserId = userId, CategoriaId = categoryId, IconId = "some-custom-icon" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetIconsByUsuarioAsync(userId, categoryId))
                .ReturnsAsync(existingIconCustom);

            _repositoryMock.Setup(repo => repo.DeleteIconCategoriaUsuario(existingIconCustom))
                .ReturnsAsync(true);

            _repositoryMock.Setup(repo => repo.SalvaIconePersonalizado(It.Is<IconeCategoriaUsuario>(i => i.UserId == userId && i.CategoriaId == categoryId && i.IconId == "new-icon")))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(repo => repo.GetIconsByUsuarioAsync(userId, categoryId), Times.Once);
            _repositoryMock.Verify(repo => repo.DeleteIconCategoriaUsuario(existingIconCustom), Times.Once);
            _repositoryMock.Verify(repo => repo.SalvaIconePersonalizado(It.IsAny<IconeCategoriaUsuario>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateIconDeleteFailure_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, iconId: "old-icon");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, IconId = "new-icon" };
            var existingIconCustom = new IconeCategoriaUsuario { UserId = userId, CategoriaId = categoryId, IconId = "some-custom-icon" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetIconsByUsuarioAsync(userId, categoryId))
                .ReturnsAsync(existingIconCustom);

            _repositoryMock.Setup(repo => repo.DeleteIconCategoriaUsuario(existingIconCustom))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Houve um problema ao atualizar a categoria!"), Times.Once);
            _repositoryMock.Verify(repo => repo.SalvaIconePersonalizado(It.IsAny<IconeCategoriaUsuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateIconSaveFailure_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, iconId: "old-icon");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, IconId = "new-icon" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetIconsByUsuarioAsync(userId, categoryId))
                .ReturnsAsync((IconeCategoriaUsuario)null!);

            _repositoryMock.Setup(repo => repo.SalvaIconePersonalizado(It.IsAny<IconeCategoriaUsuario>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Houve um problema ao atualizar a categoria!"), Times.Once);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateColorWithoutExistingCustomColorSuccess_ShouldSaveColorAndReturnCategory()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, corId: "old-cor");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, CorId = "new-cor" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetCorByUsuarioAsync(userId, categoryId))
                .ReturnsAsync((CorCategoriaUsuario)null!);

            _repositoryMock.Setup(repo => repo.SalvaCorPersonalizadaAsync(It.Is<CorCategoriaUsuario>(c => c.UserId == userId && c.CategoriaId == categoryId && c.CorId == "new-cor")))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(repo => repo.GetCorByUsuarioAsync(userId, categoryId), Times.Once);
            _repositoryMock.Verify(repo => repo.SalvaCorPersonalizadaAsync(It.IsAny<CorCategoriaUsuario>()), Times.Once);
            _repositoryMock.Verify(repo => repo.DeleteCorPersonalizadaAsync(It.IsAny<CorCategoriaUsuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateColorWithExistingCustomColorSuccess_ShouldDeleteOldSaveNewAndReturnCategory()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, corId: "old-cor");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, CorId = "new-cor" };
            var existingCorCustom = new CorCategoriaUsuario { UserId = userId, CategoriaId = categoryId, CorId = "some-custom-cor" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetCorByUsuarioAsync(userId, categoryId))
                .ReturnsAsync(existingCorCustom);

            _repositoryMock.Setup(repo => repo.DeleteCorPersonalizadaAsync(existingCorCustom))
                .ReturnsAsync(true);

            _repositoryMock.Setup(repo => repo.SalvaCorPersonalizadaAsync(It.Is<CorCategoriaUsuario>(c => c.UserId == userId && c.CategoriaId == categoryId && c.CorId == "new-cor")))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(repo => repo.GetCorByUsuarioAsync(userId, categoryId), Times.Once);
            _repositoryMock.Verify(repo => repo.DeleteCorPersonalizadaAsync(existingCorCustom), Times.Once);
            _repositoryMock.Verify(repo => repo.SalvaCorPersonalizadaAsync(It.IsAny<CorCategoriaUsuario>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateColorDeleteFailure_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, corId: "old-cor");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, CorId = "new-cor" };
            var existingCorCustom = new CorCategoriaUsuario { UserId = userId, CategoriaId = categoryId, CorId = "some-custom-cor" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetCorByUsuarioAsync(userId, categoryId))
                .ReturnsAsync(existingCorCustom);

            _repositoryMock.Setup(repo => repo.DeleteCorPersonalizadaAsync(existingCorCustom))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Houve um problema ao atualizar a categoria!"), Times.Once);
            _repositoryMock.Verify(repo => repo.SalvaCorPersonalizadaAsync(It.IsAny<CorCategoriaUsuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarCategoria_DefaultCategoryUpdateColorSaveFailure_ShouldReturnNullAndNotifyError()
        {
            // Arrange
            var categoryId = "cat-123";
            var userId = "user-123";
            var category = CreateTestCategoria(id: categoryId, userId: userId, isDefault: true, name: "Alimentacao", type: TransacaoType.Despesa, corId: "old-cor");
            var updateDto = new CategoriaUpdateDTO { Name = "Alimentacao", Type = TransacaoType.Despesa, CorId = "new-cor" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync(category);

            _repositoryMock.Setup(repo => repo.GetCorByUsuarioAsync(userId, categoryId))
                .ReturnsAsync((CorCategoriaUsuario)null!);

            _repositoryMock.Setup(repo => repo.SalvaCorPersonalizadaAsync(It.IsAny<CorCategoriaUsuario>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AtualizarCategoria(updateDto, userId, categoryId);

            // Assert
            Assert.Null(result);
            _notificadorMock.Verify(n => n.Handle<CategoriaDTO>("Houve um problema ao atualizar a categoria!"), Times.Once);
        }

        #endregion
    }
}
