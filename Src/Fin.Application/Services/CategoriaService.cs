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

            if (!categories.Any())
            {
                _notificador.Handle(new Notificacao("Ocorreu um erro ao listar as categorias"));
                return null;
            }

            return categories;
        }

        public async Task<CategoriaDTO> CreateCategoriaAsync(string userId, CategoriaDTO categoria)
        {
            var category = await ExecuteAsync(
                async() => await _repository.GetCategoryByNameAndUserIdAsync(userId, categoria.Name)
                );

            if(category is null)
            {
                categoria.UserId = userId;
                var created = await ExecuteAsync(
                    async () => await _repository.AddAsync(categoria.ToDomain())
                    );

                if (created == null)
                {
                    _notificador.Handle(new Notificacao("Ocorreu um erro ao criar categoria"));
                    return null;
                }

                return CategoriaDTO.ToDto(created);
            }


            var isHidden = await ExecuteAsync(
                async () => await _repository.IsCategoryHiddenAsync(userId, categoria.Id)
                );

            if (!isHidden)
            {
                _notificador.Handle(new Notificacao("Categoria já existe para este usuário."));
                return null;
            }
            if (category == null)
            {
                _notificador.Handle(new Notificacao("Falha ao encontrar categoria"));
                return null;
            }
            await ExecuteAsync(
                async () => await _repository.ShowHiddenCategory(userId, category)
                );

            return CategoriaDTO.ToDto(category);
        }

        public async Task<bool> DeleteCategoriaAsync(string userId, string categoriaId)
        {
            var categoria = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(categoriaId, userId)
                );

            if (categoria == null)
            {
                _notificador.Handle(new Notificacao("Categoria não encontrada!"));
                return false;
            }

            if (categoria.UserId != userId && categoria.UserId != null)
            {
                _notificador.Handle(new Notificacao("Você não tem permissão para deletar esta categoria."));
                return false;
            }

            var transacaoExists = await ExecuteAsync(
                async () => await _transacaoRepository.GetAllAsync(userId)
                );

            if (transacaoExists.Any(t => t.CategoriaId == categoria.Id))
            {
                _notificador.Handle(new Notificacao("Não é possível deletar uma categoria associada a transações."));
                return false;
            }

            if (categoria.IsDefault)
                return await ExecuteAsync(
                    async () => await _repository.HiddenCategory(userId, categoria.Id)
                    );


            if (!categoria.IsDefault)
                return await ExecuteAsync(
                    async () => await _repository.DeleteAsync(categoria)
                    );

            _notificador.Handle(new Notificacao("Ocorreu um erro ao deletar a categoria!"));
            return false;
        }

        public async Task<IEnumerable<IconDTO>> ListarIconesAsync()
        {
            var icons = await ExecuteAsync(
                async () => await _repository.GetAllIconsAsync()
                );

            if(!icons.Any())
            {
                _notificador.Handle(new Notificacao("Nenhum icone encontrado"));
                return null;
            }

            return icons;
        }

        public async Task<IEnumerable<CorDTO>> ListarCoresAsync()
        {
            var cores = await ExecuteAsync(
                async () => await _repository.GetAllCorAsync()
                );

            if (!cores.Any())
            {
                _notificador.Handle(new Notificacao("Nenuma cor encontrada"));
                return null;
            }

            return cores;
        }

        public async Task<CategoriaDTO> ObterCategoriaId(string id, string userId)
        {
            var categoria = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(id, userId)
                );

            if (categoria == null)
            {
                _notificador.Handle(new Notificacao("Categoria não encontrada!"));
                return null;
            }

            if (!categoria.IsDefault && categoria.UserId != userId) 
            {
                _notificador.Handle(new Notificacao("Você não tem acesso a essa categoria!"));
                return null;
            }

            return CategoriaDTO.ToDto(categoria);
        }

        public async Task<CategoriaDTO> AtualizarCategoria(CategoriaUpdateDTO categoriaDTO, string userId, string categoriaId)
        {
            var categoria = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(categoriaId, userId)
                );

            if(categoria == null)
            {
                _notificador.Handle(new Notificacao("Categoria não encontrada!"));
                return null;
            }
            if (categoria.IsDefault)
            {
                if(AtualizacaoInvalida(categoria, categoriaDTO))
                {
                    _notificador.Handle(new Notificacao("Não é possivel atualizar o nome ou tipo de uma categoria padrão!"));
                    return null;
                }
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
                    {
                        _notificador.Handle(new Notificacao("Houve um problema ao atualizar a categoria!"));
                        return null;
                    }
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
                    {
                        _notificador.Handle(new Notificacao("Houve um problema ao atualizar a categoria!"));
                        return null;
                    }
                }
            } else
            {
                var result = await ExecuteAsync(
                    async () => await _repository.UpdateAsync(_mapper.Map(categoriaDTO, categoria))
                    );

                if (!result)
                {
                    _notificador.Handle(new Notificacao("Houve um problema ao atualizar a categoria!"));
                    return null;
                }
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
