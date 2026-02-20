using fin_api.Data;
using fin_api.Models;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Repositories
{
    public class CategoriaRepository : BasicRepository, ICategoriaRepository
    {
        public CategoriaRepository(ApiDbContext context) : base(context)
        {
        }

        public async Task<Categoria> GetByIdAsync(string id, string userId)
            => await ExecuteAsync(async () =>
                    await _context.Categories
                        .Where(c => c.Id == id)
                        .Include(c => c.IconePadrao)
                        .Include(c => c.IconeCategoriaUsuario
                            .Where(c => c.UserId == userId))
                            .ThenInclude(c => c.Icone)
                        .Include(c => c.CorPadrao)
                            .Include(c => c.CorCategoriaUsuarios
                                .Where(c => c.UserId == userId))
                                .ThenInclude(c => c.Cor)
                        .FirstOrDefaultAsync());

        public async Task<IEnumerable<Categoria>> GetAllAsync(string userId)
            => await ExecuteAsync(
                async () => await _context.Categories
                                .Where(c => c.UserId == userId || c.IsDefault)
                                .Include(c => c.IconePadrao)
                                .Include(c => c.IconeCategoriaUsuario
                                    .Where(c => c.UserId == userId))
                                    .ThenInclude(c => c.Icone)
                                .Include(c => c.CorPadrao)
                                .Include(c => c.CorCategoriaUsuarios
                                            .Where(c => c.UserId == userId))
                                            .ThenInclude(c => c.Cor)
                                .AsNoTracking()
                                .ToListAsync());

        public async Task<bool> AddAsync(Categoria categoria)
            => await ExecuteAsync(async () =>
            {
                _context.Categories.Add(categoria);
                await SaveChangesAsync();
                return true;
            });

        public async Task<bool> UpdateAsync(Categoria categoria)
            => await ExecuteAsync(async () =>
            {
                _context.Categories.Update(categoria);
                await SaveChangesAsync();
                return true;
            });

        public async Task<bool> DeleteAsync(Categoria categoria)
            => await ExecuteAsync(async () =>
            {
                _context.Categories.Remove(categoria);
                await SaveChangesAsync();
                return true;
            });

        public async Task<bool> ExistsAsync(string userId, string name)
            => await ExecuteAsync(
                async () => await _context.Categories
                                    .AnyAsync(c => (c.UserId == userId || c.IsDefault) && c.Name.ToLower() == name.ToLower()));

        public async Task<bool> IsCategoryHiddenAsync(string userId, string name)
            => await ExecuteAsync(async () =>
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.IsDefault);

                if (category == null || !category.IsDefault) return false;

                return await _context.UserHiddenCategories
                    .AnyAsync(uhc => uhc.UserId == userId && uhc.CategoryId == category.Id);
            });

        public async Task<bool> HiddenCategory(string userId, string categoriaId)
            => await ExecuteAsync(async () =>
            {
                var hiddenCategory = new UserHiddenCategory { UserId = userId, CategoryId = categoriaId };
                await _context.UserHiddenCategories.AddAsync(hiddenCategory);
                await SaveChangesAsync();
                return true;
            });


        public async Task ShowHiddenCategory(string userId, Categoria categoria)
            => await ExecuteAsync(async () =>
            {
                var hiddenCategory = await _context.UserHiddenCategories
                    .FirstOrDefaultAsync(uhc => uhc.UserId == userId && uhc.CategoryId == categoria.Id);
                if (hiddenCategory != null)
                {
                    _context.UserHiddenCategories.Remove(hiddenCategory);
                    await SaveChangesAsync();
                }
            });

        public async Task<List<Categoria>> ListCategoriesHiddenAsync(string userId)
            => await ExecuteAsync(
                async () => await _context.Categories
                                .Where(c => c.IsDefault == true)
                                .Where(c => _context.UserHiddenCategories
                                    .Where(h => h.UserId == userId)
                                    .Select(h => h.CategoryId)
                                    .Contains(c.Id))
                                .ToListAsync());

        public async Task<List<Icon>> GetAllIconsAsync()
            => await ExecuteAsync(
                async () => await _context.Icon
                                .OrderBy(i => i.Name)
                                .ToListAsync());

        public async Task<List<Cor>> GetAllCorAsync()
            => await ExecuteAsync(
                async () => await _context.Cor
                                .ToListAsync());

        public async Task<IconeCategoriaUsuario> GetIconsByUsuarioAsync(string usuarioId, string categoriaId)
            => await ExecuteAsync(
                async () => await _context.IconeCategoriaUsuarios
                                    .FirstOrDefaultAsync(c => c.UserId == usuarioId && c.CategoriaId == categoriaId));

        public async Task<bool> DeleteIconCategoriaUsuario(IconeCategoriaUsuario iconeCategoriaUsuario)
            => await ExecuteAsync(async () =>
            {
                _context.Remove(iconeCategoriaUsuario);
                await SaveChangesAsync();
                return true;
            });

        public async Task<bool> SalvaIconePersonalizado(IconeCategoriaUsuario iconeCategoriaUsuario)
            => await ExecuteAsync(async () =>
            {
                await _context.IconeCategoriaUsuarios.AddAsync(iconeCategoriaUsuario);
                await SaveChangesAsync();
                return true;
            });

        public async Task<CorCategoriaUsuario> GetCorByUsuarioAsync(string usuarioId, string categoriaId)
            => await ExecuteAsync(
                async () => await _context.CorCategoriaUsuarios
                                    .FirstOrDefaultAsync(c => c.UserId == usuarioId && c.CategoriaId == categoriaId));

        public async Task<bool> DeleteCorPersonalizadaAsync(CorCategoriaUsuario corCategoriaUsuario)
            => await ExecuteAsync(async () =>
            {
                _context.Remove(corCategoriaUsuario);
                await SaveChangesAsync();
                return true;
            });

        public async Task<bool> SalvaCorPersonalizadaAsync(CorCategoriaUsuario corCategoriaUsuario)
            => await ExecuteAsync(async () =>
            {
                await _context.CorCategoriaUsuarios.AddAsync(corCategoriaUsuario);
                await SaveChangesAsync();
                return true;
            });
    }
}
