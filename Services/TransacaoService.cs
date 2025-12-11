using fin_api.Enums;
using fin_api.Models;
using fin_api.Notificacoes;
using fin_api.Repositories;

namespace fin_api.Services
{
    public class TransactionService : ITransacaoService
    {
        private readonly ITransacaoRepository _repository;
        private readonly INotificador _notificador;

        public TransactionService(ITransacaoRepository repository, INotificador notificador)
        {
            _repository = repository;
            _notificador = notificador;
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
            var existente = await _repository.GetByIdAsync(transacao.Id);
            if (existente == null)
                throw new Exception("Transação não existe");

            if (existente.IsRecurring && !transacao.IsRecurring)
            {
                await RemoverRecorrencias(existente.Id);

                existente.IsRecurring = false;
                existente.RecorrenciaType = null;
                existente.RecorrenciaEndDate = null;
                existente.ParentTransactionId = null;

                existente.Titulo = transacao.Titulo;
                existente.Valor = transacao.Valor;
                existente.CategoriaId = transacao.CategoriaId;
                existente.DataMovimentacao = transacao.DataMovimentacao;

                if (existente.Type == TransacaoType.Despesa && existente.Parcelas > 1)
                    existente.Valor = Math.Round(existente.Valor * existente.Parcelas.Value, 2);

                await _repository.UpdateAsync(existente);
                return existente;
            }

            if (!existente.IsRecurring && transacao.IsRecurring)
            {
                existente.IsRecurring = true;
                existente.RecorrenciaType = RecorrenciaType.Mensalmente;
                existente.RecorrenciaEndDate = transacao.DataMovimentacao.AddMonths(12);

                existente.Titulo = transacao.Titulo;
                existente.Valor = transacao.Valor;
                existente.CategoriaId = transacao.CategoriaId;
                existente.DataMovimentacao = transacao.DataMovimentacao;

                await _repository.UpdateAsync(existente);

                if (existente.Type == TransacaoType.Renda)
                    await GerarRecorrencias(existente);

                if (existente.Type == TransacaoType.Despesa)
                    await GerarParcelas(existente);

                return existente;
            }

            if (existente.IsRecurring && transacao.IsRecurring)
            {
                await AtualizarRecorrencia(existente, transacao);
                return existente;
            }

            existente.Titulo = transacao.Titulo;
            existente.Valor = transacao.Valor;
            existente.CategoriaId = transacao.CategoriaId;
            existente.DataMovimentacao = transacao.DataMovimentacao;

            await _repository.UpdateAsync(existente);

            return existente;
        }



        public async Task<bool> DeleteTransactionAsync(string id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.IsRecurring) await RemoverRecorrencias(existing.Id);

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<Transacao>> ListTransactionsByPeriodAsync(string userId, DateTime startDate, DateTime endDate)
        {

            if(startDate > endDate)
            {
                _notificador.Handle(new Notificacao("A data de fim deve ser maior que a data de início!"));
            }

            var startLocal = startDate.Date;
            var endLocal = endDate.Date.AddDays(1).AddTicks(-1);

            var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal);
            var fimUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal);

            var result = await _repository.GetByPeriodAsync(userId, startDate, endDate);
            if(result == null)
            {
                _notificador.Handle(new Notificacao("Houve um problema ao buscar as transações!"));
                return null;
            }
            return result;
        }

        private async Task GerarRecorrencias(Transacao origem)
        {
            if (origem.RecorrenciaEndDate == null || origem.RecorrenciaType == null) throw new InvalidOperationException("A recorrência não pode ser nula");

            var data = origem.DataMovimentacao.AddMonths(1);
            var transactions = new List<Transacao>();

            while (data <= origem.RecorrenciaEndDate)
            {
               transactions.Add(new Transacao
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
                });

                data = data.AddMonths(1);
            }
            if (transactions.Count == 0) throw new InvalidOperationException("Falha ao gerar recorrências");
                
            await _repository.AddRangeAsync(transactions);
        }

        private async Task AtualizarRecorrencia(Transacao existente, Transacao novosDados)
        {
            existente.Titulo = novosDados.Titulo;
            existente.Valor = novosDados.Valor;
            existente.CategoriaId = novosDados.CategoriaId;
            existente.DataMovimentacao = novosDados.DataMovimentacao;
            existente.RecorrenciaType = RecorrenciaType.Mensalmente;

            await _repository.UpdateAsync(existente);

            await RemoverRecorrencias(existente.Id);

            if (existente.Type == TransacaoType.Renda) 
            {
                existente.RecorrenciaEndDate = novosDados.DataMovimentacao.AddMonths(11);
                await GerarRecorrencias(existente);
            }
            if (existente.Type == TransacaoType.Despesa)
            {
                existente.Parcelas = novosDados.Parcelas;
                await GerarParcelas(existente);
            }
        }

        private async Task RemoverRecorrencias(string parentId)
        {
            var filhos = await _repository.GetByParentTransactionId(parentId);
            if (filhos.Any())
            {
                await _repository.RemoveRangeAsync(filhos);
            }
        }

        private async Task GerarParcelas(Transacao origem)
        {
            var parcelas = origem.Parcelas ?? 1;

            if (parcelas < 2)
                throw new InvalidOperationException("Parcelas deve ser no mínimo 2");

            var valorParcela = Math.Round(origem.Valor / parcelas, 2);
            var nomeOriginal = origem.Titulo;

            origem.ParcelaAtual = 1;
            origem.Titulo = $"{nomeOriginal} (1/{parcelas})";
            origem.Valor = valorParcela;
            await _repository.UpdateAsync(origem);

            var list = new List<Transacao>();

            for (int i = 2; i <= parcelas; i++)
            {
                list.Add(new Transacao
                {
                    UserId = origem.UserId,
                    Valor = valorParcela,
                    Type = TransacaoType.Despesa,
                    Titulo = $"{nomeOriginal} ({i}/{parcelas})",
                    CategoriaId = origem.CategoriaId,
                    ParcelaAtual = i,
                    Parcelas = parcelas,
                    ParentTransactionId = origem.Id,
                    DataMovimentacao = origem.DataMovimentacao.AddMonths(i - 1)
                });
            }

            if (!list.Any())
                throw new InvalidOperationException("Falha ao gerar parcelas");

            await _repository.AddRangeAsync(list);
        }

    }
}
