using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Services
{
    public interface IUsuarioService
    {
        Task<bool> CriarUsuarioAsync(Usuario usuario);
        Task<UsuarioDTO> BuscarUsuarioPorIdAsync(string id);
    }
}
