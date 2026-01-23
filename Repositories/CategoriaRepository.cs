using fin_api.Data;
using fin_api.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace fin_api.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {

        private readonly ApiDbContext _context;

        public CategoriaRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<List<IconeCategoriaUsuario>> GetIconsByUsuarioAsync(string id)
            => await _context.CategoriaUsuarios
                .Where(c => c.UserId == id)
                .ToListAsync();


        public async Task<Categoria> GetByIdAsync(string id, string userId)
            => await _context.Categories
                .Include(c => c.Icone)
                .Include(c => c.IconeCategoriaUsuario
                    .Where(c => c.UserId == userId))
                    .ThenInclude(c => c.Icone)
                .Include(c => c.Cor)
                .Include(c => c.CorCategoriaUsuarios
                    .Where(c => c.UserId == userId))
                    .ThenInclude(c => c.Cor)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Categoria>> GetAllAsync(string userId)
            => await _context.Categories
                        .Where(c => c.UserId == userId || c.IsDefault)
                        .Include(c => c.Icone)
                        .Include(c => c.IconeCategoriaUsuario
                            .Where(c => c.UserId == userId))
                            .ThenInclude(c => c.Icone)
                        .Include(c => c.Cor)
                        .Include(c => c.CorCategoriaUsuarios
                                    .Where(c => c.UserId == userId))
                                    .ThenInclude(c => c.Cor)
                        .ToListAsync();

        public async Task<bool> AddAsync(Categoria categoria)
        {
            _context.Categories.Add(categoria);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(Categoria categoria)
        {
            _context.Categories.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Categoria categoria)
        {
                _context.Categories.Remove(categoria);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string userId, string name)
        {
            return await _context.Categories
                .AnyAsync(c => (c.UserId == userId || c.IsDefault ) && c.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> IsCategoryHiddenAsync(string userId, string name)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.IsDefault);

            if (category == null || !category.IsDefault) return false;

            return await _context.UserHiddenCategories
                .AnyAsync(uhc => uhc.UserId == userId && uhc.CategoryId == category.Id);
        }

        public async Task<bool> HiddenCategory(string userId, string categoriaId)
        {
            var hiddenCategory = new UserHiddenCategory { UserId = userId, CategoryId = categoriaId };
            await _context.UserHiddenCategories.AddAsync(hiddenCategory);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task ShowHiddenCategory(string userId, Categoria categoria)
        {
            var hiddenCategory = await _context.UserHiddenCategories
                .FirstOrDefaultAsync(uhc => uhc.UserId == userId && uhc.CategoryId == categoria.Id);
            if (hiddenCategory != null)
            {
                _context.UserHiddenCategories.Remove(hiddenCategory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Categoria>> ListCategoriesHiddenAsync(string userId)
        {
            var result = await _context.Categories
                .Where(c => c.IsDefault == true)
                .Where(c => _context.UserHiddenCategories
                    .Where(h => h.UserId == userId)
                    .Select(h => h.CategoryId)
                    .Contains(c.Id))
                .ToListAsync();


            return result;
        }

        public async Task<List<Icon>> GetAllIconsAsync()
            => await _context.Icon
                .OrderBy(i => i.Name)
                .ToListAsync();

        public async Task<List<Cor>> GetAllCorAsync()
            => await _context.Cor
                .ToListAsync();
    }
}
