using AutoMapper;
using Fin.Application.Interfaces.Repositories;
using Fin.Application.Interfaces.Services;
using Fin.Domain.Enums;
using Fin.Domain.Models;
using Fin.Application.DTOs;
using Fin.Application.http.RequestDTO;
using Fin.Application.Notificacoes;

namespace Fin.Application.Services
{
    public class TransactionService : BaseService, ITransacaoService
    {
        private readonly ITransacaoRepository _repository;
        private readonly IMapper _mapper;

        public TransactionService(
            ITransacaoRepository repository, 
            INotificador notificador, 
            IMapper mapper) 
            : base(notificador)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<decimal> GetSaldoTotalAsync(string userId)
        {
            var now = DateTime.UtcNow;

            var inicioProximoMes = new DateTime(
                now.Year,
                now.Month,
                1
            ).AddMonths(1).ToUniversalTime();

            return await ExecuteAsync(
                async() => await _repository.GetSaldoTotal(userId, inicioProximoMes));
        }

        public async Task<IEnumerable<TransacaoDTO>> ListTransactionsAsync(string userId)
            => _mapper.Map<IEnumerable<TransacaoDTO>>(
                await ExecuteAsync(
                    async () => await _repository.GetAllAsync(userId)));

        public async Task<TransacaoDTO> GetTransactionAsync(string id, string userId)
        {
            var transacao = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(id, userId)
                );

            if(transacao == null)
                return RetornaErroProcessamento<TransacaoDTO>("Transação não existe!");

            if (transacao.UserId != userId) 
                return RetornaErroProcessamento<TransacaoDTO>("Você não tem permissão para ver essa Transação!");

            return _mapper.Map<TransacaoDTO>(transacao);
        }

        public async Task<TransacaoDTO> CreateTransactionAsync(TransactionRequest transactionRequest, string userId)
        {
            var transacao = transactionRequest.ToDomain();
            transacao.DataMovimentacao =
                DateTime.SpecifyKind(transacao.DataMovimentacao, DateTimeKind.Utc);
            transacao.UserId = userId;
            transacao.CreatedAt = DateTime.UtcNow;
            if (transacao.Type == TransacaoType.Renda && transacao.IsRecurring)
            {
                transacao.RecorrenciaType = RecorrenciaType.Mensalmente;
                transacao.RecorrenciaEndDate = transacao.DataMovimentacao.AddMonths(11);
            }

            if(!await ExecuteAsync(async () => await _repository.AddAsync(transacao)))
                return RetornaErroProcessamento<TransacaoDTO>("Falha ao salvar transação!");

            if (transacao.Type == TransacaoType.Renda && transacao.IsRecurring)
                if (!await GerarRecorrencias(transacao)) return null;

            if (transacao.Type == TransacaoType.Despesa && transacao.IsRecurring)
                    if(!await GerarParcelas(transacao)) return null;

            return _mapper.Map<TransacaoDTO>(transacao);
        }


        public async Task<TransacaoDTO> UpdateTransactionAsync(string id, TransacaoDTO transacaoDTO, string userId)
        { 
            var existente = await ExecuteAsync(async () => await _repository.GetByIdAsync(id, userId));
            if (existente == null)
                return RetornaErroProcessamento<TransacaoDTO>("Transação não encontrada!");

            if(existente.UserId != userId)
                return RetornaErroProcessamento<TransacaoDTO>("Você não tem permissão para atualizar essa transação!");

            transacaoDTO.Id = existente.Id;
            transacaoDTO.UserId = existente.UserId;
            transacaoDTO.Type = existente.Type;


            // Se não vai ser mais recorrente
            if (existente.IsRecurring && !transacaoDTO.IsRecurring)
                return await TransformaRecorrenciaEmUnica(existente, transacaoDTO);

            // Se vai ser recorrente
            if (!existente.IsRecurring && transacaoDTO.IsRecurring)
                return await TransformaEmRecorrente(existente, transacaoDTO);

            // Atualização de recorrencia
            if (existente.IsRecurring && transacaoDTO.IsRecurring)
            {
                if (!await AtualizarRecorrencia(existente, transacaoDTO))
                    return null;

                return _mapper.Map<TransacaoDTO>(existente);
            }

            existente.Titulo = transacaoDTO.Titulo;
            existente.Valor = transacaoDTO.Valor;
            existente.CategoriaId = transacaoDTO.CategoriaId;
            existente.DataMovimentacao = transacaoDTO.DataMovimentacao.ToUniversalTime();

            // Atualiza somente dados basicos
            if(!await ExecuteAsync(async () => await _repository.UpdateAsync(existente)))
                return RetornaErroProcessamento<TransacaoDTO>("Falha ao atualizar transação!");

            return _mapper.Map<TransacaoDTO>(existente);
        }
        public async Task<bool> DeleteTransactionAsync(string id, string usuarioId)
        {

            var existing = await ExecuteAsync(
                async () => await _repository.GetByIdAsync(id, usuarioId));
            if (existing == null) 
                return RetornaErroProcessamento<bool>("Transação não encontrada!");

            if(existing.UserId != usuarioId)
                return RetornaErroProcessamento<bool>("Você não tem permissão para excluir essa transação!");

            if (existing.IsRecurring)
                if (!await RemoverRecorrencias(existing.Id))
                    return RetornaErroProcessamento<bool>("Houve uma falha ao excluir as recorrências!");

            if(!await ExecuteAsync(async () => await _repository.DeleteAsync(existing)))
                return RetornaErroProcessamento<bool>("Houve um problema ao excluir a transação!");

            return true;
        }

        public async Task<IEnumerable<TransacaoDTO>> ListTransactionsByPeriodAsync(string userId, DateTime? startDate, DateTime? endDate)
        {
            if(startDate is null || endDate is null)
                return RetornaErroProcessamento<IEnumerable<TransacaoDTO>>("As datas iniciais e finais são obrigatórias!");

            var start = startDate.Value;
            var end = endDate.Value;

            if(start > end)
                return RetornaErroProcessamento<IEnumerable<TransacaoDTO>>("A data de fim deve ser maior que a data de início!");

            var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(start.Date);
            var fimUtc = TimeZoneInfo.ConvertTimeToUtc(end.Date.AddDays(1).AddTicks(-1));
            var transacoes = await ExecuteAsync(
                async () => await _repository.GetByPeriodAsync(userId, inicioUtc, fimUtc)
                );

            var result = _mapper.Map<IEnumerable<TransacaoDTO>>(transacoes);
            if(result is null)
                return RetornaErroProcessamento<IEnumerable<TransacaoDTO>>("Houve um problema ao buscar as transações!");

            return result;
        }

        public async Task<IEnumerable<SaldoMensalDTO>> GetValuesByMonth(string userId, DateTime? startDate, DateTime? endDate)
        {
            if (startDate is null || endDate is null)
                return RetornaErroProcessamento<IEnumerable<SaldoMensalDTO>>("As datas iniciais e finais são obrigatórias!");

            var start = startDate.Value;
            var end = endDate.Value;

            if (start > end)
                return RetornaErroProcessamento<IEnumerable<SaldoMensalDTO>>("A data de fim deve ser maior que a data de início!");

            var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(start.Date);
            var fimUtc = TimeZoneInfo.ConvertTimeToUtc(end.Date.AddDays(1).AddTicks(-1));
            
            return await ExecuteAsync(
                async () => await _repository.GetValuesByMonth(userId, inicioUtc, fimUtc)
                );
        }

        private async Task<TransacaoDTO> TransformaEmRecorrente(Transacao existente, TransacaoDTO transacaoDTO)
        {
            existente.IsRecurring = true;
            existente.RecorrenciaType = RecorrenciaType.Mensalmente;
            existente.RecorrenciaEndDate = transacaoDTO.DataMovimentacao.AddMonths(12);

            existente.Titulo = transacaoDTO.Titulo;
            existente.Valor = transacaoDTO.Valor;
            existente.CategoriaId = transacaoDTO.CategoriaId;
            existente.DataMovimentacao = transacaoDTO.DataMovimentacao;

            if (!await ExecuteAsync(async () => await _repository.UpdateAsync(existente)))
                return RetornaErroProcessamento<TransacaoDTO>("Houve um erro ao atualizar a transação!");

            if (existente.Type == TransacaoType.Renda)
                if (!await GerarRecorrencias(existente)) return null;

            if (existente.Type == TransacaoType.Despesa)
                if (!await GerarParcelas(existente)) return null;

            return _mapper.Map<TransacaoDTO>(existente);
        }


        private async Task<TransacaoDTO> TransformaRecorrenciaEmUnica(Transacao existente, TransacaoDTO transacaoDTO)
        {
            if (!await RemoverRecorrencias(existente.Id))
                return RetornaErroProcessamento<TransacaoDTO>("Houve um erro excluindo as parcelas/recorrências");

            existente.IsRecurring = false;
            existente.RecorrenciaType = null;
            existente.RecorrenciaEndDate = null;
            existente.ParentTransactionId = null;

            existente.Titulo = transacaoDTO.Titulo;
            existente.Valor = transacaoDTO.Valor;
            existente.CategoriaId = transacaoDTO.CategoriaId;
            existente.DataMovimentacao = transacaoDTO.DataMovimentacao;

            // Recalcula parcelas caso o numero minimo for de parcelas for atendido
            if (existente.Type == TransacaoType.Despesa && existente.Parcelas > 1)
                existente.Valor = Math.Round(existente.Valor * existente.Parcelas.Value, 2);

            if (!await ExecuteAsync(async () => await _repository.UpdateAsync(existente)))
                return RetornaErroProcessamento<TransacaoDTO>("Houve um erro ao atualizar a transação!");

            return _mapper.Map<TransacaoDTO>(existente);
        }

        private async Task<bool> GerarRecorrencias(Transacao origem)
        {
            if (origem.RecorrenciaEndDate == null || origem.RecorrenciaType == null)
                return RetornaErroProcessamento<bool>("A recorrência não pode ser nula");

            var transactions = GerarListaDeRecorrencia(origem);
            if (!transactions.Any())
                return RetornaErroProcessamento<bool>("Falha ao gerar recorrência");

            if (!await ExecuteAsync(async () => await _repository.AddRangeAsync(transactions)))
                return RetornaErroProcessamento<bool>("Houve um erro ao salvar as recorrências!");

            return true;
        }

        private async Task<bool> AtualizarRecorrencia(Transacao existente, TransacaoDTO novosDados)
        {
            existente.Titulo = novosDados.Titulo;
            existente.Valor = novosDados.Valor;
            existente.CategoriaId = novosDados.CategoriaId;
            existente.DataMovimentacao = novosDados.DataMovimentacao;
            existente.RecorrenciaType = RecorrenciaType.Mensalmente;

            if(!await ExecuteAsync(async () => await _repository.UpdateAsync(existente)))
                return RetornaErroProcessamento<bool>("Falha ao atualizar base!");

            if(!await RemoverRecorrencias(existente.Id))
                return RetornaErroProcessamento<bool>("Falha ao remover recorrências!");

            if (existente.Type == TransacaoType.Renda) 
            {
                existente.RecorrenciaEndDate = novosDados.DataMovimentacao.AddMonths(11);
                return await GerarRecorrencias(existente);
            }
            if (existente.Type == TransacaoType.Despesa)
            {
                existente.Parcelas = novosDados.Parcelas;
                return await GerarParcelas(existente);
            }
            return true;
        }

        private async Task<bool> RemoverRecorrencias(string parentId)
        {
            var filhos = await ExecuteAsync(async () => await _repository.GetByParentTransactionId(parentId));

            if (!filhos.Any())
                return true;

            return await ExecuteAsync(async () => await _repository.RemoveRangeAsync(filhos));
        }

        private async Task<bool> GerarParcelas(Transacao origem)
        {
            var parcelas = origem.Parcelas ?? 1;
            if (!origem.ParcelaValida()) 
                return RetornaErroProcessamento<bool>("Parcelas deve ser no mínimo 2");

            var valorParcela = Math.Round(origem.Valor / parcelas, 2);
            var nomeOriginal = origem.Titulo;

            origem.ParcelaAtual = 1;
            origem.Titulo = $"{nomeOriginal} (1/{parcelas})";
            origem.Valor = valorParcela;

            if(!await ExecuteAsync(async () => await _repository.UpdateAsync(origem)))
                return RetornaErroProcessamento<bool>("Houve um erro ao atualizar o a parcela base");

            var list = GeraListaDeParcelas(
                origem, 
                valorParcela, 
                parcelas, 
                nomeOriginal
                );

            if (!list.Any())
                return RetornaErroProcessamento<bool>("Falha ao gerar parcelas!");

            if(!await ExecuteAsync(async () => await _repository.AddRangeAsync(list)))
                return RetornaErroProcessamento<bool>("Houve um erro ao salvar as parcelas");

            return true;
        }

        private List<Transacao> GeraListaDeParcelas(
            Transacao origem, 
            Decimal valorParcela, 
            int parcelas, 
            string nomeOriginal)
        {
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

            return list;
        }


        private List<Transacao> GerarListaDeRecorrencia(Transacao origem)
        {
            var transacoes = new List<Transacao>();
            var data = origem.DataMovimentacao.AddMonths(1);

            while (data <= origem.RecorrenciaEndDate)
            {
                transacoes.Add(new Transacao
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

            return transacoes;
        }
    }
}
