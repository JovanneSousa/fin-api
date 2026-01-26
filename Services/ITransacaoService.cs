using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Services
{
    public interface ITransacaoService
    {
        Task<IEnumerable<TransacaoDTO>> ListTransactionsAsync(string userId);
        Task<IEnumerable<TransacaoDTO>> ListTransactionsByPeriodAsync(string userId, DateTime? startDate, DateTime? endDate);
        Task<TransacaoDTO> GetTransactionAsync(string id, string userId);
        Task<Transacao> CreateTransactionAsync(Transacao transacao, string userId);
        Task<TransacaoDTO> UpdateTransactionAsync(string id, TransacaoDTO transacao, string userId);
        Task<bool> DeleteTransactionAsync(string id, string usuarioId);
        Task<decimal> GetSaldoTotalAsync(string userId);
    }
}
