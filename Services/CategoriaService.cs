using AutoMapper;
using fin_api.DTOs;
using fin_api.Models;
using fin_api.Notificacoes;
using fin_api.Repositories;

namespace fin_api.Services
{
    public class CategoriaService : ICategoriaService
    {

        private readonly ICategoriaRepository _repository;
        private readonly INotificador _notificador;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IMapper _mapper;

        public CategoriaService(
            ICategoriaRepository repository,
            ITransacaoRepository transacaoRepository, 
            INotificador notificador, 
            ITransacaoService transacaoService,
            IMapper mapper)
        {
            _repository = repository;
            _notificador = notificador;
            _transacaoRepository = transacaoRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoriaDTO>> ListCategoriasAsync(string userId)
        { 
            var categories = await _repository.GetAllAsync(userId);
            var hiddenCategoriesId = await _repository.ListCategoriesHiddenAsync(userId);
            var visibleCategories = categories.Where(c => !hiddenCategoriesId.Contains(c));

            var result = _mapper.Map<IEnumerable<CategoriaDTO>>(visibleCategories);
            if (!result.Any())
            {
                _notificador.Handle(new Notificacao("Ocorreu um erro ao listar as categorias"));
                return null;
            }

            return result;
        }

        public async Task<CategoriaDTO> CreateCategoriaAsync(string userId, CategoriaDTO categoria)
        {
            var exists = await _repository.ExistsAsync(userId, categoria.Name);
            var isHidden = await _repository.IsCategoryHiddenAsync(userId, categoria.Name);
            if (exists && !isHidden)
            {
                _notificador.Handle(new Notificacao("Categoria já existe para este usuário."));
                return null;
            }

            if(exists && isHidden)
            {
                var category = await _repository.GetAllAsync(userId);
                var toUnhide = category.FirstOrDefault(c => c.Name.ToLower() == categoria.Name.ToLower() && c.IsDefault);
                if (toUnhide != null)
                {
                    await _repository.ShowHiddenCategory(userId, toUnhide);
                    return _mapper.Map<CategoriaDTO>(toUnhide);
                }
            }
            categoria.UserId = userId;
            var result = await _repository.AddAsync(_mapper.Map<Categoria>(categoria));

            if(!result)
            {
                _notificador.Handle(new Notificacao("Ocorreu um erro ao criar categoria"));
                return null;
            }

            return categoria;
        }

        public async Task<Categoria> UpdateCategoriaAsync(Categoria categoria)
        {
            await _repository.UpdateAsync(categoria);
            return categoria;
        }

        public async Task<bool> DeleteCategoriaAsync(string userId, string categoriaId)
        {
            var categoria = await _repository.GetByIdAsync(categoriaId, userId);
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

            var transacaoExists = await _transacaoRepository.GetAllAsync(userId);
            if (transacaoExists.Any(t => t.CategoriaId == categoria.Id))
            {
                _notificador.Handle(new Notificacao("Não é possível deletar uma categoria associada a transações."));
                return false;
            }

            if (categoria.IsDefault)
                return await _repository.HiddenCategory(userId, categoria.Id);


            if (!categoria.IsDefault)
                return await _repository.DeleteAsync(categoria);


            _notificador.Handle(new Notificacao("Ocorreu um erro ao deletar a categoria!"));
            return false;
        }

        public async Task<IEnumerable<IconDTO>> ListarIconesAsync()
        {
            var icons = await _repository.GetAllIconsAsync();
            if(!icons.Any())
            {
                _notificador.Handle(new Notificacao("Nenhum icone encontrado"));
                return null;
            }

            return _mapper.Map<IEnumerable<IconDTO>>(icons);
        }

        public async Task<IEnumerable<CorDTO>> ListarCoresAsync()
        {
            var cores = await _repository.GetAllCorAsync();
            if (!cores.Any())
            {
                _notificador.Handle(new Notificacao("Nenuma cor encontrada"));
                return null;
            }

            return _mapper.Map<IEnumerable<CorDTO>>(cores);
        }

        public async Task<CategoriaDTO> ObterCategoriaId(string id, string userId)
        {
            var categoria = await _repository.GetByIdAsync(id, userId);
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

            return _mapper.Map<CategoriaDTO>(categoria);
        }

        private bool AtualizacaoInvalida(Categoria categoria, CategoriaUpdateDTO categoriaDTO)
            => categoria.Name != categoriaDTO.Name || categoria.Type != categoriaDTO.Type;


        public async Task<CategoriaDTO> AtualizarCategoria(CategoriaUpdateDTO categoriaDTO, string userId, string categoriaId)
        {
            var categoria = await _repository.GetByIdAsync(categoriaId, userId);
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
                _mapper.Map(categoriaDTO, categoria);

                var result = await _repository.UpdateAsync(categoria);
                if (!result)
                {
                    _notificador.Handle(new Notificacao("Houve um problema ao atualizar a categoria!"));
                    return null;
                }
            }
            return _mapper.Map<CategoriaDTO>(categoria);
        }

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
            var existente = await getExistente(userId, categoriaId);
            if (existente != null)
            {
                var deleted = await delete(existente);
                if (!deleted)
                    return false;
            }

            var novo = factory(userId, categoriaId, novoValor);
            return await salvar(novo);
        }

    }
}
