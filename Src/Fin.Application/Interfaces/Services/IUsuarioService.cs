using Fin.Application.DTOs;
using Fin.Domain.Models;

namespace Fin.Application.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<bool> CriarUsuarioAsync(Usuario usuario);
        Task<UsuarioDTO> BuscarUsuarioPorIdAsync(string id);
    }
}
