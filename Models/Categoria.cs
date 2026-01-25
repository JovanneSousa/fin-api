using fin_api.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace fin_api.Models
{
    public class Categoria : Entity
    {
        public string Name { get; set; }
        public TransacaoType Type { get; set; }
        public string UserId { get; set; }
        public bool IsDefault { get; set; } = false;

        [JsonIgnore]
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public string IconId { get; set; }
        public Icon IconePadrao { get; set; }
        [NotMapped]
        public ICollection<IconeCategoriaUsuario> IconeCategoriaUsuario { get; set; }
            = new List<IconeCategoriaUsuario>();
        public Icon Icone => IconeCategoriaUsuario.FirstOrDefault()?.Icone ?? IconePadrao;

        public string CorId { get; set; }
        public Cor CorPadrao { get; set; }
        [NotMapped]
        public ICollection<CorCategoriaUsuario> CorCategoriaUsuarios { get; set; } 
            = new List<CorCategoriaUsuario>();
        public Cor Cor => CorCategoriaUsuarios.FirstOrDefault()?.Cor ?? CorPadrao;
    }
}
