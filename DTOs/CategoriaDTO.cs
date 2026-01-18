using fin_api.Enums;
using fin_api.Models;
using System.Text.Json.Serialization;

namespace fin_api.DTOs
{
    public class CategoriaDTO
    {
        public string Name { get; set; }
        public TransacaoType Type { get; set; }
        public string UserId { get; set; }
        public bool IsDefault { get; set; }
        public IconDTO DefaultIcon { get; set; }
    }
}
