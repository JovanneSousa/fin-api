using fin_api.Models;

namespace fin_api.Repositories
{
    public interface ICategoriaRepository
    {

        Task<Categoria> GetByIdAsync(string id);
        Task<IEnumerable<Categoria>> GetAllAsync(string userId);
        Task AddAsync(Categoria categoria);
        Task UpdateAsync(Categoria categoria);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string userId, string name);

    }
}
