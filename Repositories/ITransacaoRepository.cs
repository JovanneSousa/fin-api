using fin_api.Models;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Repositories
{
    public interface ITransacaoRepository
    {
        public Task<Transacao> GetByIdAsync(string id, string userId);
        public Task<List<Transacao>> GetByParentTransactionId(string parentTrancationId);
        public Task<IEnumerable<Transacao>> GetAllAsync(string userId);
        Task<IEnumerable<Transacao>> GetByPeriodAsync(string userId, DateTime startDate, DateTime endDate);
        public Task<bool> AddAsync(Transacao transaction);
        public Task<bool> AddRangeAsync(List<Transacao> transactions);
        public Task<bool> UpdateAsync(Transacao transaction);
        public Task<bool> DeleteAsync(Transacao transacao);
        public Task<bool> RemoveRangeAsync(List<Transacao> transacao);
        Task<decimal> GetTotalReceitaAsync(string userId);
        Task<decimal> GetTotalDespesaAsync(string userId, DateTime dataLimiteUtc);
    }
}
