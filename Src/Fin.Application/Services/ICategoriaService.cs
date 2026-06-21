using Fin.Infra.DTOs;

namespace Fin.Application.Services
{
    public interface ICategoriaService
    {
        Task<IEnumerable<IconDTO>> ListarIconesAsync();
        Task<IEnumerable<CorDTO>> ListarCoresAsync();
        Task<CategoriaDTO> ObterCategoriaId(string id, string userId);
        Task<CategoriaDTO> AtualizarCategoria(CategoriaUpdateDTO categoria, string userId, string categoriaId);
        Task<IEnumerable<CategoriaDTO>> ListCategoriasAsync(string userId);
        Task<CategoriaDTO> CreateCategoriaAsync(string userId, CategoriaDTO categoria);
        Task<bool> DeleteCategoriaAsync(string id, string categoriaId);
    }
}
