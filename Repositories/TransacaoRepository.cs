using fin_api.Data;
using fin_api.DTOs;
using fin_api.Enums;
using fin_api.Models;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Repositories
{
    public class TransacaoRepository : BaseRepository, ITransacaoRepository
    {
        public TransacaoRepository(ApiDbContext context) : base(context)
        {
        }

        public async Task<decimal> GetSaldoTotal(string userId, DateTime date)
            => await ExecuteAsync(async () =>
                 await _context.Transactions
                    .Where(t => t.UserId == userId && t.DataMovimentacao <= date)
                    .SumAsync(t => t.Type == TransacaoType.Renda ? (decimal?)t.Valor ?? 0 : -(decimal?)t.Valor ?? 0)
            );

        public async Task<List<SaldoMensalDTO>> GetValuesByMonth(
            string userId, 
            DateTime dataInicial, 
            DateTime dataFinal
            )
            => await ExecuteAsync(async ()
                 =>  await _context.Transactions
                    .Where(t => t.UserId == userId &&
                            t.DataMovimentacao >= dataInicial &&
                            t.DataMovimentacao <= dataFinal)
                    .GroupBy(t => new {t.DataMovimentacao.Year, t.DataMovimentacao.Month})
                    .Select(g => new SaldoMensalDTO
                    {
                        Mes = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Receita = g.Where(t => t.Type == TransacaoType.Renda).Sum(t => t.Valor),
                        Despesa = g.Where(t => t.Type == TransacaoType.Despesa).Sum(t => t.Valor)
                    })
                    .OrderBy(x => x.Mes)
                    .ToListAsync());

        public async Task<Transacao> GetByIdAsync(string id, string userId)
            => await ExecuteAsync(
                async () => await _context.Transactions
                    .Include(t => t.Categoria)
                        .ThenInclude(t => t.IconePadrao)
                    .Include(t => t.Categoria)
                        .ThenInclude(t => t.CorPadrao)
                    .Include(t => t.Categoria)
                        .ThenInclude(t => t.IconeCategoriaUsuario
                            .Where(c => c.UserId == userId))
                            .ThenInclude(c => c.Icone)
                    .Include(t => t.Categoria)
                        .ThenInclude(c => c.CorCategoriaUsuarios
                            .Where(c => c.UserId == userId))
                            .ThenInclude(c => c.Cor)
                    .FirstOrDefaultAsync(t => t.Id == id));

        public async Task<IEnumerable<Transacao>> GetAllAsync(string userId)
            => await ExecuteAsync(
                async () => await _context.Transactions
                    .Include(t => t.Categoria)
                    .Where(t => t.UserId == userId)
                    .ToListAsync());

        public async Task<bool> AddAsync(Transacao transaction)
            => await ExecuteAsync(async () =>
            {
                _context.Transactions.Add(transaction);
                await SaveChangesAsync();
                return true;
            });

        public async Task<bool> UpdateAsync(Transacao transaction)
            => await ExecuteAsync(
                async () =>
                {
                    _context.Transactions.Update(transaction);
                    await SaveChangesAsync();
                    return true;
                });

        public async Task<bool> DeleteAsync(Transacao transacao)
            => await ExecuteAsync(async () =>
            {
                _context.Transactions.Remove(transacao);
                await SaveChangesAsync();
                return true;
            });

        public async Task<IEnumerable<Transacao>> GetByPeriodAsync(string userId, DateTime startDate, DateTime endDate)
            => await ExecuteAsync(async () =>
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
            });

        public async Task<bool> UpdateRangeAsync(List<Transacao> transactions)
            => await ExecuteAsync(async () =>
            {
                _context.UpdateRange(transactions);
                await SaveChangesAsync();
                return true;
            });

        public async Task<List<Transacao>> GetByParentTransactionId(string parentTrancationId)
            => await ExecuteAsync(
                async () => await _context.Transactions
                            .Where(t => t.ParentTransactionId == parentTrancationId)
                            .ToListAsync());

        public async Task<bool> RemoveRangeAsync(List<Transacao> transacao)
            => await ExecuteAsync(async () =>
            {
                _context.Transactions.RemoveRange(transacao);
                await SaveChangesAsync();
                return true;
            });

        public async Task<bool> AddRangeAsync(List<Transacao> transactions)
            => await ExecuteAsync(async () =>
            {
                await _context.AddRangeAsync(transactions);
                await SaveChangesAsync();
                return true;
            });
    }
}
