
using Fin.Domain.Models;

namespace Fin.Application.DTOs
{
    public class CorDTO 
    {
        public string Id { get; set; }
        public string Url {  get; set; }

        public static CorDTO ToDto(Cor cor)
            => new CorDTO
            {
                Id = cor.Id,
                Url = cor.Url,
            };

        public static IEnumerable<CorDTO> ToDtoList(IEnumerable<Cor> colors)
                => colors.Select(c => ToDto(c));

        public Cor ToDomain()
                => new Cor
                {
                    Id = Id,
                    Url = Url,
                };

        public static IEnumerable<Cor> ToDomainList(IEnumerable<CorDTO> colors)
            => colors.Select(c => c.ToDomain());
    }
}
