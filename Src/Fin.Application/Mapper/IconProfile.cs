using AutoMapper;
using Fin.Application.DTOs;
using Fin.Domain.Models;

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
