using fin_api.Models;
using fin_api.Repositories;

namespace fin_api.Services
{
    public class CategoriaService : ICategoriaService
    {

        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository, ITransacaoRepository transacaoRepository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Categoria>> ListCategoriasAsync(string userId)
        { 
            var categories = await _repository.GetAllAsync(userId);
            var hiddenCategoriesId = await _repository.ListCategoriesHiddenAsync(userId);

            return categories.Where(c => !hiddenCategoriesId.Contains(c));
        }

        public async Task<Categoria> GetCategoriaAsync(string id)
            => await _repository.GetByIdAsync(id);

        public async Task<Categoria> CreateCategoriaAsync(Categoria categoria)
        {
            var exists = await _repository.ExistsAsync(categoria.UserId, categoria.Name);
            var isHidden = await _repository.IsCategoryHiddenAsync(categoria.UserId, categoria.Name);
            if (exists && !isHidden)
                throw new InvalidOperationException("Categoria já existe para este usuário.");

            if(exists && isHidden)
            {
                var category = await _repository.GetAllAsync(categoria.UserId);
                var toUnhide = category.FirstOrDefault(c => c.Name.ToLower() == categoria.Name.ToLower() && c.IsDefault);
                if (toUnhide != null)
                {
                    await _repository.ShowHiddenCategory(toUnhide);
                    return toUnhide;
                }

            }

            await _repository.AddAsync(categoria);
            return categoria;
        }

        public async Task<Categoria> UpdateCategoriaAsync(Categoria categoria)
        {
            await _repository.UpdateAsync(categoria);
            return categoria;
        }

        public async Task<bool> DeleteCategoriaAsync(Categoria categoria)
        {

            await _repository.DeleteAsync(categoria);
            return true;
        }

    }
}
