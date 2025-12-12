using fin_api.Models;

namespace fin_api.Services
{
    public interface ITransacaoService
    {
        Task<IEnumerable<Transacao>> ListTransactionsAsync(string userId);
        Task<IEnumerable<Transacao>> ListTransactionsByPeriodAsync(string userId, DateTime? startDate, DateTime? endDate);
        Task<Transacao> GetTransactionAsync(string id, string userId);
        Task<Transacao> CreateTransactionAsync(Transacao transacao, string userId);
        Task<Transacao> UpdateTransactionAsync(string id, Transacao transacao, string userId);
        Task<bool> DeleteTransactionAsync(string id, string usuarioId);
    }
}
