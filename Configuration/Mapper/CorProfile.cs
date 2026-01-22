using AutoMapper;
using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Configuration.Mapper
{
    public class CorProfile : Profile
    {
        public CorProfile()
        {
            CreateMap<Cor, CorDTO>();
            CreateMap<CorDTO, Cor>();
        }
    }
}
