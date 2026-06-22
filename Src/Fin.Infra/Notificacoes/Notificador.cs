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

        public void Handle(Notificacao notificacao) => _notificacoes.Add(notificacao);

        public List<Notificacao> ObterNotificacoes() => _notificacoes;

        public bool TemNotificacao() =>
            _notificacoes.Any();
    }
}
