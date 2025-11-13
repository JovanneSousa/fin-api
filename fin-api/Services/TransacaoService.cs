using fin_api.Models;
using fin_api.Repositories;

namespace fin_api.Services
{
    public class TransactionService : ITransacaoService
    {
        private readonly ITransacaoRepository _repository;

        public TransactionService(ITransacaoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Transacao>> ListTransactionsAsync(string userId)
            => await _repository.GetAllAsync(userId);

        public async Task<Transacao> GetTransactionAsync(string id)
            => await _repository.GetByIdAsync(id);

        public async Task<Transacao> CreateTransactionAsync(Transacao transacao)
        {
            await _repository.AddAsync(transacao);
            return transacao;
        }

        public async Task<Transacao> UpdateTransactionAsync(Transacao transacao)
        {
            await _repository.UpdateAsync(transacao);
            return transacao;
        }

        public async Task<bool> DeleteTransactionAsync(string id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;
            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
