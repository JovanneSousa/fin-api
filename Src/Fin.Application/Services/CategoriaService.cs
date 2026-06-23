using Fin.Application.Interfaces.Repositories;
using Fin.Application.Interfaces.Services;
using Fin.Domain.Models;
using Fin.Application.DTOs;
using Fin.Application.Notificacoes;
using AutoMapper;

namespace Fin.Application.Services
{
    public class CategoriaService : BaseService, ICategoriaService
    {
        private readonly ICategoriaRepository _repository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IMapper _mapper;

        public CategoriaService(
            ICategoriaRepository repository,
            ITransacaoRepository transacaoRepository,
            INotificador notificador,
            ITransacaoService transacaoService,
            IMapper mapper
            ) : base(notificador)
        {
            _mapper = mapper;
            _repository = repository;
            _transacaoRepository = transacaoRepository;
        }

        public async Task<IEnumerable<CategoriaDTO>> ListCategoriasAsync(string userId)
        { 
            var categories = await ExecuteAsync(
                async () => await _repository.GetAllAsync(userId)
                );

            return categories ?? new List<CategoriaDTO>();
        }

        public async Task<CategoriaDTO> CreateCategoriaAsync(string userId, CategoriaDTO categoria)
        {
            var category = await ExecuteAsync(
                async() => await _repository.GetCategoryByNameAndUserIdAsync(userId, categoria.Name, categoria.Type)
                );

            if(category is null)
                return await BuildCategory(userId, categoria);


            var isHidden = await ExecuteAsync(
                async () => await _repository.IsCategoryHiddenAsync(userId, category.Id)
                );
            if (!isHidden)
                return RetornaErroProcessamento<CategoriaDTO>("Categoria já existe para este usuário.");

            return await ShowHiddenCategory(userId, category);
        }

        private async Task<CategoriaDTO> ShowHiddenCategory(string userId, Categoria category)
        {
            await ExecuteAsync(
                async () => await _repository.ShowHiddenCategory(userId, category)
                );

            return CategoriaDTO.ToDto(category);
        }

        private async Task<CategoriaDTO> BuildCategory(string userId, CategoriaDTO categoria)
        {
            categoria.UserId = userId;
            var created = await ExecuteAsync(
                async () => await _repository.AddAsync(categoria.ToDomain())
                );

            if (created == null)
                return RetornaErroProcessamento<CategoriaDTO>("Ocorreu um erro ao criar categoria");

            return CategoriaDTO.ToDto(created);
        }

        public async Task<bool> DeleteCategoriaAsync(string userId, string categoriaId)
        {
            var categoria = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(categoriaId, userId)
                );

            if (categoria == null)
                return RetornaErroProcessamento<bool>("Categoria não encontrada!");

            if (categoria.UserId != userId && categoria.UserId != null)
                return RetornaErroProcessamento<bool>("Você não tem permissão para deletar esta categoria.");

            var transactionExists = await ExecuteAsync(
                async () => await _transacaoRepository.TransactionsExistsByCategoryAsync(userId, categoriaId));

            if (transactionExists)
                return RetornaErroProcessamento<bool>("Não é possível deletar uma categoria associada a transações.");

            return await ExecuteAsync(
                async () => categoria.IsDefault ?
                    await _repository.HiddenCategory(userId, categoria.Id) : 
                    await _repository.DeleteAsync(categoria)
                );
        }

        public async Task<IEnumerable<IconDTO>> ListarIconesAsync()
        {
            var icons = await ExecuteAsync(
                async () => await _repository.GetAllIconsAsync()
                );

            return icons ?? new List<IconDTO>();
        }

        public async Task<IEnumerable<CorDTO>> ListarCoresAsync()
        {
            var cores = await ExecuteAsync(
                async () => await _repository.GetAllCorAsync()
                );

            return cores ?? new List<CorDTO>();
        }

        public async Task<CategoriaDTO> ObterCategoriaId(string id, string userId)
        {
            var categoria = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(id, userId)
                );

            if (categoria == null)
                return RetornaErroProcessamento<CategoriaDTO>("Categoria não encontrada!");

            if (!categoria.IsDefault && categoria.UserId != userId) 
                return RetornaErroProcessamento<CategoriaDTO>("Você não tem acesso a essa categoria!");

            return CategoriaDTO.ToDto(categoria);
        }

        public async Task<CategoriaDTO> AtualizarCategoria(CategoriaUpdateDTO categoriaDTO, string userId, string categoriaId)
        {
            var categoria = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(categoriaId, userId)
                );

            if(categoria == null)
                return RetornaErroProcessamento<CategoriaDTO>("Categoria não encontrada!");

            if (categoria.IsDefault)
            {
                if(AtualizacaoInvalida(categoria, categoriaDTO))
                    return RetornaErroProcessamento<CategoriaDTO>("Não é possivel atualizar o nome ou tipo de uma categoria padrão!");

                if (!string.IsNullOrEmpty(categoriaDTO.IconId) && categoria.Icone.Id != categoriaDTO.IconId)
                {
                    var result = await AtualizarValorPersonalizadoAsync<IconeCategoriaUsuario>(
                                    userId,
                                    categoria.Id,
                                    categoriaDTO.IconId,
                                    _repository.GetIconsByUsuarioAsync,
                                    _repository.DeleteIconCategoriaUsuario,
                                    _repository.SalvaIconePersonalizado,
                                    (userId, categoriaId, iconId) => new IconeCategoriaUsuario
                                    {
                                        UserId = userId,
                                        CategoriaId = categoriaId,
                                        IconId = iconId
                                    });

                    if (!result)
                        return RetornaErroProcessamento<CategoriaDTO>("Houve um problema ao atualizar a categoria!");
                }

                if (!string.IsNullOrEmpty(categoriaDTO.CorId) && categoria.Cor.Id != categoriaDTO.CorId)
                {
                    var result = await AtualizarValorPersonalizadoAsync<CorCategoriaUsuario>(
                                    userId,
                                    categoria.Id,
                                    categoriaDTO.CorId,
                                    _repository.GetCorByUsuarioAsync,
                                    _repository.DeleteCorPersonalizadaAsync,
                                    _repository.SalvaCorPersonalizadaAsync,
                                    (userId, categoriaId, corId) => new CorCategoriaUsuario
                                    {
                                        UserId = userId,
                                        CategoriaId = categoriaId,
                                        CorId = corId
                                    });
                    if (!result)
                        return RetornaErroProcessamento<CategoriaDTO>("Houve um problema ao atualizar a categoria!");
                }
            } else
            {
                var result = await ExecuteAsync(
                    async () => await _repository.UpdateAsync(_mapper.Map(categoriaDTO, categoria))
                    );

                if (!result)
                    return RetornaErroProcessamento<CategoriaDTO>("Houve um problema ao atualizar a categoria!");
            }
            return CategoriaDTO.ToDto(categoria);
        }

        private bool AtualizacaoInvalida(Categoria categoria, CategoriaUpdateDTO categoriaDTO)
                        => categoria.Name != categoriaDTO.Name || categoria.Type != categoriaDTO.Type;

        private async Task<bool> AtualizarValorPersonalizadoAsync<T>(
            string userId,
            string categoriaId,
            string novoValor,
            Func<string, string, Task<T>> getExistente,
            Func<T, Task<bool>> delete,
            Func<T, Task<bool>> salvar,
            Func<string, string, string, T> factory
        )
        {
            var existente = await ExecuteAsync(
                async () => await getExistente(userId, categoriaId)
                );
            if (existente != null)
            {
                var deleted = await ExecuteAsync(
                    async () => await delete(existente)
                    );

                if (!deleted)
                    return false;
            }

            var novo = factory(userId, categoriaId, novoValor);
            return await ExecuteAsync(async () => await salvar(novo));
        }

    }
}
