
using Fin.Domain.Models;

namespace Fin.Application.DTOs
{
    public record IconDTO 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public Icon ToDomain()
            => new Icon
            {
                Name = Name,
                Id = Id,
                Url = Url
            };

        public static IEnumerable<Icon> ToDomainList(IEnumerable<IconDTO> dto)
            => dto.Select(i => i.ToDomain());

        public static IEnumerable<IconDTO> ToDtoList(IEnumerable<Icon> icon)
            => icon.Select(i => IconDTO.ToDto(i));

        public static IconDTO ToDto(Icon domain)
            => new IconDTO
            {
                Id = domain.Id,
                Name = domain.Name,
                Url = domain.Url
            };
    }
}
