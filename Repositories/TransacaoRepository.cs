using fin_api.Data;
using fin_api.Models;
using fin_api.Enums;
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
            => await _context.Transactions
                    .Include(t => t.Categoria)
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

        public async Task<bool> AddAsync(Transacao transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalReceitaAsync(string userId)
            => await _context.Transactions
                .Where(t => 
                    t.UserId == userId && 
                    t.Type == TransacaoType.Renda &&
                    t.DataMovimentacao <= DateTime.UtcNow)
                .SumAsync(t => (decimal?)t.Valor) ?? 0;

        public async Task<decimal> GetTotalDespesaAsync(string userId, DateTime dataLimiteUtc)
            => await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransacaoType.Despesa &&
                    t.DataMovimentacao < dataLimiteUtc)
                .SumAsync(t => (decimal?)t.Valor) ?? 0;

        public async Task<bool> UpdateAsync(Transacao transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Transacao transacao)
        {
            _context.Transactions.Remove(transacao);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Transacao>> GetByPeriodAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId 
                            && t.DataMovimentacao >= startDate 
                            && t.DataMovimentacao <= endDate)
                    .AsNoTracking()
                    .OrderByDescending(t => t.DataMovimentacao)
                    .Include(t => t.Categoria)
                        .ThenInclude(c => c.IconePadrao)
                    .Include(t => t.Categoria)
                        .ThenInclude(c => c.IconeCategoriaUsuario
                        .Where(c => c.UserId == userId))
                        .ThenInclude(c => c.Icone)
                    .Include(c => c.Categoria)
                        .ThenInclude(c => c.CorPadrao)
                    .Include(t => t.Categoria)
                        .ThenInclude(c => c.CorCategoriaUsuarios
                        .Where(c => c.UserId == userId))
                        .ThenInclude(c => c.Cor)
                    .ToListAsync();
        }

        public async Task<bool> UpdateRangeAsync(List<Transacao> transactions)
        {
            _context.UpdateRange(transactions);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Transacao>> GetByParentTransactionId(string parentTrancationId) =>
            await _context.Transactions.Where(t => t.ParentTransactionId == parentTrancationId).ToListAsync();

        public async Task<bool> RemoveRangeAsync(List<Transacao> transacao)
        {
            _context.Transactions.RemoveRange(transacao);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddRangeAsync(List<Transacao> transactions)
        {
            await _context.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
