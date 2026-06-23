using Fin.Domain.Models;

namespace Fin.Application.Notificacoes
{
    public interface INotificador
    {
        bool TemNotificacao();
        List<Notificacao> ObterNotificacoes();
        void Handle(string erro);
        T? Handle<T>(string notificacao);
    }
}
