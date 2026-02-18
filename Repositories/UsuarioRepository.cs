using fin_api.Data;
using fin_api.Models;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApiDbContext _dbContext;

        public UsuarioRepository(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<bool> CreateUsuarioAsync(Usuario usuario)
        {
            await _dbContext.Usuarios.AddAsync(usuario);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Usuario> GetUsuarioByIdAsync(string id)
            => await _dbContext.Usuarios
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();

        public async Task<List<Usuario>> GetUsuariosAsync()
            => await _dbContext.Usuarios
                .AsNoTracking()
                .ToListAsync();
    }
}
