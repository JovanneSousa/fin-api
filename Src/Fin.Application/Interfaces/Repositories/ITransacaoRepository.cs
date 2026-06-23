using Fin.Domain.Models;
using Fin.Application.DTOs;

namespace Fin.Application.Interfaces.Repositories
{
    public interface ITransacaoRepository
    {
        public Task<Transacao> GetByIdAsync(string id, string userId);
        public Task<List<Transacao>> GetByParentTransactionId(string parentTrancationId);
        Task<bool> TransactionsExistsByCategoryAsync(string userId, string categoryId);
        public Task<IEnumerable<Transacao>> GetAllAsync(string userId);
        Task<IEnumerable<Transacao>> GetByPeriodAsync(string userId, DateTime startDate, DateTime endDate);
        Task<List<SaldoMensalDTO>> GetValuesByMonth(string userId, DateTime dataInicial, DateTime dataFinal);
        public Task<bool> AddAsync(Transacao transaction);
        public Task<bool> AddRangeAsync(List<Transacao> transactions);
        public Task<bool> UpdateAsync(Transacao transaction);
        public Task<bool> DeleteAsync(Transacao transacao);
        public Task<bool> RemoveRangeAsync(List<Transacao> transacao);
        Task<decimal> GetSaldoTotal(string userId, DateTime date);
    }
}
