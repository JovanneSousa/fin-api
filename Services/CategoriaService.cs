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

            var result = _mapper.Map<IEnumerable<CategoriaDTO>>(categories.Where(c => !hiddenCategoriesId.Contains(c)));
            if (!result.Any())
            {
                _notificador.Handle(new Notificacao("Ocorreu um erro ao listar as categorias"));
                return null;
            }

            return result;
        }

        public async Task<Categoria> GetCategoriaAsync(string id)
            => await _repository.GetByIdAsync(id);

        public async Task<Categoria> CreateCategoriaAsync(string userId, Categoria categoria)
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
                    return toUnhide;
                }

            }
            categoria.UserId = userId;
            var result = await _repository.AddAsync(categoria);

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
            var categoria = await _repository.GetByIdAsync(categoriaId);
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

    }
}
