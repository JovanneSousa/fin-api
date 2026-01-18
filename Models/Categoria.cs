using fin_api.Enums;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace fin_api.Models
{
    public class Categoria : Entity
    {
        public string Name { get; set; }
        public TransacaoType Type { get; set; }
        public string UserId { get; set; }
        public bool IsDefault { get; set; }
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public string DefaultIconId { get; set; }
        public Icon DefaultIcon { get; set; }
    }
}
