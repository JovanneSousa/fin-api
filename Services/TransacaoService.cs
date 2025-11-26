using fin_api.Enums;
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
            transacao.CreatedAt = DateTime.UtcNow;
            if (transacao.Type == TransacaoType.Renda && transacao.IsRecurring)
            {
                transacao.RecorrenciaType = RecorrenciaType.Mensalmente;
                transacao.RecorrenciaEndDate = transacao.DataMovimentacao.AddMonths(11);
            }

            await _repository.AddAsync(transacao);

            if (transacao.Type == TransacaoType.Renda && transacao.IsRecurring)
                await GerarRecorrencias(transacao);

            if (transacao.Type == TransacaoType.Despesa && transacao.IsRecurring)
            {
                var parcelas = transacao.Parcelas ?? 1;

                if (parcelas > 1)
                    await GerarParcelas(transacao);
            }

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

        public async Task<IEnumerable<Transacao>> ListTransactionsByPeriodAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _repository.GetByPeriodAsync(userId, startDate, endDate);
        }

        private async Task GerarRecorrencias(Transacao origem)
        {
            if (origem.RecorrenciaEndDate == null || origem.RecorrenciaType == null) return;

            var data = origem.DataMovimentacao;

            while (true)
            {
                data = data.AddMonths(1);

                if (data > origem.RecorrenciaEndDate)
                    break;

                var nova = new Transacao
                {
                    UserId = origem.UserId,
                    Valor = origem.Valor,
                    Type = origem.Type,
                    Titulo = origem.Titulo,
                    CategoriaId = origem.CategoriaId,
                    IsRecurring = true,
                    RecorrenciaType = origem.RecorrenciaType,
                    ParentTransactionId = origem.Id,
                    DataMovimentacao = data
                };

                await _repository.AddAsync(nova);
            }
        }
        private async Task GerarParcelas(Transacao origem)
        {
            var parcelas = origem.Parcelas ?? 1;

            origem.ParcelaAtual = 1;
            await _repository.UpdateAsync(origem);

            for (int i = 2; i <= parcelas; i++)
            {
                var nova = new Transacao
                {
                    UserId = origem.UserId,
                    Valor = origem.Valor / parcelas,
                    Type = TransacaoType.Despesa,
                    Titulo = $"{origem.Titulo} ({i}/{parcelas})",
                    CategoriaId = origem.CategoriaId,
                    ParcelaAtual = i,
                    Parcelas = parcelas,
                    ParentTransactionId = origem.Id,
                    DataMovimentacao = origem.DataMovimentacao.AddMonths(i - 1)
                };

                await _repository.AddAsync(nova);
            }
        }

    }
}
