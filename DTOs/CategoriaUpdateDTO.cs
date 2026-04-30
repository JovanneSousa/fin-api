using fin_api.Enums;

namespace fin_api.DTOs
{
    public class CategoriaUpdateDTO
    {
        public required string Name { get; set; }
        public required TransacaoType Type { get; set; }
        public required string IconId { get; set; }
        public required string CorId { get; set; }
    }
}
