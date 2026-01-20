namespace fin_api.Models;

public class Usuario : Entity
{
    public string Nome { get; set; }
    public ICollection<Transacao> transacaos {  get; set; }
}
