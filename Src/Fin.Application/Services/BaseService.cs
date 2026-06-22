using Fin.Application.Notificacoes;
using Fin.Domain.Exceptions;
using Fin.Domain.Models;
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
                _notificador.Handle(new Notificacao($"Erro no banco: {ex.Message}"));
                return default;
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
    }
}
