using fin_api.Models;

namespace fin_api.Services
{
    public interface ITransacaoService
    {
        Task<IEnumerable<Transacao>> ListTransactionsAsync(string userId);
        Task<Transacao> GetTransactionAsync(string id);
        Task<Transacao> CreateTransactionAsync(Transacao transacao);
        Task<Transacao> UpdateTransactionAsync(Transacao transacao);
        Task<bool> DeleteTransactionAsync(string id);
    }
}
