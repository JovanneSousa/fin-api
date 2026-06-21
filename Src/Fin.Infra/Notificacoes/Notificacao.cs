namespace Fin.Infra.Notificacoes
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
