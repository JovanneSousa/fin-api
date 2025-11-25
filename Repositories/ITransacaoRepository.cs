using fin_api.Models;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Repositories
{
    public interface ITransacaoRepository
    {
        public Task<Transacao> GetByIdAsync(string id);

        public Task<IEnumerable<Transacao>> GetAllAsync(string userId);
        Task<IEnumerable<Transacao>> GetByPeriodAsync(string userId, DateTime startDate, DateTime endDate);
        public Task AddAsync(Transacao transaction);

        public Task UpdateAsync(Transacao transaction);

        public Task DeleteAsync(string id);
    }
}
