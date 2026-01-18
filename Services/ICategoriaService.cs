using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Services
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDTO>> ListCategoriasAsync(string userId);
        Task<Categoria> GetCategoriaAsync(string id);
        Task<Categoria> CreateCategoriaAsync(string userId, Categoria categoria);
        Task<Categoria> UpdateCategoriaAsync(Categoria categoria);
        Task<bool> DeleteCategoriaAsync(string id, string categoriaId);
    }
}
