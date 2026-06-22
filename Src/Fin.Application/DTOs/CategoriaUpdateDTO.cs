using Fin.Domain.Enums;

namespace Fin.Application.DTOs
{
    public class CategoriaUpdateDTO
    {
        public string? Name { get; set; }
        public TransacaoType? Type { get; set; }
        public string? IconId { get; set; }
        public string? CorId { get; set; }
    }
}
