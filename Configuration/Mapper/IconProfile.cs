using AutoMapper;
using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Configuration.Mapper
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
