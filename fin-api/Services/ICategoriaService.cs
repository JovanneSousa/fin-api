using fin_api.Models;

namespace fin_api.Services
{
    public interface ICategoriaService
    {
        Task<IEnumerable<Categoria>> ListCategoriasAsync(string userId);
        Task<Categoria> GetCategoriaAsync(string id);
        Task<Categoria> CreateCategoriaAsync(Categoria categoria);
        Task<Categoria> UpdateCategoriaAsync(Categoria categoria);
        Task<bool> DeleteCategoriaAsync(string id);
    }
}
