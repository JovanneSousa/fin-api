using Fin.Application.DTOs;
using Fin.Application.Interfaces.Repositories;
using Fin.Domain.Models;
using Fin.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Fin.Infra.Repositories
{
    public class CategoriaRepository : BaseRepository, ICategoriaRepository
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

        public async Task<IEnumerable<CategoriaDTO>> GetAllAsync(string userId)
            => await ExecuteAsync(
                async () => await _context.Categories
                                .Where(c => (c.UserId == userId || c.IsDefault) &&  
                                    !_context.UserHiddenCategories
                                        .Any(h => h.UserId == userId && h.CategoryId == c.Id))
                                .Include(c => c.IconePadrao)
                                .Include(c => c.IconeCategoriaUsuario
                                    .Where(c => c.UserId == userId))
                                    .ThenInclude(c => c.Icone)
                                .Include(c => c.CorPadrao)
                                .Include(c => c.CorCategoriaUsuarios
                                            .Where(c => c.UserId == userId))
                                            .ThenInclude(c => c.Cor)
                                .Select(c => CategoriaDTO.ToDto(c))
                                .AsNoTracking()
                                .ToListAsync());

        public async Task<Categoria> AddAsync(Categoria categoria)
            => await ExecuteAsync(async () =>
            {
                _context.Categories.Add(categoria);
                await SaveChangesAsync();
                return categoria;
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

        public async Task<bool> IsCategoryHiddenAsync(string userId, string categoryId)
            => await ExecuteAsync(async () 
                => await _context.UserHiddenCategories
                    .AnyAsync(uhc => uhc.UserId == userId && uhc.CategoryId == categoryId));

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

        public async Task<IList<IconDTO>> GetAllIconsAsync()
            => await ExecuteAsync(
                async () => await _context.Icon
                                .Select(i => new IconDTO
                                {
                                    Url = i.Url,
                                    Name = i.Name,
                                    Id = i.Id
                                })
                                .OrderBy(i => i.Name)
                                .ToListAsync());

        public async Task<IList<CorDTO>> GetAllCorAsync()
            => await ExecuteAsync(
                async () => await _context.Cor
                                .Select(c => new CorDTO
                                {
                                    Id = c.Id,
                                    Url = c.Url,
                                })
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

        public async Task<Categoria> GetCategoryByNameAndUserIdAsync(string userId, string name)
            => await ExecuteAsync(async () =>
                await _context.Categories
                    .FirstOrDefaultAsync(c => 
                        (c.UserId == userId || c.IsDefault) &&
                        c.Name.ToLower() == name.ToLower()));
    }
}
