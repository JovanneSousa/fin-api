using Fin.Domain.Models;

namespace Fin.Application.Interfaces.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetUsuarioByIdAsync(string id);
        Task<List<Usuario>> GetUsuariosAsync();
        Task<bool> CreateUsuarioAsync(Usuario usuario);
    }
}
