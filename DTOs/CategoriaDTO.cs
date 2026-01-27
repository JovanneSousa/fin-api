using fin_api.Enums;
using fin_api.Models;
using System.Text.Json.Serialization;

namespace fin_api.DTOs
{
    public class CategoriaDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TransacaoType Type { get; set; }
        public string UserId { get; set; }
        public string IconId { get; set; }
        public IconDTO Icone { get; set; }
        public string CorId { get; set; }
        public CorDTO Cor { get; set; }
    }
}
