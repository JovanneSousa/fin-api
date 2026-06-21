using Fin.Domain.Models;

namespace Fin.Infra.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetUsuarioByIdAsync(string id);
        Task<List<Usuario>> GetUsuariosAsync();
        Task<bool> CreateUsuarioAsync(Usuario usuario);
    }
}
