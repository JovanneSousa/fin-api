namespace Fin.Domain.Models
{
    public class Notificacao
    {
        public string Mensagem { get; }
        public Notificacao(string mensagem)
        {
            Mensagem = mensagem;
        }
    }
}
