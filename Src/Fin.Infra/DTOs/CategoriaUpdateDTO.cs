using Fin.Domain.Enums;

namespace Fin.Infra.DTOs
{
    public class CategoriaUpdateDTO
    {
        public string? Name { get; set; }
        public TransacaoType? Type { get; set; }
        public string? IconId { get; set; }
        public string? CorId { get; set; }
    }
}
