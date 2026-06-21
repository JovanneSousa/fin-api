using Fin.Infra.DTOs;
using Fin.Domain.Models;

namespace Fin.Application.Services
{
    public interface IUsuarioService
    {
        Task<bool> CriarUsuarioAsync(Usuario usuario);
        Task<UsuarioDTO> BuscarUsuarioPorIdAsync(string id);
    }
}
