using Fin.Infra.DTOs;
using Fin.Domain.Models;

namespace Fin.Application.Services
{
    public interface ITransacaoService 
    {
        Task<decimal> GetSaldoTotalAsync(string userId);
        Task<IEnumerable<TransacaoDTO>> ListTransactionsAsync(string userId);
        Task<TransacaoDTO> GetTransactionAsync(string id, string userId);
        Task<IEnumerable<TransacaoDTO>> ListTransactionsByPeriodAsync(string userId, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<SaldoMensalDTO>> GetValuesByMonth(string userId, DateTime? startDate, DateTime? endDate);
        Task<Transacao> CreateTransactionAsync(Transacao transacao, string userId);
        Task<TransacaoDTO> UpdateTransactionAsync(string id, TransacaoDTO transacao, string userId);
        Task<bool> DeleteTransactionAsync(string id, string usuarioId);
    }
}
