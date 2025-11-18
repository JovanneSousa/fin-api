using fin_api.Models;
using fin_api.Repositories;

namespace fin_api.Services
{
    public class CategoriaService : ICategoriaService
    {

        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Categoria>> ListCategoriasAsync(string userId)
            => await _repository.GetAllAsync(userId);

        public async Task<Categoria> GetCategoriaAsync(string id)
            => await _repository.GetByIdAsync(id);

        public async Task<Categoria> CreateCategoriaAsync(Categoria categoria)
        {
            await _repository.AddAsync(categoria);
            return categoria;
        }

        public async Task<Categoria> UpdateCategoriaAsync(Categoria categoria)
        {
            await _repository.UpdateAsync(categoria);
            return categoria;
        }

        public async Task<bool> DeleteCategoriaAsync(string id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }

    }
}
