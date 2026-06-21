using Fin.Domain.Models;
using Fin.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Fin.Infra.Repositories
{
    public class UsuarioRepository : BaseRepository, IUsuarioRepository
    {
        public UsuarioRepository(ApiDbContext context) : base(context)
        {
        }

        public async Task<bool> CreateUsuarioAsync(Usuario usuario)
            => await ExecuteAsync(async () =>
            {
                await _context.Usuarios.AddAsync(usuario);
                await _context.SaveChangesAsync();
                return true;
            });

        public async Task<Usuario> GetUsuarioByIdAsync(string id)
            => await ExecuteAsync(
                async () => await _context.Usuarios
                                .Where(u => u.Id == id)
                                .FirstOrDefaultAsync());

        public async Task<List<Usuario>> GetUsuariosAsync()
             => await ExecuteAsync(
                async () => await _context.Usuarios
                                .AsNoTracking()
                                .ToListAsync());

    }
}
