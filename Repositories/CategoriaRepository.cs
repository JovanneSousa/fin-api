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

        public async Task<Categoria> GetByIdAsync(string id)
            => await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Categoria>> GetAllAsync(string userId)
            => await _context.Categories.Where(c => c.UserId == userId || c.IsDefault).ToListAsync();

        public async Task AddAsync(Categoria categoria)
        {
            _context.Categories.Add(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Categoria categoria)
        {
            _context.Categories.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var categoria = await GetByIdAsync(id);
            if (categoria != null && categoria.IsDefault) await HiddenCategory(categoria);
            if (categoria != null && !categoria.IsDefault)
            {
                _context.Categories.Remove(categoria);
                await _context.SaveChangesAsync();
            }
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

        private async Task HiddenCategory(Categoria categoria)
        {
            var hiddenCategory = new UserHiddenCategory { UserId = categoria.UserId, CategoryId = categoria.Id };
            await _context.UserHiddenCategories.AddAsync(hiddenCategory);
            await _context.SaveChangesAsync();
        }
        public async Task ShowHiddenCategory(Categoria categoria)
        {
            var hiddenCategory = await _context.UserHiddenCategories
                .FirstOrDefaultAsync(uhc => uhc.UserId == categoria.UserId && uhc.CategoryId == categoria.Id);
            if (hiddenCategory != null)
            {
                _context.UserHiddenCategories.Remove(hiddenCategory);
                await _context.SaveChangesAsync();
            }
        }
    }
}
