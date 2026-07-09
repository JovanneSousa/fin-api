using AutoMapper;
using Fin.Domain.Models;
using Fin.Application.DTOs;

namespace Fin.Application.Mapper
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
