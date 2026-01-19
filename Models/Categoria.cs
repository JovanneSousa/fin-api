using fin_api.Enums;
using System.Text.Json.Serialization;

namespace fin_api.Models
{
    public class Categoria : Entity
    {
        public string Name { get; set; }
        public TransacaoType Type { get; set; }
        public string UserId { get; set; }
        public bool IsDefault { get; set; }

        [JsonIgnore]
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public string IconId { get; set; }
        public Icon Icone { get; set; }
        public ICollection<IconeCategoriaUsuario> IconeCategoriaUsuario { get; set; }
            = new List<IconeCategoriaUsuario>();
    }
}
