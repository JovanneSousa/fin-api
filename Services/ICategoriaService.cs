using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Services
{
    public interface ICategoriaService
    {
        Task<IEnumerable<IconDTO>> ListarIconesAsync();
        Task<IEnumerable<CorDTO>> ListarCoresAsync();
        Task<CategoriaDTO> ObterCategoriaId(string id, string userId);
        Task<CategoriaDTO> AtualizarCategoria(CategoriaDTO categoria, string userId, string categoriaId);
        Task<IEnumerable<CategoriaDTO>> ListCategoriasAsync(string userId);
        Task<CategoriaDTO> CreateCategoriaAsync(string userId, CategoriaDTO categoria);
        Task<bool> DeleteCategoriaAsync(string id, string categoriaId);
    }
}
