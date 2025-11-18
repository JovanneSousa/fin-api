using fin_api.Data;
using fin_api.Models;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Repositories
{
    public class TransacaoRepository : ITransacaoRepository
    {

        private readonly ApiDbContext _context;

        public TransacaoRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<Transacao> GetByIdAsync(string id)
            => await _context.Transactions.Include(t => t.Categoria).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Transacao>> GetAllAsync(string userId)
            => await _context.Transactions.Include(t => t.Categoria)
                                          .Where(t => t.UserId == userId)
                                          .ToListAsync();

        public async Task AddAsync(Transacao transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Transacao transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var transaction = await GetByIdAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

    }
}
