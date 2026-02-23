using fin_api.Models;

namespace fin_api.Repositories
{
    public interface ICategoriaRepository
    {
        Task<Categoria> GetByIdAsync(string id, string userId); 
        Task<IEnumerable<Categoria>> GetAllAsync(string userId);
        Task<Categoria> AddAsync(Categoria categoria);
        Task<bool> UpdateAsync(Categoria categoria);
        Task<bool> DeleteAsync(Categoria categoria);
        Task<bool> ExistsAsync(string userId, string name);
        Task<List<Categoria>> ListCategoriesHiddenAsync(string userId);
        Task<bool> IsCategoryHiddenAsync(string userId, string name);
        Task ShowHiddenCategory(string userId, Categoria categoria);
        Task<bool> HiddenCategory(string userId, string categoriaId);

        Task<List<Icon>> GetAllIconsAsync();
        Task<IconeCategoriaUsuario> GetIconsByUsuarioAsync(string usuarioId, string categoriaId);
        Task<bool> DeleteIconCategoriaUsuario(IconeCategoriaUsuario iconeCategoriaUsuario);
        Task<bool> SalvaIconePersonalizado(IconeCategoriaUsuario iconeCategoriaUsuario);

        Task<List<Cor>> GetAllCorAsync();
        Task<CorCategoriaUsuario> GetCorByUsuarioAsync(string usuarioId, string categoriaId);
        Task<bool> DeleteCorPersonalizadaAsync(CorCategoriaUsuario corCategoriaUsuario);
        Task<bool> SalvaCorPersonalizadaAsync(CorCategoriaUsuario corCategoriaUsuario);
    }
}
