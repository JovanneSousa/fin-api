using Fin.Application.DTOs;
using Fin.Domain.Models;

namespace Fin.Application.Interfaces.Repositories
{
    public interface ICategoriaRepository
    {
        // Categorias
        Task<Categoria> GetByIdAsync(string id, string userId);
        Task<IEnumerable<CategoriaDTO>> GetAllAsync(string userId);
        Task<Categoria> AddAsync(Categoria categoria);
        Task<bool> UpdateAsync(Categoria categoria);
        Task<bool> DeleteAsync(Categoria categoria);
        Task<bool> ExistsAsync(string userId, string name);
        Task<bool> IsCategoryHiddenAsync(string userId, string name);
        Task ShowHiddenCategory(string userId, Categoria categoria);
        Task<bool> HiddenCategory(string userId, string categoriaId);


        // Icones
        Task<IList<IconDTO>> GetAllIconsAsync();
        Task<IconeCategoriaUsuario> GetIconsByUsuarioAsync(string usuarioId, string categoriaId);
        Task<bool> DeleteIconCategoriaUsuario(IconeCategoriaUsuario iconeCategoriaUsuario);
        Task<bool> SalvaIconePersonalizado(IconeCategoriaUsuario iconeCategoriaUsuario);


        // Cores
        Task<IList<CorDTO>> GetAllCorAsync();
        Task<CorCategoriaUsuario> GetCorByUsuarioAsync(string usuarioId, string categoriaId);
        Task<bool> DeleteCorPersonalizadaAsync(CorCategoriaUsuario corCategoriaUsuario);
        Task<bool> SalvaCorPersonalizadaAsync(CorCategoriaUsuario corCategoriaUsuario);
    }
}
