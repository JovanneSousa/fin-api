using Fin.Application.Notificacoes;
using Fin.Domain.Exceptions;

namespace Fin.Application.Services
{
    public abstract class BaseService
    {
        protected readonly INotificador _notificador;

        protected BaseService(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            } catch (DatabaseException ex)
            {
                return RetornaErroProcessamento<T>($"Erro no banco: {ex.Message}");
            }
        }

        protected async Task ExecuteAsync(Func<Task> action)
        {
            await ExecuteAsync(async () =>
            {
                await action();
                return true;
            });
        }

        protected T? RetornaSerieErrosProcessamento<T>(IEnumerable<string> erros)
        {
            foreach (var erro in erros)
                _notificador.Handle(erro);
            return default(T?);
        }

        protected T? RetornaErroProcessamento<T>(string erro)
            => _notificador.Handle<T>(erro);
    }
}
