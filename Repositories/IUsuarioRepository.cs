using fin_api.Models;

namespace fin_api.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetUsuarioByIdAsync(string id);
        Task<List<Usuario>> GetUsuariosAsync();
        Task<bool> CreateUsuarioAsync(Usuario usuario);
    }
}
