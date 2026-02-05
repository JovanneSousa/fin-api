using fin_api.Enums;

namespace fin_api.DTOs
{
    public class CategoriaUpdateDTO
    {
        public string? Name { get; set; }
        public TransacaoType? Type { get; set; }
        public string? IconId { get; set; }
        public string? CorId { get; set; }
    }
}
