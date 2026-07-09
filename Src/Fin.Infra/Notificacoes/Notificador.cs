using Fin.Application.Notificacoes;
using Fin.Domain.Models;

namespace Fin.Infra.Notificacoes
{
    public class Notificador : INotificador
    {
        private List<Notificacao> _notificacoes;

        public Notificador()
        {
            _notificacoes = new List<Notificacao>();
        }

        private void Handle(Notificacao notificacao)
        => _notificacoes.Add(notificacao);

        public void Handle(string erro)
            => _notificacoes.Add(new Notificacao(erro));

        public T? Handle<T>(string notificacao)
        {
            Handle(new Notificacao(notificacao));
            return default(T?);
        }

        public List<Notificacao> ObterNotificacoes() => _notificacoes;

        public bool TemNotificacao() =>
            _notificacoes.Any();
    }
}
