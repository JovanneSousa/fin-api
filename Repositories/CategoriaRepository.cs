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
            => await _context.Categories.Where(c => c.UserId == userId).ToListAsync();

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
            if (categoria != null)
            {
                _context.Categories.Remove(categoria);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string userId, string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());
        }


    }
}
