using AutoMapper;
using Fin.Domain.Models;
using Fin.Application.DTOs;

namespace Fin.Application.Mapper
{
    public class IconProfile : Profile
    {
        public IconProfile()
        {
            CreateMap<Icon, IconDTO>();
            CreateMap<IconDTO, Icon>();
        }
    }
}
