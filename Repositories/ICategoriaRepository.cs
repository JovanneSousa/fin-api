using fin_api.Models;

namespace fin_api.Repositories
{
    public interface ICategoriaRepository
    {

        Task<Categoria> GetByIdAsync(string id);
        Task<IEnumerable<Categoria>> GetAllAsync(string userId);
        Task<bool> AddAsync(Categoria categoria);
        Task UpdateAsync(Categoria categoria);
        Task<bool> DeleteAsync(Categoria categoria);
        Task<bool> ExistsAsync(string userId, string name);
        Task<List<Categoria>> ListCategoriesHiddenAsync(string userId);
        Task<bool> IsCategoryHiddenAsync(string userId, string name);
        Task ShowHiddenCategory(string userId, Categoria categoria);
        Task<bool> HiddenCategory(string userId, string categoriaId);
    }
}
