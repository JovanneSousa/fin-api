using fin_api.Enums;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace fin_api.Models
{
    public class Categoria
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public TransacaoType Type { get; set; }
        public string UserId { get; set; }
        public bool IsDefault { get; set; }
        [JsonIgnore]
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
